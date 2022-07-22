using HunterPie.Core.Game.Enums;
using HunterPie.UI.Assets.Application;
using System;
using System.Globalization;
using System.Windows.Data;

namespace HunterPie.UI.Architecture.Converters
{
    public class ElementToIconConverter : IValueConverter
    {
        const string FIRE = "ELEMENT_FIRE";
        const string WATER = "ELEMENT_WATER";
        const string THUNDER = "ELEMENT_THUNDER";
        const string ICE = "ELEMENT_ICE";
        const string DRAGON = "ELEMENT_DRAGON";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Element element)
            {
                return element switch
                {
                    Element.Fire => Resources.Icon(FIRE),
                    Element.Water => Resources.Icon(WATER),
                    Element.Ice => Resources.Icon(ICE),
                    Element.Thunder => Resources.Icon(THUNDER),
                    Element.Dragon => Resources.Icon(DRAGON),
                    _ => throw new NotImplementedException($"element {value} not a valid element")
                };
            }

            throw new ArgumentException($"argument must be of type {typeof(Element)}");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
