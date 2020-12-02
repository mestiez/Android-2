using AndroidServer.Domain;
using AndroidServer.Domain.Listeners;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace AndroidServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ListenerTypes.RegisterAssembly(Assembly.GetAssembly(typeof(AndroidListener)));
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
