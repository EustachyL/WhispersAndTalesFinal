using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Services;

namespace WhispersAndTales.Model
{
    public class Character : BaseObject, IHasProperties, ITaged, IHasAttributes, IHasModifiers, IHasActions, ICanFight, IHasInventory
    {
        public Dictionary<string, Property> Properties { get; set; } = [];
        public CharacterClass CharacterClass { get; set; }
        public List<Item> Inventory { get; set; } = [];
        public Dictionary<string, Item> Equipment { get; set; } = [];
        public List<GameAction> AvailableActions { get; set; } = [];
        public List<Modifier> ActiveModifiers { get; set; } = [];
        public bool IsPlayer { get; set; } = false;
        public event Action OnDeath;
        public string Tag { get; set; }

        public Type Type { get { return typeof(Attribute); } }

        public List<Attribute> Attributes { get; set; } = [];
        public List<Modifier> Modifiers { get; set; } = [];
        public int Hitpoints { get; set; } = 20;
        public List<GameAction> Actions { get; set; } = [];
        public bool IsAlive { get { return Hitpoints > 0; } }

        // Implementacja interfejsów
        public string Name => Properties.TryGetValue("Name", out Property prop) ? prop.Value.ToString() : "Nieznany";


        public Character()
        {
            Equipment = new Dictionary<string, Item>
            {
                { "Weapon", null },
                { "OffHand", null },
                { "Armour", null }
            };
        }
        // Konstruktor inicjalizujący postać
        public Character(CharacterClass characterClass, string name, bool isPlayer = false) : this()
        {
            CharacterClass = characterClass;
            Attributes = characterClass.Attributes;
            MainDictionary.CommandList.Add("postać", new GameCommand("postać", () => ReadCharacterStats()));
            MainDictionary.CommandList.Add("ekwipunek", new GameCommand("ekwipunek", () => ReadInventory()));
            Inventory = characterClass.Inventory;

            var interact = new GameCommand("użyj", (x) => { if (x is IHasInteraction) ((IHasInteraction)x).Interact(this); });
            interact.Targets = new();
            interact.Targets.AddRange(Inventory);
            interact.StageIntro = "Wybierz przedmiot do użycia";
            interact.isTwoStage = true;
            MainDictionary.CommandList.Add("użyj", interact);
        }

        public void AddItemToInventory(Item item)
        {
            Inventory.Add(item);
        }

        public void AddAction(GameAction action)
        {
            AvailableActions.Add(action);
        }
        public void ReadCharacterStats()
        {
            var stats = new StringBuilder();
            stats.AppendLine($"Statystyki postaci:");
            if (CharacterClass != null)
            {
                var className = CharacterClass.Properties.TryGetValue("Name", out Property classProp)
                    ? classProp.Value.ToString()
                    : "Nieznana klasa";

                stats.AppendLine($"Klasa postaci: {className}");
            }
            stats.AppendLine($"Punkty życia: {Hitpoints}");

            stats.AppendLine("Atrybuty:");
            foreach (var attribute in Attributes)
            {
                var attrName = attribute.Properties.TryGetValue("Name", out Property nameProp)
                    ? nameProp.Value.ToString()
                    : "Nieznany atrybut";

                stats.AppendLine($"- {attrName}: {attribute.Properties["Value"].Value.ToString()}");
            }
            TextToSpeechService.SpeakAsync(stats.ToString());

        }
        public void ReadInventory()
        {
            var stats = new StringBuilder();
            stats.AppendLine("\nEkwiunek:");
            stats.AppendLine($"Broń: {Equipment["Weapon"]?.Properties["Name"]?.Value ?? "Brak"}");
            stats.AppendLine($"Broń boczna: {Equipment["OffHand"]?.Properties["Name"]?.Value ?? "Brak"}");
            stats.AppendLine($"Zbroja: {Equipment["Armour"]?.Properties["Name"]?.Value ?? "Brak"}");

            TextToSpeechService.SpeakAsync(stats.ToString());

            if (Inventory.Count == 0)
            {
                TextToSpeechService.SpeakAsync("Twój plecak jest pusty");
                return;
            }

            var inventoryInfo = new StringBuilder();
            inventoryInfo.AppendLine("Zawartość ekwipunku:");

            foreach (var item in Inventory)
            {
                var itemName = item.Properties.TryGetValue("Name", out Property nameProp)
                    ? nameProp.Value.ToString()
                    : "Nieznany przedmiot";

                var description = item.Properties.TryGetValue("Description", out Property descProp)
                    ? descProp.Value.ToString()
                    : "Brak opisu";

                inventoryInfo.AppendLine($"- {itemName}: {description}");
            }

            TextToSpeechService.SpeakAsync(inventoryInfo.ToString());

        }
        public void Die()
        {
            OnDeath.Invoke();
            MainDictionary.Objectlist.OfType<Event>().FirstOrDefault(x => x.Tag == "Defeat")?.Trigger(this);
        }

        public void AddModifier(Modifier modifier)
        {
            ActiveModifiers.Add(modifier);
        }



        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Characters")
                return null;
            throw new NotImplementedException();
        }

        public int GetDamage()
        {
            // Przykładowa logika obliczania obrażeń
            var weapon = Equipment.TryGetValue("Weapon", out Item item) ? item : null;
            int baseDamage = weapon != null
                ? DiceRoller.ParseAndRoll(weapon.Properties["Damage"].Value.ToString())
                : 5;

            //var strength = (Attribute)Attributes.FirstOrDefault(x => x.Properties["Name"].Value.ToString() == "Strength");
            //var addVal = strength.Value;
            return baseDamage;
        }

        public void DealDamage(int damage)
        {
            Hitpoints -= damage;
            if (Hitpoints <= 0) Die();
        }
    }
}
