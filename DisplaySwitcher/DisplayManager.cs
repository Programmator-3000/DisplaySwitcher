using System;
using System.Runtime.InteropServices;

namespace DisplaySwitcher
{
    class DisplayManager
    {
        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int ENUM_REGISTRY_SETTINGS = 0;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int CDS_NORESET = 0x10000000;
        private const int CDS_RESET = 0x40000000;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        private const int DM_PELSWIDTH = 0x80000;
        private const int DM_PELSHEIGHT = 0x100000;
        private const int DM_POSITION = 0x20;
        private const int DM_DISPLAYFREQUENCY = 0x400000;
        private const int DM_BITSPERPEL = 0x00040000;
        private const int DM_DISPLAYFLAGS = 0x00200000;

        private const int EDD_GET_DEVICE_INTERFACE_NAME = 0x00000001;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DEVMODE
        {
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public ushort dmSpecVersion;
            public ushort dmDriverVersion;
            public ushort dmSize;
            public ushort dmDriverExtra;
            public uint dmFields;
            public POINTL dmPosition;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public ushort dmLogPixels;
            public uint dmBitsPerPel;
            public uint dmPelsWidth;
            public uint dmPelsHeight;
            public uint dmDisplayFlags;
            public uint dmDisplayFrequency;
            public uint dmICMMethod;
            public uint dmICMIntent;
            public uint dmMediaType;
            public uint dmDitherType;
            public uint dmReserved1;
            public uint dmReserved2;
            public uint dmPanningWidth;
            public uint dmPanningHeight;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINTL
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern int ChangeDisplaySettingsEx(
            string lpszDeviceName, ref DEVMODE lpDevMode,
            IntPtr hwnd, uint dwflags, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplayDevices(
            string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        public static extern bool EnumDisplaySettings(
            string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            public int StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        public static void SetDisplayState2(bool enable)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            try
            {
                for (uint id = 0; EnumDisplayDevices(null, id, ref d, 0); id++)
                {
                    string adapterName = d.DeviceName;

                    Console.WriteLine(
                        String.Format("{0}, {1}, {2}, {3}, {4}, {5}",
                                 id,
                                 d.DeviceName,
                                 d.DeviceString,
                                 d.StateFlags,
                                 d.DeviceID,
                                 d.DeviceKey
                                 )
                                  );
                    d.cb = Marshal.SizeOf(d);
                    ProcessDisplay(adapterName, enable);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("{0}", ex.ToString()));
            }
        }

        private static void ProcessDisplay(string adapter, bool enable)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            try
            {
                for (uint id = 0; EnumDisplayDevices(adapter, id, ref d, 0); id++)
                {
                    if(d.DeviceID.Contains("SAM0C1A"))
                    {
                        DEVMODE dm = new DEVMODE();
                        dm.dmSize = (ushort)Marshal.SizeOf(dm);

                        if (!EnumDisplaySettings(adapter, ENUM_REGISTRY_SETTINGS, ref dm))
                        {
                            Console.WriteLine("Could not get display settings.");
                            return;
                        }

                        if (enable)
                        {
                            // Enable and position it to the right of primary (e.g., 1920x0)
                            //dm.dmFields = DM_POSITION | DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY | DM_BITSPERPEL | DM_DISPLAYFLAGS;


                            dm.dmFields = 544997536;
                            dm.dmPosition.x = 3840; // adjust if needed
                            dm.dmPosition.y = 876;
                            //dm.dmPelsWidth = 1920;
                            //dm.dmPelsHeight = 1080;
                        }
                        else
                        {
                            // Disable it
                            //dm.dmFields = 8126592;

                            dm.dmFields = DM_POSITION | DM_PELSWIDTH | DM_PELSHEIGHT;
                            //dm.dmPosition.x = -32000; // push off screen to effectively "disable"
                            //dm.dmPosition.y = 0;

                            dm.dmPosition.x = 0; // adjust if needed
                            dm.dmPosition.y = 0;

                            dm.dmPelsWidth = 0;
                            dm.dmPelsHeight = 0;
                        }

                        int result = ChangeDisplaySettingsEx(adapter, ref dm, IntPtr.Zero,
                            CDS_UPDATEREGISTRY | CDS_RESET, IntPtr.Zero);

                        if (result == DISP_CHANGE_SUCCESSFUL)
                        {
                            Console.WriteLine(enable ? "Display enabled." : "Display disabled.");
                        }
                        else
                        {
                            Console.WriteLine($"ChangeDisplaySettingsEx failed: {result}");
                        }
                    }

                    Console.WriteLine(
                        String.Format("{0}, {1}, {2}, {3}, {4}",
                                 d.DeviceName,
                                 d.DeviceString,
                                 d.StateFlags,
                                 d.DeviceID,
                                 d.DeviceKey
                                 )
                                  );
                    d.cb = Marshal.SizeOf(d);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("{0}", ex.ToString()));
            }
        }


        // Find external display and apply mode
        public static void SetDisplayState(bool enable)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);

            uint deviceIndex = 0;
            while (EnumDisplayDevices(null, deviceIndex, ref d, EDD_GET_DEVICE_INTERFACE_NAME))
            {
                if ((d.StateFlags & 0x00000004) != 0) // AttachedToDesktop
                {
                    // skip primary
                    deviceIndex++;
                    continue;
                }

                Console.WriteLine($"Found display: {d.DeviceName}");

                DEVMODE dm = new DEVMODE();
                dm.dmSize = (ushort)Marshal.SizeOf(dm);

                if (!EnumDisplaySettings(d.DeviceName, ENUM_CURRENT_SETTINGS, ref dm))
                {
                    Console.WriteLine("Could not get display settings.");
                    return;
                }

                if (enable)
                {
                    // Enable and position it to the right of primary (e.g., 1920x0)
                    dm.dmFields = DM_POSITION | DM_PELSWIDTH | DM_PELSHEIGHT;
                    dm.dmPosition.x = 1920; // adjust if needed
                    dm.dmPosition.y = 0;
                }
                else
                {
                    // Disable it
                    dm.dmFields = 0;
                    dm.dmPelsWidth = 0;
                    dm.dmPelsHeight = 0;
                }

                int result = ChangeDisplaySettingsEx(d.DeviceName, ref dm, IntPtr.Zero,
                    CDS_UPDATEREGISTRY | CDS_RESET, IntPtr.Zero);

                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    Console.WriteLine(enable ? "Display enabled." : "Display disabled.");
                }
                else
                {
                    Console.WriteLine($"ChangeDisplaySettingsEx failed: {result}");
                }

                break;
            }
        }
    }

}
