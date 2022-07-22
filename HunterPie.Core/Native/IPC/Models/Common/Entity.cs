using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Models.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Entity
    {
        public int Index { get; set; }
        public EntityType Type { get; set; }
    }
}
