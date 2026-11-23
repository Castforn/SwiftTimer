using Castforn.CommandAndCallback;
using System.Collections.Generic;

namespace SwiftTimer.Core
{

    public static partial class TimerManager
    {
        static void DispatchCallback(TimerTiming timing, BaseActionCommand callback)
        {
            if (callback == null) return;
            switch (timing)
            {
                case TimerTiming.BeginFrame: _beginFrameQueue.Enqueue(callback); return;

                case TimerTiming.PreFixedUpdate: _preFixedUpdateQueue.Enqueue(callback); return;
                case TimerTiming.PostFixedUpdate: _postFixedUpdateQueue.Enqueue(callback); return;

                case TimerTiming.PreUpdate: _preUpdateQueue.Enqueue(callback); return;
                case TimerTiming.PostUpdate: _postUpdateQueue.Enqueue(callback); return;

                case TimerTiming.PreLateUpdate: _preLateUpdateQueue.Enqueue(callback); return;
                case TimerTiming.PostLateUpdate: _postLateUpdateQueue.Enqueue(callback); return;

                case TimerTiming.EndFrame: _endFrameQueue.Enqueue(callback); return;
            }
        }



        static readonly Queue<BaseActionCommand> _beginFrameQueue = new();
        static void BeginFrame()
        {
            while (_beginFrameQueue.TryDequeue(out var callback)) callback.Execute();
        }


        
        static readonly Queue<BaseActionCommand> _preFixedUpdateQueue = new();
        static void PreFixedUpdate()
        {
            _fixedFrameTimeline++;
            while (_preFixedUpdateQueue.TryDequeue(out var callback)) callback.Execute();
        }

        static readonly Queue<BaseActionCommand> _postFixedUpdateQueue = new();
        static void PostFixedUpdate()
        {
            while (_postFixedUpdateQueue.TryDequeue(out var callback)) callback.Execute();
        }



        static readonly Queue<BaseActionCommand> _preUpdateQueue = new();
        static void PreUpdate()
        {
            while (_preUpdateQueue.TryDequeue(out var callback)) callback.Execute();
        }

        static readonly Queue<BaseActionCommand> _postUpdateQueue = new();
        static void PostUpdate()
        {
            while (_postUpdateQueue.TryDequeue(out var callback)) callback.Execute();
        }



        static readonly Queue<BaseActionCommand> _preLateUpdateQueue = new();
        static void PreLateUpdate()
        {
            while (_preLateUpdateQueue.TryDequeue(out var callback)) callback.Execute();
        }

        static readonly Queue<BaseActionCommand> _postLateUpdateQueue = new();
        static void PostLateUpdate()
        {
            while (_postLateUpdateQueue.TryDequeue(out var callback)) callback.Execute();
        }



        static readonly Queue<BaseActionCommand> _endFrameQueue = new();
        static void EndFrame()
        {
            while (_endFrameQueue.TryDequeue(out var callback)) callback.Execute();
        }
    }

}