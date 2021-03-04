namespace MPCRemote.Enumerations
{
    /// <summary>
    /// Commands that can be sent to the MPC TCP server
    /// </summary>
    public class MpcCommands
    {
        /// <summary>
        /// Request the opening of a file in MPC.
        /// The parameter to pass is the path to the file
        /// </summary>
        public static string OpenFile => "OpenFile";

        /// <summary>
        /// Request the media player to start playback.
        /// This is equivalent to hitting the play button in the MPC UI.
        /// </summary>
        public static string Play => "Play";

        /// <summary>
        /// Request the media player to stop playback.
        /// This is equivalent to hitting the stop button in the MPC UI.
        /// </summary>
        public static string Stop => "Stop";

        /// <summary>
        /// Request the media player to pause playback.
        /// This is equivalent to hitting the pause button in the MPC UI.
        /// </summary>
        public static string Pause => "Pause";
    }
}
