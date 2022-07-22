using HunterPie.Core.Game.Data.Schemas;
using HunterPie.Core.Game.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace HunterPie.Core.Game.Data
{
    public class MonsterData
    {
        private static XmlDocument _monsterXmlDocument;
        public static Dictionary<int, MonsterDataSchema> Monsters { get; private set; }
        public static Dictionary<int, AilmentDataSchema> Ailments { get; private set; }
        
        internal static void Init(string path)
        {
            _monsterXmlDocument = new();

            _monsterXmlDocument.Load(path);
            ParseAllMonsters();
            ParseAllAilments();
        }

        private static void ParseAllAilments()
        {
            XmlNodeList ailments = _monsterXmlDocument.SelectNodes("//GameData/Ailments/Ailment");

            Ailments = new(ailments.Count);
            foreach (XmlNode ailment in ailments) {
                int id = int.Parse(ailment.Attributes["Id"].Value);
                AilmentDataSchema ailm = new()
                {
                    String = ailment.Attributes["String"]?.Value
                };

                Ailments[id] = ailm;
            }
        }

        private static void ParseAllMonsters()
        {
            XmlNodeList monsters = _monsterXmlDocument.SelectNodes("//GameData/Monsters/Monster");

            Monsters = new Dictionary<int, MonsterDataSchema>(monsters.Count);
            int i = 0;
            foreach (XmlNode monster in monsters)
            {
                XmlNodeList parts = monster.SelectNodes("Parts/Part");
                MonsterPartSchema[] partsArray = new MonsterPartSchema[parts.Count];

                for (int j = 0; j < partsArray.Length; j++)
                {
                    partsArray[j] = new()
                    {
                        Id = int.Parse(parts[j].Attributes["Id"].Value),
                        String = parts[j].Attributes["String"].Value,
                        TenderizeIds = ParseTenderizeIds(parts[j].Attributes["TenderizeIds"]?.Value)
                    };

                    bool.TryParse(parts[j].Attributes["IsSeverable"]?.Value, out partsArray[j].IsSeverable);
                }

                
                MonsterSizeSchema sizeSchema = ParseSize(monster);
                Element[] weaknesses = ParseWeakness(monster);

                MonsterDataSchema schema = new()
                {
                    Id = int.Parse(monster.Attributes["Id"].Value),
                    Size = sizeSchema,
                    Parts = partsArray,
                    Weaknesses = weaknesses
                };

                Monsters.Add(schema.Id, schema);

                i++;
            }
        }

        private static uint[] ParseTenderizeIds(string? tenderizeIds)
        {
            if (tenderizeIds is not null)
                return tenderizeIds.Split(',')
                                   .Select(id => uint.Parse(id))
                                   .ToArray();

            return Array.Empty<uint>();
        }

        private static MonsterSizeSchema ParseSize(XmlNode monster)
        {
            XmlNode crowns = monster.SelectSingleNode("Crowns");

            float size = 0;
            float mini = 0.9f;
            float silver = 1.15f;
            float gold = 1.23f;

            float.TryParse(
                monster.Attributes["Size"]?.Value, 
                NumberStyles.Float,
                CultureInfo.InvariantCulture, 
                out size
            );

            if (crowns is not null)
            {

                if (crowns.Attributes["Mini"]?.Value is string miniValue)
                    float.TryParse(
                        miniValue,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out mini
                    );


                if (crowns.Attributes["Silver"]?.Value is string silverValue)
                    float.TryParse(
                        silverValue,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out silver
                    );

                if (crowns.Attributes["Gold"]?.Value is string goldValue)
                    float.TryParse(
                        goldValue,
                        NumberStyles.Float,
                        CultureInfo.InvariantCulture,
                        out gold
                    );
            }

            return new()
            {
                Size = size,
                Mini = mini,
                Silver = silver,
                Gold = gold
            };
        }
        
        private static Element[] ParseWeakness(XmlNode monster)
        {
            XmlNodeList weaknesses = monster.SelectNodes("Weaknesses/Weakness");
            Element[] elements = new Element[weaknesses.Count];

            for (int i = 0; i < weaknesses.Count; i++)
            {
                XmlNode weakness = weaknesses[i];

                Enum.TryParse(typeof(Element), weakness.Attributes["Name"]?.Value, out object element);

                elements[i] = (Element)element;
            }

            return elements;
        }

        public static MonsterDataSchema? GetMonsterData(int id)
        {
            if (!Monsters.ContainsKey(id))
                return null;

            return Monsters[id];
        }

        public static MonsterPartSchema? GetMonsterPartData(int id, int index)
        {
            if (!Monsters.ContainsKey(id))
                return null;

            if (Monsters[id].Parts.Length == 0 || index >= Monsters[id].Parts.Length)
                return null;

            return Monsters[id].Parts[index];
        }

        public static AilmentDataSchema GetAilmentData(int id)
        {
            if (!Ailments.ContainsKey(id))
                return new AilmentDataSchema() { String = $"{id}_UNKNOWN", IsUnknown = true };

            return Ailments[id];
        }
    }
}
