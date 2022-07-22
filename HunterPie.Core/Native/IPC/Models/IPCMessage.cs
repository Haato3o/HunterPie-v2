using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IPCMessage
    {
        public IPCMessageType Type;
        public int Version;
    }
}
