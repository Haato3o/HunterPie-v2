using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Game;
using HunterPie.Core.Game.Client;
using HunterPie.UI.Overlay.Widgets.SpecializedTools.ViewModels;
using HunterPie.UI.Overlay.Widgets.SpecializedTools.Views;
using System;

namespace HunterPie.UI.Overlay.Widgets.SpecializedTools
{
    public class SpecializedToolWidgetContextHandler : IContextHandler
    {
        private readonly SpecializedToolViewModel ViewModel;
        private readonly SpecializedToolView View;
        private readonly Context Context;
        private readonly ISpecializedTool ToolContext;

        public SpecializedToolWidgetContextHandler(
            Context context,
            ISpecializedTool specializedTool,
            SpecializedToolWidgetConfig config
        )
        {
            View = new SpecializedToolView(config);
            WidgetManager.Register<SpecializedToolView, SpecializedToolWidgetConfig>(View);

            ViewModel = View.ViewModel;
            Context = context;
            ToolContext = specializedTool;

            HookEvents();
            UpdateData();
        }

        public void HookEvents()
        {
            Context.Game.Player.OnVillageEnter += OnVillageEnter;
            Context.Game.Player.OnVillageLeave += OnVillageLeave;
            ToolContext.OnChange += OnChange;
            ToolContext.OnCooldownUpdate += OnCooldownUpdate;
            ToolContext.OnTimerUpdate += OnTimerUpdate;
        }

        public void UnhookEvents()
        {
            Context.Game.Player.OnVillageEnter -= OnVillageEnter;
            Context.Game.Player.OnVillageLeave -= OnVillageLeave;
            ToolContext.OnChange -= OnChange;
            ToolContext.OnCooldownUpdate -= OnCooldownUpdate;
            ToolContext.OnTimerUpdate -= OnTimerUpdate;
            WidgetManager.Unregister<SpecializedToolView, SpecializedToolWidgetConfig>(View);
        }

        private void UpdateData()
        {
            ViewModel.ShouldBeVisible = Context.Game.Player.InHuntingZone;
            ViewModel.Id = ToolContext.Id;
            ViewModel.MaxCooldown = ToolContext.MaxCooldown;
            ViewModel.Cooldown = ToolContext.MaxCooldown - ToolContext.Cooldown;
            ViewModel.Timer = ToolContext.Timer;
            ViewModel.MaxTimer = ToolContext.MaxTimer;
            ViewModel.IsRecharging = ViewModel.Cooldown > 0 && ViewModel.Timer == 0;

            if (ViewModel.Cooldown == 0 && ViewModel.Timer == 0)
                ViewModel.Timer = ViewModel.MaxTimer;
        }

        #region Event handlers
        private void OnVillageEnter(object sender, EventArgs e)
        {
            ViewModel.ShouldBeVisible = false;
        }

        private void OnVillageLeave(object sender, EventArgs e)
        {
            ViewModel.ShouldBeVisible = true;
        }

        private void OnTimerUpdate(object sender, ISpecializedTool e)
        {
            ViewModel.MaxTimer = e.MaxTimer;
            ViewModel.Timer = e.Timer;
            ViewModel.IsRecharging = e.Cooldown > 0 && ViewModel.Timer == 0;
        }

        private void OnCooldownUpdate(object sender, ISpecializedTool e)
        {
            ViewModel.MaxCooldown = e.MaxCooldown;
            ViewModel.Cooldown = e.MaxCooldown - e.Cooldown;
            ViewModel.IsRecharging = e.Cooldown > 0 && ViewModel.Timer == 0;
        }

        private void OnChange(object sender, ISpecializedTool e)
        {
            UpdateData();
        }

        #endregion
    }
}
