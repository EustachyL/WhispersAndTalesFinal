using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders.Parsers;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Services;

namespace WhispersAndTales.Model
{
    public class Structure : BaseObject, IHasProperties, ITaged, IHasInteraction, ICanInspect, IHasEvents
    {
        public string Tag { get; set; }
        public Type Type { get { return typeof(Structure); } }
        public Dictionary<string, Property> Properties { get; set; } = new();
        public List<Item> StoredItems { get; set; } = new();// Lista przedmiotów przechowywanych w strukturze
        public List<Event> Events { get; set; } = new();

        public Structure() { }
        // Konstruktor inicjalizujący strukturę
        public Structure(string name, string description)
        {
        }

        public event Action OnInteract;

        // Metoda dodająca przedmiot do struktury
        public void AddItem(Item item)
        {
            StoredItems.Add(item);
        }

        // Metoda interakcji z strukturą
        public void Interact(ITaged source)
        {
            OnInteract.Invoke();
            if (!Properties.TryGetValue("OnInteract", out Property interactType))
            {
                TextToSpeechService.SpeakAsync("Nie da się nic z tym zrobić");
                return;
            }

            switch (interactType.Value.ToString().ToLower())
            {
                case "note":
                    break;

                case "giveitem":
                    HandleGiveItems(source);
                    break;

                default:
                    TextToSpeechService.SpeakAsync($"Nieznany typ interakcji: {interactType.Value}");
                    break;
            }
            HandleTriggerEvent();
        }


        private void HandleTriggerEvent()
        {
            if (Properties.TryGetValue("TriggerEvent", out Property eventTag))
            {
                var gameEvent = MainDictionary.Objectlist
                    .OfType<Event>()
                    .FirstOrDefault(e => e.Tag == eventTag.Value.ToString());

                gameEvent?.Trigger(this);
            }
        }

        private void HandleGiveItems(ITaged source)
        {
            if (source is Character character)
            {
                foreach (var item in StoredItems)
                {
                    character.Inventory.Add(item);
                    TextToSpeechService.SpeakAsync($"Otrzymano: {item.Properties["Name"].Value}");
                }
                StoredItems.Clear();
            }
        }

        // Dodatkowa metoda, by uzyskać pełny opis struktury
        public string GetDescription()
        {
            return $"{Properties["Name"]}: {Properties["Description"]}";
        }

        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Structures")
                return null;
            IXmlSubElementParser<BaseObject> builder = XmlBuilderFactory.CreateBuilder<Structure>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new ItemsParser());
            builder.AddParser(new EventsParser());
            return builder;
        }

        public void Inspect()
        {
            var description = new StringBuilder();

            // Podstawowy opis struktury
            description.AppendLine(GetPropertyValue("Name", "Nieznana struktura"));
            description.AppendLine(GetPropertyValue("Description", "Brak opisu."));

            // Informacja o przedmiotach
            description.AppendLine("\nPrzechowywane przedmioty:");
            if (StoredItems.Count > 0)
            {
                foreach (var item in StoredItems)
                {
                    var itemName = item.Properties.TryGetValue("Name", out Property nameProp)
                        ? nameProp.Value.ToString()
                        : "Nieznany przedmiot";

                    var itemDescription = item.Properties.TryGetValue("Description", out Property descProp)
                        ? descProp.Value.ToString()
                        : "Brak opisu";

                    description.AppendLine($"{itemName}: {itemDescription}");
                }
            }
            else
            {
                description.AppendLine("Brak przedmiotów");
            }

            TextToSpeechService.SpeakAsync(description.ToString().TrimEnd());
        }

        private string GetPropertyValue(string key, string defaultValue = "")
        {
            return Properties.TryGetValue(key, out Property prop) ? prop.Value.ToString() : defaultValue;
        }
    }
}
