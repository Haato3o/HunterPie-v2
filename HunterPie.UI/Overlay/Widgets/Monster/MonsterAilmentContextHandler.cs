using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Game.Environment;
using HunterPie.UI.Overlay.Widgets.Monster.ViewModels;

namespace HunterPie.UI.Overlay.Widgets.Monster
{
    public class MonsterAilmentContextHandler: MonsterAilmentViewModel
    {

        public readonly IMonsterAilment Context;

        public MonsterAilmentContextHandler(IMonsterAilment context, MonsterWidgetConfig config) : base(config)
        {
            Context = context;

            Update();
            HookEvents();
        }

        ~MonsterAilmentContextHandler()
        {
            UnhookEvents();
        }

        private void HookEvents()
        {
            Context.OnTimerUpdate += OnTimerUpdate;
            Context.OnBuildUpUpdate += OnBuildUpUpdate;
            Context.OnCounterUpdate += OnCounterUpdate;
        }

        private void UnhookEvents()
        {
            Context.OnTimerUpdate -= OnTimerUpdate;
            Context.OnBuildUpUpdate -= OnBuildUpUpdate;
            Context.OnCounterUpdate -= OnCounterUpdate;
        }

        private void OnCounterUpdate(object sender, IMonsterAilment e)
        {
            Count = e.Counter;
        }

        private void OnBuildUpUpdate(object sender, IMonsterAilment e)
        {
            if (e.MaxBuildUp <= 0)
                return;

            MaxBuildup = e.MaxBuildUp;
            Buildup = e.BuildUp;
        }

        private void OnTimerUpdate(object sender, IMonsterAilment e)
        {
            if (e.MaxTimer <= 0)
                return;

            MaxTimer = e.MaxTimer;
            Timer = e.Timer;
        }

        private void Update()
        {
            Name = Context.Id;
            Count = Context.Counter;
            MaxBuildup = Context.MaxBuildUp;
            Buildup = Context.BuildUp;
            MaxTimer = Context.MaxTimer;
            Timer = Context.Timer;
        }
    }
}
