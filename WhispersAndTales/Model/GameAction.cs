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
    public class GameAction : BaseObject, IHasProperties, ITaged, IHasConditions
    {
        public string Tag { get; set; }
        private string _keyWord { get; set; }
        private ITaged _source { get; set; }
        private ITaged _target { get; set; }
        public Type Type { get { return typeof(GameAction); } }
        public Dictionary<string, Property> Properties { get; set; } = new();
        public Action<ITaged, ITaged> Action { get; private set; }
        public List<Condition> Conditions { get; set; } = new();

        public GameAction() { }

        public void Execute()
        {
            if (Action != null)
            {
                Action(_source, _target);
            }
            else
            {
                TextToSpeechService.SpeakAsync("Akcja jest niedostępna lub niezdefiniowana");
            }
        }
        public override string ToString()
        {
            return $"{Properties["Name"]}: {Properties["Description"]}";
        }
        public GameCommand GetCommands(Scenario scenario)
        {
            return new GameCommand(_keyWord, () => Execute());
        }
        public void Execute(ITaged source, ITaged target)
        {
            if (Action != null)
            {
                Action(source, target);
            }
            else
            {
                throw new InvalidOperationException("Action is not defined.");
            }
        }
        public IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement)
        {
            if (rootElement.Name.LocalName != "GameActions")
                return null;
            var builder = XmlBuilderFactory.CreateBuilder<GameAction>(); ;

            builder.AddParser(new TagParser());
            builder.AddParser(new PropertiesParser());
            builder.AddParser(new ConditionsParser());
            return builder;
        }
        public int RollDice(string diceNotation)
        {
            var parts = diceNotation.Split('D');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int numberOfDice) || !int.TryParse(parts[1], out int diceSides))
            {
                return 0;
            }

            var random = new Random();
            int total = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                total += random.Next(1, diceSides + 1);
            }
            return total;
        }
        public void MeleeAttack(ITaged source, ITaged target)
        {
            _keyWord = "atakuj";
        }

        public void PickUp(ITaged source, ITaged target)
        {
            _keyWord = "podnieś";
        }

        public void DropItem(ITaged source, ITaged target)
        {
            _keyWord = "porzuć";
        }

    }
}
