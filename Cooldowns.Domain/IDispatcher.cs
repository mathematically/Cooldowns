using System;

namespace Cooldowns.Domain
{
    public interface IDispatcher
    {
        void BeginInvoke(Action action);
    }
}