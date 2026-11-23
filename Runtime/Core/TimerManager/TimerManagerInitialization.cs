using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

using static SwiftTimer.PlayerLoop.PlayerLoopRunners;
using static SwiftTimer.PlayerLoop.PlayerLoopUtility;

using static UnityEngine.PlayerLoop.FixedUpdate;
using static UnityEngine.PlayerLoop.PreLateUpdate;
using static UnityEngine.PlayerLoop.Update;

namespace SwiftTimer.Core
{

    public static partial class TimerManager
    {
#if UNITY_EDITOR
        static double _editorDeltaTime;
        static double _lastEditorTime;

        [InitializeOnLoadMethod]
        static void InitializeEditor()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;

            AssemblyReloadEvents.beforeAssemblyReload -= DisposeNativeCollections;
            AssemblyReloadEvents.beforeAssemblyReload += DisposeNativeCollections;
        }

        static void EditorUpdate()
        {
            if (!_isActive || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                _editorDeltaTime = 0;
                _lastEditorTime = EditorApplication.timeSinceStartup;
                return;
            }

            double editorTime = EditorApplication.timeSinceStartup;
            _editorDeltaTime = editorTime - _lastEditorTime;
            _lastEditorTime = editorTime;

            SystemUpdate();

            BeginFrame();

            PreFixedUpdate();
            PostFixedUpdate();

            PreUpdate();
            PostUpdate();

            PreLateUpdate();
            PostLateUpdate();

            EndFrame();
        }

        static void DisposeNativeCollections()
        {
            _handle.Complete();
            if (_versions.IsCreated) _versions.Dispose();
            if (_states.IsCreated) _states.Dispose();
            if (_dependencies.IsCreated) _dependencies.Dispose();
            if (_activatedTimers.IsCreated) _activatedTimers.Dispose();
            if (_terminatedTimers.IsCreated) _terminatedTimers.Dispose();
        }
#endif



        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void InitializeRuntime()
        {
            var loop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            RemoveRunners(ref loop);

            if (!InsertRunners(ref loop)) return;
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(loop);
        }

        static bool InsertRunners(ref PlayerLoopSystem loop)
        {
            bool success = true;
            bool result = true;

            result = InsertAfter(ref loop, typeof(TimeUpdate.WaitForLastPresentationAndUpdateTime), CreateRunner<SwiftTimerLoopRunnerSystemUpdate>(SystemUpdate)); Assert(nameof(SystemUpdate));

            result = InsertAtBegin(ref loop, typeof(Initialization), CreateRunner<SwiftTimerLoopRunnerBeginFrame>(BeginFrame)); Assert(nameof(BeginFrame));

            result = InsertBefore(ref loop, typeof(ScriptRunBehaviourFixedUpdate), CreateRunner<SwiftTimerLoopRunnerPreFixedUpdate>(PreFixedUpdate)); Assert(nameof(PreFixedUpdate));
            result = InsertAfter(ref loop, typeof(ScriptRunBehaviourFixedUpdate), CreateRunner<SwiftTimerLoopRunnerPostFixedUpdate>(PostFixedUpdate)); Assert(nameof(PostFixedUpdate));

            result = InsertBefore(ref loop, typeof(ScriptRunBehaviourUpdate), CreateRunner<SwiftTimerLoopRunnerPreUpdate>(PreUpdate)); Assert(nameof(PreUpdate));
            result = InsertAfter(ref loop, typeof(ScriptRunBehaviourUpdate), CreateRunner<SwiftTimerLoopRunnerPostUpdate>(PostUpdate)); Assert(nameof(PostUpdate));

            result = InsertBefore(ref loop, typeof(ScriptRunBehaviourLateUpdate), CreateRunner<SwiftTimerLoopRunnerPreLateUpdate>(PreLateUpdate)); Assert(nameof(PreLateUpdate));
            result = InsertAfter(ref loop, typeof(ScriptRunBehaviourLateUpdate), CreateRunner<SwiftTimerLoopRunnerPostLateUpdate>(PostLateUpdate)); Assert(nameof(PostLateUpdate));

            result = InsertAtEnd(ref loop, typeof(PostLateUpdate), CreateRunner<SwiftTimerLoopRunnerEndFrame>(EndFrame)); Assert(nameof(EndFrame));

            return success;
            void Assert(string name)
            {
                Debug.Assert(result, $"{nameof(SwiftTimer)} has failed to register '{name}'");
                success &= result;
            }
        }

        static void RemoveRunners(ref PlayerLoopSystem loop)
        {
            Remove(ref loop, typeof(SwiftTimerLoopRunnerSystemUpdate));

            Remove(ref loop, typeof(SwiftTimerLoopRunnerBeginFrame));

            Remove(ref loop, typeof(SwiftTimerLoopRunnerPreFixedUpdate));
            Remove(ref loop, typeof(SwiftTimerLoopRunnerPostFixedUpdate));

            Remove(ref loop, typeof(SwiftTimerLoopRunnerPreUpdate));
            Remove(ref loop, typeof(SwiftTimerLoopRunnerPostUpdate));

            Remove(ref loop, typeof(SwiftTimerLoopRunnerPreLateUpdate));
            Remove(ref loop, typeof(SwiftTimerLoopRunnerPostLateUpdate));

            Remove(ref loop, typeof(SwiftTimerLoopRunnerEndFrame));
        }
    }

}