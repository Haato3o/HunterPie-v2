using HunterPie.Core.Architecture;
using HunterPie.Core.Client.Configuration.Enums;
using HunterPie.Core.Domain.Enums;
using HunterPie.Core.Settings;
using HunterPie.Core.Settings.Types;

namespace HunterPie.Core.Client.Configuration.Overlay
{
    [SettingsGroup("METER_WIDGET_STRING", "ICON_METER", availableGames: GameProcess.MonsterHunterWorld | GameProcess.MonsterHunterRise)]
    public class DamageMeterWidgetConfig : IWidgetSettings, ISettings
    {
        [SettingField("INITIALIZE_WIDGET_STRING", requiresRestart: true)]
        public Observable<bool> Initialize { get; set; } = true;

        [SettingField("ENABLE_WIDGET_STRING")]
        public Observable<bool> Enabled { get; set; } = true;

        [SettingField("HIDE_WHEN_UI_VISIBLE_STRING")]
        public Observable<bool> HideWhenUiOpen { get; set; } = false;

        [SettingField("DAMAGE_METER_ENABLE_SHOULD_HIGHLIGHT_MYSELF")]
        public Observable<bool> ShouldHighlightMyself { get; set; } = false;

        [SettingField("DAMAGE_METER_ENABLE_SHOULD_BLUR_NAMES")]
        public Observable<bool> ShouldBlurNames { get; set; } = false;

        [SettingField("DAMAGE_METER_DPS_CALCULATION_STRATEGY_STRING")]

        public Observable<DPSCalculationStrategy> DpsCalculationStrategy { get; set; } = DPSCalculationStrategy.RelativeToJoin;

        [SettingField("DAMAGE_METER_DAMAGE_PLOT_STRATEGY_STRING")]
        public Observable<DamagePlotStrategy> DamagePlotStrategy { get; set; } = Enums.DamagePlotStrategy.DamagePerSecond;

        [SettingField("DAMAGE_METER_SELF_COLOR_STRING")]
        public Color PlayerSelf { get; set; } = "#FF725AC1";

        [SettingField("DAMAGE_METER_PLAYER_1_COLOR_STRING")]
        public Color PlayerFirst { get; set; } = "#FF6184D8";

        [SettingField("DAMAGE_METER_PLAYER_2_COLOR_STRING")]
        public Color PlayerSecond { get; set; } = "#FF50C5B7";

        [SettingField("DAMAGE_METER_PLAYER_3_COLOR_STRING")]
        public Color PlayerThird { get; set; } = "#FF9CEC5B";

        [SettingField("DAMAGE_METER_PLAYER_4_COLOR_STRING")]
        public Color PlayerFourth { get; set; } = "#FFF0F465";

        [SettingField("DAMAGE_METER_ENABLE_DPS_PLOT")]
        public Observable<bool> ShouldShowPlots { get; set; } = true;

        [SettingField("WIDGET_POSITION")]
        public Position Position { get; set; } = new(0, 400);

        [SettingField("WIDGET_OPACITY")]
        public Range Opacity { get; set; } = new(1, 1, 0, 0.1);

        [SettingField("WIDGET_SCALE")]
        public Range Scale { get; set; } = new(1, 2, 0, 0.1);

        [SettingField("ENABLE_STREAMER_MODE")]
        public Observable<bool> StreamerMode { get; set; } = false;
    }
}
