namespace HunterPie.Core.Game.Rise.Definitions
{
    public ref struct MHREquipmentData
    {
        public MHRWeaponRaw Weapon;
        public MHREquipmentRaw Helm;
        public MHREquipmentRaw Armor;
        public MHREquipmentRaw Gloves;
        public MHREquipmentRaw Belt;
        public MHREquipmentRaw Legs;

        public int PetalaceId;

        public int TalismanId;
        public (int, int)[] TalismanSkills;
        public int TalismanLevel;
    }
}
