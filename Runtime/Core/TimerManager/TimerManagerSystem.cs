using Castforn.CommandAndCallback;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SwiftTimer.Core
{

    public static partial class TimerManager
    {
        const int InnerLoopBatchCount = 128;
        const int MaxTrailingSlotsRemoval = 16;

        static NativeList<int> _versions = new(Allocator.Persistent);
        static NativeList<TimerState> _states = new(Allocator.Persistent);
        static NativeList<TimerToken> _dependencies = new(Allocator.Persistent);
        static readonly List<BaseActionCommand> _callbacks = new();

        static NativeQueue<int> _activatedTimers = new(Allocator.Persistent);
        static NativeQueue<int> _terminatedTimers = new(Allocator.Persistent);
        static readonly Stack<int> _freeSlots = new();

        static JobHandle _handle;
        static bool _isActive;

        static double _scaledTimeline;
        static double _unscaledTimeline;
        static ulong _frameTimeline;
        static ulong _fixedFrameTimeline;

        static double _oldScaledTimeline;
        static double _oldUnscaledTimeline;
        static ulong _oldFrameTimeline;
        static ulong _oldFixedFrameTimeline;



        static void SystemUpdate()
        {
            if (!_isActive) return;

#if UNITY_EDITOR
            bool inRuntime = EditorApplication.isPlayingOrWillChangePlaymode;
            _scaledTimeline += inRuntime ? Time.deltaTime : _editorDeltaTime * Time.timeScale;
            _unscaledTimeline += inRuntime ? Time.unscaledDeltaTime : _editorDeltaTime;
            _frameTimeline++;
#else
            const bool inRuntime = true;
            _scaledTimeline += Time.deltaTime;
            _unscaledTimeline += Time.unscaledDeltaTime;
            _frameTimeline++;
#endif

            if (!_handle.IsCompleted) return;
            _handle.Complete();

            RegisterScheduledTimers();
            RegisterModifiedCallbacks();
            RegisterModifiedStates();

            ResolveActivatedTimers();
            ResolveTerminatedTimers();
            RemoveTrailingSlots();

            if (_versions.Count > 0)
            {
                _handle = new TimerJob()
                {
                    Versions = _versions.AsReadOnly(),
                    TimerStates = _states.AsReadOnly(),
                    Dependencies = _dependencies.AsReadOnly(),

                    ActivatedTimers = _activatedTimers.AsParallelWriter(),
                    TerminatedTimers = _terminatedTimers.AsParallelWriter(),

                    InRuntime = inRuntime,
                    ScaledTimeline = _scaledTimeline,
                    UnscaledTimeline = _unscaledTimeline,
                    FrameTimeline = _frameTimeline,
                    FixedFrameTimeline = _fixedFrameTimeline,
                }
                .ScheduleParallel(_versions.Count, InnerLoopBatchCount, _handle);
            }
            else _isActive = false;

            _oldScaledTimeline = _scaledTimeline;
            _oldUnscaledTimeline = _unscaledTimeline;
            _oldFrameTimeline = _frameTimeline;
            _oldFixedFrameTimeline = _fixedFrameTimeline;
        }



        static void ResolveActivatedTimers()
        {
            while (_activatedTimers.TryDequeue(out int index))
            {
                if (_versions[index] == 0) continue;
                var state = _states[index];

                state.RefreshAnchor(GetOldTimeline(state.Clock));
                _dependencies[index] = default;

                double timeline = GetTimeline(state.Clock);
                if (state.HasReachedEnd(timeline))
                {
                    DispatchCallback(state.Timing, _callbacks[index]);
                    ReleaseSlotAt(index);
                    continue;
                }

                state.RefreshTime(timeline);
                _states[index] = state;
            }
        }

        static void ResolveTerminatedTimers()
        {
            while (_terminatedTimers.TryDequeue(out int index))
            {
                if (_versions[index] == 0) continue;
                var state = _states[index];

                if (state.HasStopped || !InValidEnvironment(state.Environment))
                {
                    _callbacks[index]?.Release();
                    ReleaseSlotAt(index);
                    continue;
                }

                if (state.HasReachedEnd(GetTimeline(state.Clock)) || state.HasCompleted)
                {
                    DispatchCallback(state.Timing, _callbacks[index]);
                    ReleaseSlotAt(index);
                }
            }
        }

        static void RemoveTrailingSlots()
        {
            int removalIndex = _versions.Count;
            int removedTrailing = 0;

            for (int i = _versions.Count - 1; i >= 0 && removedTrailing < MaxTrailingSlotsRemoval; i--)
            {
                if (_versions[i] != 0) break;
                removalIndex = i;
                removedTrailing++;
            }

            int removalCount = _versions.Count - removalIndex;
            if (removalCount > 0)
            {
                _versions.RemoveRange(removalIndex, removalCount);
                _states.Resize(removalIndex, NativeArrayOptions.UninitializedMemory);
                _dependencies.Resize(removalIndex, NativeArrayOptions.UninitializedMemory);
                _callbacks.RemoveRange(removalIndex, removalCount);
            }

            while (_freeSlots.TryPeek(out int index))
            {
                if (index < _versions.Count) return;
                _freeSlots.Pop();
            }
        }



        static void ReleaseSlotAt(int index)
        {
            _versions[index] = 0;
            _states[index] = default;
            _callbacks[index] = null;
            _dependencies[index] = default;
            _freeSlots.Push(index);
        }

        static double GetTimeline(TimerClock clock)
        {
            return clock switch
            {
                TimerClock.Scaled => _scaledTimeline,
                TimerClock.Unscaled => _unscaledTimeline,
                TimerClock.Frame => _frameTimeline,
                TimerClock.FixedFrame => _fixedFrameTimeline,
                _ => 0,
            };
        }

        static double GetOldTimeline(TimerClock clock)
        {
            return clock switch
            {
                TimerClock.Scaled => _oldScaledTimeline,
                TimerClock.Unscaled => _oldUnscaledTimeline,
                TimerClock.Frame => _oldFrameTimeline,
                TimerClock.FixedFrame => _oldFixedFrameTimeline,
                _ => 0,
            };
        }

        static bool InValidEnvironment(TimerEnvironment environment)
        {
#if UNITY_EDITOR
            return environment switch
            {
                TimerEnvironment.Runtime => EditorApplication.isPlayingOrWillChangePlaymode,
                TimerEnvironment.Editor => !EditorApplication.isPlayingOrWillChangePlaymode,
                TimerEnvironment.Both => true,
                _ => false,
            };
#else
            return environment is TimerEnvironment.Runtime or TimerEnvironment.Both;
#endif
        }



        [BurstCompile]
        struct TimerJob : IJobFor
        {
            public NativeArray<int>.ReadOnly Versions;
            public NativeArray<TimerState>.ReadOnly TimerStates;
            public NativeArray<TimerToken>.ReadOnly Dependencies;

            public NativeQueue<int>.ParallelWriter ActivatedTimers;
            public NativeQueue<int>.ParallelWriter TerminatedTimers;

            public bool InRuntime;
            public double ScaledTimeline;
            public double UnscaledTimeline;
            public double FrameTimeline;
            public double FixedFrameTimeline;



            void IJobFor.Execute(int index)
            {
                if (Versions[index] == 0) return;
                var state = TimerStates[index];

                if (!InValidEnvironment(state.Environment))
                {
                    TerminatedTimers.Enqueue(index);
                    return;
                }

                var dependency = Dependencies[index];
                if (dependency.IsInitialized)
                {
                    bool isAlive = dependency.Index < Versions.Length && dependency.Version == Versions[dependency.Index];
                    if (!isAlive) ActivatedTimers.Enqueue(index);
                    return;
                }

                if (state.Pause) return;
                double timeline = GetTimeline(state.Clock);

                if (!state.HasReachedEnd(timeline)) return;
                TerminatedTimers.Enqueue(index);
            }



            readonly double GetTimeline(TimerClock clock)
            {
                return clock switch
                {
                    TimerClock.Scaled => ScaledTimeline,
                    TimerClock.Unscaled => UnscaledTimeline,
                    TimerClock.Frame => FrameTimeline,
                    TimerClock.FixedFrame => FixedFrameTimeline,
                    _ => 0,
                };
            }

            readonly bool InValidEnvironment(TimerEnvironment environment)
            {
                return environment switch
                {
                    TimerEnvironment.Runtime => InRuntime,
                    TimerEnvironment.Editor => !InRuntime,
                    TimerEnvironment.Both => true,
                    _ => false,
                };
            }
        }
    }

}