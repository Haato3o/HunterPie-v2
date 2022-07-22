using System;

namespace HunterPie.Core.Client.Localization
{
    public class LocalizationAttribute : Attribute
    {
        public string XPath { get; }

        public LocalizationAttribute(string xpath)
        {
            XPath = xpath;
        }
    }
}
