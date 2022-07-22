using System.Runtime.InteropServices;

namespace HunterPie.Core.Native.IPC.Models.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EntityDamageData
    {
        public long Target { get; set; }
        public Entity Entity { get; set; }
        public float RawDamage { get; set; }
        public float ElementalDamage { get; set; }
    }
}
