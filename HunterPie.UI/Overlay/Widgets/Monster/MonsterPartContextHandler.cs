using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Game.Environment;
using HunterPie.UI.Overlay.Widgets.Monster.ViewModels;

namespace HunterPie.UI.Overlay.Widgets.Monster
{
    public class MonsterPartContextHandler : MonsterPartViewModel
    {
        public readonly IMonsterPart Context;

        public MonsterPartContextHandler(IMonsterPart context, MonsterWidgetConfig config) : base(config)
        {
            Context = context;
            Type = Context.Type;

            HookEvents();
            Update();
        }

        ~MonsterPartContextHandler()
        {
            UnhookEvents();
        }

        private void HookEvents()
        {
            Context.OnHealthUpdate += OnHealthUpdate;
            Context.OnFlinchUpdate += OnFlinchUpdate;
            Context.OnTenderizeUpdate += OnTenderizeUpdate;
            Context.OnSeverUpdate += OnSeverUpdate;
            Context.OnBreakCountUpdate += OnBreakCountUpdate;
        }

        private void UnhookEvents()
        {
            Context.OnHealthUpdate -= OnHealthUpdate;
            Context.OnFlinchUpdate -= OnFlinchUpdate;
            Context.OnTenderizeUpdate -= OnTenderizeUpdate;
            Context.OnSeverUpdate -= OnSeverUpdate;
            Context.OnBreakCountUpdate -= OnBreakCountUpdate;
        }

        private void OnSeverUpdate(object sender, IMonsterPart e)
        {
            MaxSever = e.MaxSever;
            Sever = e.Sever;

            IsPartSevered = MaxSever == Sever && (Breaks > 0 || Flinch != MaxFlinch);
        }

        private void OnTenderizeUpdate(object sender, IMonsterPart e)
        {
            Tenderize = e.MaxTenderize - e.Tenderize;
            MaxTenderize = e.MaxTenderize;
        }

        private void OnBreakCountUpdate(object sender, IMonsterPart e)
        {
            Breaks = e.Count;
        }

        private void OnFlinchUpdate(object sender, IMonsterPart e)
        {
            if (Flinch < e.Flinch && MaxFlinch > 0)
                Breaks++;

            MaxFlinch = e.MaxFlinch;
            Flinch = e.Flinch;

            IsPartBroken = MaxHealth <= 0 || Health == MaxHealth && (Breaks > 0 || Flinch != MaxFlinch);
            IsPartSevered = MaxSever == Sever && (Breaks > 0 || Flinch != MaxFlinch);
        }

        private void OnHealthUpdate(object sender, IMonsterPart e)
        {
            MaxHealth = e.MaxHealth;
            Health = e.Health;

            IsPartBroken = MaxHealth <= 0 || Health == MaxHealth && (Breaks > 0 || Flinch != MaxFlinch);
        }

        private void Update()
        {
            Name = Context.Id;
            IsKnownPart = Context.Id != "PART_UNKNOWN";

            MaxHealth = Context.MaxHealth;
            Health = Context.Health;
            MaxFlinch = Context.MaxFlinch;
            Flinch = Context.Flinch;
            MaxSever = Context.MaxSever;
            Sever = Context.Sever;

            IsPartSevered = MaxSever == Sever && (Breaks > 0 || Flinch != MaxFlinch);
            IsPartBroken = MaxHealth <= 0 || (Health == MaxHealth && (Breaks > 0 || Flinch != MaxFlinch));

        }
    }
}
