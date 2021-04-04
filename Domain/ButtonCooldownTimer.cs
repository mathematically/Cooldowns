using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cooldowns.Domain
{
    public class ButtonCooldownTimer
    {
        private static Brush ForegroundBrush => new SolidColorBrush(Color.FromRgb(32, 36, 36));
        private static Brush BackgroundBrush => Brushes.DarkGoldenrod;

        private readonly Dispatcher dispatcher;
        private readonly Button button;
        private readonly int cooldownMs;

        private CooldownButtonState buttonState;
        private Timer? timer;

        public ButtonCooldownTimer(Dispatcher dispatcher, Button button, int cooldownMs)
        {
            this.dispatcher = dispatcher;
            this.button = button;
            this.cooldownMs = cooldownMs;

            SetButtonState(cooldownMs == 0 ? CooldownButtonState.Disabled : CooldownButtonState.Up);
        }

        private void SetButtonState(CooldownButtonState updatedState)
        {
            switch (updatedState)
            {
                case CooldownButtonState.Disabled:
                    ShowDisabled();
                    break;
                case CooldownButtonState.OnCooldown:
                    ShowCooldown();
                    break;
                case CooldownButtonState.Up:
                    ShowReady();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(this.buttonState), this.buttonState, null);
            }
            
            buttonState = updatedState;
        }

        private void ShowDisabled()
        {
            button.Visibility = Visibility.Hidden;
        }

        private void ShowCooldown()
        {
            button.Visibility = Visibility.Hidden;
            button.Background = ForegroundBrush;
            button.Foreground = BackgroundBrush;
            button.BorderBrush = ForegroundBrush;
        }

        private void ShowReady()
        {
            button.Visibility = Visibility.Visible;
            button.Background = BackgroundBrush;
            button.Foreground = ForegroundBrush;
            button.BorderBrush = ForegroundBrush;
        }
        
        public void Start()
        {
            if (buttonState != CooldownButtonState.Up) return;
            
            SetButtonState(CooldownButtonState.OnCooldown);
            timer = new Timer(CooldownEnded, button, cooldownMs, Timeout.Infinite);
        }
        
        private void CooldownEnded(object? state)
        {
            timer?.Dispose();
            timer = null;
            
            dispatcher.BeginInvoke(() =>
            {
                SetButtonState(CooldownButtonState.Up);
            });
        }
    }
}