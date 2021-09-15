using System;
using System.Windows;
using Cooldowns.Domain;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using Cooldowns.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cooldowns
{
    public partial class App : Application
    {
        private readonly IHost host;
 
        // This a bit OTT for a Windows app but wanted to do some latest .NET Core
        
        public App()
        {
            host = Host.CreateDefaultBuilder()
                       .ConfigureAppConfiguration((_, builder) => builder.AddJsonFile("appsettings.json", optional: false))
                       .ConfigureServices((context, services) => ConfigureServices(context.Configuration, services))
                       .Build();
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<Config>(configuration)
                    .AddSingleton<IKeyboardListener, Win32KeyboardListener>()
                    .AddSingleton<IKeyboard, KeyboardSimulator>()
                    .AddSingleton<IDispatcher, AppDispatcher>()
                    .AddSingleton<IScreen, Screen>()
                    .AddSingleton<Toolbar>();
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