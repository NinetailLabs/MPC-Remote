namespace MPCRemote.Enumerations
{
    /// <summary>
    /// Contains the different player states
    /// </summary>
    public class PlayerStates
    {
        /// <summary>
        /// Indicates that the player is currently loading a file
        /// </summary>
        public static string Loading => "Loading";

        /// <summary>
        /// Indicates that the player is currently playing a file
        /// </summary>
        public static string Playing => "Playing";

        /// <summary>
        /// Indicate that the media is currently closed
        /// </summary>
        public static string Closed => "Closed";

        /// <summary>
        /// Indicate that the player is currently paused
        /// </summary>
        public static string Paused => "Paused";
    }
}
