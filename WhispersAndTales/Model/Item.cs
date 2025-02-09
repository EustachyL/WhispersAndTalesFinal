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
    public class Item : BaseObject, IHasProperties, ITaged, IHasModifiers, IHasActions, IHasInteraction
    {

        public string Tag { get; set; }
        public Type Type { get { return typeof(Item); } }
        public Dictionary<string, Property> Properties { get; set; } = [];
        public List<Modifier> Modifiers { get; set; } = new();
        public List<GameAction> Actions { get; set; } = new();

        public Item() { }

        public event Action OnInteract;


        // Dodatkowe metody (opcjonalnie)
        public override string ToString()
        {
            return $"{Properties["Name"]}: {Properties["Description"]}";
        }
        public void Inspect()
        {
            var description = new StringBuilder();

            // Nazwa i główny opis
            description.AppendLine(GetPropertyValue("Name", "Nieznany przedmiot"));
            description.AppendLine(GetPropertyValue("Description", "Brak opisu."));

            TextToSpeechService.SpeakAsync(description.ToString().TrimEnd());
        }
        private string FormatPropertyValue(Property property)
        {
            if (property.Type == "bool" && bool.TryParse(property.Value.ToString(), out bool boolValue))
            {
                return boolValue ? "Tak" : "Nie";
            }
            return property.Value.ToString();
        }
        private string GetPropertyValue(string key, string defaultValue = "")
        {
            return Properties.TryGetValue(key, out Property prop) ? prop.Value.ToString() : defaultValue;
        }
        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Items")
                return null;
            IXmlSubElementParser<BaseObject> builder = XmlBuilderFactory.CreateBuilder<Item>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            return builder;
        }

        public void Interact(ITaged source)
        {
            if (!Properties.TryGetValue("OnUse", out Property useTypeProp)) return;

            var character = source as Character;
            if (character == null) return;

            switch (useTypeProp.Value.ToString().ToLower())
            {
                case "describe":
                    HandleDescribe();
                    break;

                case "equip":
                    HandleEquip(character);
                    break;
            }
        }

        private void HandleDescribe()
        {
            var description = new StringBuilder();
            description.AppendLine(GetPropertyValue("Name", "Nieznany przedmiot"));
            description.AppendLine(GetPropertyValue("Description", "Brak opisu."));
            TextToSpeechService.SpeakAsync(description.ToString().TrimEnd());
        }

        private void HandleEquip(Character character)
        {
            if (!Properties.TryGetValue("EquipSlot", out Property slotProp))
            {
                TextToSpeechService.SpeakAsync("Ten przedmiot nie może być założony");
                return;
            }

            var slot = slotProp.Value.ToString();

            if (!character.Equipment.ContainsKey(slot))
            {
                TextToSpeechService.SpeakAsync("Nieprawidłowy slot ekwipunku");
                return;
            }

            // Zwolnij obecny przedmiot w slocie
            var currentItem = character.Equipment[slot];
            if (currentItem != null)
            {
                character.Inventory.Add(currentItem);
                TextToSpeechService.SpeakAsync($"Zdjęto: {currentItem.Properties["Name"].Value}");
            }

            // Załóż nowy przedmiot
            character.Equipment[slot] = this;
            character.Inventory.Remove(this);

            TextToSpeechService.SpeakAsync($"Założono: {Properties["Name"].Value} na slot");
        }
    }
}
