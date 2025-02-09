using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Services;
using WhispersAndTales.FileBuilders.Parsers;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.Model
{
    public class Location : BaseObject, IHasProperties, ITaged, ICanInspect
    {
        public string Tag { get; set; }
        public Type Type { get { return typeof(Location); } }
        public Dictionary<string, Property> Properties { get; set; } = new();
        public List<Structure> Structures { get; set; } = new(); // Lista struktur w tej lokacji
        public List<NonPlayerCharacter> NPCs { get; set; } = new(); // Lista NPC w tej lokacji
        public Dictionary<string, Location> ConnectedLocations { get; set; } = [];
        public bool inspected = false;
        Character player;
        public Location()
        {
        }

        // Metoda dodająca strukturę do lokacji
        public void AddStructure(Structure structure)
        {
            Structures.Add(structure);
        }
        public void Enter(Character nplayer)
        {
            player = nplayer;
            TextToSpeechService.SpeakAsync("Przechodzisz do lokacji" + Properties["Name"]?.Value);
            UpdateNPCActions(player);

        }
        public void Exit()
        {
            MainDictionary.CommandList.Remove("północ");
            MainDictionary.CommandList.Remove("południe");
            MainDictionary.CommandList.Remove("wschód");
            MainDictionary.CommandList.Remove("zachód");
            MainDictionary.CommandList.Remove("zbadaj");
            MainDictionary.CommandList.Remove("interakcja");
        }

        // Metoda dodająca NPC do lokacji
        public void AddNPC(NonPlayerCharacter npc)
        {
            NPCs.Add(npc);
        }

        // Metoda dodająca połączenie z inną lokacją
        public void ConnectToLocation(string direction, Location location)
        {
            ConnectedLocations[direction] = location;
        }
        public void UpdateMovmentOptions()
        {
            var scenario = (Scenario)MainDictionary.Objectlist.FirstOrDefault(x => x is Scenario);
            if (ConnectedLocations.ContainsKey("N"))
                MainDictionary.CommandList["północ"] = new GameCommand("północ", () => scenario.ChangeCurrentLocation(ConnectedLocations["N"]));

            if (ConnectedLocations.ContainsKey("S"))
                MainDictionary.CommandList["południe"] = new GameCommand("południe", () => scenario.ChangeCurrentLocation(ConnectedLocations["S"]));

            if (ConnectedLocations.ContainsKey("E"))
                MainDictionary.CommandList["wschód"] = new GameCommand("wschód", () => scenario.ChangeCurrentLocation(ConnectedLocations["E"]));

            if (ConnectedLocations.ContainsKey("W"))
                MainDictionary.CommandList["zachód"] = new GameCommand("zachód", () => scenario.ChangeCurrentLocation(ConnectedLocations["W"]));

        }
        public void UpdateNPCActions(Character player)
        {
            GameCommand gm;
            if (!inspected)
                gm = new GameCommand("zbadaj", () => { this.Inspect(); });
            else
            {
                UpdateMovmentOptions();
                List<ITaged> targets = new();
                targets.AddRange(Structures);
                targets.AddRange(NPCs);
                var interact = new GameCommand("interakcja", (x) => { if (x is IHasInteraction) ((IHasInteraction)x).Interact(player); });
                interact.Targets = targets.Cast<IHasProperties>().ToList();
                interact.StageIntro = "Wybierz cel dla interakcji";
                interact.isTwoStage = true;
                targets.Add(this);
                gm = new GameCommand("zbadaj", (x) => { if (x is ICanInspect) ((ICanInspect)x).Inspect(); });
                gm.Targets = targets.Cast<IHasProperties>().ToList();
                gm.StageIntro = "Wybierz cel do zbadania";
                gm.isTwoStage = true;

                MainDictionary.CommandList.Add("interakcja", interact);
            }
            MainDictionary.CommandList["zbadaj"] = gm;
        }
        public void UpdateStructureActions()
        {


        }
        public void Inspect()
        {
            inspected = true;
            TextToSpeechService.SpeakAsync($"rozglądasz się {Properties["Description"].Value}");
            if (NPCs == null)
                NPCs = new List<NonPlayerCharacter>();
            if (NPCs.Count > 0)
            {
                TextToSpeechService.SpeakAsync("Zauważasz:");
                foreach (var npc in NPCs)
                {
                    TextToSpeechService.SpeakAsync((string)npc.Properties["Name"].Value);
                }
            }
            else
            {
                TextToSpeechService.SpeakAsync("Nie ma tu nikogo");
            }

            if (Structures.Count > 0)
            {
                TextToSpeechService.SpeakAsync("Widzisz następujące struktury:");
                foreach (var structure in Structures)
                {
                    TextToSpeechService.SpeakAsync($" {structure.Properties["Name"]}");
                }
            }
            else
            {
                TextToSpeechService.SpeakAsync("Nie ma tu struktur");
            }

            if (ConnectedLocations.Count > 0)
            {
                TextToSpeechService.SpeakAsync("możesz pójść na:");
                foreach (var location in ConnectedLocations)
                {
                    switch (location.Key)
                    {
                        case "N":
                            TextToSpeechService.SpeakAsync("północ");
                            break;
                        case "E":
                            TextToSpeechService.SpeakAsync("wschód");
                            break;
                        case "W":
                            TextToSpeechService.SpeakAsync("zachód");
                            break;
                        case "S":
                            TextToSpeechService.SpeakAsync("południe");
                            break;

                    }
                }
                UpdateMovmentOptions();
            }
            else
            {
                Console.WriteLine("Nie można nigdzie pójść");
            }
            UpdateNPCActions(player);
        }

        // Dodatkowa metoda, by uzyskać pełny opis lokacji
        public string GetDescription()
        {
            return $"{Properties["Name"]}: {Properties["Description"]}";
        }

        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Locations")
                return null;
            IXmlSubElementParser<BaseObject> builder = XmlBuilderFactory.CreateBuilder<Location>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new StructuresParser());
            return builder;
        }
    }
}
