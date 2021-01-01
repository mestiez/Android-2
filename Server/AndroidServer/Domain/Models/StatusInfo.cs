namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable Discord user game info
    /// </summary>
    public class StatusInfo
    {
        /// <summary>
        /// Status kind
        /// </summary>
        public string Kind { get; set; }
        /// <summary>
        /// Status text
        /// </summary>
        public string Status { get; set; }
    }
}
