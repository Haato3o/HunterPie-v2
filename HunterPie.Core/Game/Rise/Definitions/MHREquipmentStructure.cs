using System.Runtime.InteropServices;

namespace HunterPie.Core.Game.Rise.Definitions
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MHREquipmentStructure
    {
        [FieldOffset(0x24)]
        public int Level;

        [FieldOffset(0x30)]
        public int Id;

        [FieldOffset(0x48)]
        public long RampageSkill;

        [FieldOffset(0x88)]
        public long Extras;
    }
}
