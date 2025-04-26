using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DisplaySwitcher
{
    internal class KeyLogger
    {
        // Hook ID
        public static IntPtr _hookID = IntPtr.Zero;

        // Keyboard hook callback delegate
        public static LowLevelKeyboardProc _proc = HookCallback;

        // Entry point

        // Set the keyboard hook
        internal static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // Hook constants and declarations
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        internal delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Keys key = (Keys)vkCode;

                bool shift = (Control.ModifierKeys & Keys.Shift) != 0;
                bool ctrl = (Control.ModifierKeys & Keys.Control) != 0;
                bool alt = (Control.ModifierKeys & Keys.Alt) != 0;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Key Pressed: {key} (Ctrl: {ctrl}, Shift: {shift}, Alt: {alt})");

                // Exit on ESC
                if (key == Keys.Escape)
                    Application.Exit();
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        // WinAPI functions
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }

}
