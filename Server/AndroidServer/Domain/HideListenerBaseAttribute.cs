using System;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Attribute that can be applied to a listener to hide the base properties from <see cref="AndroidListener"/> from the client. Useful to force a listener to be global or non-global
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HideListenerBaseAttribute : Attribute
    {
    }
}
