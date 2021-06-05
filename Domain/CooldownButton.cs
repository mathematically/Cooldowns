// ReSharper disable TemplateIsNotCompileTimeConstantProblem

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsInput.Native;
using Cooldowns.Configuration;
using Cooldowns.Keyboard;
using Cooldowns.Screen;
using NLog;

namespace Cooldowns.Domain
{
    public class CooldownButton
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private static bool IsSkillAvailable(System.Drawing.Color p) => p.R == 255 && p.G == 255 && p.B == 255;
        private static bool IsSkillOnCooldown(System.Drawing.Color p) => p.R == 17 && p.G == 17 && p.B == 21;
        
        private static Brush ForegroundBrush => new SolidColorBrush(Color.FromRgb(32, 36, 36));
        private static Brush BackgroundBrush => Brushes.DarkGoldenrod;

        private const int AutoCheckInterval = 100;
        private const int AutoCheckDelay = 1000;
        
        private readonly Dispatcher dispatcher;
        private readonly Button button;
        private readonly Key keyConfig;
        private readonly VirtualKeyCode autocastKeyCode;
        
        private CooldownButtonState buttonState;
        private Timer? timer;

        private readonly KeyboardSimulator keyboard = new();
        
        // ReSharper disable once ContextualLoggerProblem
        public CooldownButton(Dispatcher dispatcher, Button button, Key keyConfig)
        {
            this.dispatcher = dispatcher;
            this.button = button;
            this.keyConfig = keyConfig;
            autocastKeyCode = Enum.Parse<VirtualKeyCode>(keyConfig.AutocastKey);
            
            if (keyConfig.Autocast)
            {
                log.Debug($"Key {this.button.Content} will auto cast as {autocastKeyCode}");
            }

            if (keyConfig.AutoDetectCooldown)
            {
                log.Debug($"Key {this.button.Content} will auto detect cooldonws based on screen pixels");
                timer = CreateTimer(AutoCheckInterval, AutoCheckDelay);
            }

            SetButtonState(keyConfig.Enabled ? CooldownButtonState.Up : CooldownButtonState.Disabled);
        }

        Timer CreateTimer(int period, int dueTime)
        {
            return new(OnCooldownEnded, button, dueTime, period);
        }

        private void SetButtonState(CooldownButtonState updatedState)
        {
            switch (updatedState)
            {
                case CooldownButtonState.Disabled:
                    ShowDisabled();
                    break;
                case CooldownButtonState.AutoCasting:
                case CooldownButtonState.OnCooldown:
                    ShowCooldown();
                    break;
                case CooldownButtonState.Up:
                    ShowReady();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(updatedState), updatedState, null);
            }
            
            buttonState = updatedState;
        }

        private void ShowDisabled()
        {
            log.Debug($"Button {button.Content} disabled");
            button.Visibility = Visibility.Hidden;
        }

        private void ShowCooldown()
        {
            log.Debug($"Button {button.Content} on cooldown.");
            button.Visibility = Visibility.Hidden;
        }

        private void ShowReady()
        {
            log.Debug($"Button {button.Content} ready.");
            button.Visibility = Visibility.Visible;
            
            button.Background = BackgroundBrush;
            button.Foreground = ForegroundBrush;
            button.BorderBrush = ForegroundBrush;
        }
        
        public void Press()
        {
            if (buttonState == CooldownButtonState.Disabled || keyConfig.AutoDetectCooldown) return;

            switch (keyConfig.Autocast)
            {
                case true when buttonState == CooldownButtonState.AutoCasting:
                    log.Debug($"Auto cast stopped for {button.Content} will be disabled on next timer end.");
                    SetButtonState(CooldownButtonState.OnCooldown);
                    
                    break;
                
                case true when buttonState == CooldownButtonState.Up:
                    log.Debug($"Enabling auto cast for {button.Content} as {autocastKeyCode}.");
                    timer = CreateTimer(keyConfig.Cooldown, keyConfig.Cooldown);
                    SetButtonState(CooldownButtonState.AutoCasting);
                    
                    break;
                
                default:
                {
                    if (buttonState == CooldownButtonState.Up)
                    {
                        log.Debug($"Creating one shot timer for {button.Content}.");
                        timer = CreateTimer(Timeout.Infinite, keyConfig.Cooldown);
                        SetButtonState(CooldownButtonState.OnCooldown);
                    }

                    break;
                }
            }
        }
        
        private void OnCooldownEnded(object? state)
        {
            dispatcher.BeginInvoke(() =>
            {
                if (keyConfig.AutoDetectCooldown)
                {
                    ProcessAutoDetectCooldown();
                }
                else
                {
                    ProcessCooldown();
                }
            });
        }

        private void ProcessAutoDetectCooldown()
        {
            var pixel = ScreenPixel.GetColor(keyConfig.DetectX, keyConfig.DetectY);
            
            var isAvailable = IsSkillAvailable(pixel);
            var isOnCooldown = IsSkillOnCooldown(pixel);

            if (isAvailable && buttonState == CooldownButtonState.OnCooldown)
            {
                OnButtonAvailable();
            }
            else if (isOnCooldown && buttonState is CooldownButtonState.Up)
            {
                OnButtonOnCooldown();
            }
            else if (!isAvailable && !isOnCooldown && buttonState is CooldownButtonState.Up)
            {
                log.Debug($"{button.Content} has been toggled on {pixel}");
                OnButtonOnCooldown();
            }
        }

        private void ProcessCooldown()
        {
            if (buttonState == CooldownButtonState.AutoCasting)
            {
                keyboard.PressKey(autocastKeyCode);
            }
            else
            {
                OnButtonAvailable();
            }
        }

        void OnButtonOnCooldown()
        {
            log.Debug($"Button {button.Content} on cooldown");
            if (!keyConfig.AutoDetectCooldown)
            {
                UnloadTimer();
            }
            SetButtonState(CooldownButtonState.OnCooldown);
        }
                
        void OnButtonAvailable()
        {
            log.Debug($"Button {button.Content} back up");
            if (!keyConfig.AutoDetectCooldown)
            {
                UnloadTimer();
            }
            SetButtonState(CooldownButtonState.Up);
        }

        public void UnloadTimer()
        {
            timer?.Dispose();
            timer = null;
        }
    }
}