using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Serialisable information about a message sent to the server via the client
    /// </summary>
    [Serializable]
    public class SayInfo
    {
        /// <summary>
        /// Content
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Optional filename
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Optional file data encoded as a base64 string
        /// </summary>
        public string FileB64 { get; set; }
    }
}
