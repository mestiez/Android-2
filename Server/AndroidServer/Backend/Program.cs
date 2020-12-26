using AndroidServer.Domain;
using Backend.Controllers;
using RestApi;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public class Program
    {
        private static AndroidService android;
        private static RestServer server;

        private const string ListeningAddressPath = "listening_address";
        private const string ClientAddressPath = "client_address";
        private const int SaveServiceTimer = 30;

        static void Main(string[] args)
        {
            if (!File.Exists(ListeningAddressPath))
                throw new Exception("Could not find listening address file at " + ListeningAddressPath);

            if (!File.Exists(ClientAddressPath))
                throw new Exception("Could not find client address file at " + ClientAddressPath);

            var listeningAddress = File.ReadAllText(ListeningAddressPath);
            var clientAddress = File.ReadAllText(ClientAddressPath);

            ListenerTypes.RegisterAssembly(Assembly.GetAssembly(typeof(AndroidListener)));

            android = new AndroidService();
            server = new RestServer(listeningAddress);
            server.Start();

            _ = Task.Run(android.StartDiscordLoop);

            server.AddController(new AndroidInstanceController());
            server.AddController(new GuildController());
            server.AddController(new ListenerController());
            server.AddController(new SystemController());

            server.AddFilter(new TokenFilter());

            server.AllowedOrigins.Clear();
            server.AllowedOrigins.Add(clientAddress);

            System.Timers.Timer saveTimer = new System.Timers.Timer(TimeSpan.FromMinutes(SaveServiceTimer).TotalMilliseconds);
            saveTimer.Elapsed += SaveAll;
            saveTimer.Start();

            static void SaveAll(object sender, System.Timers.ElapsedEventArgs e)
            {
                AndroidStateSerialiser.SaveAll(android);
            }
        }
    }
}
