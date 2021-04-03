using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using WindowsInput.Native;
using Cooldowns.Keyboard;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Cooldowns
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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

        private ButtonCooldownTimer buttonTimerQ;
        private ButtonCooldownTimer buttonTimerW;
        private ButtonCooldownTimer buttonTimerE;
        private ButtonCooldownTimer buttonTimerR;

        public MainWindow()
        {
            InitializeComponent();
            ConfigureLogging();
            
            keyboardListener = new Win32KeyboardListener();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ResetWindowPosition();
            
            keyboardListener.OnKeyPressed += OnKeyPressed;
            keyboardListener.HookKeyboard();
            
            //Automation.AddAutomationFocusChangedEventHandler(OnFocusChanged);

            buttonTimerQ = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonQ, 1900);
            buttonTimerW = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonW, 5200);
            buttonTimerE = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonE, 3700);
            buttonTimerR = new ButtonCooldownTimer(Application.Current.Dispatcher, ButtonR, 1000);

            ButtonQ.Visibility = Visibility.Visible;
            ButtonQ.IsEnabled = true;
            ButtonW.Visibility = Visibility.Visible;
            ButtonW.IsEnabled = true;
            ButtonE.Visibility = Visibility.Visible;
            ButtonE.IsEnabled = true;
            ButtonR.Visibility = Visibility.Hidden;
            ButtonR.IsEnabled = false;
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
            if (processName.Contains("Poet") || processName.Contains("dotnet")) return;
            
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                if (processName.Contains("Epoch"))
                {
                    // Enable
                }
                else
                {
                    // Disable
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

        private void ProcessKeys(KeyPressArgs e)
        {
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (e.KeyCode)
            {
                case VirtualKeyCode.PAUSE:
                    ToggleEnabled();
                    break;
                
                case VirtualKeyCode.SCROLL:
                    Application.Current.Shutdown();
                    break;
                
                case VirtualKeyCode.VK_Q:
                    if (ButtonQ.IsEnabled && ButtonQ.Visibility == Visibility.Visible)
                    {
                        buttonTimerQ.Start();
                    }
                    break;
                
                case VirtualKeyCode.VK_W:
                    if (ButtonW.IsEnabled && ButtonW.Visibility == Visibility.Visible)
                    {
                        buttonTimerW.Start();
                    }
                    break;
                
                case VirtualKeyCode.VK_E:
                    if (ButtonE.IsEnabled && ButtonE.Visibility == Visibility.Visible)
                    {
                        buttonTimerE.Start();
                    }
                    break;
                
                case VirtualKeyCode.VK_R:
                    if (ButtonR.IsEnabled && ButtonR.Visibility == Visibility.Visible)
                    {
                        buttonTimerR.Start();
                    }
                    break;
            }
        }

        private enum AppState
        {
            Off, On
        }

        private AppState state = AppState.On;

        private bool IsOff() => state == AppState.Off;
        private bool IsOn() => state == AppState.On;
        
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

        private void ShowReady()
        {
        }
        
        private void ShowOnCooldown()
        {
        }

        private void OnClosed(object? sender, EventArgs e)
        {
        }
    }
}