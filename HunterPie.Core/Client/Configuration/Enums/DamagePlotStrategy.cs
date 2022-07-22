using HunterPie.Core.Client.Localization;

namespace HunterPie.Core.Client.Configuration.Enums
{
    public enum DamagePlotStrategy
    {
        [Localization("//Strings/Client/Enums/Enum[@Id='PLOT_TOTAL_DAMAGE_STRING']")]
        TotalDamage,

        [Localization("//Strings/Client/Enums/Enum[@Id='PLOT_DPS_STRING']")]
        DamagePerSecond
    }
}
