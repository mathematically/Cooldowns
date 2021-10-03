using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Cooldowns.Domain;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Factory;
using Cooldowns.Domain.Keyboard;
using Cooldowns.Domain.Status;
using Cooldowns.Domain.Timer;
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

        private static readonly SolidColorBrush BlackBrush = new(Colors.Black);
        private static readonly SolidColorBrush TransparentBrush = new(Colors.Transparent);
        private static readonly SolidColorBrush GoldenrodBrush = new(Colors.DarkGoldenrod);

        private static void FreezeButtons()
        {
            BlackBrush.Freeze();
            TransparentBrush.Freeze();
            GoldenrodBrush.Freeze();
        }

        private readonly ICooldownTimer gameCheckTimer;
        private readonly IKeyboardListener keyboardListener;

        private readonly ToolbarViewModel viewModel = new();

        // These are nullable as they might not be disabled completely in config.
        private readonly CooldownButton? q, w, e, r;
        private readonly StatusChecker<SigilsOfHope>? sigilsOfHope;

        private readonly int buttonFontSize;
        private readonly double posX;
        private readonly double posY;

        public Toolbar(IOptions<Config> config, IKeyboardListener keyboardListener, ICooldownButtonFactory cooldownButtonFactory, ISigilsOfHopeFactory sigilsOfHopeFactory)
        {
            this.keyboardListener = keyboardListener;

            InitializeComponent();
            ConfigureLogging();
            FreezeButtons();

            DataContext = viewModel;

            buttonFontSize = config.Value.Toolbar.ButtonFontSize;
            posX = config.Value.Toolbar.PosX;
            posY = config.Value.Toolbar.PosY;

            gameCheckTimer = new CooldownTimer(config.Value.PollIntervalMilliseconds);

            q = config.Value.Toolbar.QButton ? Create(ButtonQ, config.Value.Q, cooldownButtonFactory) : null;
            w = config.Value.Toolbar.WButton ? Create(ButtonW, config.Value.W, cooldownButtonFactory) : null;
            e = config.Value.Toolbar.EButton ? Create(ButtonE, config.Value.E, cooldownButtonFactory) : null;
            r = config.Value.Toolbar.RButton ? Create(ButtonR, config.Value.R, cooldownButtonFactory) : null;

            if (config.Value.Toolbar.SigilsOfHope)
            {
                sigilsOfHope = sigilsOfHopeFactory.Create(gameCheckTimer, OnSigilsOfHopeStatusChanged);
                SigilsStatusIndicator.FontSize = config.Value.Toolbar.IndicatorFontSize;
                SigilsStatusIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                SigilsStatusIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private CooldownButton? Create(Button button, KeyConfig keyConfig, ICooldownButtonFactory cooldownButtonFactory)
        {
            button.Content = keyConfig.Label;
            button.FontSize = buttonFontSize;
            button.Visibility = Visibility.Visible;

            return cooldownButtonFactory.Create(keyConfig, gameCheckTimer, OnToolbarButtonStateChanged, OnToolbarButtonModeChanged);
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
            Left = SystemParameters.PrimaryScreenWidth * posX - Width * 0.5;
            Top = SystemParameters.FullPrimaryScreenHeight * posY - Height * 0.5;
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
                    SwitchAppOn();
                }
                else
                {
                    SwitchAppOff();
                }
            });
        }

        private void OnToolbarButtonStateChanged(ButtonStateEventArgs args)
        {
            Button button = GetButton(args.Name);

            switch (args.State)
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
                    throw new ArgumentOutOfRangeException(args.Name, args.State, null);
            }
        }

        private Button GetButton(string buttonName)
        {
            if (Equals(buttonName, ButtonQ.Content)) return ButtonQ;
            if (Equals(buttonName, ButtonW.Content)) return ButtonW;
            if (Equals(buttonName, ButtonE.Content)) return ButtonE;
            if (Equals(buttonName, ButtonR.Content)) return ButtonR;

            throw new ArgumentOutOfRangeException($"Could not find button with name {buttonName}");
        }

        private void OnToolbarButtonModeChanged(ButtonModeEventArgs args)
        {
            Button button = GetButton(args.Name);

            string message = $"Button {args.Name} mode is now {args.Mode.ToString().ToUpper()}";
            viewModel.StatusText = message;
            log.Debug(message);

            switch (args.Mode)
            {
                case CooldownButtonMode.Disabled:
                    // Button state changes visibility only, mode switches colours to keep the events completely separate.
                    // Hence we don't use Visibility here but set everything transparent.
                    button.BorderBrush = TransparentBrush;
                    button.Foreground = TransparentBrush;
                    button.Background = TransparentBrush;
                    break;
                case CooldownButtonMode.Manual:
                    button.BorderBrush = BlackBrush;
                    button.Foreground = BlackBrush;
                    button.Background = GoldenrodBrush;
                    break;
                case CooldownButtonMode.AutoCast:
                    button.BorderBrush = BlackBrush;
                    button.Foreground = GoldenrodBrush;
                    button.Background = TransparentBrush;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(args.Name, args.Mode, null);
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
                    SwitchAppOff();
                    Application.Current.Shutdown();
                    return;
                case VirtualKeyCode.PAUSE:
                    ToggleEnabled();
                    return;
                default:
                    return;
            }
        }

        private static bool CheckModeKey(VirtualKeyCode pressedKeyCode, CooldownButton? cooldownButton)
        {
            if (cooldownButton == null || pressedKeyCode != cooldownButton.ModeKeyCode) return false;
            cooldownButton.ChangeMode();
            return true;
        }

        private void OnSigilsOfHopeStatusChanged(SigilsOfHope state)
        {
            SigilsStatusIndicator.Content = state switch
            {
                SigilsOfHope.None => "-",
                SigilsOfHope.One => "1",
                SigilsOfHope.Two => "2",
                SigilsOfHope.Three => "3",
                SigilsOfHope.Four => "4",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private void ToggleEnabled()
        {
            if (gameCheckTimer.IsRunning())
            {
                log.Debug($"App manually switched OFF at {DateTime.UtcNow}");
                SwitchAppOff();
            }
            else
            {
                log.Debug($"App manually switched ON at {DateTime.UtcNow}");
                SwitchAppOn();
            }
        }

        private void SwitchAppOn()
        {
            if (gameCheckTimer.IsRunning()) return;
            gameCheckTimer.Start();
        }

        private void SwitchAppOff()
        {
            if (!gameCheckTimer.IsRunning()) return;
            gameCheckTimer.Stop();
        }

        private void OnClosed(object? sender, EventArgs args)
        {
            gameCheckTimer.Stop();
            gameCheckTimer.Dispose();

            Automation.RemoveAutomationFocusChangedEventHandler(OnFocusChanged);

            keyboardListener.UnHookKeyboard();
            keyboardListener.OnKeyPressed -= OnKeyPressed;

            q?.Dispose();
            w?.Dispose();
            e?.Dispose();
            r?.Dispose();

            sigilsOfHope?.Dispose();
        }
    }
}