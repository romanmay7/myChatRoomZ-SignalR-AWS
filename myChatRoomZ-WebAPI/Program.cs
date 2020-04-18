using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using myChatRoomZ_WebAPI.Data;

namespace myChatRoomZ_WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();

            var host = BuildWebHost(args);

            SeedDb(host);

            host.Run();
        }

        private static void SeedDb(IWebHost host)
        {
            var scopeFactory = host.Services.GetService<IServiceScopeFactory>();

            using (var scope = scopeFactory.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetService<ChatRoomZSeeder>();
                seeder.SeedAsync().Wait();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>

             Microsoft.AspNetCore.WebHost.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(SetupConfiguration)
            .UseStartup<Startup>()
            .Build();


        private static void SetupConfiguration(WebHostBuilderContext ctx, IConfigurationBuilder builder)
        {
            //removing the default configuration options
            builder.Sources.Clear();

            builder.AddJsonFile("appsettings.json", false, true)
                //.AddXmlFile("web.config,true") //Legacy .NET way for config is XML file
                .AddEnvironmentVariables();

        }
    }
}
