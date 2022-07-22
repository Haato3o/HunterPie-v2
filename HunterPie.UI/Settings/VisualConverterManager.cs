using HunterPie.Core.Domain.Features;
using HunterPie.Core.Settings;
using HunterPie.Core.Settings.Types;
using HunterPie.UI.Controls.Settings.ViewModel;
using HunterPie.UI.Settings.Converter;
using HunterPie.UI.Settings.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using Range = HunterPie.Core.Settings.Types.Range;
using Localization = HunterPie.Core.Client.Localization.Localization;
using System.Xml;
using HunterPie.Core.Domain.Enums;
using HunterPie.Core.Client;

namespace HunterPie.UI.Settings
{
    public class VisualConverterManager
    {
        private static VisualConverterManager _instance;
        private static VisualConverterManager Instance
        {
            get 
            {
                if (_instance is null)
                    _instance = new();

                return _instance;
            }
        }

        private readonly Dictionary<Type, IVisualConverter> _converters = new Dictionary<Type, IVisualConverter>()
        {
            { typeof(bool), new BooleanVisualConverter() },
            { typeof(string), new StringVisualConverter() },
            { typeof(Range), new RangeVisualConverter() },
            { typeof(Secret), new SecretVisualConverter() },
            { typeof(IFileSelector), new FileSelectorVisualConverter() },
            { typeof(Enum), new EnumVisualConverter() },
            { typeof(Position), new PositionVisualConveter() },
            { typeof(Keybinding), new KeybindingVisualConverter() },
            { typeof(AbnormalityTrays), new AbnormalityTraysVisualConverter() },
            { typeof(Color), new ColorVisualConverter() },
        };

        private VisualConverterManager() {}

        public static ISettingElement[] Build(object settings)
        {
            List<ISettingElement> holder = new();

            Type parentType = settings.GetType();
            GameProcess currentConfiguration = ClientConfig.Config.Client.LastConfiguredGame.Value;

            foreach (PropertyInfo property in parentType.GetProperties())
            {
                Type type = property.PropertyType;
                if (type.GetInterfaces().Contains(typeof(ISettings)))
                {
                    SettingsGroup metadata = (SettingsGroup)Attribute.GetCustomAttribute(type, typeof(SettingsGroup));

                    if (metadata.DependsOnFeature is not null && 
                        !FeatureFlagManager.IsEnabled(metadata.DependsOnFeature))
                        continue;

                    if (!metadata.AvailableGames.HasFlag(currentConfiguration))
                        continue;

                    XmlNode locNode = Localization.Query($"//Strings/Client/Settings/Setting[@Id='{metadata.Name}']");
                    string title = locNode?.Attributes["String"].Value ?? metadata.Name;
                    string description = locNode?.Attributes["Description"].Value ?? metadata.Description;

                    SettingElementViewModel vm = new(title, description, metadata.Icon);

                    object parent = property.GetValue(settings);
                    BuildChildren(parent, vm, holder);

                    // Only adds panel if it has elements in it
                    if (vm.Elements.Count > 0)
                        holder.Add(vm);
                }
            }

            return holder.ToArray();
        }

        public static ISettingElementType[] BuildSubElements(object parent)
        {
            List<ISettingElementType> elements = new();

            Type parentType = parent.GetType();

            foreach (PropertyInfo prop in parentType.GetProperties())
            {
                SettingField metadata = prop.GetCustomAttribute<SettingField>();

                if (metadata is null)
                    continue;

                XmlNode locNode = Localization.Query($"//Strings/Client/Settings/Setting[@Id='{metadata.Name}']");
                string title = locNode?.Attributes["String"]?.Value ?? metadata.Name;
                string description = locNode?.Attributes["Description"]?.Value ?? metadata.Description;

                SettingElementType settingHost = new(
                    name: title,
                    description: description,
                    parent,
                    prop,
                    metadata.RequiresRestart
                );

                elements.Add(settingHost);
            }

            return elements.ToArray();
        }

        private static void BuildChildren(object parent, ISettingElement panel, List<ISettingElement> parentPanel)
        {
            Type parentType = parent.GetType();
            GameProcess currentConfiguration = ClientConfig.Config.Client.LastConfiguredGame.Value;

            foreach (PropertyInfo prop in parentType.GetProperties())
            {
                SettingField metadata = prop.GetCustomAttribute<SettingField>();

                if (prop.PropertyType.GetInterfaces().Contains(typeof(ISettings)))
                {
                    SettingsGroup meta = (SettingsGroup)Attribute.GetCustomAttribute(prop.PropertyType, typeof(SettingsGroup));

                    if (meta.DependsOnFeature is not null && 
                        !FeatureFlagManager.IsEnabled(meta.DependsOnFeature))
                        continue;

                    if (!meta.AvailableGames.HasFlag(currentConfiguration))
                        continue;

                    XmlNode locNode = Localization.Query($"//Strings/Client/Settings/Setting[@Id='{meta.Name}']");
                    string title = locNode?.Attributes["String"]?.Value ?? meta.Name;
                    string description = locNode?.Attributes["Description"]?.Value ?? meta.Description;

                    SettingElementViewModel vm = new(title, description, meta.Icon);

                    object newParent = prop.GetValue(parent);
                    BuildChildren(newParent, vm, parentPanel);

                    parentPanel.Add(vm);
                } else
                {
                    if (metadata is null)
                        continue;

                    XmlNode locNode = Localization.Query($"//Strings/Client/Settings/Setting[@Id='{metadata.Name}']");
                    string title = locNode?.Attributes["String"]?.Value ?? metadata.Name;
                    string description = locNode?.Attributes["Description"]?.Value ?? metadata.Description;

                    SettingElementType settingHost = new(
                        name: title,
                        description: description,
                        parent,
                        prop,
                        metadata.RequiresRestart
                    );

                    panel.Add(settingHost);
                }
            }

        }
             
        public static void AddConverterFor<T>(IVisualConverter converter)
        {
            if (Instance._converters.ContainsKey(typeof(T)))
                return;

            Instance._converters.Add(typeof(T), converter);
        }

        public static UIElement ConvertElement(object parent, PropertyInfo childInfo)
        {
            // In case of interfaces we can still convert property
            foreach (Type @interface in childInfo.PropertyType.GetInterfaces())
            {
                if (Instance._converters.ContainsKey(@interface))
                    return ConvertElementHelper(@interface, parent, childInfo);
            }

            Type type = childInfo.PropertyType;

            if (type.IsGenericType)
                type = type.GenericTypeArguments.FirstOrDefault();


            if (type.IsEnum)
                return ConvertElementHelper(typeof(Enum), parent, childInfo);

            if (!Instance._converters.ContainsKey(type))
                return null;

            return ConvertElementHelper(type, parent, childInfo);
        }

        private static FrameworkElement ConvertElementHelper(Type type, object parent, PropertyInfo childInfo)
        {
            FrameworkElement uiElement = Instance._converters[type].Build(parent, childInfo);
            
            uiElement.Unloaded += UICleanup;

            return uiElement;
        }

        private static void UICleanup(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.Unloaded -= UICleanup;
                BindingOperations.ClearAllBindings(element);
            }
        }
    }
}
