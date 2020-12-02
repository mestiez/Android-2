using System;
using System.Collections.Generic;

namespace AndroidServer.Domain.Listeners.Commands
{
    /// <summary>
    /// Tells the command system that a method is a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// All ways to address the command. The method name is ignored
        /// </summary>
        public List<string> Addressables;
        /// <summary>
        /// Minimum required access level
        /// </summary>
        public int AccessLevel;

        public CommandAttribute(CommandAccessLevel accessLevel, params string[] addressables)
        {
            if (addressables == null || addressables.Length == 0)
                throw new Exception("Cannot create command without addressables");

            Addressables = new(addressables);
            AccessLevel = (int)accessLevel;
        }
        public CommandAttribute(CommandAccessLevel accessLevel, string addressable)
        {
            Addressables = new() { addressable };
            AccessLevel = (int)accessLevel;
        }
    }

    /// <summary>
    /// Access level of a user as configured by the client
    /// </summary>
    public enum CommandAccessLevel
    {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4
    }
}
