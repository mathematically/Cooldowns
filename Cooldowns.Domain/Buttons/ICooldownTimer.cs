using System;

namespace Cooldowns.Domain.Buttons
{
    public interface ICooldownTimer : IDisposable
    {
        event EventHandler? CooldownEnded;
        void StartRepeating();
        void StartOnce(int dueTime);
    }
}