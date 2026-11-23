using Castforn.CommandAndCallback;
using SwiftTimer.Core;
using System;
using UnityEngine;

namespace SwiftTimer
{

    /// <summary>
    /// Represents a reusable blueprint that can create independent timer instances.
    /// </summary>
    public struct TimerBlueprint
    {
        #region Constructor
        public TimerBlueprint(float duration, float time, bool pause, TimerClock clock, TimerTiming timing, TimerEnvironment environment)
        {
            _duration = duration;
            _time = time;

            Pause = pause;
            Clock = clock;
            Timing = timing;
            Environment = environment;
        }
        #endregion

        #region API
        /// <summary>
        /// Creates a timer blueprint with the specified configuration.
        /// </summary>
        public static TimerBlueprint Create(float duration = 0, float time = 0, bool pause = false, TimerClock clock = TimerClock.Scaled, TimerTiming timing = TimerTiming.BeginFrame, TimerEnvironment environment = TimerEnvironment.Runtime) => new(duration, time, pause, clock, timing, environment);



        /// <summary>
        /// Specified duration of the timer.
        /// </summary>
        public float Duration
        {
            readonly get => Clock is TimerClock.Frame or TimerClock.FixedFrame ? Mathf.Round(_duration) : _duration;
            set => _duration = value;
        }

        /// <summary>
        /// Initial elapsed time of the timer.
        /// </summary>
        public float Time
        {
            readonly get => Clock is TimerClock.Frame or TimerClock.FixedFrame ? Mathf.Round(_time) : _time;
            set => _time = value;
        }

        /// <summary>
        /// Whether the timer will be paused upon creation.
        /// </summary>
        public bool Pause;

        /// <summary>
        /// Time source used to measure the timer's elapsed duration.
        /// </summary>
        public TimerClock Clock;

        /// <summary>
        /// Determines when the timer's callback is invoked.
        /// </summary>
        public TimerTiming Timing;

        /// <summary>
        /// Determines the environment in which the timer is allowed to exist.
        /// </summary>
        public TimerEnvironment Environment;



        /// <summary>
        /// Sets this blueprint with the specified configuration.
        /// </summary>
        public TimerBlueprint Set(float duration, float time, bool pause, TimerClock clock, TimerTiming timing)
        {
            _duration = duration;
            _time = time;
            Pause = pause;
            Clock = clock;
            Timing = timing;
            return this;
        }

        /// <summary>
        /// Sets this blueprint with the specified duration.
        /// </summary>
        public TimerBlueprint SetDuration(float value)
        {
            _duration = value;
            return this;
        }

        /// <summary>
        /// Sets this blueprint with the specified elapsed time.
        /// </summary>
        public TimerBlueprint SetTime(float value)
        {
            _time = value;
            return this;
        }

        /// <summary>
        /// Sets this blueprint with the specified pause state.
        /// </summary>
        public TimerBlueprint SetPause(bool value)
        {
            Pause = value;
            return this;
        }

        /// <summary>
        /// Sets this blueprint with the specified time measurement.
        /// </summary>
        public TimerBlueprint SetClock(TimerClock value)
        {
            Clock = value;
            return this;
        }

        /// <summary>
        /// Sets this blueprint with the specified callback execution timing.
        /// </summary>
        public TimerBlueprint SetTiming(TimerTiming value)
        {
            Timing = value;
            return this;
        }

        /// <summary>
        /// Sets this blueprint with the specified environment.
        /// </summary>
        public TimerBlueprint SetEnvironment(TimerEnvironment value)
        {
            Environment = value;
            return this;
        }



        /// <summary>
        /// Schedules a timer and invokes the specified callback upon completion.
        /// </summary>
        /// <returns>
        /// A handle identifying the scheduled timer.
        /// </returns>
        public readonly TimerHandle Schedule(TimerToken dependency = default) => new(TimerManager.Schedule(this, null, dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule(Action callback, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1>(Action<T1> callback, T1 arg1, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2>(Action<T1, T2> callback, T1 arg1, T2 arg2, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2, T3>(Action<T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2, arg3), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2, arg3, arg4), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5, arg6), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5, arg6, arg7), dependency));

        /// <inheritdoc cref="Schedule(TimerToken)"/>
        public readonly TimerHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, TimerToken dependency = default) => new(TimerManager.Schedule(this, Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8), dependency));



        public static implicit operator TimerState(TimerBlueprint value) => new(value.Duration, value.Time, value.Pause, value.Clock, value.Timing, value.Environment);
        #endregion

        #region Implementation
        float _duration;
        float _time;
        #endregion
    }

}