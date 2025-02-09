using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.Services;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class PropertiesParser : IXmlSubElementParser
    {
        private Dictionary<string, Property> _properties = new();

        public void Parse(XElement element)
        {
            _properties = new();
            foreach (var propertiesElement in element.Elements("Properties"))
            {
                foreach (var property in propertiesElement.Elements("Property"))
                {
                    try
                    {
                        string name = property.Attribute("name")?.Value
                                        ?? throw new Exception("Property is missing 'name' attribute.");
                        string type = property.Attribute("type")?.Value
                                        ?? throw new Exception("Property is missing 'type' attribute.");
                        string value = property.Attribute("value")?.Value
                                        ?? throw new Exception("Property is missing 'value' attribute.");

                        _properties[name] = new Property(name, type, value);
                    }
                    catch (Exception ex)
                    {
                        Log.Write($"Error parsing property: {ex.Message}");
                        break; // Przerywamy dalsze parsowanie w przypadku błędu
                    }
                }
            }
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is IHasProperties hasProperties)
            {
                foreach (var property in _properties)
                {
                    hasProperties.Properties[property.Key] = property.Value;
                }
            }
        }
    }
}
