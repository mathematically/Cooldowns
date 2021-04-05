using System;
using System.Windows;
using Cooldowns.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cooldowns
{
    public partial class App : Application
    {
        private readonly IHost host;
 
        // This is all completely pointless in the context of this app, but I have not written WPF
        // since .NET Core came about so I wanted to play with this stuff.
        
        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: false); 
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context.Configuration, services); 
                })
                .Build();
        }
 
        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<AppConfiguration>(configuration);
            services.AddSingleton<Toolbar>();
        }
 
        protected override async void OnStartup(StartupEventArgs e)
        {
            await host.StartAsync();
 
            var toolbar = host.Services.GetRequiredService<Toolbar>();
            toolbar.Show();
 
            base.OnStartup(e);
        }
 
        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync(TimeSpan.FromSeconds(5));
            }
 
            base.OnExit(e);
        }
    }
}