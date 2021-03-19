namespace MPCRemote.Models
{
    /// <summary>
    /// Class used to send commands from MPC
    /// </summary>
    public class MpcCommand
    {
        /// <summary>
        /// The command to execute
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The parameters for the command
        /// </summary>
        public string Parameters { get; set; }

        /// <summary>
        /// The index of the entry
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Index used when items in the playlist are moved
        /// </summary>
        public int TargetIndex { get; set; }
    }
}
