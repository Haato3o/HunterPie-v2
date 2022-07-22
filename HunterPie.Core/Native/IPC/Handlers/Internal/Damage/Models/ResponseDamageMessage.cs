using HunterPie.Core.Native.IPC.Models;
using HunterPie.Core.Native.IPC.Models.Common;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Handlers.Internal.Damage.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ResponseDamageMessage
    {
        public IPCMessage Header;
        public long Target;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public EntityDamageData[] Entities;
    }
}
