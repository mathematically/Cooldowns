using System;
using System.Collections.Generic;
using Cooldowns.Domain.Buttons;

namespace Cooldowns.Tests.Fixtures
{
    public sealed class FakeCooldownTimer : ICooldownTimer
    {
        private readonly Dictionary<int, bool> times = new();

        public event EventHandler? CooldownEnded;
        
        private void OnCooldownEnded()
        {
            CooldownEnded?.Invoke(this, EventArgs.Empty);
        }

        public void StartRepeating()
        {
            times.TryAdd(CooldownTimer.AutoCheckInterval, true);
        }

        public void StartOnce(int dueTime)
        {
            times.TryAdd(dueTime, false);
        }

        public void Tick(int time)
        {
            if (!times.TryGetValue(time, out bool isRepeating)) return;

            if (!isRepeating)
            {
                times.Remove(time);
            }

            OnCooldownEnded();
        }
        
        public void Dispose()
        {
        }
    }
}