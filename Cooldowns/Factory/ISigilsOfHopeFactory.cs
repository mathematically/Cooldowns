using System;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Status;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Factory
{
    public interface ISigilsOfHopeFactory
    {
        StatusChecker<SigilsOfHope> Create(ICooldownTimer timer, Action<SigilsOfHope> onChanged);
    }
}