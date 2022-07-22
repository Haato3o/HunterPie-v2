using HunterPie.Core.Game.Rise.Entities;
using HunterPie.UI.Overlay.Widgets.Wirebug.ViewModel;

namespace HunterPie.UI.Overlay.Widgets.Wirebug
{
    class WirebugContextHandler : WirebugViewModel, IContextHandler
    {
        public readonly MHRWirebug Context;

        public WirebugContextHandler(MHRWirebug context)
        {
            Context = context;

            UpdateData();
            HookEvents();
        }

        public void HookEvents()
        {
            Context.OnCooldownUpdate += OnCooldownUpdate;
            Context.OnTimerUpdate += OnTimerUpdate;
            Context.OnAvailable += OnAvailable;
        }

        public void UnhookEvents()
        {
            Context.OnCooldownUpdate -= OnCooldownUpdate;
            Context.OnTimerUpdate -= OnTimerUpdate;
            Context.OnAvailable -= OnAvailable;
        }

        private void OnTimerUpdate(object sender, MHRWirebug e)
        {
            MaxTimer = e.MaxTimer;
            Timer = e.Timer;
            IsTemporary = Context.Timer > 0;
        }

        private void OnAvailable(object sender, MHRWirebug e)
        {
            IsAvailable = e.IsAvailable;
        }

        private void OnCooldownUpdate(object sender, MHRWirebug e)
        {
            MaxCooldown = e.MaxCooldown;
            Cooldown = e.Cooldown;
            OnCooldown = Cooldown > 0;
        }

        private void UpdateData()
        {
            if (Context.MaxCooldown == 0)
                MaxCooldown = 400;
            else
                MaxCooldown = Context.MaxCooldown;
            Cooldown = Context.Cooldown;
            IsAvailable = Context.IsAvailable;

            MaxTimer = Context.MaxTimer;
            Timer = Context.Timer;
            IsTemporary = Context.Timer > 0;
        }
    }
}
