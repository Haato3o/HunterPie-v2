using HunterPie.Core.Architecture;
using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Game.Enums;
using HunterPie.UI.Architecture;

namespace HunterPie.UI.Overlay.Widgets.Damage.ViewModels
{
    public class PlayerViewModel : ViewModel
    {
        private DamageMeterWidgetConfig _config;
        private string _name;
        private Weapon _weapon;
        private int _damage;
        private double _dps;
        private double _percentage;
        private string _color;
        private bool _isIncreasing;
        private bool _isUser;

        public PlayerViewModel(DamageMeterWidgetConfig config) => _config = config;

        public string Name
        {
            get => _name;
            set { SetValue(ref _name, value); }
        }
        public Weapon Weapon
        {
            get => _weapon;
            set { SetValue(ref _weapon, value); }
        }
        public int Damage
        {
            get => _damage;
            set { SetValue(ref _damage, value); }
        }
        public double DPS
        {
            get => _dps;
            set { SetValue(ref _dps, value); }
        }
        public double Percentage
        {
            get => _percentage;
            set { SetValue(ref _percentage, value); }
        }
        public string Color
        {
            get => _color;
            set { SetValue(ref _color, value); }
        }
        public bool IsIncreasing
        {
            get => _isIncreasing;
            set { SetValue(ref _isIncreasing, value); }
        }
        public bool IsUser
        {
            get => _isUser;
            set { SetValue(ref _isUser, value); }
        }

        // Settings related
        public Observable<bool> ShouldHighlightMyself => _config.ShouldHighlightMyself;
        public Observable<bool> ShouldBlurNames => _config.ShouldBlurNames;

    }
}
