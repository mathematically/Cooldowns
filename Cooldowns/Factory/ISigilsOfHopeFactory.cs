using System;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Status;

namespace Cooldowns.Factory
{
    public interface ISigilsOfHopeFactory
    {
        StatusChecker<SigilsOfHope> Create(ICooldownTimer gameCheckTimer, Action<SigilsOfHope> onSigilsOfHopeStatusChanged);
    }
}