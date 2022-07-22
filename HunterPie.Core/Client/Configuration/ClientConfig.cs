using HunterPie.Core.Architecture;
using HunterPie.Core.Client.Configuration.Enums;
using HunterPie.Core.Domain.Enums;
using HunterPie.Core.Domain.Generics;
using HunterPie.Core.Settings;
using HunterPie.Core.Settings.Types;

namespace HunterPie.Core.Client.Configuration
{
    [SettingsGroup("CLIENT_STRING", "ICON_HUNTERPIE")]
    public class ClientConfig : ISettings
    {
        [SettingField("ENABLE_SELF_UPDATE")]
        public Observable<bool> EnableAutoUpdate { get; set; } = true;

        [SettingField("ENABLE_SELF_UPDATE_CONFIRMATION")]
        public Observable<bool> EnableAutoUpdateConfirmation { get; set; } = true;

        [SettingField("SUPPORTER_SECRET_TOKEN_STRING")]
        public Secret SupporterSecretToken { get; set; } = new();

        [SettingField("LANGUAGE_STRING", requiresRestart: true)]
        public GenericFileSelector Language { get; set; } = new GenericFileSelector("en-us.xml", "*.xml", ClientInfo.LanguagesPath);

        [SettingField("MINIMIZE_TO_SYSTEM_TRAY_STRING")]
        public Observable<bool> MinimizeToSystemTray { get; set; } = true;

        [SettingField("DEFAULT_RUN_GAME_TYPE")]
        public Observable<GameType> DefaultGameType { get; set; } = GameType.Rise;

        [SettingField("SEAMLESS_STARTUP_STRING")]
        public Observable<bool> EnableSeamlessStartup { get; set; } = false;

        [SettingField("SEAMLESS_SHUTDOWN_STRING")]
        public Observable<bool> EnableSeamlessShutdown { get; set; } = false;

        [SettingField("SHUTDOWN_ON_GAME_EXIT")]
        public Observable<bool> ShouldShutdownOnGameExit { get; set; } = false;

        [SettingField("RENDERING_STRATEGY_STRING", requiresRestart: true)]
        public Observable<RenderingStrategy> Render { get; set; } = RenderingStrategy.Software;

        [SettingField("RENDERING_FPS_STRING", requiresRestart: true)]
        public Range RenderFramePerSecond { get; set; } = new Range(60, 60, 1, 1);

        [SettingField("POLLING_RATE_STRING")]
        public Range PollingRate { get; set; } = new(100, 1000, 1, 1);

        [SettingField("DEV_ENABLE_FEATURE_FLAG", requiresRestart: true)]
        public Observable<bool> EnableFeatureFlags { get; set; } = false;

        // States
        public Observable<GameProcess> LastConfiguredGame { get; set; } = GameProcess.MonsterHunterRise;
    }
}
