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
 
        // This is somewhat pointless in the context of this app, but I have not written WPF
        // since .NET Core came about so I wanted to play with this stuff.
        
        // The settings/config is the most useful, the DI stuff a bit overkill.
        // Not using the logging (no file logs?) just a normal simple NLog to console and file.
        
        public App()
        {
            host = Host.CreateDefaultBuilder()
                       .ConfigureAppConfiguration((_, builder) => builder.AddJsonFile("appsettings.json", optional: false))
                       .ConfigureServices((context, services) => ConfigureServices(context.Configuration, services))
                       .Build();
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<CooldownsApp>(configuration)
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