using System;

namespace Cooldowns.Domain.Buttons
{
    public class ButtonStateEventArgs : EventArgs
    {
        public string Name { get; init; }
        public CooldownButtonState State{ get; init; }

        public ButtonStateEventArgs(string name, CooldownButtonState state)
        {
            Name = name;
            State = state;
        }
    }
}