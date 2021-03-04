namespace MPCRemote.Models
{
    public class MpcReceiveCommand
    {
        /// <summary>
        /// The command to execute
        /// </summary>
        public string Command { get; set; }

        /// <summary>
        /// The parameters for the command
        /// </summary>
        public StatusParameter Parameters { get; set; }
    }
}
