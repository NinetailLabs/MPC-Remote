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
    }
}
