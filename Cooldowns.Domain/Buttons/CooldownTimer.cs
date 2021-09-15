using System;
using System.Threading;
using NLog;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownTimer : ICooldownTimer
    {
        private const int FirstCheckDelay = 1000; // todo probably don't need this
        private const int ButtonCheckInterval = 100;
        private readonly Logger log = LogManager.GetCurrentClassLogger();

        private Timer? timer;

        public event EventHandler? Ticked;

        public void Start()
        {
            if (timer != null)
            {
                Stop();
            }

            log.Debug($"Timer started at {DateTime.UtcNow}");
            timer = new Timer(OnTicked, null, FirstCheckDelay, ButtonCheckInterval);
        }

        public void Stop()
        {
            log.Debug($"Timer stopped at {DateTime.UtcNow}");
            timer?.Dispose();
            timer = null;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        private void OnTicked(object? state)
        {
            Ticked?.Invoke(this, EventArgs.Empty);
        }
    }
}