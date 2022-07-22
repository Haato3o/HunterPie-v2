using HunterPie.Core.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HunterPie.Core.Client.Localization
{
    public class Localization
    {

        private XmlDocument document;
        private static Localization _instance;

        public static Localization Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new();

                return _instance;
            }
        }

        private Localization()
        {
            string englishXml = Path.Combine(ClientInfo.LanguagesPath, "en-us.xml");
            document = new();
            document.Load(englishXml);

            string xmlPath = Path.Combine(ClientInfo.LanguagesPath, ClientConfig.Config.Client.Language);

            if (!File.Exists(xmlPath))
            {
                Log.Error($"Failed to find {Path.GetFileNameWithoutExtension(xmlPath)} localization");
                return;
            }
            
            try
            {
                // Merges selected custom localization with the default en-us one
                // to avoid missing strings
                if (ClientConfig.Config.Client.Language != "en-us.xml")
                {
                    XmlDocument otherLanguage = new();
                    otherLanguage.Load(xmlPath);

                    XmlNodeList englishNodes = document.DocumentElement.SelectNodes("//*");
                    foreach (XmlNode node in otherLanguage.DocumentElement.SelectNodes("//*"))
                    {
                        string id = node.Attributes["Id"]?.Value;

                        if (id is null)
                            continue;

                        string path = GetFullParentPath(node);
                        XmlNode match = document.DocumentElement.SelectSingleNode($"//{path}/*[@Id='{id}']");

                        if (match is null)
                            continue;

                        if (match.Attributes["String"] != null)
                            match.Attributes["String"].Value = node.Attributes["String"]?.Value ?? match.Attributes["String"].Value;

                        if (match.Attributes["Description"] != null)
                            match.Attributes["Description"].Value = node.Attributes["Description"]?.Value ?? match.Attributes["Description"].Value;
                    }
                }
            } catch(Exception err) { Log.Error(err.ToString()); }
            

            Log.Info($"Loaded localization {Path.GetFileNameWithoutExtension(xmlPath)}");
        }

        private static string GetFullParentPath(XmlNode node, string path = "")
        {
            if (node.ParentNode?.Name == null || node.ParentNode.Name == "#document")
                return path;

            return GetFullParentPath(node.ParentNode, $"{node.ParentNode.Name}/{path}");
        } 

        public static XmlNode Query(string query) => _instance.document.SelectSingleNode(query);
        public static XmlNodeList QueryMany(string query) => _instance.document.SelectNodes(query);
        public static string QueryString(string query) => _instance.document.SelectSingleNode(query)?.Attributes["String"]?.Value;
        public static string QueryDescription(string query) => Query(query)?.Attributes["Description"]?.Value;

        public static string GetEnumString(object enumValue)
        {
            MemberInfo memberInfo = enumValue.GetType()
                                             .GetMember(enumValue.ToString())
                                             .First();

            LocalizationAttribute? attribute = memberInfo.GetCustomAttribute<LocalizationAttribute>();

            if (attribute is null)
                return enumValue.ToString();

            return QueryString(attribute.XPath);
        }
    }
}
