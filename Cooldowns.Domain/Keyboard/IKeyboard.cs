using WindowsInput.Native;

namespace Cooldowns.Domain.Keyboard
{
    public interface IKeyboard
    {
        void PressKey(VirtualKeyCode key, int delay = 15);
    }
}