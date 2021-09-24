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
using Cooldowns.Domain.Status;
using Cooldowns.Domain.Timer;
using Cooldowns.Factory;
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

        private readonly double posX;
        private readonly double posY;

        public Toolbar(IOptions<Config> config, IKeyboardListener keyboardListener, ICooldownButtonFactory cooldownButtonFactory, ISigilsOfHopeFactory sigilsOfHopFactory)
        {
            this.keyboardListener = keyboardListener;

            InitializeComponent();
            ConfigureLogging();
            FreezeButtons();

            DataContext = viewModel;

            posX = config.Value.Toolbar.PosX;
            posY = config.Value.Toolbar.PosY;

            gameCheckTimer = new CooldownTimer(config.Value.PollIntervalMilliseconds);

            if (config.Value.Toolbar.QButton)
            {
                q = cooldownButtonFactory.Create(ButtonQ, config.Value.Q, gameCheckTimer, OnToolbarButtonStateChanged, OnToolbarButtonModeChanged);
                ButtonQ.Content = config.Value.Q.Label;
                ButtonQ.FontSize = config.Value.Toolbar.ButtonFontSize;
            }
            else
            {
                ButtonQ.Visibility = Visibility.Hidden;
            }

            if (config.Value.Toolbar.WButton)
            {
                w = cooldownButtonFactory.Create(ButtonW, config.Value.W, gameCheckTimer, OnToolbarButtonStateChanged, OnToolbarButtonModeChanged);
                ButtonW.Content = config.Value.W.Label;
                ButtonW.FontSize = config.Value.Toolbar.ButtonFontSize;
            }
            else
            {
                ButtonW.Visibility = Visibility.Hidden;
            }
            
            if (config.Value.Toolbar.EButton)
            {
                e = cooldownButtonFactory.Create(ButtonE, config.Value.E, gameCheckTimer, OnToolbarButtonStateChanged, OnToolbarButtonModeChanged);
                ButtonE.Content = config.Value.E.Label;
                ButtonE.FontSize = config.Value.Toolbar.ButtonFontSize;
            }
            else
            {
                ButtonE.Visibility = Visibility.Hidden;
            }
            
            if (config.Value.Toolbar.RButton)
            {
                r = cooldownButtonFactory.Create(ButtonR, config.Value.R, gameCheckTimer, OnToolbarButtonStateChanged, OnToolbarButtonModeChanged);
                ButtonR.Content = config.Value.R.Label;
                ButtonR.FontSize = config.Value.Toolbar.ButtonFontSize;
            }
            else
            {
                ButtonR.Visibility = Visibility.Hidden;
            }
            
            if (config.Value.Toolbar.SigilsOfHope)
            {
                sigilsOfHope = sigilsOfHopFactory.Create(gameCheckTimer, OnSigilsOfHopeStatusChanged);
                SigilsStatusIndicator.FontSize = config.Value.Toolbar.IndicatorFontSize;
            }
            else
            {
                SigilsStatusIndicator.Visibility = Visibility.Collapsed;
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