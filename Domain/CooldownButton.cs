// ReSharper disable TemplateIsNotCompileTimeConstantProblem

using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input.Manipulations;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsInput.Native;
using Cooldowns.Configuration;
using Cooldowns.Keyboard;
using NLog;

namespace Cooldowns.Domain
{
    public class CooldownButton
    {
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        
        private static Brush ForegroundBrush => new SolidColorBrush(Color.FromRgb(32, 36, 36));
        private static Brush BackgroundBrush => Brushes.DarkGoldenrod;

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

            SetButtonState(keyConfig.Enabled ? CooldownButtonState.Up : CooldownButtonState.Disabled);
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
            log.Debug($"Button {button.Content} pressed.");
            if (buttonState == CooldownButtonState.Disabled) return;

            switch (keyConfig.Autocast)
            {
                case true when buttonState == CooldownButtonState.AutoCasting:
                    log.Debug($"Auto cast stopped for {button.Content} will be disabled on next timer end.");
                    SetButtonState(CooldownButtonState.OnCooldown);
                    break;
                case true when buttonState == CooldownButtonState.Up:
                    log.Debug($"Enabling auto cast for {button.Content} as {autocastKeyCode}");
                    timer = CreateTimer(keyConfig.Cooldown);
                    SetButtonState(CooldownButtonState.AutoCasting);
                    break;
                default:
                {
                    if (buttonState == CooldownButtonState.Up)
                    {
                        log.Debug($"Creating one shot timer for {button.Content}");
                        timer = CreateTimer(Timeout.Infinite);
                        SetButtonState(CooldownButtonState.OnCooldown);
                    }

                    break;
                }
            }
            
            Timer CreateTimer(int period)
            {
                return new(CooldownEnded, button, keyConfig.Cooldown, period);
            }
        }
        
        private void CooldownEnded(object? state)
        {
            dispatcher.BeginInvoke(() =>
            {
                log.Debug($"Cooldown ended for {button.Content}");
                
                if (buttonState == CooldownButtonState.AutoCasting)
                {
                    log.Debug($"Autocasting {autocastKeyCode} from button {button.Content}");
                    keyboard.PressKey(autocastKeyCode);
                }
                else
                {
                    Unload();

                    log.Debug($"Button {button.Content} back up");
                    SetButtonState(CooldownButtonState.Up);
                }
            });
        }

        public void Unload()
        {
            timer?.Dispose();
            timer = null;
        }
    }
}