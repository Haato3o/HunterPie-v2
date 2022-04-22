﻿using HunterPie.Core.Client;
using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.UI.Overlay;
using HunterPie.UI.Overlay.Widgets.Damage.View;
using HunterPie.UI.Overlay.Widgets.Damage.ViewModel;

namespace HunterPie.Features.Debug
{
    internal class DamageWidgetMocker : IWidgetMocker
    {
        public void Mock()
        {
            var mockConfig = ClientConfig.Config.Rise.Overlay;

            if (ClientConfig.Config.Development.MockDamageWidget)
                WidgetManager.Register<MeterView, DamageMeterWidgetConfig>(
                    new MeterView(mockConfig.DamageMeterWidget)
                    {
                        DataContext = new MockMeterViewModel(mockConfig.DamageMeterWidget)
                    }
                );
        }
    }
}