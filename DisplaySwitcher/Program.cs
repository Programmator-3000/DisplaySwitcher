using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplaySwitcher
{
    internal class Program
    {
        public static void Main()
        {
            KeySimulator.Press(KeySimulator.VK_F14);
            //KeySimulator.Press(KeySimulator.VK_F15);
            //KeySimulator.Press(KeySimulator.VK_F16);
            //KeySimulator.Press(KeySimulator.VK_F17);
            KeyLogger._hookID = KeyLogger.SetHook(KeyLogger._proc);
            Console.WriteLine("Listening for key presses. Press ESC to exit.\n");

            Application.Run(); // Keeps the app running

            KeyLogger.UnhookWindowsHookEx(KeyLogger._hookID); // Unhook when done
        }

        static void Main2(string[] args)
        {
            DisplayHelper.ResetSecondaryDisplay();
            //DisplayHelper.DisableSecondaryDisplay();
            //DisplayManager.SetDisplayState2(true);
            //WinHelpers.Test();
            Console.ReadLine();
        }
    }
}
