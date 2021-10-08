using System;

namespace Cooldowns.Domain.Buttons
{
    public class ButtonModeEventArgs : EventArgs
    {
        public string Name { get; init; }
        public ButtonMode Mode { get; init; }

        public ButtonModeEventArgs(string name, ButtonMode mode)
        {
            Name = name;
            Mode = mode;
        }
    }
}