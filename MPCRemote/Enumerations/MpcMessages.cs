namespace MPCRemote.Enumerations
{
    /// <summary>
    /// Commands that can be received from MPC
    /// </summary>
    public class MpcMessages
    {
        #region API Messages

        /// <summary>
        /// Indicates that the API has connected
        /// </summary>
        public static string Connection => "API.Connection";

        /// <summary>
        /// Indicates the current API position
        /// </summary>
        public static string ApiVersion => "API.Version";

        #endregion

        #region Player Messages

        /// <summary>
        /// Indicates the player`s current playback state
        /// </summary>
        public static string PlayerStateChanged => "Player.StateChanged";

        /// <summary>
        /// Indicates the current playback position
        /// </summary>
        public static string PlaybackPosition => "Player.Position";

        /// <summary>
        /// Indicates the player`s current fullscreen status
        /// </summary>
        public static string FullscreenStatus => "Player.Fullscreen";

        #endregion

        #region Playlist Messages

        /// <summary>
        /// The current playlist
        /// </summary>
        public static string Playlist => "Playlist";

        #endregion

    }
}
