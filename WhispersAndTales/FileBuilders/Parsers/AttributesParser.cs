using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;
using Attribute = WhispersAndTales.Model.Attribute;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class AttributesParser : IXmlSubElementParser
    {
        private class AttributeTemplate
        {
            public string Tag { get; set; }
            public string Type { get; set; }
            public string Value { get; set; }
        }

        private readonly List<AttributeTemplate> attributeTemplates = new();

        public void Parse(XElement parentElement)
        {
            var attributesElement = parentElement.Element("Attributes");
            if (attributesElement == null) return;

            foreach (var attrElement in attributesElement.Elements("Attribute"))
            {
                attributeTemplates.Add(new AttributeTemplate
                {
                    Tag = attrElement.Attribute("tag")?.Value,
                    Type = attrElement.Attribute("type")?.Value,
                    Value = attrElement.Attribute("value")?.Value,
                });
            }
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is not CharacterClass characterClass) return;

            foreach (var template in attributeTemplates)
            {
                // Znajdź oryginalny atrybut w globalnym słowniku
                var original = MainDictionary.Objectlist
                    .OfType<Attribute>()
                    .FirstOrDefault(a => a.Tag == template.Tag);

                if (original == null)
                {
                    // Obsługa błędu: brakującego atrybutu
                    continue;
                }

                // Utwórz głęboką kopię
                var copiedAttribute = new Attribute(original);

                // Przetwórz wartość w zależności od typu
                switch (template.Type?.ToLowerInvariant())
                {
                    case "fixed":
                        copiedAttribute.Properties["Value"].Value = template.Value;
                        break;

                    case "random":
                        try
                        {
                            copiedAttribute.Properties["Value"].Value = DiceRoller.ParseAndRoll(template.Value).ToString();
                        }
                        catch (Exception ex)
                        {
                        }
                        break;

                    default:
                        break;
                }

                characterClass.Attributes.Add(copiedAttribute);
            }
        }
    }
}
