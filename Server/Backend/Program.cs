using AndroidServer.Domain;
using Backend.Controllers;
using RestApi;
using System.Reflection;
using System.Threading.Tasks;

namespace Backend
{
    public class Program
    {
        private static AndroidService android;
        private static RestServer server;

        static void Main(string[] args)
        {
            ListenerTypes.RegisterAssembly(Assembly.GetAssembly(typeof(AndroidListener)));

            android = new AndroidService();
            server = new RestServer("http://localhost:3042/");
            server.Start();

            _ = Task.Run(android.StartDiscordLoop);

            server.AddController(new AndroidInstanceController());
            server.AddController(new GuildController());
            server.AddController(new ListenerController());
            server.AddController(new SystemController());

            server.AllowedOrigins.Clear();
            server.AllowedOrigins.Add("http://localhost:4200/");
        }
    }
}
