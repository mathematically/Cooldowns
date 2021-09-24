using System.Windows;
using Cooldowns.Domain;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using Cooldowns.Factory;
using Cooldowns.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cooldowns
{
    public partial class App : Application
    {
        private readonly IHost host;
 
        public App()
        {
            host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(ConfigureApp)
                .ConfigureServices((context, services) => ConfigureServices(context.Configuration, services))
                .Build();
        }

        private static void ConfigureApp(HostBuilderContext _, IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", false, false);
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<Config>(configuration)
                    .AddSingleton<IKeyboardListener, Win32KeyboardListener>()
                    .AddSingleton<IKeyboard, KeyboardSimulator>()
                    .AddSingleton<IDispatcher, AppDispatcher>()
                    .AddSingleton<IScreen, Screen>()
                    .AddSingleton<ICooldownButtonFactory, CooldownButtonFactory>()
                    .AddSingleton<ISigilsOfHopeFactory, SigilsOfHopeFactory>()
                    .AddSingleton<Toolbar>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            host.StartAsync();
            host.Services.GetRequiredService<Toolbar>().Show();
 
            base.OnStartup(e);
        }
 
        protected override async void OnExit(ExitEventArgs e)
        {
            using (host)
            {
                await host.StopAsync();
            }
 
            base.OnExit(e);
        }
    }
}