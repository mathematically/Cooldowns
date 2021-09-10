using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
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

        private readonly SolidColorBrush goldenrodBrush = new(Colors.DarkGoldenrod);
        private readonly SolidColorBrush blackBrush = new(Colors.Black);

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
        private readonly ICooldownTimer cooldownTimer;

        private readonly CooldownButton q, w, e, r;
        private readonly ToolbarViewModel viewModel = new();

        public Toolbar(IOptions<CooldownsApp> configuration, IDispatcher dispatcher, IScreen screen, IKeyboardListener keyboardListener, IKeyboard keyboard)
        {
            this.keyboardListener = keyboardListener;
            this.keyboard = keyboard;

            InitializeComponent();
            ConfigureLogging();

            DataContext = viewModel;
            cooldownTimer = new CooldownTimer();

            q = ButtonFactory(ButtonQ, configuration, dispatcher, screen, cooldownTimer, configuration.Value.Q);
            w = ButtonFactory(ButtonW, configuration, dispatcher, screen, cooldownTimer, configuration.Value.W);
            e = ButtonFactory(ButtonE, configuration, dispatcher, screen, cooldownTimer, configuration.Value.E);
            r = ButtonFactory(ButtonR, configuration, dispatcher, screen, cooldownTimer, configuration.Value.R);

            PosX = configuration.Value.Toolbar.PosX;
            PosY = configuration.Value.Toolbar.PosY;
        }

        private CooldownButton ButtonFactory(Button button, IOptions<CooldownsApp> configuration,
            IDispatcher dispatcher, IScreen screen, ICooldownTimer cooldownTimer, Key key)
        {
            var cooldownButton = new CooldownButton(screen, keyboard, dispatcher, cooldownTimer, key);
            cooldownButton.ButtonStateChanged += (_, buttonState) => OnToolbarButtonStateChanged(button, buttonState);
            cooldownButton.ButtonModeChanged += (_, buttonMode) => OnToolbarButtonModeChanged(button, buttonMode);

            button.Content = key.Label;
            button.FontSize = configuration.Value.Toolbar.FontSize;

            return cooldownButton;
        }

        private void OnToolbarButtonStateChanged(Button button, CooldownButtonState buttonState)
        {
            switch (buttonState)
            {
                case CooldownButtonState.Cooldown:
                    log.Debug($"Button {button.Content} is on COOLDOWN.");
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonState.Active:
                    log.Debug($"Button {button.Content} skill is now ACTIVE.");
                    button.Visibility = Visibility.Visible;
                    button.Opacity = 0.30;
                    break;
                case CooldownButtonState.Ready:
                    log.Debug($"Button {button.Content} is now READY.");
                    button.Visibility = Visibility.Visible;
                    button.Opacity = 1.0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
            }
        }

        private void OnToolbarButtonModeChanged(Button button, CooldownButtonMode buttonMode)
        {
            string message = $"Button {button.Content} {buttonMode.ToString().ToUpper()}";

            viewModel.StatusText = message;
            log.Debug(message);

            switch (buttonMode)
            {
                case CooldownButtonMode.Disabled:
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonMode.Manual:
                    button.Foreground = blackBrush;
                    button.Background = goldenrodBrush;
                    button.Visibility = Visibility.Visible;
                    break;
                case CooldownButtonMode.AutoCast:
                    button.Foreground = goldenrodBrush;
                    button.Background = blackBrush;
                    button.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonMode), buttonMode, null);
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

            // ignore ourselves and the compiler.
            if (processName.Contains("Cooldowns")) return;
            if (processName.Contains("Visual Studio")) return;
            if (processName.Contains("Rider")) return;
            
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

        private void OnKeyPressed(object? sender, KeyPressArgs e)
        {
            switch (e.KeyCode)
            {
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
                // todo are these configurable or not?
                case VirtualKeyCode.F5:
                    q.ChangeMode();
                    break;

                case VirtualKeyCode.F6:
                    w.ChangeMode();
                    break;

                case VirtualKeyCode.F7:
                    e.ChangeMode();
                    break;

                case VirtualKeyCode.F8:
                    r.ChangeMode();
                    break;
                
                default:
                    return;
            }
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

            //cooldownTimer.Stop();
            cooldownTimer.Dispose();
        }
    }
}