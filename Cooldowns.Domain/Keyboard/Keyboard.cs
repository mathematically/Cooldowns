using WindowsInput;
using WindowsInput.Native;

namespace Cooldowns.Domain.Keyboard
{
    public class KeyboardSimulator
    {
        private readonly InputSimulator inputSimulator;

        public KeyboardSimulator()
        {
            inputSimulator = new InputSimulator();
        }

        public void PressKey(VirtualKeyCode key, int delay = 15)
        {
            inputSimulator
                .Keyboard.KeyDown(key)
                .Sleep(delay)
                .KeyUp(key);
        }
    }}