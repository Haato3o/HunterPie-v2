using HunterPie.Core.Game.Client;


namespace HunterPie.Core.Game.Rise.Entities.Player
{
    public class MHRGear : IGear
    {
        public IEquipment Weapon { get; init; }

        public IEquipment Helm { get; init; }

        public IEquipment Armor { get; init; }

        public IEquipment Gloves { get; init; }

        public IEquipment Belt { get; init; }

        public IEquipment Legs { get; init; }

    }
}
