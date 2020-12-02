using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Utility struct that detects all listener types in the given assemblies
    /// </summary>
    public struct ListenerTypes
    {
        private static readonly Dictionary<string, Type> types = new();

        /// <summary>
        /// Registers an assembly to scan for listener types
        /// </summary>
        /// <param name="asm"></param>
        public static void RegisterAssembly(Assembly asm)
        {
            var listenerBase = typeof(AndroidListener);
            var allTypes = asm.GetTypes();

            foreach (var type in allTypes)
            {
                if (type.IsAbstract || !type.IsAssignableTo(listenerBase) || type.GetCustomAttribute<ObsoleteAttribute>() != null) continue;
                types.Add(IdentityGen.Generate(), type);
            }
        }
        
        /// <summary>
        /// Get all listener types
        /// </summary>
        public static IEnumerable<ListenerType> GetListenerTypes()
        {
            return types.Select(p => new ListenerType
            {
                TypeID = p.Key,
                TypeName = p.Value.Name
            });
        }

        /// <summary>
        /// Get a listener type structure from a type instance
        /// </summary>
        public static ListenerType GetListenerType(Type type)
        {
            var found = types.FirstOrDefault(t => t.Value == type);
            if (found.Value == null)
                return null;

            return new ListenerType
            {
                TypeID = found.Key,
                TypeName = found.Value.Name
            };
        }

        /// <summary>
        /// Get the raw <see cref="Type"/> instance from a listener type ID
        /// </summary>
        public static Type GetRawType(string id)
        {
            if (types.TryGetValue(id, out var result))
                return result;
            return null;
        }
    }
}
