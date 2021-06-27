using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using WindowsInput.Native;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Config;
using NLog.Targets;
namespace Cooldowns
{
    public partial class Toolbar : Window
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();
            
            var fileTarget = new FileTarget("logfile") {FileName = "logs.txt", DeleteOldFileOnStartup = true};
            var consoleTarget = new ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        private enum AppState
        {
            Off, On
        }

        private AppState state = AppState.On;

        private bool IsOff() => state == AppState.Off;
        private bool IsOn() => state == AppState.On;

        private double PosX { get; }
        private double PosY { get; }

        private readonly IKeyboardListener keyboardListener;
        private readonly CooldownButton q, w, e, r;

        public Toolbar(IOptions<CooldownsApp> configuration, IDispatcher dispatcher, IScreen screen, IKeyboardListener keyboardListener)
        {
            this.keyboardListener = keyboardListener;

            InitializeComponent();
            ConfigureLogging();
            
            q = ButtonFactory(ButtonQ, configuration, dispatcher, screen, configuration.Value.Q);
            w = ButtonFactory(ButtonW, configuration, dispatcher, screen, configuration.Value.W);
            e = ButtonFactory(ButtonE, configuration, dispatcher, screen, configuration.Value.E);
            r = ButtonFactory(ButtonR, configuration, dispatcher, screen, configuration.Value.R);

            PosX = configuration.Value.Toolbar.PosX;
            PosY = configuration.Value.Toolbar.PosY;
        }

        private CooldownButton ButtonFactory(Button button, IOptions<CooldownsApp> configuration, IDispatcher dispatcher, IScreen screen, Key key)
        {
            var cooldownButton = new CooldownButton(screen, dispatcher, key);

            button.Content = key.Label;
            button.FontSize = configuration.Value.Toolbar.FontSize;

            cooldownButton.ButtonStateChanged += (_, buttonState) => Update(button, buttonState);

            return cooldownButton;
        }

        private void Update(Button button, CooldownButtonState buttonState)
        {
            switch (buttonState)
            {
                case CooldownButtonState.Disabled:
                    log.Debug($"Button {button.Content} disabled");
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonState.OnCooldown:
                case CooldownButtonState.AutoCasting:
                    log.Debug($"Button {button.Content} on cooldown.");
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonState.Up:
                    log.Debug($"Button {button.Content} ready.");
                    button.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ResetWindowPosition();
            
            keyboardListener.OnKeyPressed += OnKeyPressed;
            keyboardListener.HookKeyboard();
            
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        private void ResetWindowPosition()
        {
            Left = SystemParameters.PrimaryScreenWidth * PosX - Width * 0.5;
            Top = SystemParameters.FullPrimaryScreenHeight * PosY - Height * 0.5;
        }

        private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            var focusedElement = sender as AutomationElement;
            if (focusedElement == null) return;
            
            using var process = Process.GetProcessById(focusedElement.Current.ProcessId);
            var processName = process.ProcessName;
            
            log.Debug($"Focus changed {processName}");
            if (processName.Contains("Cooldowns")) return;
            
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (processName.Contains("Epoch"))
                {
                    SetAppOn();
                }
                else
                {
                    SetAppOff();
                }
            });
        }

        private void OnKeyPressed(object? sender, KeyPressArgs e)
        {
            // todo remove pause? It's not actually useful.

            if (e.KeyCode == VirtualKeyCode.PAUSE)
            {
                ToggleEnabled();
                return;
            }
            
            if (IsOff()) return;
            
            ProcessKeys(e);
        }

        private void ProcessKeys(KeyPressArgs args)
        {
            switch (args.KeyCode)
            {
                case VirtualKeyCode.VK_Q:
                    q.Press();
                    break;
                
                case VirtualKeyCode.VK_W:
                    w.Press();
                    break;
                
                case VirtualKeyCode.VK_E:
                    this.e.Press();
                    break;
                
                case VirtualKeyCode.VK_R:
                    r.Press();
                    break;
                
                // todo move this up surely?
                case VirtualKeyCode.SCROLL:
                    Application.Current.Shutdown();
                    break;

                default:
                    return;
            }
        }

        private void ToggleEnabled()
        {
            if (IsOff())
            {
                SetAppOn();
            }
            else if (IsOn())
            {
                SetAppOff();
            }
        }

        private void SetAppOn()
        {
            state = AppState.On;
            Visibility = Visibility.Visible;
        }
        
        private void SetAppOff()
        {
            state = AppState.Off;
            Visibility = Visibility.Collapsed;
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            q.UnloadTimer();
            w.UnloadTimer();
            this.e.UnloadTimer();
            r.UnloadTimer();
            
            Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChanged);
            
            keyboardListener.UnHookKeyboard();
            keyboardListener.OnKeyPressed -= OnKeyPressed;
        }
    }
}