using System;
using WindowsInput.Native;

namespace Cooldowns.Domain.Keyboard
{
    public class KeyPressArgs : EventArgs
    {
        public VirtualKeyCode KeyCode { get; }

        public KeyPressArgs(VirtualKeyCode keyCode)
        {
            KeyCode = keyCode;
        }
    }
}