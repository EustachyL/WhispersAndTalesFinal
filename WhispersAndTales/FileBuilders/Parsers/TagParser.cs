using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model.Interfaces;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class TagParser : IXmlSubElementParser
    {
        // Przykładowe pole Tag, które będzie ustawiane
        public string Tag { get; private set; }

        public void ApplyTo<T>(T instance)
        {
            if (instance is ITaged taggedInstance)
                taggedInstance.Tag = Tag;
        }

        public void Parse(XElement element)
        {
            var tagAttribute = element?.Attribute("tag");

            if (tagAttribute != null)
            {
                Tag = tagAttribute.Value;
            }
            else
            {
                throw new Exception("Tag attribute expected but missing");
            }
        }
    }
}
