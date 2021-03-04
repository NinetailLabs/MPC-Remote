namespace MPCRemote.Enumerations
{
    /// <summary>
    /// Commands that can be received from MPC
    /// </summary>
    public class MpcReceivedCommands
    {
        /// <summary>
        /// Commands relating to the connection status
        /// </summary>
        public static string Connection => "Connection";

        /// <summary>
        /// Commands relating to the current status of the media player
        /// </summary>
        public static string Status => "Status";
    }
}
