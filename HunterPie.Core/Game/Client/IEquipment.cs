namespace HunterPie.Core.Game.Client
{
    public interface IEquipment
    {
        public int Id { get; }
        public int[] Decorations { get; }
        public int Level { get; }
    }
}
