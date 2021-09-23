using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Keyboard;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Config;
using NLog.Targets;
using WindowsInput.Native;

namespace Cooldowns
{
    public partial class Toolbar : Window
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private static void ConfigureLogging()
        {
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget("logfile") { FileName = "logs.txt", DeleteOldFileOnStartup = true };
            var consoleTarget = new ConsoleTarget("logconsole");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

            LogManager.Configuration = config;
        }

        private void FreezeButtons()
        {
            goldenrodBrush.Freeze();
            blackBrush.Freeze();
            transparentBrush.Freeze();
        }

        private readonly SolidColorBrush blackBrush = new(Colors.Black);
        private readonly SolidColorBrush goldenrodBrush = new(Colors.DarkGoldenrod);
        private readonly SolidColorBrush transparentBrush = new(Colors.Transparent);

        private readonly ICooldownTimer cooldownTimer;
        private readonly IKeyboard keyboard;
        private readonly IKeyboardListener keyboardListener;

        private readonly CooldownButton q, w, e, r;
        private readonly ToolbarViewModel viewModel = new();

        private readonly double PosX;
        private readonly double PosY;

        private AppState state = AppState.On;

        public Toolbar(IOptions<Config> configuration, IDispatcher dispatcher, IScreen screen,
            IKeyboardListener keyboardListener, IKeyboard keyboard)
        {
            this.keyboardListener = keyboardListener;
            this.keyboard = keyboard;

            InitializeComponent();
            ConfigureLogging();
            FreezeButtons();

            DataContext = viewModel;
            cooldownTimer = new CooldownTimer();

            q = ButtonFactory(ButtonQ, configuration.Value.Toolbar, dispatcher, screen, cooldownTimer, configuration.Value.Q);
            w = ButtonFactory(ButtonW, configuration.Value.Toolbar, dispatcher, screen, cooldownTimer, configuration.Value.W);
            e = ButtonFactory(ButtonE, configuration.Value.Toolbar, dispatcher, screen, cooldownTimer, configuration.Value.E);
            r = ButtonFactory(ButtonR, configuration.Value.Toolbar, dispatcher, screen, cooldownTimer, configuration.Value.R);

            PosX = configuration.Value.Toolbar.PosX;
            PosY = configuration.Value.Toolbar.PosY;
        }

        // todo push some of this down into view model / models?

        private CooldownButton ButtonFactory(Button button, ToolbarConfig toolbar,
            IDispatcher dispatcher, IScreen screen, ICooldownTimer cooldownTimer, KeyConfig key)
        {
            var cooldownButton = new CooldownButton(screen, keyboard, dispatcher, cooldownTimer, key);

            cooldownButton.ButtonStateChanged += (_, buttonState) => OnToolbarButtonStateChanged(button, buttonState);
            cooldownButton.ButtonModeChanged += (_, buttonMode) => OnToolbarButtonModeChanged(button, buttonMode);

            button.Content = key.Label;
            button.FontSize = toolbar.FontSize;

            return cooldownButton;
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

            using Process process = Process.GetProcessById(focusedElement.Current.ProcessId);
            var processName = process.ProcessName;

            log.Debug($"Focus changed {processName}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (processName.Contains("Epoch") || processName.Contains("Cooldowns"))
                {
                    SetAppOn();
                }
                else
                {
                    SetAppOff();
                }
            });
        }

        private void OnToolbarButtonStateChanged(Button button, CooldownButtonState buttonState)
        {
            switch (buttonState)
            {
                case CooldownButtonState.Cooldown:
                    log.Debug($"Toolbar Button {button.Content} is on COOLDOWN.");
                    button.Visibility = Visibility.Hidden;
                    break;
                case CooldownButtonState.Active:
                    log.Debug($"Toolbar Button {button.Content} skill is now ACTIVE.");
                    button.Visibility = Visibility.Visible;
                    button.Opacity = 0.30;
                    break;
                case CooldownButtonState.Ready:
                    log.Debug($"Toolbar Button {button.Content} is now READY.");
                    button.Visibility = Visibility.Visible;
                    button.Opacity = 1.0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonState), buttonState, null);
            }
        }

        private void OnToolbarButtonModeChanged(Button button, CooldownButtonMode buttonMode)
        {
            string message = $"Button {button.Content} mode is now {buttonMode.ToString().ToUpper()}";

            viewModel.StatusText = message;
            log.Debug(message);

            switch (buttonMode)
            {
                case CooldownButtonMode.Disabled:
                    // Button state changes visibility only, mode switches colours to keep the events completely separate.
                    // Hence we don't use Visibility here but set everything transparent.
                    button.BorderBrush = transparentBrush;
                    button.Foreground = transparentBrush;
                    button.Background = transparentBrush;
                    break;
                case CooldownButtonMode.Manual:
                    button.BorderBrush = blackBrush;
                    button.Foreground = blackBrush;
                    button.Background = goldenrodBrush;
                    break;
                case CooldownButtonMode.AutoCast:
                    button.BorderBrush = blackBrush;
                    button.Foreground = goldenrodBrush;
                    button.Background = transparentBrush;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonMode), buttonMode, null);
            }
        }

        private void OnKeyPressed(object? _, KeyPressArgs args)
        {
            if (CheckModeKey(args.KeyCode, q)) return;
            if (CheckModeKey(args.KeyCode, w)) return;
            if (CheckModeKey(args.KeyCode, e)) return;
            if (CheckModeKey(args.KeyCode, r)) return;

            switch (args.KeyCode)
            {
                case VirtualKeyCode.SCROLL:
                    Application.Current.Shutdown();
                    return;
                case VirtualKeyCode.PAUSE:
                    ToggleEnabled();
                    return;
                default:
                    return;
            }
        }

        private static bool CheckModeKey(VirtualKeyCode pressedKeyCode, CooldownButton cooldownButton)
        {
            if (pressedKeyCode != cooldownButton.ModeKeyCode) return false;
            cooldownButton.ChangeMode();
            return true;
        }

        private void ToggleEnabled()
        {
            switch (state)
            {
                case AppState.Off:
                    log.Debug($"App manually switched OFF at {DateTime.UtcNow}");
                    SetAppOn();
                    break;
                case AppState.On:
                    log.Debug($"App manually switched ON at {DateTime.UtcNow}");
                    SetAppOff();
                    break;
            }
        }

        private void SetAppOn()
        {
            state = AppState.On;
            cooldownTimer.Start();
        }

        private void SetAppOff()
        {
            state = AppState.Off;
            cooldownTimer.Stop();
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

            cooldownTimer.Stop();
            cooldownTimer.Dispose();
        }

        private enum AppState
        {
            Off,
            On
        }
    }
}