using HunterPie.Core.Client.Localization;
using LocalizationService = HunterPie.Core.Client.Localization.Localization;
using HunterPie.UI.Settings.Converter;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HunterPie.UI.Assets.Application;

namespace HunterPie.UI.Settings.Internal
{
    internal class EnumVisualConverter : IVisualConverter
    {
        public DataTemplate EnumElementDataTemplate { get; } = Resources.Get<DataTemplate>("DATA_TEMPLATE_SETTINGS_ENUM_ELEMENT");

        public FrameworkElement Build(object parent, PropertyInfo childInfo)
        {
            object observable = childInfo.GetValue(parent);
            var binding = VisualConverterHelper.CreateBinding(observable);

            ObservableCollection<object> elements = new();

            foreach (object value in Enum.GetValues(childInfo.PropertyType.GenericTypeArguments[0]))
                elements.Add(value);

            ComboBox box = new()
            {
                ItemsSource = elements,
                ItemTemplate = EnumElementDataTemplate,
            };
            BindingOperations.SetBinding(box, ComboBox.SelectedItemProperty, binding);

            return box;
        }
    }
}
