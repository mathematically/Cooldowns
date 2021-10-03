using System;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Domain.Factory
{
    public interface ICooldownButtonFactory
    {
        CooldownButton? Create(KeyConfig config, ICooldownTimer cooldownTimer, Action<ButtonStateEventArgs> onToolbarButtonStateChanged, 
            Action<ButtonModeEventArgs> onToolbarButtonModeChanged);
    }
}