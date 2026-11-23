using System;
using Unity.Mathematics;

namespace SwiftTimer.Core
{
    /// <summary>
    /// Represents the current state and configurations of a timer in the <see cref="TimerManager"/>.
    /// </summary>
    public struct TimerState
    {
        #region Lifecycle
        public TimerState(float duration, float time, bool pause, TimerClock clock, TimerTiming timing, TimerEnvironment environment)
        {
            _duration = duration;
            _time = time;

            Clock = clock;
            Timing = timing;
            Environment = environment;

            _status = Status.None;
            _anchoredTimeline = default;

            if (pause) _status |= Status.Paused;
            else _status &= ~Status.Paused;
        }
        #endregion

        #region API
        /// <summary>
        /// Duration of the timer.
        /// </summary>
        public float Duration
        {
            readonly get => Clock is TimerClock.Frame or TimerClock.FixedFrame ? math.round(_duration) : _duration;
            set => _duration = value;
        }

        /// <summary>
        /// Elapsed time of the timer.
        /// </summary>
        public float Time
        {
            readonly get => Clock is TimerClock.Frame or TimerClock.FixedFrame ? math.round(_time) : _time;
            set => _time = value;
        }

        /// <summary>
        /// Whether the timer is paused.
        /// </summary>
        public bool Pause
        {
            readonly get => (_status & Status.Paused) != 0;
            set
            {
                if (value) _status |= Status.Paused;
                else _status &= ~Status.Paused;
            }
        }

        /// <summary>
        /// Clock applied to the timer. Changing it keeps the elapsed time of the timer.
        /// </summary>
        public TimerClock Clock;

        /// <summary>
        /// Timing applied to the timer.
        /// </summary>
        public TimerTiming Timing;

        /// <summary>
        /// Environment applied to the timer.
        /// </summary>
        public TimerEnvironment Environment;



        /// <summary>
        /// Whether the timer has completed.
        /// </summary>
        public readonly bool HasCompleted => (_status & Status.Completed) != 0;

        /// <summary>
        /// Whether the timer has stopped.
        /// </summary>
        public readonly bool HasStopped => (_status & Status.Stopped) != 0;

        /// <summary>
        /// Whether the timer has either been marked as completed or stopped.
        /// </summary>
        public readonly bool HasTerminated => HasCompleted || HasStopped;



        /// <summary>
        /// Marks the timer for completion.
        /// </summary>
        /// <remarks>
        /// The timer's callback is not invoked immediately. Completion is processed by the timer manager during its next update, which may introduce a frame delay.
        /// </remarks>
        /// <returns>
        /// True if the timer was successfully completed.
        /// </returns>
        public bool Complete()
        {
            if (HasTerminated) return false;
            _status |= Status.Completed;
            return true;
        }

        /// <summary>
        /// Marks the timer for stopping without invoking its callback.
        /// </summary>
        /// <returns>
        /// True if the timer was successfully stopped.
        /// </returns>
        public bool Stop()
        {
            if (HasTerminated) return false;
            _status |= Status.Stopped;
            return true;
        }
        #endregion

        #region Implementation
        Status _status;
        float _duration;
        float _time;
        double _anchoredTimeline;



        internal readonly bool HasReachedEnd(double timeline) => timeline >= _anchoredTimeline + Duration;
        internal void RefreshAnchor(double timeline) => _anchoredTimeline = timeline - _time;
        internal void RefreshTime(double timeline) => _time = (float)(timeline - _anchoredTimeline);



        [Flags]
        enum Status : byte
        {
            None = byte.MinValue,
            Paused = 1 << 0,
            Completed = 1 << 1,
            Stopped = 1 << 2,
        }
        #endregion
    }
}