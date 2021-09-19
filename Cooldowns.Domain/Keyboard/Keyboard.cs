using WindowsInput;
using WindowsInput.Native;

namespace Cooldowns.Domain.Keyboard
{
    public class KeyboardSimulator: IKeyboard
    {
        private readonly InputSimulator inputSimulator;

        public KeyboardSimulator()
        {
            inputSimulator = new InputSimulator();
        }

        public void PressKey(VirtualKeyCode key, int delay = 0)
        {
            inputSimulator
                .Keyboard.KeyDown(key)
                .Sleep(delay)
                .KeyUp(key);
        }
    }
}