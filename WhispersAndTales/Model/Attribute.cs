using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders.Parsers;

namespace WhispersAndTales.Model
{
    public class Attribute : BaseObject, IHasProperties, ITaged
    {
        public string Tag { get; set; }
        public Type Type { get { return typeof(Attribute); } }
        public int Value { get { return (int)Properties["Value"].Value; } }
        public Dictionary<string, Property> Properties { get; set; } = new();
        public Attribute()
        {
        }
        public override string ToString()
        {
            return $"{Type}:{Tag}: {Properties["Name"]}: {Properties["Value"]} ({Properties["Description"]})";

        }
        public Attribute(Attribute other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Tag = other.Tag;

            Properties = new Dictionary<string, Property>();
            foreach (var kvp in other.Properties)
            {
                Properties[kvp.Key] = new Property(kvp.Value); // Tworzymy nową instancję Property
            }
        }
        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Attributes")
                return null;
            var builder = XmlBuilderFactory.CreateBuilder<Attribute>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            return builder;
        }
    }
}
