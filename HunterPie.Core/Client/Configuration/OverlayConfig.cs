﻿using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Settings;

namespace HunterPie.Core.Client.Configuration
{
    [SettingsGroup("OVERLAY_STRING", "OVERLAY_STRING_DESC", "Icon")]
    public class OverlayConfig : ISettings
    {
        public MonsterWidgetConfig EndemicWidget { get; set; } = new MonsterWidgetConfig();
        public AbnormalityWidgetConfig AbnormalityWidget { get; set; } = new AbnormalityWidgetConfig();
    }
}