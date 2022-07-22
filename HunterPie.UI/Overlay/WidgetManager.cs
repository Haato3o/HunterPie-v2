using HunterPie.Core.Architecture;
using HunterPie.Core.Client.Configuration.Overlay;
using HunterPie.Core.Events;
using HunterPie.Core.Game;
using HunterPie.Core.Input;
using HunterPie.Core.Logger;
using HunterPie.Core.Settings;
using HunterPie.UI.Overlay.Components;
using System.Collections.Generic;
using System.Linq;
using ClientConfig = HunterPie.Core.Client.ClientConfig;

namespace HunterPie.UI.Overlay
{
    public class WidgetManager : Bindable
    {
        private Context _context;
        private bool _isDesignModeEnabled;
        private bool _isGameFocused;
        private bool _isGameHudOpen;
        private readonly Dictionary<IWidgetWindow, WidgetBase> _widgets = new Dictionary<IWidgetWindow, WidgetBase>();

        public bool IsDesignModeEnabled { get => _isDesignModeEnabled; private set { SetValue(ref _isDesignModeEnabled, value); } }
        public bool IsGameFocused { get => _isGameFocused; private set { SetValue(ref _isGameFocused, value); } }
        public bool IsGameHudOpen { get => _isGameHudOpen; private set { SetValue(ref _isGameHudOpen, value); } }
        public ref readonly Dictionary<IWidgetWindow, WidgetBase> Widgets => ref _widgets;
        public OverlayClientConfig Settings => ClientConfig.Config.Overlay;

        private static WidgetManager _instance;

        public static WidgetManager Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new WidgetManager();

                return _instance;
            }
        }

        private WidgetManager()
        {
            Hotkey.Register(Settings.ToggleDesignMode, ToggleDesignMode);
        }

        internal static void Hook(Context context)
        {
            Instance._context = context;
            context.Process.OnGameFocus += OnGameFocus;
            context.Process.OnGameUnfocus += OnGameUnfocus;
            context.Game.OnHudStateChange += OnHudStateChange;
        }

        private static void OnHudStateChange(object sender, IGame e) => Instance.IsGameHudOpen = e.IsHudOpen;

        private static void OnGameUnfocus(object sender, ProcessEventArgs e) => Instance.IsGameFocused = false;

        private static void OnGameFocus(object sender, ProcessEventArgs e) => Instance.IsGameFocused = true;

        public static bool Register<T, K>(T widget) where T : IWidgetWindow, IWidget<K>
                                                    where K : IWidgetSettings
        {
            if (Instance.Widgets.ContainsKey(widget))
                return false;

            WidgetBase wnd = new WidgetBase() { Widget = widget };
            Instance._widgets.Add(widget, wnd);
            wnd.Show();
            
            Log.Debug($"Added new widget: {widget.Title}");

            return true;
        }

        public static bool Unregister<T, K>(T widget) where T : IWidgetWindow, IWidget<K>
                                                      where K : IWidgetSettings
        {
            if (Instance.Widgets.ContainsKey(widget))
                return false;

            WidgetBase wnd = Instance.Widgets[widget];
            wnd.Close();
            
            return Instance._widgets.Remove(widget);
        }

        internal static void Dispose()
        {
            Instance._context.Process.OnGameFocus += OnGameFocus;
            Instance._context.Process.OnGameUnfocus += OnGameUnfocus;
            Instance._context.Game.OnHudStateChange += OnHudStateChange;
        }

        private void ToggleDesignMode()
        {
            IsDesignModeEnabled = !IsDesignModeEnabled;

            foreach (WidgetBase widget in Widgets.Values)
                widget.HandleTransparencyFlag(!IsDesignModeEnabled);
        }
    }
}
