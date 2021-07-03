using System;
using Cooldowns.Domain.Keyboard;

namespace Cooldowns.Domain
{
    public interface IKeyboardListener
    {
        event EventHandler<KeyPressArgs> OnKeyPressed;
        void HookKeyboard();
        void UnHookKeyboard();
    }
}