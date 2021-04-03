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
        private Timer? timer;

        private CooldownButtonState ButtonState { get; }

        public ButtonCooldownTimer(Dispatcher dispatcher, Button button, int cooldownMs, CooldownButtonState buttonState = CooldownButtonState.Up)
        {
            this.dispatcher = dispatcher;
            this.button = button;
            this.cooldownMs = cooldownMs;

            ButtonState = buttonState;
            SetButtonState();
        }

        private void SetButtonState()
        {
            switch (ButtonState)
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
                    throw new ArgumentOutOfRangeException(nameof(ButtonState), ButtonState, null);
            }
        }

        private void ShowDisabled()
        {
            button.Visibility = Visibility.Hidden;
        }

        private void ShowCooldown()
        {
            button.Visibility = Visibility.Visible;
            button.Background = ForegroundBrush;
            button.Foreground = BackgroundBrush;
            button.BorderBrush = BackgroundBrush;
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
            if (ButtonState != CooldownButtonState.Up) return;
            
            timer = new Timer(CooldownEnded, button, cooldownMs, Timeout.Infinite);
            ShowCooldown();
        }
        
        private void CooldownEnded(object? state)
        {
            timer?.Dispose();
            timer = null;
            dispatcher.BeginInvoke(ShowReady);
        }

    }
}