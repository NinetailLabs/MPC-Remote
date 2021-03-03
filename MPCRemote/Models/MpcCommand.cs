namespace MPCRemote.Models
{
    /// <summary>
    /// Class used to send or received commands from MPC
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
    }
}
