using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace DisplaySwitcherService
{
    public class DisplayHelper
    {
        private const int QDC_ONLY_ACTIVE_PATHS = 0x00000002;
        private const int SDC_APPLY = 0x00000080;
        private const int SDC_USE_SUPPLIED_DISPLAY_CONFIG = 0x00000020;
        private const int SDC_TOPOLOGY_INTERNAL = 0x00000001;
        private const int SDC_TOPOLOGY_CLONE = 0x00000002;
        private const int SDC_TOPOLOGY_EXTEND = 0x00000004;
        private const int SDC_TOPOLOGY_EXTERNAL = 0x00000008;
        private const int SDC_USE_DATABASE_CURRENT = (SDC_TOPOLOGY_INTERNAL | SDC_TOPOLOGY_CLONE | SDC_TOPOLOGY_EXTEND | SDC_TOPOLOGY_EXTERNAL);
        private const int SDC_ALLOW_CHANGES = 0x00000400;



        [DllImport("user32.dll")]
        private static extern int GetDisplayConfigBufferSizes(
            uint flags,
            out uint numPathArrayElements,
            out uint numModeInfoArrayElements);

        [DllImport("user32.dll")]
        private static extern int QueryDisplayConfig(
            uint flags,
            ref uint numPathArrayElements,
            [Out] DISPLAYCONFIG_PATH_INFO[] pathArray,
            ref uint numModeInfoArrayElements,
            [Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            IntPtr currentTopologyId);

        [DllImport("user32.dll")]
        private static extern int SetDisplayConfig(
            uint numPathArrayElements,
            [In] DISPLAYCONFIG_PATH_INFO[] pathArray,
            uint numModeInfoArrayElements,
            [In] DISPLAYCONFIG_MODE_INFO[] modeInfoArray,
            uint flags);

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public uint outputTechnology;
            public uint rotation;
            public uint scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public uint scanLineOrdering;
            public bool targetAvailable;
            public uint statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_MODE_INFO
        {
            public uint infoType;
            public uint id;
            public LUID adapterId;
            public DISPLAYCONFIG_TARGET_MODE targetMode;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_TARGET_MODE
        {
            public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;
            public DISPLAYCONFIG_RATIONAL hSyncFreq;
            public DISPLAYCONFIG_RATIONAL vSyncFreq;
            public DISPLAYCONFIG_2DREGION activeSize;
            public DISPLAYCONFIG_2DREGION totalSize;
            public uint videoStandard;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;
        }

        enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
        {
            UNSPECIFIED = 0,
            PROGRESSIVE = 1,
            INTERLACED = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;
        }

        public static void FlipSecondaryDisplayState()
        {
            if (!TryGetThirdMonitor(out uint idOfThirdMonitor))
            {
                ReconnectSecondaryDisplay();
            }
            else
            {
                GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
                DISPLAYCONFIG_PATH_INFO[] pathInfoArray = new DISPLAYCONFIG_PATH_INFO[pathCount];
                DISPLAYCONFIG_MODE_INFO[] modeInfoArray = new DISPLAYCONFIG_MODE_INFO[modeCount];

                QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, pathInfoArray, ref modeCount, modeInfoArray, IntPtr.Zero);

                // Remove second display
                if (pathCount < 2)
                {
                    Console.WriteLine("Only one display detected.");
                    ReconnectSecondaryDisplay();
                    return;
                }

                //DISPLAYCONFIG_PATH_INFO[] newPathArray = new DISPLAYCONFIG_PATH_INFO[] { pathInfoArray[0], pathInfoArray[1] }; // keep only primary

                pathInfoArray[idOfThirdMonitor].flags = 0;
                int result = SetDisplayConfig((uint)pathInfoArray.Length, pathInfoArray, modeCount, modeInfoArray, SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES);

                if (result != 0)
                    Console.WriteLine($"SetDisplayConfig failed: {result}");
                else
                    Console.WriteLine("Secondary display disabled.");
            }
        }

        public static bool TryGetThirdMonitor(out uint id)
        {
            id = uint.MaxValue;
            GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
            DISPLAYCONFIG_PATH_INFO[] pathInfoArray = new DISPLAYCONFIG_PATH_INFO[pathCount];
            DISPLAYCONFIG_MODE_INFO[] modeInfoArray = new DISPLAYCONFIG_MODE_INFO[modeCount];

            QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, pathInfoArray, ref modeCount, modeInfoArray, IntPtr.Zero);

            for (uint i = pathCount; i > 0; i--)
            {
                var pathInfo = pathInfoArray[i-1];
                if (modeInfoArray.Any(x => x.id == pathInfo.targetInfo.id))
                {
                    var modeInfo = modeInfoArray.First(x => x.id == pathInfo.targetInfo.id);
                    var size = modeInfo.targetMode.targetVideoSignalInfo.activeSize;
                    if (size.cx == 1920 && size.cy == 1080)
                    {
                        id = i-1;
                        return true;
                    }
                }
            }

            return false;
        }

        // Entry point: Disables the first secondary display
        public static void DisconnectSecondaryDisplay()
        {
            GetDisplayConfigBufferSizes(QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
            DISPLAYCONFIG_PATH_INFO[] pathInfoArray = new DISPLAYCONFIG_PATH_INFO[pathCount];
            DISPLAYCONFIG_MODE_INFO[] modeInfoArray = new DISPLAYCONFIG_MODE_INFO[modeCount];

            QueryDisplayConfig(QDC_ONLY_ACTIVE_PATHS, ref pathCount, pathInfoArray, ref modeCount, modeInfoArray, IntPtr.Zero);

            // Remove second display
            if (pathCount < 2)
            {
                Console.WriteLine("Only one display detected.");
                return;
            }

            //DISPLAYCONFIG_PATH_INFO[] newPathArray = new DISPLAYCONFIG_PATH_INFO[] { pathInfoArray[0], pathInfoArray[1] }; // keep only primary

            pathInfoArray[pathCount - 1].flags = 0;
            int result = SetDisplayConfig((uint)pathInfoArray.Length, pathInfoArray, modeCount, modeInfoArray, SDC_APPLY | SDC_USE_SUPPLIED_DISPLAY_CONFIG | SDC_ALLOW_CHANGES);

            if (result != 0)
                Console.WriteLine($"SetDisplayConfig failed: {result}");
            else
                Console.WriteLine("Secondary display disabled.");
        }

        public static void ReconnectSecondaryDisplay()
        {
            int result = SetDisplayConfig(0, null, 0, null, SDC_APPLY | SDC_USE_DATABASE_CURRENT);
            if (result != 0)
                Console.WriteLine($"SetDisplayConfig failed: {result}");
            else
                Console.WriteLine("Secondary display disabled.");
        }
    }

}
