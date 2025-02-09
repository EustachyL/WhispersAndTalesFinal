using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders.Parsers;
using WhispersAndTales.Services;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.Model
{
    public class Event : BaseObject, IHasProperties, ITaged, IHasInstanceList<Event>
    {
        private static List<Event> InstanceList { get; set; } = new List<Event>();
        public string Tag { get; set; }
        public Type Type { get { return typeof(Event); } }
        public Dictionary<string, Property> Properties { get; set; } = new();
        public Action<ITaged, Event> ActionType { get; set; }
        public Event() { }

        // Konstruktor inicjalizujący wydarzenie
        public Event(string name, string description, string effect)
        {
        }

        // Metoda wywołująca efekt wydarzenia na liście obiektów
        public void Trigger(ITaged triggeringObject)
        {
            ActionType?.Invoke(triggeringObject, this);
        }


        public List<Event> GetInstanceList()
        {
            return InstanceList;
        }

        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Events")
                return null;
            var builder = XmlBuilderFactory.CreateBuilder<Event>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new EventTypeParser());
            return builder;
        }
        public class EventActions
        {

            // Metoda dla typu Narrate
            public void Narrate(ITaged triggeringObject, Event eventObj)
            {
                if (eventObj.Properties.TryGetValue("Description", out Property prop))
                {
                    TextToSpeechService.SpeakAsync(prop.Value.ToString());
                }
            }

            // Metoda dla typu AddConnection
            public void AddConnection(ITaged triggeringObject, Event eventObj)
            {
                try
                {
                    var firstTag = eventObj.Properties["FirstLocationTag"].Value.ToString();
                    var secondTag = eventObj.Properties["SecondLocationTag"].Value.ToString();
                    var direction = eventObj.Properties["Direction"].Value.ToString();

                    var firstLocation = MainDictionary.Objectlist
                        .OfType<Location>()
                        .FirstOrDefault(l => l.Tag == firstTag);

                    var secondLocation = MainDictionary.Objectlist
                        .OfType<Location>()
                        .FirstOrDefault(l => l.Tag == secondTag);

                    if (firstLocation != null && secondLocation != null)
                    {
                        firstLocation.ConnectedLocations[direction] = secondLocation;
                        TextToSpeechService.SpeakAsync(
                            $"Otworzono  nowe przejście z: {firstLocation.Properties["Name"].Value} do {secondLocation.Properties["Name"].Value}"
                        );
                    }
                }
                catch (Exception e)
                {
                    Log.Write($"Błąd podczas łączenia lokacji: {e.Message}");
                    TextToSpeechService.SpeakAsync("błąd wydarzenia, nie udało się połączyć lokacji");
                }
                Narrate(triggeringObject, eventObj);
            }

            // Istniejąca metoda EndScenario
            public void EndScenario(ITaged triggeringObject, Event eventObj)
            {
                try
                {
                    TextToSpeechService.SpeakAsync(eventObj.Properties["Description"].Value.ToString());
                    var scenario = MainDictionary.Objectlist.OfType<Scenario>().First();
                    ((Scenario)scenario).EndGame();
                }
                catch (Exception e)
                {
                    Log.Write("Could not find open scenario to end game");
                }
            }
        }
    }

}
