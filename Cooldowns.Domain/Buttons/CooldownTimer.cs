using System;
using System.Threading;
using JetBrains.Annotations;

namespace Cooldowns.Domain.Buttons
{
    public sealed class CooldownTimer : ICooldownTimer
    {
        private const int FirstCheckDelay = 1000;
        private const int ButtonCheckInterval = 100;

        private readonly Timer timer;

        [NotNull]
        public event EventHandler? Ticked;

        public CooldownTimer()
        {
            timer = new Timer(OnTicked, null, FirstCheckDelay, ButtonCheckInterval);
        }

        private void OnTicked(object? state)
        {
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}