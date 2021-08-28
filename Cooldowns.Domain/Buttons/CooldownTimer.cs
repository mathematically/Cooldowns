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

        public event EventHandler? Ticked;

        public CooldownTimer(IDispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public void Start()
        {
            timer?.Dispose();
            timer = new Timer(OnTimerTicked, null, AutoCheckInterval, AutoCheckDelay);
        }

        public void Stop()
        {
            timer?.Dispose();
            timer = null;
        }

        private void OnTimerTicked(object? state)
        {
            dispatcher.BeginInvoke(() =>
            {
                Ticked?.Invoke(this, EventArgs.Empty);
            });
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}