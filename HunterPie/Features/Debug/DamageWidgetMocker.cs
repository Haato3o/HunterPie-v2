using HunterPie.Core.Client;
using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.UI.Overlay;
using HunterPie.UI.Overlay.Widgets.Damage.View;
using HunterPie.UI.Overlay.Widgets.Damage.ViewModels;

namespace HunterPie.Features.Debug
{
    internal class DamageWidgetMocker : IWidgetMocker
    {
        public void Mock()
        {
            var mockConfig = ClientConfig.Config.World.Overlay;

            if (ClientConfig.Config.Development.MockDamageWidget)
                WidgetManager.Register<MeterView, DamageMeterWidgetConfig>(
                    new MeterView(mockConfig.DamageMeterWidget)
                    {
                        DataContext = new MockMeterViewModel()
                    }
                );
        }
    }
}
