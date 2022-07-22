using HunterPie.Core.Native.IPC.Models;
using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Handlers.Internal.Damage.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RequestDeleteHuntStatisticsMessage
    {
        public IPCMessage Header;
        public long Target;
    }
}
