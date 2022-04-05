using HunterPie.Core.Game;
using HunterPie.Domain.Interfaces;
using HunterPie.GUI.Parts.Sidebar;
using HunterPie.Integrations.Build;

namespace HunterPie.Features.Initializers
{
    internal class BuildExportInitializer : IContextInitializer
    {
        private BuildExporterSideBarElementViewModel ViewModel;

        public void Initialize(Context context)
        {
            ViewModel = new(context);
            SideBarContainer.Add(ViewModel);
        }

        public void Unload(Context context)
        {
            SideBarContainer.Remove(ViewModel);
        }
    }
}
