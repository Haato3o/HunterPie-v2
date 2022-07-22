using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.UI.Architecture;

namespace HunterPie.UI.Overlay.Widgets.Monster.ViewModels
{
    public class MonsterAilmentViewModel : AutoVisibilityViewModel
    {
        private string _name;
        private double _timer;
        private double _maxTimer;
        private double _buildup;
        private double _maxBuildup;
        private int _count;
        
        public string Name { get => _name; set { SetValue(ref _name, value); } }
        public double Timer
        {
            get => _timer;
            set
            {
                if (value != _timer)
                    RefreshTimer();

                SetValue(ref _timer, value);
            }
        }
        public double MaxTimer { get => _maxTimer; set { SetValue(ref _maxTimer, value); } }
        public double Buildup
        {
            get => _buildup;
            set
            {
                if (value != _buildup)
                    RefreshTimer();

                SetValue(ref _buildup, value);
            }
        }
        public double MaxBuildup { get => _maxBuildup; set { SetValue(ref _maxBuildup, value); } }
        public int Count { get => _count; set { SetValue(ref _count, value); } }

        public MonsterAilmentViewModel(MonsterWidgetConfig config) : base(config.AutoHideAilmentsDelay) {}
    }
}
