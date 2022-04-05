using HunterPie.Core.Game;
using HunterPie.Core.Game.Client;
using HunterPie.Core.Logger;
using HunterPie.GUI.Parts.Sidebar.ViewModels;
using HunterPie.UI.Assets.Application;
using System.Windows.Media;

namespace HunterPie.Integrations.Build
{
    internal class BuildExporterSideBarElementViewModel : ISideBarElement
    {
        private readonly Context _context;

        public ImageSource Icon => Resources.Icon<ImageSource>("ICON_BUILD");

        public string Text => "Export Build";

        public bool IsActivable => false;

        public bool IsEnabled => true;

        public bool ShouldNotify => true;

        public void ExecuteOnClick()
        {
            IGear gear = _context.Game.Player.GetCurrentGear();

        }

        public BuildExporterSideBarElementViewModel(Context context)
        {
            _context = context;
        }
    }
}
