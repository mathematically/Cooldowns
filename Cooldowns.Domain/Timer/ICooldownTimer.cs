using System;

namespace Cooldowns.Domain.Timer
{
    public interface ICooldownTimer : IDisposable
    {
        event EventHandler Ticked;
        void Start();
        void Stop();
        bool IsRunning();
    }
}