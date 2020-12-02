using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AndroidServer.Domain.Listeners.Commands
{
    /// <summary>
    /// Abstract class that registers its methods as commands when <see cref="CommandAttribute"/> is applied to them. Does nothing if the channel does not have a <see cref="CommandListener"/>
    /// </summary>
    public abstract class CommandContainerListener : AndroidListener
    {
        public CommandContainerListener(AndroidInstance android, ulong channelID) : base(android, channelID) { }

        private void SetDirty()
        {
            if (Android == null) return;

            var cmd = Android.GetListener<CommandListener>();
            if (cmd != null)
                cmd.SetDirty();
        }

        public override void Initialise()
        {
            SetDirty();
        }

        public override void OnDelete()
        {
            SetDirty();
        }

        protected override void OnEnable()
        {
            SetDirty();
        }

        protected override void OnDisable()
        {
            SetDirty();
        }

        /// <summary>
        /// Gets all valid command methods from this type
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CommandListener.RegisteredCommand> RetrieveCommands()
        {
            var methods = GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(Task)) continue;

                var attributes = method.GetCustomAttributes(typeof(CommandAttribute), false);
                if (attributes == null || attributes.Length == 0) continue;
                var attribute = attributes[0] as CommandAttribute;

                attribute.Addressables = new(attribute.Addressables.OrderByDescending(s => s.Length).Select(s => s.ToLower()));

                Func<CommandParameters, Task> del;

                try
                {
                    del = method.CreateDelegate<Func<CommandParameters, Task>>(this);
                }
                catch (Exception)
                {
                    continue;
                }

                yield return new CommandListener.RegisteredCommand
                {
                    Attribute = attribute,
                    Delegate = del,
                    Listener = this
                };
            }
        }
    }
}
