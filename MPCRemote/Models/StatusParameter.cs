namespace MPCRemote.Models
{
    /// <summary>
    /// Parameters returned in a Status command
    /// </summary>
    public class StatusParameter
    {
        /// <summary>
        /// The name of the file currently being played
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// The current playback position
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// The total duration of the file
        /// </summary>
        public string Duration { get; set; }

        /// <summary>
        /// The current playback state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Indicate if the connection to the player has been opened
        /// </summary>
        public bool Connected { get; set; }
    }
}
