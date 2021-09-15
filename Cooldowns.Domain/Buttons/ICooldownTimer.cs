using System;

namespace Cooldowns.Domain.Buttons
{
    public interface ICooldownTimer : IDisposable
    {
        event EventHandler Ticked;
        void Start();
        void Stop();
    }
}