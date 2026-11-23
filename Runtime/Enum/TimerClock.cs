using UnityEngine;

namespace SwiftTimer
{

    /// <summary>
    /// Specifies the time source used to measure a timer's elapsed duration.
    /// </summary>
    public enum TimerClock : byte
    {
        /// <summary>
        /// Uses scaled time, which is affected by <see cref="Time.timeScale"/>.
        /// </summary>
        Scaled,

        /// <summary>
        /// Uses unscaled time, which is unaffected by <see cref="Time.timeScale"/>.
        /// </summary>
        Unscaled,

        /// <summary>
        /// Uses the number of rendered frames to measure elapsed duration.
        /// </summary>
        Frame,

        /// <summary>
        /// Uses the number of fixed update cycles to measure elapsed duration.
        /// </summary>
        FixedFrame,
    }

}