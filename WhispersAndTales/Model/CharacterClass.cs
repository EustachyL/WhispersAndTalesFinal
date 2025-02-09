using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders.Parsers;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Services;

namespace WhispersAndTales.Model
{
    public class CharacterClass : BaseObject, IHasProperties, ITaged, IHasAttributes, IHasInventory, IHasActions
    {
        public Dictionary<string, Property> Properties { get; set; } = new();
        public string Tag { get; set; }
        public Type Type => typeof(CharacterClass);
        public List<Item> Inventory { get; set; } = new();
        public List<Attribute> Attributes { get; set; } = new();
        public List<GameAction> Actions { get; set; } = new();
        public int Health { get; set; }

        public CharacterClass()
        {
        }


        public void Inspect()
        {
            var sb = new StringBuilder();

            // Nazwa i opis
            sb.AppendLine($"{GetPropertyValue("Name", "Nieznana klasa")}");
            sb.AppendLine(GetPropertyValue("Description", "Brak opisu"));

            // Właściwości (z pominięciem Name i Description)
            foreach (var prop in Properties
                .Where(p => p.Key != "Name" && p.Key != "Description"))
            {
                sb.AppendLine($"{prop.Key}: {FormatPropertyValue(prop.Value)}");
            }

            // Atrybuty
            sb.AppendLine("\nAtrybuty:");
            if (Attributes.Count > 0)
            {
                foreach (var attr in Attributes)
                {
                    sb.AppendLine($"- {attr.Tag}: {attr.Value} ({attr.Type})");
                }
            }
            else
            {
                sb.AppendLine("Brak");
            }

            // Ekwipunek
            sb.AppendLine("\nEkwipunek:");
            if (Inventory.Count > 0)
            {
                Inventory.ForEach(item => sb.AppendLine($"- {item}"));
            }
            else
            {
                sb.AppendLine("Brak");
            }

            // Akcje
            sb.AppendLine("\nAkcje:");
            if (Actions.Count > 0)
            {
                Actions.ForEach(action => sb.AppendLine($"- {action.Tag}"));
            }
            else
            {
                sb.AppendLine("Brak");
            }

            TextToSpeechService.SpeakAsync(sb.ToString().TrimEnd());
        }
        private string FormatPropertyValue(Property property)
        {
            if (property.Type == "bool")
            {
                return bool.Parse(property.Value.ToString()) ? "Tak" : "Nie";
            }
            return property.Value.ToString();
        }

        private string GetPropertyValue(string key, string defaultValue = "")
        {
            return Properties.TryGetValue(key, out var prop) ? prop.Value.ToString() : defaultValue;
        }
        public override string ToString()
        {
            return $"{GetPropertyValue("Name")} [{Tag}]";
        }

        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "CharacterClasses")
                return null;

            var builder = XmlBuilderFactory.CreateBuilder<CharacterClass>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new AttributesParser());
            builder.AddParser(new ItemsParser());
            builder.AddParser(new ActionsParser());

            return builder;
        }
    }


}
