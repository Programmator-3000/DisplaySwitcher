using System;
using System.Runtime.InteropServices;

namespace DisplaySwitcher
{

    public class KeySimulator
    {
        public const byte VK_F14 = 0x7D; // 0x7D is F14
        public const byte VK_F15 = 0x7E; // Virtual-Key code for F14
        public const byte VK_F16 = 0x7F; // Virtual-Key code for F14
        public const byte VK_F17 = 0x80; // Virtual-Key code for F14

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static void Press(byte key)
        {
            Console.WriteLine("Simulating F14 key press...");

            keybd_event(key, 0, KEYEVENTF_KEYDOWN, 0); // Key down
            keybd_event(key, 0, KEYEVENTF_KEYUP, 0);   // Key up
        }

        private const int INPUT_KEYBOARD = 1;
        public const ushort uVK_F14 = 0x7D; // Virtual-Key code for F14
        public const ushort uVK_F15 = 0x7F; // Virtual-Key code for F14
        public const ushort uVK_F16 = 0x80; // Virtual-Key code for F14
        public const ushort uVK_F17 = 0x81; // Virtual-Key code for F14


        [StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public static void Press(ushort key)
        {
            INPUT[] inputs = new INPUT[2];

            // Key down
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki = new KEYBDINPUT
            {
                wVk = key,
                wScan = 0,
                dwFlags = 0,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };

            // Key up
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki = new KEYBDINPUT
            {
                wVk = key,
                wScan = 0,
                dwFlags = KEYEVENTF_KEYUP,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };

            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            Console.WriteLine("Simulated F14 key press using SendInput.");
        }
    }

}
