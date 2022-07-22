using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Game.Enums;

namespace HunterPie.UI.Overlay.Widgets.Monster.ViewModels
{
    public class MockMonstersViewModel : MonstersViewModel
    {
        public MockMonstersViewModel(MonsterWidgetConfig config)
        {
            Monsters.Add(new MockBossMonsterViewModel(config) 
            {
                Name = "Monster",
                Em = "Rise_32",
                MaxHealth = 35000,
                Health = 35000,
                Stamina = 10000,
                MaxStamina = 10000,
                Crown = Crown.Gold,
                TargetType = Target.Another,
                IsTarget = false,
                IsAlive = true,
                CaptureThreshold = 0.2,
            });
            Monsters.Add(new MockBossMonsterViewModel(config)
            {
                Name = "Monster 2",
                Em = "Rise_32",
                MaxHealth = 35000,
                Health = 35000,
                Stamina = 10000,
                MaxStamina = 10000,
                Crown = Crown.Silver,
                TargetType = Target.Another,
                IsTarget = false,
                IsAlive = true,
                CaptureThreshold = 0.25,
            });
            Monsters.Add(new MockBossMonsterViewModel(config)
            {
                Name = "Monster 3",
                Em = "Rise_32",
                MaxHealth = 35000,
                Health = 35000 * 0.53,
                Stamina = 8500,
                MaxStamina = 10000,
                Crown = Crown.Mini,
                TargetType = Target.Self,
                IsTarget = true,
                IsAlive = true,
                CaptureThreshold = 0.5,
                IsCapturable = true,
            });
            VisibleMonsters = 1;
            MonstersCount = 3;

            foreach (BossMonsterViewModel vm in Monsters)
                vm.FetchMonsterIcon();
        }
    }
}
