using Castforn.CommandAndCallback;
using SwiftTimer.Core;
using System;
using System.Collections.Generic;

namespace SwiftTimer
{

    /// <summary>
    /// Provides access to a timer managed by <see cref="TimerManager"/>.
    /// </summary>
    public readonly struct TimerHandle : IEquatable<TimerHandle>
    {
        #region Lifecycle
        public TimerHandle(TimerToken token) => _token = token;
        #endregion

        #region API
        /// <inheritdoc cref="TimerState.Duration"/>
        public float Duration
        {
            get => GetState().Duration;
            set
            {
                if (!TryGetState(out var state)) return;
                if (value == state.Duration) return;

                state.Duration = value;
                SetState(state);
            }
        }

        /// <inheritdoc cref="TimerState.Time"/>
        public float Time
        {
            get => GetState().Time;
            set
            {
                if (!TryGetState(out var state)) return;
                if (value == state.Time) return;

                state.Time = value;
                SetState(state);
            }
        }

        /// <summary>
        /// Remaining time of the timer.
        /// </summary>
        public float TimeRemaining
        {
            get
            {
                var state = GetState();
                return state.Duration - state.Time;
            }
            set
            {
                if (!TryGetState(out var state)) return;

                float time = state.Duration - value;
                if (time == state.Time) return;

                state.Time = time;
                SetState(state);
            }
        }

        /// <inheritdoc cref="TimerState.Pause"/>
        public bool Pause
        {
            get => GetState().Pause;
            set
            {
                if (!TryGetState(out var state)) return;
                if (value == state.Pause) return;

                state.Pause = value;
                SetState(state);
            }
        }

        /// <inheritdoc cref="TimerState.Clock"/>
        public TimerClock Clock
        {
            get => GetState().Clock;
            set
            {
                if (!TryGetState(out var state)) return;
                if (value == state.Clock) return;

                state.Clock = value;
                SetState(state);
            }
        }

        /// <inheritdoc cref="TimerState.Timing"/>
        public TimerTiming Timing
        {
            get => GetState().Timing;
            set
            {
                if (!TryGetState(out var state)) return;
                if (value == state.Timing) return;

                state.Timing = value;
                SetState(state);
            }
        }

        /// <inheritdoc cref="TimerState.Environment"/>
        public TimerEnvironment Environment
        {
            get => GetState().Environment;
            set
            {
                if (!TryGetState(out var state)) return;
                if (value == state.Environment) return;

                state.Environment = value;
                SetState(state);
            }
        }



        /// <summary>
        /// Entire duration of the timer and all of its dependencies.
        /// </summary>
        public float TotalDuration
        {
            get
            {
                float total = Duration;
                if (!TryGetDependency(out var dependency)) return total;

                _uniqueTokens.Clear();
                _uniqueTokens.Add(_token);

                while (dependency.IsInitialized)
                {
                    if (_uniqueTokens.Contains(dependency)) break;
                    _uniqueTokens.Add(dependency);

                    total += TimerManager.GetState(dependency).Duration;
                    if (!TimerManager.TryGetDependency(dependency, out dependency)) break;
                }

                return total;
            }
        }

        /// <summary>
        /// Entire elapsed time of the timer and all of its dependencies.
        /// </summary>
        public float TotalTime
        {
            get
            {
                float total = Time;
                if (!TryGetDependency(out var dependency)) return total;

                _uniqueTokens.Clear();
                _uniqueTokens.Add(_token);

                while (dependency.IsInitialized)
                {
                    if (_uniqueTokens.Contains(dependency)) break;
                    _uniqueTokens.Add(dependency);

                    total += TimerManager.GetState(dependency).Time;
                    if (!TimerManager.TryGetDependency(dependency, out dependency)) break;
                }

                return total;
            }
        }

        /// <summary>
        /// Entire remaining time of the timer and all of its dependencies.
        /// </summary>
        public float TotalTimeRemaining
        {
            get
            {
                float total = TimeRemaining;
                if (!TryGetDependency(out var dependency)) return total;

                _uniqueTokens.Clear();
                _uniqueTokens.Add(_token);

                while (dependency.IsInitialized)
                {
                    if (_uniqueTokens.Contains(dependency)) break;
                    _uniqueTokens.Add(dependency);

                    var state = TimerManager.GetState(dependency);
                    total += state.Duration - state.Time;

                    if (!TimerManager.TryGetDependency(dependency, out dependency)) break;
                }

                return total;
            }
        }



        /// <summary>
        /// Whether the timer is currently registered in <see cref="TimerManager"/>.
        /// </summary>
        public bool IsActive => TimerManager.Contains(_token);

        /// <summary>
        /// Whether the timer is active and is currently waiting for its dependency to complete.
        /// </summary>
        public bool IsPending => IsActive && GetDependency().IsInitialized;

        /// <summary>
        /// Whether the timer is active and is currently advancing its elapsed time.
        /// </summary>
        public bool IsRunning => IsActive && !IsPending && !Pause;



        /// <inheritdoc cref="TimerToken.Version"/>
        public int Version => _token.Version;

        /// <inheritdoc cref="TimerToken.Index"/>
        public int Index => _token.Index;

        /// <inheritdoc cref="TimerToken.IsInitialized"/>
        public bool IsInitialized => _token.IsInitialized;

        /// <inheritdoc cref="TimerState.HasCompleted"/>
        public bool HasCompleted => GetState().HasCompleted;

        /// <inheritdoc cref="TimerState.HasStopped"/>
        public bool HasStopped => GetState().HasStopped;

        /// <inheritdoc cref="TimerState.HasTerminated"/>
        public bool HasTerminated => GetState().HasTerminated;



        /// <summary>
        /// Changes the state of the timer.
        /// </summary>
        /// <returns>
        /// True if the change was successful.
        /// </returns>
        public bool ChangeState(float duration, float time, bool pause, TimerClock clock, TimerTiming timing, TimerEnvironment environment) => SetState(new(duration, time, pause, clock, timing, environment));



        /// <summary>
        /// Changes the callback of the timer.
        /// </summary>
        /// <returns>
        /// True if the change was successful.
        /// </returns>
        public bool ChangeCallback(Action callback) => SetCallback(Command.GetAction(callback));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1>(Action<T1> callback, T1 arg1) => SetCallback(Command.GetAction(callback, arg1));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2>(Action<T1, T2> callback, T1 arg1, T2 arg2) => SetCallback(Command.GetAction(callback, arg1, arg2));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2, T3>(Action<T1, T2, T3> callback, T1 arg1, T2 arg2, T3 arg3) => SetCallback(Command.GetAction(callback, arg1, arg2, arg3));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2, T3, T4>(Action<T1, T2, T3, T4> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4) => SetCallback(Command.GetAction(callback, arg1, arg2, arg3, arg4));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) => SetCallback(Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) => SetCallback(Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5, arg6));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) => SetCallback(Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5, arg6, arg7));

        /// <inheritdoc cref="ChangeCallback(Action)"/>
        public bool ChangeCallback<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> callback, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) => SetCallback(Command.GetAction(callback, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8));



        /// <inheritdoc cref="TimerState.Complete"/>
        public bool Complete()
        {
            if (!TryGetState(out var state)) return false;
            if (!state.Complete()) return false;

            SetState(state);
            return true;
        }

        /// <inheritdoc cref="TimerState.Stop"/>
        public bool Stop()
        {
            if (!TryGetState(out var state)) return false;
            if (!state.Stop()) return false;

            SetState(state);
            return true;
        }

        /// <summary>
        /// Marks the timer and all of its dependencies for completion.
        /// </summary>
        /// <returns>
        /// True if at least one timer was successfully completed.
        /// </returns>
        public bool CompleteAll()
        {
            if (!TryGetDependency(out var dependency)) return Complete();
            bool hasCompleted = false;

            _uniqueTokens.Clear();
            _uniqueTokens.Add(_token);

            while (dependency.IsInitialized)
            {
                if (_uniqueTokens.Contains(dependency)) break;
                _uniqueTokens.Add(dependency);
                if (!TimerManager.TryGetDependency(dependency, out dependency)) break;
            }

            for (int i = _uniqueTokens.Count - 1; i >= 0; i--)
            {
                var token = _uniqueTokens[i];
                var state = TimerManager.GetState(token);

                if (state.HasTerminated) continue;
                hasCompleted = true;

                state.Complete();
                TimerManager.SetState(token, state);
            }

            return hasCompleted;
        }

        /// <summary>
        /// Marks the timer and all of its dependencies for completion without invoking their callbacks.
        /// </summary>
        /// <returns>
        /// True if at least one timer was successfully stopped.
        /// </returns>
        public bool StopAll()
        {
            if (!TryGetDependency(out var dependency)) return Stop();
            bool hasStopped = false;

            _uniqueTokens.Clear();
            _uniqueTokens.Add(_token);

            while (dependency.IsInitialized)
            {
                if (_uniqueTokens.Contains(dependency)) break;
                _uniqueTokens.Add(dependency);
                if (!TimerManager.TryGetDependency(dependency, out dependency)) break;
            }

            for (int i = _uniqueTokens.Count - 1; i >= 0; i--)
            {
                var token = _uniqueTokens[i];
                var state = TimerManager.GetState(token);

                if (state.HasTerminated) continue;
                hasStopped = true;

                state.Stop();
                TimerManager.SetState(token, state);
            }

            return hasStopped;
        }



        /// <inheritdoc cref="TimerManager.GetState(TimerToken)"/>
        public TimerState GetState() => TimerManager.GetState(_token);

        /// <inheritdoc cref="TimerManager.SetState(TimerToken, TimerState)"/>
        public bool SetState(TimerState state) => TimerManager.SetState(_token, state);

        /// <inheritdoc cref="TimerManager.TryGetState(TimerToken, out TimerState)"/>
        public bool TryGetState(out TimerState state) => TimerManager.TryGetState(_token, out state);



        /// <inheritdoc cref="TimerManager.GetCallback(TimerToken)"/>
        public BaseActionCommand GetCallback() => TimerManager.GetCallback(_token);

        /// <inheritdoc cref="TimerManager.SetCallback(TimerToken, BaseActionCommand)"/>
        public bool SetCallback(BaseActionCommand callback) => TimerManager.SetCallback(_token, callback);

        /// <inheritdoc cref="TimerManager.TryGetCallback(TimerToken, out BaseActionCommand)"/>
        public bool TryGetCallback(out BaseActionCommand callback) => TimerManager.TryGetCallback(_token, out callback);



        /// <inheritdoc cref="TimerManager.GetDependency(TimerToken)"/>
        public TimerToken GetDependency() => TimerManager.GetDependency(_token);

        /// <inheritdoc cref="TimerManager.TryGetDependency(TimerToken, out TimerToken)"/>
        public bool TryGetDependency(out TimerToken dependency) => TimerManager.TryGetDependency(_token, out dependency);



        public bool Equals(TimerHandle other) => _token == other._token;

        public override bool Equals(object obj) => obj is TimerHandle other && Equals(other);
        public override int GetHashCode() => _token.GetHashCode();

        public static bool operator ==(TimerHandle left, TimerHandle right) => left.Equals(right);
        public static bool operator !=(TimerHandle left, TimerHandle right) => !(left == right);

        public static implicit operator bool(TimerHandle value) => value._token;
        public static implicit operator TimerToken(TimerHandle value) => value._token;
        #endregion

        #region Implementation
        static readonly List<TimerToken> _uniqueTokens = new();
        readonly TimerToken _token;
        #endregion
    }

}