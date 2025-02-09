using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Model;
using Location = WhispersAndTales.Model.Location;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class StructuresParser : IXmlSubElementParser
    {
        private List<string> _structureTags = new List<string>();

        public void Parse(XElement parentElement)
        {
            var structuresElement = parentElement.Element("Structures");
            if (structuresElement == null) return;

            // Zbierz wszystkie tagi struktur
            _structureTags = structuresElement.Elements("Structure")
                .Select(x => x.Attribute("tag")?.Value)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .ToList();
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is Location location)
            {

                // Znajdź struktury w głównym słowniku i dodaj do lokacji
                location.Structures = MainDictionary.Objectlist
                    .OfType<Structure>()
                    .Where(s => _structureTags.Contains(s.Tag))
                    .ToList();
            }
        }
    }
}