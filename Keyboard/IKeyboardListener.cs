using System;

namespace Cooldowns.Keyboard
{
    public interface IKeyboardListener
    {
        event EventHandler<KeyPressArgs> OnKeyPressed;
        event EventHandler<KeyPressArgs> OnKeyReleased;
    }
}