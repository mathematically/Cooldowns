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

        private readonly ICooldownTimer timer;
        private readonly IKeyboardListener keyboardListener;

        private readonly ToolbarViewModel viewModel = new();

        // todo add IButtonModel interface to tidy this up.
        // These are nullable as they might not be disabled completely in config.
        private readonly CooldownButton? q, w, e, r;
        private string? sigilsOfHopeButtonName; // todo push down
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

            posX = config.Value.Toolbar.PosX;
            posY = config.Value.Toolbar.PosY;
            buttonFontSize = config.Value.Toolbar.FontSize;

            timer = new CooldownTimer(config.Value.IntervalMilliseconds);

            (q, sigilsOfHope) = CreateButton(ButtonQ, config.Value.Q, cooldownButtonFactory, sigilsOfHopeFactory);
            (w, sigilsOfHope) = CreateButton(ButtonW, config.Value.W, cooldownButtonFactory, sigilsOfHopeFactory);
            (e, sigilsOfHope) = CreateButton(ButtonE, config.Value.E, cooldownButtonFactory, sigilsOfHopeFactory);
            (r, sigilsOfHope) = CreateButton(ButtonR, config.Value.R, cooldownButtonFactory, sigilsOfHopeFactory);
        }

        private (CooldownButton? cooldownButton, StatusChecker<SigilsOfHope>? statusChecker) CreateButton(Button button, KeyConfig config, ICooldownButtonFactory cooldownButtonFactory, ISigilsOfHopeFactory sigilsOfHopeFactory)
        {
            SetAppearance(button, config);
            return CreateButtonModel(config, cooldownButtonFactory, sigilsOfHopeFactory);
        }

        private void SetAppearance(Button button, KeyConfig config)
        {
            button.Content = config.Label;
            button.FontSize = buttonFontSize;
            button.Visibility = Visibility.Visible;

            switch (config.Mode)
            {
                case ButtonMode.Disabled:
                    button.Visibility = Visibility.Hidden;
                    break;
                case ButtonMode.Manual:
                    button.BorderBrush = BlackBrush;
                    button.Foreground = BlackBrush;
                    button.Background = GoldenrodBrush;
                    break;
                case ButtonMode.AutoCast:
                    button.BorderBrush = BlackBrush;
                    button.Foreground = GoldenrodBrush;
                    button.Background = TransparentBrush;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private (CooldownButton? cooldownButton, StatusChecker<SigilsOfHope>? statusChecker) CreateButtonModel(KeyConfig config,
            ICooldownButtonFactory cooldownButtonFactory, ISigilsOfHopeFactory sigilsOfHopeFactory)
        {
            switch (config.Type)
            {
                case ButtonType.Cooldown:
                    return (cooldownButtonFactory.Create(config, timer, OnToolbarButtonStateChanged), null);
                case ButtonType.SigilsOfHope:
                    sigilsOfHopeButtonName = config.Label;
                    return (null, sigilsOfHopeFactory.Create(timer, OnSigilsOfHopeStatusChanged));
                default:
                    throw new ArgumentOutOfRangeException();
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
            return buttonName switch
            {
                "Q" => ButtonQ,
                "W" => ButtonW,
                "E" => ButtonE,
                "R" => ButtonR,
                _ => throw new ArgumentOutOfRangeException($"Could not find button with name {buttonName}")
            };
        }

        private void OnKeyPressed(object? _, KeyPressArgs args)
        {
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

        private void OnSigilsOfHopeStatusChanged(SigilsOfHope state)
        {
            var button = GetButton(sigilsOfHopeButtonName ?? "");

            button.Content = state switch
            {
                SigilsOfHope.None => sigilsOfHopeButtonName,
                SigilsOfHope.One => "1",
                SigilsOfHope.Two => "2",
                SigilsOfHope.Three => "3",
                SigilsOfHope.Four => "4",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        private void ToggleEnabled()
        {
            if (timer.IsRunning())
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
            if (timer.IsRunning()) return;
            timer.Start();
        }

        private void SwitchAppOff()
        {
            if (!timer.IsRunning()) return;
            timer.Stop();
        }

        private void OnClosed(object? sender, EventArgs args)
        {
            timer.Stop();
            timer.Dispose();

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