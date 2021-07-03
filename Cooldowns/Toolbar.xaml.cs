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
        private enum AppState
        {
            Off, On
        }

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

        private AppState state = AppState.On;

        private bool IsAppOff() => state == AppState.Off;
        private bool IsAppOn() => state == AppState.On;

        private double PosX { get; }
        private double PosY { get; }

        private readonly IKeyboardListener keyboardListener;
        private readonly IKeyboard keyboard;
        private readonly CooldownButton q, w, e, r;

        public Toolbar(IOptions<CooldownsApp> configuration, IDispatcher dispatcher, IScreen screen, IKeyboardListener keyboardListener, IKeyboard keyboard)
        {
            this.keyboardListener = keyboardListener;
            this.keyboard = keyboard;

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
            var cooldownButton = new CooldownButton(screen, keyboard, new CooldownTimer(dispatcher), key);
            cooldownButton.ButtonStateChanged += (_, buttonState) => OnToolbarButtonStateChanged(button, buttonState);

            button.Content = key.Label;
            button.FontSize = configuration.Value.Toolbar.FontSize;

            return cooldownButton;
        }

        private void OnToolbarButtonStateChanged(Button button, CooldownButtonState buttonState)
        {
            switch (buttonState)
            {
                case CooldownButtonState.Disabled:
                    log.Debug($"Button {button.Content} disabled");
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonState.Cooldown:
                case CooldownButtonState.AutoCasting:
                    log.Debug($"Button {button.Content} on cooldown.");
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonState.Ready:
                    log.Debug($"Button {button.Content} ready.");
                    button.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
            }
        }

        private void ResetWindowPosition()
        {
            Left = SystemParameters.PrimaryScreenWidth * PosX - Width * 0.5;
            Top = SystemParameters.FullPrimaryScreenHeight * PosY - Height * 0.5;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ResetWindowPosition();
            
            keyboardListener.OnKeyPressed += OnKeyPressed;
            keyboardListener.HookKeyboard();
            
            Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);
        }

        private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            var focusedElement = sender as AutomationElement;
            if (focusedElement == null) return;
            
            using var process = Process.GetProcessById(focusedElement.Current.ProcessId);
            var processName = process.ProcessName;
            
            log.Debug($"Focus changed {processName}");

            // Disabled auto switch off when in IDE todo ?
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
            switch (e.KeyCode)
            {
                // todo remove pause? It's not actually useful. Quick way to stop chat spam maybe?
                case VirtualKeyCode.PAUSE:
                    ToggleEnabled();
                    return;
                case VirtualKeyCode.SCROLL:
                    Application.Current.Shutdown();
                    return;
            }

            if (IsAppOff()) return;
            
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
                
                default:
                    return;
            }
        }

        private void ToggleEnabled()
        {
            if (IsAppOff())
            {
                SetAppOn();
            }
            else if (IsAppOn())
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

        private void OnClosed(object? sender, EventArgs args)
        {
            q.Dispose();
            w.Dispose();
            e.Dispose();
            r.Dispose();
            
            Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChanged);
            
            keyboardListener.UnHookKeyboard();
            keyboardListener.OnKeyPressed -= OnKeyPressed;
        }
    }
}