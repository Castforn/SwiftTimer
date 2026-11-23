using Castforn.CommandAndCallback;
using System.Collections.Generic;

namespace SwiftTimer.Core
{

    public static partial class TimerManager
    {
        static readonly List<ScheduledTimer> _scheduledTimers = new();
        static readonly List<ModifiedState> _modifiedStates = new();
        static readonly List<ModifiedCallback> _modifiedCallbacks = new();
        static int _scheduledAppendCount;



        static bool InRegisteredTimers(TimerToken token)
        {
            if (token.Index >= _versions.Count) return false;
            return token.Version == _versions[token.Index];
        }

        static bool InScheduledTimers(TimerToken token, out int index)
        {
            for (int i = _scheduledTimers.Count - 1; i >= 0; i--)
                if (_scheduledTimers[i].Token == token)
                {
                    index = i;
                    return true;
                }

            index = 0;
            return false;
        }

        static bool InModifiedStates(TimerToken token, out int index)
        {
            for (int i = _modifiedStates.Count - 1; i >= 0; i--)
                if (_modifiedStates[i].Token == token)
                {
                    index = i;
                    return true;
                }

            index = 0;
            return false;
        }

        static bool InModifiedCallbacks(TimerToken token, out int index)
        {
            for (int i = _modifiedCallbacks.Count - 1; i >= 0; i--)
                if (_modifiedCallbacks[i].Token == token)
                {
                    index = i;
                    return true;
                }

            index = 0;
            return false;
        }



        static void RegisterScheduledTimers()
        {
            for (int i = 0; i < _scheduledTimers.Count; i++)
            {
                var scheduledTimer = _scheduledTimers[i];
                var token = scheduledTimer.Token;
                var state = scheduledTimer.State;

                if (token.Index < _versions.Count)
                {
                    _versions[token.Index] = token.Version;
                    _states[token.Index] = state;
                    _callbacks[token.Index] = scheduledTimer.Callback;
                    _dependencies[token.Index] = scheduledTimer.Dependency;
                }
                else
                {
                    _versions.Add(token.Version);
                    _states.Add(state);
                    _callbacks.Add(scheduledTimer.Callback);
                    _dependencies.Add(scheduledTimer.Dependency);
                }

                if (state.HasStopped)
                {
                    _callbacks[token.Index]?.Release();
                    ReleaseSlotAt(token.Index);
                    continue;
                }
                
                if (state.HasCompleted || (!_dependencies[token.Index].IsInitialized && state.Time >= state.Duration))
                {
                    DispatchCallback(state.Timing, _callbacks[token.Index]);
                    ReleaseSlotAt(token.Index);
                }
            }

            _scheduledTimers.Clear();
            _scheduledAppendCount = 0;
        }

        static void RegisterModifiedStates()
        {
            for (int i = 0; i < _modifiedStates.Count; i++)
            {
                var modified = _modifiedStates[i];
                var state = modified.State;
                int index = modified.Token.Index;

                if (state.HasStopped || !InValidEnvironment(state.Environment))
                {
                    _callbacks[index]?.Release();
                    ReleaseSlotAt(index);
                    continue;
                }

                if (state.HasCompleted || (!_dependencies[index].IsInitialized && state.Time >= state.Duration))
                {
                    DispatchCallback(state.Timing, _callbacks[index]);
                    ReleaseSlotAt(index);
                    continue;
                }

                _states[index] = state;
            }

            _modifiedStates.Clear();
        }

        static void RegisterModifiedCallbacks()
        {
            for (int i = 0; i < _modifiedCallbacks.Count; i++)
            {
                var modified = _modifiedCallbacks[i];
                var oldCallback = _callbacks[modified.Token.Index];

                if (modified.Callback == oldCallback) continue;
                oldCallback?.Release();
                _callbacks[modified.Token.Index] = modified.Callback;
            }

            _modifiedCallbacks.Clear();
        }



        struct ScheduledTimer
        {
            #region Lifecycle
            public ScheduledTimer(TimerToken token, TimerState state, BaseActionCommand callback, TimerToken dependency)
            {
                Token = token;
                State = state;
                Callback = callback;
                Dependency = dependency;
            }
            #endregion

            #region API
            public TimerToken Token;
            public TimerState State;
            public BaseActionCommand Callback;
            public TimerToken Dependency;
            #endregion
        }

        struct ModifiedState
        {
            #region Lifecycle
            public ModifiedState(TimerToken token, TimerState state)
            {
                Token = token;
                State = state;
            }
            #endregion

            #region API
            public TimerToken Token;
            public TimerState State;
            #endregion
        }

        struct ModifiedCallback
        {
            #region Lifecycle
            public ModifiedCallback(TimerToken token, BaseActionCommand callback)
            {
                Token = token;
                Callback = callback;
            }
            #endregion

            #region API
            public TimerToken Token;
            public BaseActionCommand Callback;
            #endregion
        }
    }

}