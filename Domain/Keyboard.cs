using WindowsInput;
using WindowsInput.Native;

namespace Cooldowns.Domain
{
    public class KeyboardSimulator
    {
        private readonly InputSimulator inputSimulator;

        public KeyboardSimulator()
        {
            inputSimulator = new InputSimulator();
        }

        public void Type(string text)
        {
            inputSimulator.Keyboard.TextEntry(text);
        }

        public void PressKey(VirtualKeyCode key, int delay = 15)
        {
            inputSimulator
                .Keyboard.KeyDown(key)
                .Sleep(delay)
                .KeyUp(key);
        }
    }}