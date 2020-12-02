namespace AndroidServer.Domain
{
    /// <summary>
    /// Variable type for sending to the client
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// ???
        /// </summary>
        Unknown,
        /// <summary>
        /// A float
        /// </summary>
        Number,
        /// <summary>
        /// Text
        /// </summary>
        String,
        /// <summary>
        /// Boolean
        /// </summary>
        Boolean,
        /// <summary>
        /// String with a text channel ID
        /// </summary>
        TextChannel,
        /// <summary>
        /// String with a voice channel ID
        /// </summary>
        VoiceChannel,
        /// <summary>
        /// String with a role ID
        /// </summary>
        RoleID,
        /// <summary>
        /// Text but it will be a big multiline editor
        /// </summary>
        TextArea,
        /// <summary>
        /// String with an emote ID
        /// </summary>
        EmoteID
    }
}
