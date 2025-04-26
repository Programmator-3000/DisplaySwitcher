using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DisplaySwitcherService
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //var form = new Form();
            //form.WindowState = FormWindowState.Minimized;
            //form.ShowInTaskbar = false;
            listener = new GlobalHotkeyListener();
            //listener.Visible = false;
            //listener.WindowState = FormWindowState.Minimized;
            //listener.ShowInTaskbar = false;
            // Create and configure the NotifyIcon (tray icon)
            trayIcon = new NotifyIcon
            {
                Icon = Properties.Resources.monitor, // Set to a built-in system icon, or use a custom .ico file
                Visible = true,
                Text = "DisplaySwitcher" // Text that shows when hovering over the icon
            };

            // Create a context menu with an Exit option
            var contextMenu = new ContextMenuStrip();
            var exitMenuItem = new ToolStripMenuItem("Exit", null, OnExit);
            contextMenu.Items.Add(exitMenuItem);

            AddAppToStartup();

            // Attach the context menu to the tray icon
            trayIcon.ContextMenuStrip = contextMenu;
            Application.Run();
        }

        static void AddAppToStartup()
        {
            try
            {
                // Not allowed
                string appName = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;
                string appPath = Assembly.GetExecutingAssembly().Location;
                // Open the registry key for the current user (Run registry location)
                RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

                // Check if the app is already in the registry
                if (registryKey.GetValue(appName) == null)
                {
                    // Add the app to startup
                    registryKey.SetValue(appName, "\"" + appPath + "\"");
                }
                else
                {
                    Debug.WriteLine("Application is already in the startup registry.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private static void OnExit(object sender, EventArgs e)
        {
            // Clean up before exiting
            trayIcon.Visible = false; // Hide tray icon
            listener.Close();
            Application.Exit();       // Exit the application
        }

        private static NotifyIcon trayIcon;
        private static GlobalHotkeyListener listener;

        class GlobalHotkeyListener : Form
        {
            // Modifier key constants
            private const int MOD_ALT = 0x0001;
            private const int MOD_CONTROL = 0x0002;
            private const int MOD_WIN = 0x0008;

            // Windows message ID for hotkey
            private const int WM_HOTKEY = 0x0312;

            // Hotkey IDs (arbitrary unique ints)
            private const int HOTKEY_ID_U = 12345;
            private const int HOTKEY_ID_I = 22345;
            private const int HOTKEY_ID_O = 32345;
            private const int HOTKEY_ID_P = 42345;
            private const int HOTKEY_ID_Bracket = 52345;

            [DllImport("user32.dll")]
            private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

            [DllImport("user32.dll")]
            private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

            public GlobalHotkeyListener()
            {
                // Register hotkeys
                RegisterHotKey(this.Handle, HOTKEY_ID_U, MOD_WIN | MOD_CONTROL | MOD_ALT, (int)Keys.U);
                RegisterHotKey(this.Handle, HOTKEY_ID_I, MOD_WIN | MOD_CONTROL | MOD_ALT, (int)Keys.I);
                RegisterHotKey(this.Handle, HOTKEY_ID_O, MOD_WIN | MOD_CONTROL | MOD_ALT, (int)Keys.O);
                RegisterHotKey(this.Handle, HOTKEY_ID_P, MOD_WIN | MOD_CONTROL | MOD_ALT, (int)Keys.P);
                RegisterHotKey(this.Handle, HOTKEY_ID_Bracket, MOD_WIN | MOD_CONTROL | MOD_ALT, (int)Keys.OemOpenBrackets);

                Debug.WriteLine("Listening for global hotkeys...");
                Debug.WriteLine("Win + Ctrl + Alt + U/I/O/P\nPress ESC to exit.");
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_HOTKEY)
                {
                    int id = m.WParam.ToInt32();

                    switch (id)
                    {
                        case HOTKEY_ID_U:
                            Debug.WriteLine("[U] Hotkey pressed");
                            RunDisplaySwitchInternal(HOTKEY_ID_U);
                            break;
                        case HOTKEY_ID_I:
                            RunDisplaySwitchInternal(HOTKEY_ID_I);
                            Debug.WriteLine("[I] Hotkey pressed");
                            break;
                        case HOTKEY_ID_O:
                            RunDisplaySwitchInternal(HOTKEY_ID_O);
                            Debug.WriteLine("[O] Hotkey pressed");
                            break;
                        case HOTKEY_ID_P:
                            RunDisplaySwitchInternal(HOTKEY_ID_P);
                            Debug.WriteLine("[P] Hotkey pressed");
                            break;
                        case HOTKEY_ID_Bracket:
                            RunDisplaySwitchInternal(HOTKEY_ID_Bracket);
                            Debug.WriteLine("[P] Hotkey pressed");
                            break;
                    }
                }

                base.WndProc(ref m);
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                // Unregister hotkeys
                UnregisterHotKey(this.Handle, HOTKEY_ID_U);
                UnregisterHotKey(this.Handle, HOTKEY_ID_I);
                UnregisterHotKey(this.Handle, HOTKEY_ID_O);
                UnregisterHotKey(this.Handle, HOTKEY_ID_P);
                UnregisterHotKey(this.Handle, HOTKEY_ID_Bracket);

                base.OnFormClosing(e);
            }

            // Runs DisplaySwitch.exe with "/internal" argument
            private void RunDisplaySwitchInternal(int hotKeyID)
            {
                try
                {
                    string arguments = string.Empty;
                    switch (hotKeyID)
                    {
                        case HOTKEY_ID_U:
                            {
                                arguments = @"/internal";
                                break;
                            }
                        case HOTKEY_ID_I:
                            {
                                arguments = @"/clone";
                                break;
                            }
                        case HOTKEY_ID_O:
                            {
                                arguments = @"/extend";
                                break;
                            }
                        case HOTKEY_ID_P:
                            {
                                arguments = @"/external";
                                break;
                            }
                        case HOTKEY_ID_Bracket:
                            {
                                FlipThirdMonitor();
                                return;
                            }
                        default:
                            return;
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = @"C:\Windows\Sysnative\DisplaySwitch.exe",
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                    Debug.WriteLine($"DisplaySwitch {arguments} launched.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }

            private static void FlipThirdMonitor()
            {
                Debug.WriteLine("Switching third monitor.");

                try
                {
                    DisplayHelper.FlipSecondaryDisplayState();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
