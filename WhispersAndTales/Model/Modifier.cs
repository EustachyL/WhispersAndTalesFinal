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

namespace WhispersAndTales.Model
{
    public class Modifier : BaseObject, IHasProperties, ITaged
    {
        public string Tag { get; set; }
        public Type Type { get { return typeof(Modifier); } }
        public Dictionary<string, Property> Properties { get; set; } = [];

        public Modifier() { }
        // Konstruktor inicjalizujący modyfikator
        public Modifier(string name, string description, Func<object, object> effect)
        {

        }

        // Metoda wpływająca na właściwość
        public object Affect(Property property)
        {
            if (Properties.ContainsKey("Effect") && Properties["Effect"].Value is Func<object, object> effect)
            {
                var modifiedValue = effect(property.Value);
                return modifiedValue;
            }
            return property; // Jeśli brak efektu, zwraca oryginalną właściwość
        }

        // Dodatkowe metody (opcjonalnie)
        public override string ToString()
        {
            return $"{Properties["Name"]}: {Properties["Description"]}";
        }

        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Modifiers")
                return null;
            IXmlSubElementParser<BaseObject> builder = XmlBuilderFactory.CreateBuilder<Modifier>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new ConditionsParser());
            return builder;
        }
    }
}
