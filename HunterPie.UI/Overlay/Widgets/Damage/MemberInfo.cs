using HunterPie.UI.Overlay.Widgets.Damage.ViewModels;
using LiveCharts.Wpf;

namespace HunterPie.UI.Overlay.Widgets.Damage
{
    class MemberInfo
    {
        public PlayerViewModel ViewModel { get; init; }
        public Series Series { get; init; }
        public double JoinedAt { get; init; }
        public double FirstHitAt { get; set; } = -1;
    }
}
