using System;
using System.Windows.Controls;
using Cooldowns.Domain.Buttons;
using Cooldowns.Domain.Config;
using Cooldowns.Domain.Timer;

namespace Cooldowns.Factory
{
    public interface ICooldownButtonFactory
    {
        CooldownButton Create(Button button, KeyConfig config, ICooldownTimer cooldownTimer, 
            Action<Button, CooldownButtonState> onToolbarButtonStateChanged, Action<Button, CooldownButtonMode> onToolbarButtonModeChanged);
    }
}