namespace HunterPie.Core.Game.Client
{
    public interface IGear
    {
        public IEquipment Weapon { get; }
        public IEquipment Helm { get; }
        public IEquipment Armor { get; }
        public IEquipment Gloves { get; }
        public IEquipment Belt { get; }
        public IEquipment Legs { get; }

    }
}
