using Castforn.CommandAndCallback;

namespace SwiftTimer.Core
{

    /// <summary>
    /// Manages timer scheduling, execution, and lifecycle within Unity's PlayerLoop.
    /// </summary>
    public static partial class TimerManager
    {
        /// <summary>
        /// Schedules a timer for execution.
        /// </summary>
        /// <returns>
        /// The token identifying the scheduled timer.
        /// </returns>
        public static TimerToken Schedule(TimerState state, BaseActionCommand callback, TimerToken dependency = default)
        {
            if (!InValidEnvironment(state.Environment))
            {
                callback?.Release();
                return default;
            }

            _isActive = true;
            state.RefreshAnchor(GetTimeline(state.Clock));

            if (dependency.IsInitialized && !Contains(dependency)) dependency = default;
            TimerToken token;

            while (_freeSlots.TryPop(out int index))
                if (index < _versions.Count)
                {
                    token = new TimerToken(index);
                    _scheduledTimers.Add(new(token, state, callback, dependency));
                    return token;
                }
            
            token = new TimerToken(_versions.Count + _scheduledAppendCount++);
            _scheduledTimers.Add(new(token, state, callback, dependency));
            return token;
        }

        /// <summary>
        /// Determines whether the manager is handling the timer.
        /// </summary>
        /// <returns>
        /// True if the timer exists.
        /// </returns>
        public static bool Contains(TimerToken token) => token.IsInitialized && InRegisteredTimers(token) || InScheduledTimers(token, out _);



        /// <summary>
        /// Gets a snapshot of the current state of a timer.
        /// </summary>
        /// <returns>
        /// A snapshot of the current timer state.
        /// </returns>
        public static TimerState GetState(TimerToken token)
        {
            TryGetState(token, out var state);
            return state;
        }

        /// <summary>
        /// Sets a new state to a timer.
        /// </summary>
        /// <returns>
        /// True if the state was successfully set.
        /// </returns>
        public static bool SetState(TimerToken token, TimerState state)
        {
            if (!token.IsInitialized) return false;
            state.RefreshAnchor(GetTimeline(state.Clock));

            if (InRegisteredTimers(token))
            {
                if (InModifiedStates(token, out int modifiedIndex))
                {
                    var modified = _modifiedStates[modifiedIndex];
                    modified.State = state;

                    _modifiedStates[modifiedIndex] = modified;
                    return true;
                }

                _modifiedStates.Add(new(token, state));
                return true;
            }

            if (InScheduledTimers(token, out int scheduledIndex))
            {
                var scheduledTimer = _scheduledTimers[scheduledIndex];
                scheduledTimer.State = state;

                _scheduledTimers[scheduledIndex] = scheduledTimer;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to get a snapshot of the current state of a timer.
        /// </summary>
        /// <returns>
        /// True if the state was successfully retrieved.
        /// </returns>
        public static bool TryGetState(TimerToken token, out TimerState state)
        {
            state = default;
            if (!token.IsInitialized) return false;

            if (InRegisteredTimers(token))
            {
                state = InModifiedStates(token, out int modifiedIndex) ? _modifiedStates[modifiedIndex].State : _states[token.Index];
                if (!state.Pause && !_dependencies[token.Index].IsInitialized) state.RefreshTime(GetTimeline(state.Clock));
                return true;
            }

            if (InScheduledTimers(token, out int index))
            {
                var scheduledTimer = _scheduledTimers[index];
                state = scheduledTimer.State;

                if (!state.Pause && !scheduledTimer.Dependency.IsInitialized) state.RefreshTime(GetTimeline(state.Clock));
                return true;
            }
            return false;
        }



        /// <summary>
        /// Gets the callback associated with a timer.
        /// </summary>
        /// <returns>
        /// The timer callback.
        /// </returns>
        public static BaseActionCommand GetCallback(TimerToken token)
        {
            TryGetCallback(token, out var callback);
            return callback;
        }

        /// <summary>
        /// Sets the callback associated with a timer.
        /// </summary>
        /// <returns>
        /// True if the callback was successfully set.
        /// </returns>
        public static bool SetCallback(TimerToken token, BaseActionCommand callback)
        {
            if (!token.IsInitialized)
            {
                callback?.Release();
                return false;
            }

            if (InRegisteredTimers(token))
            {
                if (InModifiedCallbacks(token, out int modifiedIndex))
                {
                    var modified = _modifiedCallbacks[modifiedIndex];
                    modified.Callback?.Release();
                    modified.Callback = callback;

                    _modifiedCallbacks[modifiedIndex] = modified;
                    return true;
                }

                _modifiedCallbacks.Add(new(token, callback));
                return true;
            }

            if (InScheduledTimers(token, out int scheduledIndex))
            {
                var scheduledTimer = _scheduledTimers[scheduledIndex];
                scheduledTimer.Callback?.Release();
                scheduledTimer.Callback = callback;

                _scheduledTimers[scheduledIndex] = scheduledTimer;
                return true;
            }

            callback?.Release();
            return false;
        }

        /// <summary>
        /// Attempts to get the callback associated with a timer.
        /// </summary>
        /// <returns>
        /// True if the callback was successfully retrieved.
        /// </returns>
        public static bool TryGetCallback(TimerToken token, out BaseActionCommand callback)
        {
            callback = null;
            if (!token.IsInitialized) return false;

            if (InRegisteredTimers(token))
            {
                if (InModifiedCallbacks(token, out int modifiedIndex))
                {
                    callback = _modifiedCallbacks[modifiedIndex].Callback;
                    return true;
                }

                callback = _callbacks[token.Index];
                return true;
            }

            if (InScheduledTimers(token, out int index))
            {
                callback = _scheduledTimers[index].Callback;
                return true;
            }
            return false;
        }



        /// <summary>
        /// Gets a snapshot of the dependency associated with a timer.
        /// </summary>
        /// <returns>
        /// The dependency token.
        /// </returns>
        public static TimerToken GetDependency(TimerToken token)
        {
            TryGetDependency(token, out var dependency);
            return dependency;
        }

        /// <summary>
        /// Attempts to get a snapshot of the dependency associated with a timer.
        /// </summary>
        /// <returns>
        /// True if the dependency was successfully retrieved.
        /// </returns>
        public static bool TryGetDependency(TimerToken token, out TimerToken dependency)
        {
            dependency = default;
            if (!token.IsInitialized) return false;

            if (InRegisteredTimers(token))
            {
                dependency = _dependencies[token.Index];
                return true;
            }

            if (InScheduledTimers(token, out int index))
            {
                dependency = _scheduledTimers[index].Dependency;
                return true;
            }
            return false;
        }
    }

}