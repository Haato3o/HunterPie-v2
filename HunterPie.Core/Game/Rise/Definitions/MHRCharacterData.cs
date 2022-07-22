using System.Runtime.InteropServices;

namespace HunterPie.Core.Game.Rise.Definitions
{
    [StructLayout(LayoutKind.Explicit)]
    public struct MHRCharacterData
    {
        [FieldOffset(0x18)]
        public long NamePointer;

        [FieldOffset(0x38)]
        public int HighRank;
    }
}
