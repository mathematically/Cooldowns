using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using WindowsInput.Native;
using Cooldowns.Domain;
using Cooldowns.Keyboard;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Cooldowns
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private readonly Win32KeyboardListener keyboardListener;

        // ReSharper disable InconsistentNaming
        private readonly ButtonCooldownTimer Q;
        private readonly ButtonCooldownTimer W;
        private readonly ButtonCooldownTimer E;
        private readonly ButtonCooldownTimer R;
        // ReSharper enable InconsistentNaming

        private enum AppState
        {
            Off, On
        }

        private AppState state = AppState.On;

        private bool IsOff() => state == AppState.Off;
        private bool IsOn() => state == AppState.On;

        public Toolbar(IOptions<CooldownsConfiguration> configuration)
        {
            InitializeComponent();
            ConfigureLogging();
            
            keyboardListener = new Win32KeyboardListener();

            Q = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonQ, configuration.Value.Q);
            W = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonW, configuration.Value.W);
            E = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonE, configuration.Value.E);
            R = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonR, configuration.Value.R);

            ButtonQ.FontSize = configuration.Value.FontSize;
            ButtonW.FontSize = configuration.Value.FontSize;
            ButtonE.FontSize = configuration.Value.FontSize;
            ButtonR.FontSize = configuration.Value.FontSize;
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
            Left = SystemParameters.PrimaryScreenWidth * 0.5 - Width * 0.5;
            Top = SystemParameters.FullPrimaryScreenHeight * 0.28 - Height * 0.5;
        }

        private void OnFocusChanged(object sender, AutomationFocusChangedEventArgs e)
        {
            var focusedElement = sender as AutomationElement;
            if (focusedElement == null) return;
            
            using var process = Process.GetProcessById(focusedElement.Current.ProcessId);
            var processName = process.ProcessName;
            
            log.Debug("Focus changed " + processName);
            if (processName.Contains("Cooldowns") || processName.Contains("dotnet")) return;
            
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (processName.Contains("Epoch"))
                {
                    SetOn();
                }
                else
                {
                    SetOff();
                }
            });
        }

        private void OnKeyPressed(object? sender, KeyPressArgs e)
        {
            if (e.KeyCode == VirtualKeyCode.PAUSE)
            {
                ToggleEnabled();
                return;
            }
            
            if (IsOff()) return;
            
            ProcessKeys(e);
        }

        private void ToggleEnabled()
        {
            if (IsOff())
            {
                SetOn();
            }
            else if (IsOn())
            {
                SetOff();
            }
        }

        private void SetOn()
        {
            state = AppState.On;
            Visibility = Visibility.Visible;
        }
        
        private void SetOff()
        {
            state = AppState.Off;
            Visibility = Visibility.Collapsed;
        }


        private void ProcessKeys(KeyPressArgs e)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.KeyCode)
            {
                case VirtualKeyCode.VK_Q:
                    Q.Start();
                    break;
                
                case VirtualKeyCode.VK_W:
                    W.Start();
                    break;
                
                case VirtualKeyCode.VK_E:
                    E.Start();
                    break;
                
                case VirtualKeyCode.VK_R:
                    R.Start();
                    break;
                
                case VirtualKeyCode.SCROLL:
                    Application.Current.Shutdown();
                    break;
            }
        }

        private void OnClosed(object? sender, EventArgs e)
        {
        }
    }
}