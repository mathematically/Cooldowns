using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Cooldowns
{
    public class ButtonCooldownTimer
    {
        private readonly Dispatcher dispatcher;
        private readonly Button button;
        private readonly int cooldown;
        private Timer? timer;

        public ButtonCooldownTimer(Dispatcher dispatcher, Button button, int cooldown)
        {
            this.dispatcher = dispatcher;
            this.button = button;
            this.cooldown = cooldown;
        }

        public void Start()
        {
            timer = new Timer(CooldownEnded, button, cooldown, Timeout.Infinite);
            dispatcher.BeginInvoke(() => { button.Visibility = Visibility.Hidden;});
        }
        
        private void CooldownEnded(object state)
        {
            timer?.Dispose();
            timer = null;
            dispatcher.BeginInvoke(() => { button.Visibility = Visibility.Visible;});
        }

    }
}