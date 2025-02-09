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
    public class NonPlayerCharacter : BaseObject, IHasProperties, ITaged, IHasAttributes, IHasModifiers, ICanFight, ICanInspect, IHasInteraction
    {
        public Dictionary<string, Property> Properties { get; set; } = [];
        public CharacterClass CharacterClass { get; set; }// Odwołanie do klasy bazowej
        public List<Item> Inventory { get; set; } = [];       // Ekwipunek postaci
        public Dictionary<string, Item> Equipment { get; set; } = new();
        public List<GameAction> AvailableActions { get; set; } = [];
        public List<Modifier> ActiveModifiers { get; set; } = [];
        public bool IsPlayer { get; set; } = false;
        public event Action OnDeath;
        public event Action OnInteract;

        public string Tag { get; set; }

        public Type Type { get { return typeof(Attribute); } }

        public List<Attribute> Attributes { get; set; } = [];
        public List<Modifier> Modifiers { get; set; } = [];
        public int Hitpoints { get; set; } = 10;
        public List<GameAction> Actions { get; set; } = new();
        public bool IsAlive { get { return Hitpoints > 0; } }

        // Implementacja interfejsów
        public string Name => Properties.TryGetValue("Name", out Property prop) ? prop.Value.ToString() : "Nieznany";

        public NonPlayerCharacter() { }
        // Konstruktor inicjalizujący NPC
        public NonPlayerCharacter(CharacterClass characterClass, string name)
        {
        }
        public NonPlayerCharacter(string name)
        {
        }


        // Metoda dodająca przedmiot do ekwipunku
        public void AddItemToInventory(Item item)
        {
            Inventory.Add(item);
        }

        // Metoda dodająca akcję do listy dostępnych akcji
        public void AddAction(GameAction action)
        {
            AvailableActions.Add(action);
        }

        // Metoda dodająca modyfikator do listy modyfikatorów
        public void AddModifier(Modifier modifier)
        {
            ActiveModifiers.Add(modifier);
        }

        // Metoda wykonująca dostępne akcje
        public void PerformAction(GameAction action)
        {
            action.Execute();
        }

        public void Interact(ITaged source)
        {
            if (!IsAlive)
            {

                TextToSpeechService.SpeakAsync("Ta postać jest martwa");
                return;
            }
            if (!Properties.TryGetValue("Behaviour", out Property behaviourProp)) return;

            var behaviour = behaviourProp.Value.ToString().ToLower();

            switch (behaviour)
            {
                case "hostile":
                    HandleHostileBehaviour(source);
                    break;

                case "neutral":
                    HandleNeutralBehaviour();
                    break;

                default:
                    TextToSpeechService.SpeakAsync($"{Name} nie reaguje na Twoje działania");
                    break;
            }
        }

        private void HandleHostileBehaviour(ITaged source)
        {
            if (source is not ICanFight player) return;

            TextToSpeechService.SpeakAsync($"{Name} atakuje!");
            var combat = new CombatSystem(player, this);
            combat.StartCombat();
        }

        private void HandleNeutralBehaviour()
        {
            if (Properties.TryGetValue("HasDialog", out Property hasDialogProp)
                && bool.TryParse(hasDialogProp.Value.ToString(), out bool hasDialog)
                && hasDialog)
            {
                if (Properties.TryGetValue("Dialog", out Property dialog))
                {
                    TextToSpeechService.SpeakAsync(dialog.Value.ToString());
                }
            }
            else
            {
                TextToSpeechService.SpeakAsync($"{Name} nie chce z Tobą rozmawiać");
            }
        }

        // Dodatkowe metody (opcjonalnie)
        public override string ToString()
        {
            return $"NPC: {Properties["Name"]}, Health: {Properties["Health"]}, Mana: {Properties["Mana"]}";
        }

        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "Characters")
                return null;
            IXmlSubElementParser<BaseObject> builder = XmlBuilderFactory.CreateBuilder<NonPlayerCharacter>();

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new AttributesParser());
            builder.AddParser(new ItemsParser());
            return builder;
        }
        public void Die()
        {
            OnDeath.Invoke();
            if (Properties.TryGetValue("OnDeath", out Property eventTag))
            {
                var gameEvent = MainDictionary.Objectlist
                    .OfType<Event>()
                    .FirstOrDefault(e => e.Tag == eventTag.Value.ToString());

                gameEvent?.Trigger(this);
            }
            if (Properties.TryGetValue("Name", out Property name))
                name.Value += " - martwy";
        }

        public void Inspect()
        {
            var description = new StringBuilder();
            // Nazwa i opis NPC
            description.AppendLine($"Postać: {Name}");
            description.AppendLine(GetPropertyValue("Description", "Brak opisu."));

            // Informacje o klasie postaci
            if (CharacterClass != null)
            {
                var className = CharacterClass.Properties.TryGetValue("Name", out Property classProp)
                    ? classProp.Value.ToString()
                    : "Nieznana klasa";
                description.AppendLine($"Klasa: {className}");
            }

            // Statystyki
            description.AppendLine($"Punkty życia: {Hitpoints}");
            description.AppendLine("Atrybuty:");
            foreach (var attribute in Attributes)
            {
                var attrName = attribute.Properties.TryGetValue("Name", out Property nameProp)
                    ? nameProp.Value.ToString()
                    : "Nieznany atrybut";
                description.AppendLine($"- {attrName}: {attribute.Value}");
            }

            // Zachowanie
            if (Properties.TryGetValue("Behaviour", out Property behaviourProp))
            {
                description.AppendLine($"Zachowanie: {behaviourProp.Value}");
            }

            // Ekwipunek
            description.AppendLine("\nEkwipunek:");
            if (Equipment.Count > 0)
            {
                foreach (var slot in Equipment)
                {
                    if (slot.Value != null)
                    {
                        description.AppendLine($"- {slot.Key}: {slot.Value.Properties["Name"].Value}");
                    }
                }
            }
            else
            {
                description.AppendLine("Brak ekwipunku");
            }

            // Dialog (jeśli istnieje)
            if (Properties.TryGetValue("HasDialog", out Property hasDialogProp) &&
                bool.TryParse(hasDialogProp.Value.ToString(), out bool hasDialog) &&
                hasDialog)
            {
                description.AppendLine("\nMożliwość rozmowy");
            }

            TextToSpeechService.SpeakAsync(description.ToString().TrimEnd());
        }

        public int GetDamage()
        {
            var weapon = Equipment.TryGetValue("Weapon", out Item item) ? item : null;
            int baseDamage = 1;
            var damage = Properties.TryGetValue("Damage", out Property prop) ? prop.Value.ToString() : null;
            if (damage != null)
                baseDamage = DiceRoller.ParseAndRoll(weapon.Properties["Damage"].Value.ToString());
            if (weapon != null)
                baseDamage = DiceRoller.ParseAndRoll(weapon.Properties["Damage"].Value.ToString());
            return baseDamage;
        }

        public void DealDamage(int damage)
        {
            Hitpoints -= damage;
            if (Hitpoints <= 0) Die();
        }

        private string GetPropertyValue(string key, string defaultValue = "")
        {
            return Properties.TryGetValue(key, out Property prop) ? prop.Value.ToString() : defaultValue;
        }
    }

}
