using Newtonsoft.Json;
using System;
using System.IO;

namespace AndroidServer.Domain
{
    /// <summary>
    /// Utility class that can save and load an <see cref="AndroidState"/>
    /// </summary>
    public static class AndroidStateSerialiser
    {
        /// <summary>
        /// Base path for the used file
        /// </summary>
        public const string BasePath = ".\\state\\";

        private const string Extension = ".json";

        private static readonly JsonSerializerSettings serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Save an instance to the state files
        /// </summary>
        /// <param name="instance"></param>
        public static void Save(AndroidInstance instance)
        {
            if (!Directory.Exists(BasePath))
                Directory.CreateDirectory(BasePath);

            var path = GetPath(instance.GuildID);

            AndroidState state = new AndroidState
            {
                Listeners = instance.Listeners,
                MutedUsers = instance.Moderation.MutesByUser,
                Active = instance.Active
            };

            var json = JsonConvert.SerializeObject(state, serializerSettings);
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Load an instance by guild ID from the state files
        /// </summary>
        /// <param name="ID">guild ID</param>
        /// <param name="result">resulting state</param>
        /// <returns>Was the operation successful?</returns>
        public static bool Load(ulong ID, out AndroidState result)
        {
            result = default;
            var fullpath = GetPath(ID);
            if (!File.Exists(fullpath))
                return false;

            var json = File.ReadAllText(fullpath);
            result = JsonConvert.DeserializeObject<AndroidState>(json, serializerSettings);

            return true;
        }

        /// <summary>
        /// Save all instances in the given service to the state files
        /// </summary>
        public static void SaveAll(AndroidService service)
        {
            Console.WriteLine("Saving Android service...");

            //I want exceptions as a concept to die
            try
            {
                foreach (var instance in service.AndroidInstances)
                    Save(instance.Value);

                Console.WriteLine("Saved succesfully");
            }
            catch (Exception e)
            {
                Console.WriteLine("Saving failure: {0}", e.Message);
            }
        }

        private static string GetPath(ulong id)
        {
            return Path.Combine(BasePath, id + Extension);
        }
    }
}
