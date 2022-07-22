using HunterPie.Core.Client.Localization;

namespace HunterPie.Core.Client.Configuration.Enums
{
    public enum DPSCalculationStrategy
    {
        [Localization("//Strings/Client/Enums/Enum[@Id='RELATIVE_TO_QUEST_STRING']")]
        RelativeToQuest,

        [Localization("//Strings/Client/Enums/Enum[@Id='RELATIVE_TO_JOIN_STRING']")]
        RelativeToJoin,

        [Localization("//Strings/Client/Enums/Enum[@Id='RELATIVE_TO_FIRST_HIT_STRING']")]
        RelativeToFirstHit
    }
}
