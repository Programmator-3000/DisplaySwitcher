using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplaySwitcher
{
    internal static class WinHelpers
    {
        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [DllImport("user32.dll")]
        static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [Flags]
        public enum SetDisplayConfigFlags : uint
        {
            SDC_TOPOLOGY_INTERNAL = 0x00000001,
            SDC_TOPOLOGY_CLONE = 0x00000002,
            SDC_TOPOLOGY_EXTEND = 0x00000004,
            SDC_TOPOLOGY_EXTERNAL = 0x00000008,
            SDC_APPLY = 0x00000080
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern long SetDisplayConfig(uint numPathArrayElements,
            IntPtr pathArray, uint numModeArrayElements, IntPtr modeArray, SetDisplayConfigFlags flags);

        public static void CloneDisplays()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_CLONE | SetDisplayConfigFlags.SDC_APPLY);
        }

        public static void ExtendDisplays()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_EXTEND | SetDisplayConfigFlags.SDC_APPLY);
        }

        public static void ExternalDisplay()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_EXTERNAL | SetDisplayConfigFlags.SDC_APPLY);
        }

        public static void InternalDisplay()
        {
            SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, SetDisplayConfigFlags.SDC_TOPOLOGY_INTERNAL | SetDisplayConfigFlags.SDC_APPLY);
        }

        [Flags()]
        public enum ChangeDisplaySettingsFlags : uint
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }
        /*
        [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Ansi)]
        struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;
            public const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            [System.Runtime.InteropServices.FieldOffset(0)]
            public string dmDeviceName;
            [System.Runtime.InteropServices.FieldOffset(32)]
            public Int16 dmSpecVersion;
            [System.Runtime.InteropServices.FieldOffset(34)]
            public Int16 dmDriverVersion;
            [System.Runtime.InteropServices.FieldOffset(36)]
            public Int16 dmSize;
            [System.Runtime.InteropServices.FieldOffset(38)]
            public Int16 dmDriverExtra;
            [System.Runtime.InteropServices.FieldOffset(40)]
            public DM dmFields;

            [System.Runtime.InteropServices.FieldOffset(44)]
            Int16 dmOrientation;
            [System.Runtime.InteropServices.FieldOffset(46)]
            Int16 dmPaperSize;
            [System.Runtime.InteropServices.FieldOffset(48)]
            Int16 dmPaperLength;
            [System.Runtime.InteropServices.FieldOffset(50)]
            Int16 dmPaperWidth;
            [System.Runtime.InteropServices.FieldOffset(52)]
            Int16 dmScale;
            [System.Runtime.InteropServices.FieldOffset(54)]
            Int16 dmCopies;
            [System.Runtime.InteropServices.FieldOffset(56)]
            Int16 dmDefaultSource;
            [System.Runtime.InteropServices.FieldOffset(58)]
            Int16 dmPrintQuality;

            [System.Runtime.InteropServices.FieldOffset(44)]
            public POINTL dmPosition;
            [System.Runtime.InteropServices.FieldOffset(52)]
            public Int32 dmDisplayOrientation;
            [System.Runtime.InteropServices.FieldOffset(56)]
            public Int32 dmDisplayFixedOutput;

            [System.Runtime.InteropServices.FieldOffset(60)]
            public short dmColor; // See note below!
            [System.Runtime.InteropServices.FieldOffset(62)]
            public short dmDuplex; // See note below!
            [System.Runtime.InteropServices.FieldOffset(64)]
            public short dmYResolution;
            [System.Runtime.InteropServices.FieldOffset(66)]
            public short dmTTOption;
            [System.Runtime.InteropServices.FieldOffset(68)]
            public short dmCollate; // See note below!
            [System.Runtime.InteropServices.FieldOffset(70)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            [System.Runtime.InteropServices.FieldOffset(102)]
            public Int16 dmLogPixels;
            [System.Runtime.InteropServices.FieldOffset(104)]
            public Int32 dmBitsPerPel;
            [System.Runtime.InteropServices.FieldOffset(108)]
            public Int32 dmPelsWidth;
            [System.Runtime.InteropServices.FieldOffset(112)]
            public Int32 dmPelsHeight;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmDisplayFlags;
            [System.Runtime.InteropServices.FieldOffset(116)]
            public Int32 dmNup;
            [System.Runtime.InteropServices.FieldOffset(120)]
            public Int32 dmDisplayFrequency;
        }

        enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        [DllImport("user32.dll")]
        static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, ChangeDisplaySettingsFlags dwflags, IntPtr lParam);
        */
        public static void Test()
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
                    TestForAdapter(adapterName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("{0}", ex.ToString()));
            }
        }

        private static void TestForAdapter(string adapter)
        {
            DISPLAY_DEVICE d = new DISPLAY_DEVICE();
            d.cb = Marshal.SizeOf(d);
            try
            {
                for (uint id = 0; EnumDisplayDevices(adapter, id, ref d, 0); id++)
                {
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

        /*private static void Disconnect(string deviceName)
        {
            DEVMODE dEVMODE = new DEVMODE();

        }*/

    }
}
