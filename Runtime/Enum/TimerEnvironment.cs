namespace SwiftTimer
{

    /// <summary>
    /// Defines the environment in which a timer is allowed to operate.
    /// </summary>
    public enum TimerEnvironment : byte
    {
        /// <summary>
        /// The timer only operates while the application is running.
        /// </summary>
        Runtime,

        /// <summary>
        /// The timer only operates while the editor is not in play mode.
        /// </summary>
        Editor,

        /// <summary>
        /// The timer operates both in runtime and editor.
        /// </summary>
        Both,
    }

}