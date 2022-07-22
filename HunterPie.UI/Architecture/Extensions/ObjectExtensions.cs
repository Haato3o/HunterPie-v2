using System;
using System.Linq;
using System.Windows;

namespace HunterPie.UI.Architecture.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Converts a boxed string into thickness
        /// </summary>
        /// <param name="self">The string thickness formatted</param>
        /// <returns>Thickness</returns>
        public static Thickness ToThickness(this object self)
        {
            if (self is string str)
            {
                double[] values = str.Split(',')
                    .Select(e => double.Parse(e))
                    .ToArray();

                return new Thickness(values[0], values[1], values[2], values[3]);
            }

            throw new ArgumentException("argument must be of type string");
        }
    }
}
