using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using Cooldowns.Keyboard;

namespace Cooldowns
{
    public class Win32KeyboardListener: IKeyboardListener
    {
        // ReSharper disable InconsistentNaming
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardCallback lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        public event EventHandler<KeyPressArgs> OnKeyPressed;
        public event EventHandler<KeyPressArgs> OnKeyReleased;

        private delegate IntPtr LowLevelKeyboardCallback(int nCode, IntPtr wParam, IntPtr lParam);
        private readonly LowLevelKeyboardCallback callback;
        private IntPtr callbackId = IntPtr.Zero;
        public Win32KeyboardListener()
        {
            callback = Callback;
        }

        public bool IsHooked() => callbackId != IntPtr.Zero;
        public void HookKeyboard()
        {
            callbackId = SetHook(callback);
        }
        public void UnHookKeyboard()
        {
            UnhookWindowsHookEx(callbackId);
            callbackId = IntPtr.Zero;
        } 
        private IntPtr SetHook(LowLevelKeyboardCallback keyboardCallback)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, keyboardCallback, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private IntPtr Callback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (wParam == (IntPtr) WM_KEYDOWN || wParam == (IntPtr) WM_SYSKEYDOWN)
                {
                    OnKeyPressed?.Invoke(this, new KeyPressArgs((VirtualKeyCode) Marshal.ReadInt32(lParam)));
                }
                else if (wParam == (IntPtr) WM_KEYUP || wParam == (IntPtr) WM_SYSKEYUP)
                {
                    OnKeyReleased?.Invoke(this, new KeyPressArgs((VirtualKeyCode) Marshal.ReadInt32(lParam)));
                }
            }

            return CallNextHookEx(callbackId, nCode, wParam, lParam);
        }
    }
}