using System;

namespace Cooldowns.Domain.Buttons
{
    public class ButtonModeEventArgs : EventArgs
    {
        public string Name { get; init; }
        public CooldownButtonMode Mode { get; init; }

        public ButtonModeEventArgs(string name, CooldownButtonMode mode)
        {
            Name = name;
            Mode = mode;
        }
    }
}