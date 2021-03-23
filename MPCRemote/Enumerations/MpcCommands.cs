namespace MPCRemote.Enumerations
{
    /// <summary>
    /// Commands that can be sent to the MPC TCP server
    /// </summary>
    public class MpcCommands
    {

        #region API Commands

        /// <summary>
        /// Request the TCP API version
        /// </summary>
        public static string GetApiVersion => "API.GetVersion";

        #endregion

        #region Player Commands

        /// <summary>
        /// Request the player`s current status
        /// </summary>
        public static string GetPlayerStatus => "Player.GetStatus";

        /// <summary>
        /// Request the opening of a file in MPC.
        /// The parameter to pass is the path to the file
        /// </summary>
        public static string OpenFile => "Player.OpenFile";

        /// <summary>
        /// Request the media player to start playback.
        /// This is equivalent to hitting the play button in the MPC UI.
        /// </summary>
        public static string Play => "Player.Play";

        /// <summary>
        /// Request the media player to stop playback.
        /// This is equivalent to hitting the stop button in the MPC UI.
        /// </summary>
        public static string Stop => "Player.Stop";

        /// <summary>
        /// Request the media player to pause playback.
        /// This is equivalent to hitting the pause button in the MPC UI.
        /// </summary>
        public static string Pause => "Player.Pause";

        /// <summary>
        /// Request that the media player seek to a specific position.
        /// </summary>
        public static string SeekTo => "Player.SeekTo";
        
        /// <summary>
        /// Request that the player show an OSD message
        /// </summary>
        public static string ShowOnScreenMessage => "Player.OSD";

        #endregion

        #region Playlist Commands

        /// <summary>
        /// Request that the player play a file that is in the playlist
        /// </summary>
        public static string PlayPlaylistFile => "Playlist.PlayFile";

        /// <summary>
        /// Request that the player removes a file from the playlist
        /// </summary>
        public static string RemovePlaylistEntry => "Playlist.RemoveEntry";

        /// <summary>
        /// Request that a file is inserted into the playlist
        /// </summary>
        public static string InsertPlaylistEntry => "Playlist.InsertEntry";

        /// <summary>
        /// Request that a playlist entry is moved from one position to another
        /// </summary>
        public static string MovePlaylistEntry => "Playlist.MoveEntry";

        #endregion

    }
}
