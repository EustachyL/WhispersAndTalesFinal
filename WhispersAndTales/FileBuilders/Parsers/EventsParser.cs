using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class EventsParser : IXmlSubElementParser
    {
        private List<string> _eventTags = new();

        public void Parse(XElement parentElement)
        {
            var eventsElement = parentElement.Element("Events");
            if (eventsElement == null) return;

            _eventTags = eventsElement.Elements("Event")
                .Select(x => x.Attribute("tag")?.Value)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .ToList();
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is Structure structure)
            {
                var events = MainDictionary.Objectlist
                    .OfType<Event>()
                    .Where(e => _eventTags.Contains(e.Tag))
                    .ToList();

            }
        }
    }
}
