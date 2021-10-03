using System;
using NLog;

namespace Cooldowns.Domain.Timer
{
    public sealed class CooldownTimer : ICooldownTimer
    {
        private readonly int delay;
        private readonly int checkInterval;
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private System.Threading.Timer? timer;

        public event EventHandler? Ticked;

        public CooldownTimer(int checkInterval, int delay = 1000)
        {
            this.checkInterval = checkInterval;
            this.delay = delay;
        }

        public void Start()
        {
            if (timer != null)
            {
                Stop();
            }

            log.Debug($"Timer started at {DateTime.UtcNow}");
            timer = new System.Threading.Timer(OnTicked, null, delay, checkInterval);
        }

        public void Stop()
        {
            log.Debug($"Timer stopped at {DateTime.UtcNow}");
            timer?.Dispose();
            timer = null;
        }

        public bool IsRunning()
        {
            return timer != null;
        }

        private void OnTicked(object? state) => Ticked?.Invoke(this, EventArgs.Empty);

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}