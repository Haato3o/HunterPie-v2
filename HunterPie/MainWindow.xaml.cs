using System.Windows;
using HunterPie.Core.Domain.Dialog;
using System.ComponentModel;
using System;
using HunterPie.Core.Logger;
using HunterPie.Internal;
using System.Windows.Input;
using HunterPie.Core.Client;
using HunterPie.Internal.Tray;
using System.Windows.Media.Animation;
using HunterPie.Features.Debug;
using HunterPie.Core.API;
using HunterPie.Core.Utils;

namespace HunterPie
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            Log.Info("Initializing HunterPie GUI");

            Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata { DefaultValue = (int)ClientConfig.Config.Client.RenderFramePerSecond.Current });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ClientConfig.Config.Client.EnableSeamlessShutdown)
            {
                NativeDialogResult result = DialogManager.Info(
                    "Confirmation", 
                    "Are you sure you want to exit HunterPie?",
                    NativeDialogButtons.Accept | NativeDialogButtons.Cancel
                );

                if (result != NativeDialogResult.Accept)
                {
                    e.Cancel = true;
                    return;
                }
            }

            AsyncHelper.RunSync(PoogieApi.EndSession);

            base.OnClosing(e);
        }

        private void OnInitialized(object sender, EventArgs e)
        {
            InitializerManager.InitializeGUI();
            
            InitializeDebugWidgets();
                       
            SetupTrayIcon();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) && e.KeyboardDevice.IsKeyDown(Key.R))
                App.Restart();
        }

        private void InitializeDebugWidgets() => DebugWidgets.MockIfNeeded();

        private void SetupTrayIcon()
        {
            TrayService.AddDoubleClickHandler(OnTrayShowClick);

            TrayService.AddItem("Show")
                .Click += OnTrayShowClick;

            TrayService.AddItem("Close")
                .Click += OnTrayCloseClick;
        }

        private void OnTrayShowClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Focus();
        }

        private void OnTrayCloseClick(object sender, EventArgs e) => Close();

        private void OnStartGameClick(object sender, EventArgs e) => Steam.RunGameBy(ClientConfig.Config.Client.DefaultGameType);
    }
}
