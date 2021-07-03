using System;
using System.Threading;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownTimer : ICooldownTimer
    {
        public const int AutoCheckInterval = 100;
        private const int AutoCheckDelay = 1000; // todo really?

        private readonly IDispatcher dispatcher;
        private Timer? timer;

        public event EventHandler? CooldownEnded;

        private void OnCooldownEnded()
        {
            CooldownEnded?.Invoke(this, EventArgs.Empty);
        }

        public CooldownTimer(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void StartRepeating()
        {
            if (timer != null) throw new SystemException("Cannot start a timer that has already been started");
            timer = new Timer(OnTimerEnded, null, AutoCheckInterval, AutoCheckDelay);
        }

        public void StartOnce(int dueTime)
        {
            timer?.Dispose();
            timer = new Timer(OnTimerEnded, null, Timeout.Infinite, dueTime);
        }

        private void OnTimerEnded(object? state)
        {
            dispatcher.BeginInvoke(OnCooldownEnded);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}