using HunterPie.Core.Game.Enums;

namespace HunterPie.Core.Game.Rise.Utils
{
    public static class MHRiseUtils
    {
        public static Weapon ToWeaponId(this int self)
        {
            return self switch
            {
                0 => Weapon.Greatsword,
                1 => Weapon.SwitchAxe,
                2 => Weapon.Longsword,
                3 => Weapon.LightBowgun,
                4 => Weapon.HeavyBowgun,
                5 => Weapon.Hammer,
                6 => Weapon.GunLance,
                7 => Weapon.Lance,
                8 => Weapon.SwordAndShield,
                9 => Weapon.DualBlades,
                10 => Weapon.HuntingHorn,
                11 => Weapon.ChargeBlade,
                12 => Weapon.InsectGlaive,
                13 => Weapon.Bow,
                _ => Weapon.None,
            };
        }

        public static bool IsInQuest(this int self) => self == 2;
        public static bool IsQuestFinished(this int self) => self > 2;
        public static bool IsTrainingRoom(this int self) => self == 5;
    }
}
