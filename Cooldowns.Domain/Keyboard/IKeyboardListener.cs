using System;

namespace Cooldowns.Domain.Keyboard
{
    public interface IKeyboardListener
    {
        event EventHandler<KeyPressArgs> OnKeyPressed;
        event EventHandler<KeyPressArgs> OnKeyReleased;
        void HookKeyboard();
        void UnHookKeyboard();
    }
}