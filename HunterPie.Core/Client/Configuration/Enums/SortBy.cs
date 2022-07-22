using HunterPie.Core.Client.Localization;

namespace HunterPie.Core.Client.Configuration.Enums
{
    public enum SortBy
    {
        [Localization("//Strings/Client/Enums/Enum[@Id='SORT_BY_LOWEST_TIMER_STRING']")]
        Lowest,

        [Localization("//Strings/Client/Enums/Enum[@Id='SORT_BY_HIGHEST_TIMER_STRING']")]
        Highest,
        
        [Localization("//Strings/Client/Enums/Enum[@Id='SORT_BY_DISABLED_STRING']")]
        Off
    }
}
