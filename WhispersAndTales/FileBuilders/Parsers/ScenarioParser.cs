using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;
using Location = WhispersAndTales.Model.Location;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class ScenarioParser : IXmlSubElementParser
    {
        private XElement scenarioElement;
        private string startLocationTag;
        private List<string> locationTags = new();

        public void Parse(XElement element)
        {
            scenarioElement = element;
            startLocationTag = element.Element("StartLocation")?.Attribute("locationTag")?.Value;
            locationTags = element.Element("Locations")?
                                .Elements("Location")
                                .Select(x => x.Attribute("tag").Value)
                                .ToList() ?? new List<string>();
        }

        public void ApplyTo<T>(T instance)
        {
            if (!(instance is Scenario scenario)) return;

            // Ustawianie podstawowych właściwości
            scenario.Tag = scenarioElement.Attribute("tag")?.Value;
            scenario.Name = scenarioElement.Attribute("name")?.Value;
            scenario.StartText = scenarioElement.Element("StartText")?.Value;

            // Wyszukiwanie lokacji w głównym słowniku
            scenario.Locations = MainDictionary.Objectlist
                .OfType<Location>()
                .Where(l => locationTags.Contains(l.Tag))
                .ToList();

            // Ustawianie startowej lokacji
            scenario.StartingLocation = scenario.Locations.FirstOrDefault(l => l.Tag == startLocationTag);

            // Parsowanie szczegółów dla każdej lokacji
            foreach (var locElement in scenarioElement.Element("Locations").Elements("Location"))
            {
                var location = scenario.Locations.FirstOrDefault(l => l.Tag == locElement.Attribute("tag").Value);
                if (location == null) continue;

                ParseLocationDetails(locElement, location, scenario);
            }
        }

        private void ParseLocationDetails(XElement locElement, Location location, Scenario scenario)
        {
            // Ładowanie postaci
            location.NPCs = locElement.Element("Characters")?
                .Elements("Character")
                .Select(x => MainDictionary.Objectlist.OfType<NonPlayerCharacter>()
                    .FirstOrDefault(c => c.Tag == x.Attribute("tag").Value))
                .Where(c => c != null)
                .ToList();

            // Ładowanie połączeń w formacie Dictionary<string, Location>
            location.ConnectedLocations = locElement.Element("ConnectedLocations")?
                .Elements("Location")
                .Select(x => new
                {
                    Direction = x.Attribute("direction")?.Value,
                    Location = scenario.Locations.FirstOrDefault(l => l.Tag == x.Attribute("tag").Value)
                })
                .Where(x => !string.IsNullOrEmpty(x.Direction) && x.Location != null)
                .ToDictionary(x => x.Direction, x => x.Location);
        }
    }
}
