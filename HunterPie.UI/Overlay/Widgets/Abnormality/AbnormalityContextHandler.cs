using HunterPie.Core.Game.Client;
using HunterPie.Core.Game.Enums;
using HunterPie.UI.Overlay.Widgets.Abnormality.ViewModel;

namespace HunterPie.UI.Overlay.Widgets.Abnormality
{
    class AbnormalityContextHandler : AbnormalityViewModel, IContextHandler
    {

        public readonly IAbnormality Context;

        public AbnormalityContextHandler(IAbnormality context)
        {
            Context = context;
            
            UpdateData();
            HookEvents();
        }

        public void HookEvents()
        {
            Context.OnTimerUpdate += OnTimerUpdate;
        }

        private void OnTimerUpdate(object sender, IAbnormality e)
        {
            if ((int)e.Timer == (int)Timer)
                return;

            MaxTimer = e.MaxTimer;
            Timer = e.Timer;
        }

        public void UnhookEvents()
        {
            Context.OnTimerUpdate -= OnTimerUpdate;
        }

        private void UpdateData()
        {
            IsBuff = Context.Type != AbnormalityType.Debuff;
            Name = Context.Id;
            Icon = Context.Icon;
            MaxTimer = Context.MaxTimer;
            Timer = Context.Timer;
            IsBuildup = Context.IsBuildup;
        }
    }
}
