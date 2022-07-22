using HunterPie.Core.Native.IPC.Models;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Handlers.Internal.Damage.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RequestClearHuntStatisticsMessage
    {
        public IPCMessage Header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public long[] TargetsToKeep;
    }
}
