using UnityEngine;

namespace SwiftTimer
{

    /// <summary>
    /// Specifies when a timer's callback is executed within the Unity PlayerLoop.
    /// </summary>
    public enum TimerTiming : byte
    {
        /// <summary>
        /// Executes at the beginning of a new frame.
        /// </summary>
        BeginFrame,



        /// <summary>
        /// Executes before <see cref="MonoBehaviour"/> FixedUpdate.
        /// </summary>
        PreFixedUpdate,

        /// <summary>
        /// Executes after <see cref="MonoBehaviour"/> FixedUpdate.
        /// </summary>
        PostFixedUpdate,



        /// <summary>
        /// Executes before <see cref="MonoBehaviour"/> Update.
        /// </summary>
        PreUpdate,

        /// <summary>
        /// Executes after <see cref="MonoBehaviour"/> Update.
        /// </summary>
        PostUpdate,



        /// <summary>
        /// Executes before <see cref="MonoBehaviour"/> LateUpdate.
        /// </summary>
        PreLateUpdate,

        /// <summary>
        /// Executes after <see cref="MonoBehaviour"/> LateUpdate.
        /// </summary>
        PostLateUpdate,



        /// <summary>
        /// Executes at the end of the current frame.
        /// </summary>
        EndFrame,
    }

}