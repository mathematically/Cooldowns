using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using WindowsInput.Native;
using Microsoft.Extensions.Logging;

namespace Cooldowns.Domain
{
    public class ButtonCooldownTimer
    {
        private static Brush ForegroundBrush => new SolidColorBrush(Color.FromRgb(32, 36, 36));
        private static Brush BackgroundBrush => Brushes.DarkGoldenrod;

        private readonly ILogger<Toolbar> logger;
        private readonly Dispatcher dispatcher;
        private readonly Button button;
        private readonly KeyConfig keyConfig;

        private readonly VirtualKeyCode keyCode;
        
        private CooldownButtonState buttonState;
        private Timer? timer;

        private readonly KeyboardSimulator keyboard = new();
        
        // ReSharper disable once ContextualLoggerProblem
        public ButtonCooldownTimer(ILogger<Toolbar> logger, Dispatcher dispatcher, Button button, KeyConfig keyConfig)
        {
            this.logger = logger;
            this.dispatcher = dispatcher;
            this.button = button;
            this.keyConfig = keyConfig;

            keyCode = Enum.Parse<VirtualKeyCode>(keyConfig.AutocastKey);
            
            SetButtonState(keyConfig.Enabled ? CooldownButtonState.Up : CooldownButtonState.Disabled);
        }

        private void SetButtonState(CooldownButtonState updatedState)
        {
            switch (updatedState)
            {
                case CooldownButtonState.Disabled:
                    ShowDisabled();
                    break;
                case CooldownButtonState.Autocasting:
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
            logger.LogDebug($"Button {button.Content} disabled");
            // Disabled keys are hidden not collapsed to maintain the gapping.
            button.Visibility = Visibility.Hidden;
        }

        private void ShowCooldown()
        {
            logger.LogDebug($"Button {button.Content} on cooldown.");
            button.Visibility = Visibility.Hidden;
            button.Background = ForegroundBrush;
            button.Foreground = BackgroundBrush;
            button.BorderBrush = ForegroundBrush;
        }

        private void ShowReady()
        {
            logger.LogDebug($"Button {button.Content} ready.");
            button.Visibility = Visibility.Visible;
            button.Background = BackgroundBrush;
            button.Foreground = ForegroundBrush;
            button.BorderBrush = ForegroundBrush;
        }
        
        public void Press()
        {
            logger.LogDebug($"Button {button.Content} pressed.");
            if (buttonState == CooldownButtonState.Disabled) return;

            if (keyConfig.Autocast && buttonState == CooldownButtonState.Autocasting)
            {
                // Will stop it on next timer end.
                SetButtonState(CooldownButtonState.OnCooldown);
            }
            else if (keyConfig.Autocast && buttonState == CooldownButtonState.Up)
            {
                timer = new Timer(CooldownEnded, button, keyConfig.Cooldown, keyConfig.Cooldown);
                SetButtonState(CooldownButtonState.Autocasting);
            }
            else if (buttonState == CooldownButtonState.Up)
            {
                // One shot timer as no autocast
                timer = new Timer(CooldownEnded, button, keyConfig.Cooldown, Timeout.Infinite);
                SetButtonState(CooldownButtonState.OnCooldown);
            }
        }
        
        private void CooldownEnded(object? state)
        {
            dispatcher.BeginInvoke(() =>
            {
                logger.LogDebug($"Button cooldown ended.");
            
                if (buttonState != CooldownButtonState.Autocasting)
                {
                    logger.LogDebug("Disposing timer.");
                    timer?.Dispose();
                    timer = null;
                }

                if (buttonState == CooldownButtonState.Autocasting)
                {
                    logger.LogDebug("Autocasting...");
                    keyboard.PressKey(keyCode);
                }
                else
                {
                    logger.LogDebug("Button back up");
                    SetButtonState(CooldownButtonState.Up);
                }
            });
        }
    }
}