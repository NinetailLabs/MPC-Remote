namespace MPCRemote.Models
{
    /// <summary>
    /// Model used to store playlist information
    /// </summary>
    public class PlaylistEntry
    {
        /// <summary>
        /// The name of the file
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Indicate if this is the currently active playlist entry
        /// </summary>
        public bool IsActive { get; set; }
    }
}
