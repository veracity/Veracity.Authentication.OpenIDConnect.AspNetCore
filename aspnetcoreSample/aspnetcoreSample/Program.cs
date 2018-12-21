using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Veracity.Authentication.OpenIDConnect.Core;

namespace test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureServices(s=>s.AddSingleton<IVeracityIntegrationConfigService, VeracityIntegrationConfigService>())
                .ConfigureServices(s=>s.AddSingleton<IVeracityOpenIdManager, VeracityOpenIdManager>())
                .UseStartup<Startup>()
                .Build();
    }
}
