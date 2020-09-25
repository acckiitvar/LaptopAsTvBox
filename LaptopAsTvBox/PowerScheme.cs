using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;

namespace LaptopAsTvBoxApp
{
    class PowerScheme
    {
        private static Guid NO_SUBGROUP_GUID = new Guid("fea3413e-7e05-4911-9a71-700331f1c294");
        private static Guid GUID_DISK_SUBGROUP = new Guid("0012ee47-9041-4b5d-9b77-535fba8b1442");
        private static Guid GUID_SYSTEM_BUTTON_SUBGROUP = new Guid("4f971e89-eebd-4455-a8de-9e59040e7347");
        private static Guid GUID_PROCESSOR_SETTINGS_SUBGROUP = new Guid("54533251-82be-4824-96c1-47b60b740d00");
        private static Guid GUID_VIDEO_SUBGROUP = new Guid("7516b95f-f776-4464-8c53-06167f40cc99");
        private static Guid GUID_BATTERY_SUBGROUP = new Guid("e73a048d-bf27-4f12-9731-8b2076e8891f");
        private static Guid GUID_SLEEP_SUBGROUP = new Guid("238C9FA8-0AAD-41ED-83F4-97BE242C8F20");
        private static Guid GUID_PCIEXPRESS_SETTINGS_SUBGROUP = new Guid("501a4d13-42af-4429-9fd1-a8218c268e20");

        private static Guid CURRENT_SUBGROUP = GUID_SYSTEM_BUTTON_SUBGROUP;

        private static Guid POWER_BUTTON_ACTION = new Guid("7648efa3-dd9c-4e3e-b566-50f929386280");
        private static Guid LID_ACTION = new Guid("5ca83367-6e45-459f-a27b-476b1d01c936");

    [DllImport("powrprof.dll")]
        static extern uint PowerEnumerate(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSetting,
            uint AccessFlags,
            uint Index,
            ref Guid Buffer,
            ref uint BufferSize);

        [DllImport("powrprof.dll")]
        static extern uint PowerGetActiveScheme(
            IntPtr UserRootPowerKey,
            ref IntPtr ActivePolicyGuid);

        [DllImport("powrprof.dll")]
        static extern uint PowerReadACValue(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            ref int Type,
            ref IntPtr Buffer,
            ref uint BufferSize
            );

        [DllImport("powrprof.dll")]
        static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            uint AcValueIndex
            );

        [DllImport("powrprof.dll")]
        static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingGuid,
            ref Guid PowerSettingGuid,
            uint AcValueIndex
            );

        [DllImport("PowrProf.dll")]
        static extern uint PowerSetActiveScheme(
          IntPtr UserRootPowerKey,
          ref Guid SchemeGuid);

        [DllImport("powrprof.dll", CharSet = CharSet.Unicode)]
        static extern uint PowerReadFriendlyName(
            IntPtr RootPowerKey,
            IntPtr SchemeGuid,
            IntPtr SubGroupOfPowerSettingGuid,
            IntPtr PowerSettingGuid,
            StringBuilder Buffer,
            ref uint BufferSize
            );

        [DllImport("kernel32.dll")]
        static extern IntPtr LocalFree(
            IntPtr hMem
            );

        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerEnumerate(IntPtr RootPowerKey, IntPtr SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, UInt32 AcessFlags, UInt32 Index, ref Guid Buffer, ref UInt32 BufferSize);

        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref UInt32 BufferSize);

        public enum AccessFlags : uint
        {
            ACCESS_SCHEME = 16,
            ACCESS_SUBGROUP = 17,
            ACCESS_INDIVIDUAL_SETTING = 18
        }

        public enum LidAction : uint
        {
            DoNothing = 0,
            Sleep = 1,
            Hibernate = 2,
            ShutDown = 3
        }

        private static string ReadFriendlyName(Guid schemeGuid)
        {
            uint sizeName = 1024;
            IntPtr pSizeName = Marshal.AllocHGlobal((int)sizeName);

            string friendlyName;

            try
            {
                PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, pSizeName, ref sizeName);
                friendlyName = Marshal.PtrToStringUni(pSizeName);
            }
            finally
            {
                Marshal.FreeHGlobal(pSizeName);
            }

            return friendlyName;
        }

        public static Guid[] PowerSchemeGetAll()
        {
            var schemeGuid = Guid.Empty;

            uint sizeSchemeGuid = (uint)Marshal.SizeOf(typeof(Guid));
            uint schemeIndex = 0;
            List<Guid> guidList = new List<Guid>();

            while (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero,
                                  (uint)AccessFlags.ACCESS_SCHEME, schemeIndex,
                                  ref schemeGuid, ref sizeSchemeGuid) == 0)
            {
                guidList.Add(schemeGuid);
                schemeIndex++;
            }
            return guidList.ToArray();
        }

        public static uint PowerSchemeGetLidAction(ref Guid powerScheme, ref LidAction value)
        {
            uint res;

            //Get the Power Settings
            Guid actionGuid = Guid.Empty;
            uint index = 0;
            uint BufferSize = Convert.ToUInt32(Marshal.SizeOf(typeof(Guid)));

            while (PowerEnumerate(IntPtr.Zero, ref powerScheme, ref GUID_SYSTEM_BUTTON_SUBGROUP,
                                  18, index, ref actionGuid, ref BufferSize) == 0) 
            {

                /* Search for lid action by Guid */
                if (actionGuid != LID_ACTION) 
                {
                    index++;
                    continue;
                }

                uint size = 4;
                IntPtr temp = IntPtr.Zero;
                int type = 0;
                res = PowerReadACValue(IntPtr.Zero, ref powerScheme, IntPtr.Zero,
                    ref actionGuid, ref type, ref temp, ref size);
                
                if (res != 0) 
                {
                    /* error handling */
                    return res;
                }

                value = (LidAction)temp;
                return 0;
            }

            return 2;
        }

        public static uint PowerSchemeSetLidAction(ref Guid powerScheme, LidAction value)
        {
            uint resDC, resAC;

            resDC = PowerWriteDCValueIndex(IntPtr.Zero, ref powerScheme,
                                           ref GUID_SYSTEM_BUTTON_SUBGROUP,
                                           ref LID_ACTION, (uint)value);

            resAC = PowerWriteACValueIndex(IntPtr.Zero, ref powerScheme,
                                           ref GUID_SYSTEM_BUTTON_SUBGROUP,
                                           ref LID_ACTION, (uint)value);

            if (resDC == 0 && resAC == 0)
                resAC = PowerSetActiveScheme(IntPtr.Zero, ref powerScheme);

            return resAC;
        }

        public static int PowerSchemeSetLidActionShell(ref Guid powerScheme, LidAction value)
        {
            StringBuilder stringBuilder = new StringBuilder("powercfg -SETACVALUEINDEX");

            stringBuilder.Append(" " + powerScheme.ToString());
            stringBuilder.Append(" " + GUID_SYSTEM_BUTTON_SUBGROUP.ToString());
            stringBuilder.Append(" " + LID_ACTION.ToString());
            stringBuilder.Append(" " + (int)value);

            Shell.ShellExecuteCommand(stringBuilder.ToString(), false);

            stringBuilder = new StringBuilder("powercfg -SETDCVALUEINDEX");

            stringBuilder.Append(" " + powerScheme.ToString());
            stringBuilder.Append(" " + GUID_SYSTEM_BUTTON_SUBGROUP.ToString());
            stringBuilder.Append(" " + LID_ACTION.ToString());
            stringBuilder.Append(" " + (int)value);

            Shell.ShellExecuteCommand(stringBuilder.ToString(), false);

            stringBuilder = new StringBuilder("powercfg -SETACTIVE");
            stringBuilder.Append(" " + powerScheme.ToString());

            Shell.ShellExecuteCommand(stringBuilder.ToString(), false);

            return 0;
        }

        public static void PowerActiveSchemeSetLidAction(LidAction lidAction)
        {
            IntPtr activeGuidPtr = IntPtr.Zero;

            if (PowerGetActiveScheme(IntPtr.Zero, ref activeGuidPtr) == 0)
            {
                var tmpGuid = (Guid)Marshal.PtrToStructure(activeGuidPtr, typeof(Guid));
                PowerSchemeSetLidAction(ref tmpGuid, lidAction);
            }
        }
    }
}
