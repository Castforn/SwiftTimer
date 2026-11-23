using SwiftTimer.Core;
using UnityEngine;

namespace SwiftTimer.PlayerLoop
{

    /// <summary>
    /// Contains marker types used to identify SwiftTimer's custom PlayerLoop systems.
    /// </summary>
    public static class PlayerLoopRunners
    {
        /// <summary>
        /// Identifies as SwiftTimer's system which updates the mechanisms inside <see cref="TimerManager"/>.
        /// </summary>
        public struct SwiftTimerLoopRunnerSystemUpdate { }



        /// <summary>
        /// Identifies as SwiftTimer's system which executes at the beginning of the frame.
        /// </summary>
        public struct SwiftTimerLoopRunnerBeginFrame { }



        /// <summary>
        /// Identifies as SwiftTimer's system which executes before <see cref="MonoBehaviour"/> FixedUpdate.
        /// </summary>
        public struct SwiftTimerLoopRunnerPreFixedUpdate { }

        /// <summary>
        /// Identifies as SwiftTimer's system which executes after <see cref="MonoBehaviour"/> FixedUpdate.
        /// </summary>
        public struct SwiftTimerLoopRunnerPostFixedUpdate { }



        /// <summary>
        /// Identifies as SwiftTimer's system which executes before <see cref="MonoBehaviour"/> Update.
        /// </summary>
        public struct SwiftTimerLoopRunnerPreUpdate { }

        /// <summary>
        /// Identifies as SwiftTimer's system which executes after <see cref="MonoBehaviour"/> Update.
        /// </summary>
        public struct SwiftTimerLoopRunnerPostUpdate { }



        /// <summary>
        /// Identifies as SwiftTimer's system which executes before <see cref="MonoBehaviour"/> LateUpdate.
        /// </summary>
        public struct SwiftTimerLoopRunnerPreLateUpdate { }

        /// <summary>
        /// Identifies as SwiftTimer's system which executes after <see cref="MonoBehaviour"/> LateUpdate.
        /// </summary>
        public struct SwiftTimerLoopRunnerPostLateUpdate { }



        /// <summary>
        /// Identifies as SwiftTimer's system which executes at the end of the frame.
        /// </summary>
        public struct SwiftTimerLoopRunnerEndFrame { }
    }

}