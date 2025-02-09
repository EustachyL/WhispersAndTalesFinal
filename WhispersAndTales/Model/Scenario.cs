using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders.Parsers;
using Microsoft.Maui.Devices.Sensors;
using WhispersAndTales.Services;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.Model
{
    public class Scenario : BaseObject, ITaged, IHasEvents
    {
        public List<Location> Locations { get; set; } = [];
        public Location StartingLocation { get; set; }
        public Location CurrentLocation { get; set; }
        public List<Event> Events { get; set; } = [];
        public Character PlayerCharacter { get; set; }
        public string Name { get; set; }
        public string StartText { get; set; }

        public string Tag { set; get; }

        public Type Type => typeof(Scenario);
        public Scenario()
        {

        }

        public void AddLocation(Location location)
        {
            Locations.Add(location);
        }

        public void AddEvent(Event gameEvent)
        {
            Events.Add(gameEvent);
        }

        public void CreatePlayerCharacter(CharacterClass characterClass)
        {
            PlayerCharacter = new Character(characterClass, "player");
            PlayerCharacter.IsPlayer = true;
            TextToSpeechService.SpeakAsync($"Postać utworzona");
            ContinueStart();
        }

        public void StartGame()
        {
            TextToSpeechService.SpeakAsync($"Rozpoczynasz scenariusz: '{Name}'");
            CharacterChoice();
        }
        public void ContinueStart()
        {
            TextToSpeechService.SpeakAsync(StartText);
            ChangeCurrentLocation(StartingLocation);

        }
        public void ChangeCurrentLocation(Location newLocation)
        {
            CurrentLocation?.Exit();
            CurrentLocation = newLocation;
            newLocation.Enter(PlayerCharacter);


        }

        public void EndGame()
        {
            TextToSpeechService.SpeakAsync($"Scenariusz '{Name}' został zakończony, powiedz, koniec aby z niego wyjść.");
        }
        public void CharacterChoice()
        {
            var classes = MainDictionary.Objectlist.Where(x => x.Type == typeof(CharacterClass)).ToList(); ;
            var playerClasses = classes.Cast<IHasProperties>().Where(x => x.Properties.ContainsKey("IsPlayable"))
                .Where(x => (bool)x.Properties["IsPlayable"].Value).ToList();

            TextToSpeechService.SpeakAsync("Wybierz jedną z dostępnych klas");
            List<GameCommand> tempCommands = new();
            foreach (var c in playerClasses)
            {
                TextToSpeechService.SpeakAsync((string)c.Properties["Name"].Value);
                TextToSpeechService.SpeakAsync("Opis" + (string)c.Properties["Description"].Value);
                tempCommands.Add(new GameCommand((string)c.Properties["Name"].Value,
                    () => { CreatePlayerCharacter((CharacterClass)c); tempCommands.ForEach(x => x.Allowed = false); }));
            }
            foreach (var command in tempCommands)
            {
                MainDictionary.CommandList[command.Keyword] = command;
            }

        }


        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Scenarios")
                return null;
            IXmlSubElementParser<BaseObject> builder = XmlBuilderFactory.CreateBuilder<Scenario>();
            builder.AddParser(new ScenarioParser());
            return builder;
        }
    }
}
