using HunterPie.Core.Game.Client;

namespace HunterPie.Core.Game.Rise.Entities.Player
{
    public class MHREquipment : IEquipment
    {
        public int Id { get; init; }

        public int[] Decorations { get; init; }

        public int Level { get; init; }
    }
}
