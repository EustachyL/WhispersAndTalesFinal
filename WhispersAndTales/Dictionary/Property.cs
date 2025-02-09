using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WhispersAndTales.Dictionary
{
    public class Property
    {
        public readonly static Dictionary<string, Func<string, object>> TypeMappings = new Dictionary<string, Func<string, object>>
        {
            { "int", value => int.Parse(value) },
            { "bool", value => bool.Parse(value) },
            { "string", value => value.ToString() }
        };
        public string Name { get; set; }
        public string Type { get; set; }
        public object Value { get; set; }

        public Property(string name, string type, string value)
        {
            Name = name;
            Type = type;
            Value = ParseValue(type, value);
        }
        public Property(Property other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            Value = other.Value; // Jeśli `Value` jest referencją, rozważ głęboką kopię
            Name = other.Name;
            Type = other.Type;
        }
        static object ParseValue(string type, string value)
        {
            if (TypeMappings.TryGetValue(type, out var parser))
            {
                return parser(value);
            }

            // Default to string if type is unknown or not specified
            return value;
        }
        public static List<Property> ReadProperties(XElement propertiesElement)
        {
            var properties = new List<Property>();

            if (propertiesElement != null)
            {
                foreach (var propertyElement in propertiesElement.Elements("Property"))
                {
                    var Name = propertyElement.Attribute("name").Value;
                    var Type = propertyElement.Attribute("type").Value;
                    var Value = propertyElement.Attribute("value").Value;
                    var property = new Property(Name, Type, Value);
                    properties.Add(property);
                }
            }

            return properties;
        }
    }

}
