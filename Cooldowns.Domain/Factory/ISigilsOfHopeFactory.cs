using System;
using Cooldowns.Domain.Status;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Domain.Factory
{
    public interface ISigilsOfHopeFactory
    {
        StatusChecker<SigilsOfHope> Create(ICooldownTimer timer, Action<SigilsOfHope> onChanged);
    }
}