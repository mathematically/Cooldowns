using System;
using System.Threading;
using NLog;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownTimer : ICooldownTimer
    {
        private readonly int firstCheckDelay;
        private readonly int buttonCheckInterval;
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private Timer? timer;

        public event EventHandler? Ticked;

        public CooldownTimer(int buttonCheckInterval, int firstCheckDelay = 1000)
        {
            this.buttonCheckInterval = buttonCheckInterval;
            this.firstCheckDelay = firstCheckDelay;
        }

        public void Start()
        {
            if (timer != null)
            {
                Stop();
            }

            log.Debug($"Timer started at {DateTime.UtcNow}");
            timer = new Timer(OnTicked, null, firstCheckDelay, buttonCheckInterval);
        }

        public void Stop()
        {
            log.Debug($"Timer stopped at {DateTime.UtcNow}");
            timer?.Dispose();
            timer = null;
        }

        private void OnTicked(object? state) => Ticked?.Invoke(this, EventArgs.Empty);

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}