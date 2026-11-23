using System;

namespace SwiftTimer.Core
{

    /// <summary>
    /// Represents a token to a slot entry within the <see cref="TimerManager"/>.
    /// </summary>
    public readonly struct TimerToken : IEquatable<TimerToken>
    {
        #region Lifecycle
        internal TimerToken(int index)
        {
            Version = ++_increment == 0 ? _increment = 1 : _increment;
            Index = index;
        }
        #endregion

        #region API
        /// <summary>
        /// Used to distinguish successive occupants of the same slot in the <see cref="TimerManager"/>.
        /// </summary>
        public readonly int Version;

        /// <summary>
        /// Index of a slot within <see cref="TimerManager"/>.
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Indicates whether the token has been initialized by the <see cref="TimerManager"/>.
        /// </summary>
        /// <remarks>
        /// Only <see cref="TimerManager"/> can create a valid token.
        /// </remarks>
        public bool IsInitialized => Version != 0;

        public readonly bool Equals(TimerToken other) =>
            Version == other.Version &&
            Index == other.Index;

        public override readonly bool Equals(object obj) => obj is TimerToken other && Equals(other);
        public override readonly int GetHashCode() => HashCode.Combine(Version, Index);

        public static bool operator ==(TimerToken left, TimerToken right) => left.Equals(right);
        public static bool operator !=(TimerToken left, TimerToken right) => !(left == right);
        public static implicit operator bool(TimerToken value) => TimerManager.Contains(value);
        #endregion

        #region Implementation
        static int _increment;
        #endregion
    }

}