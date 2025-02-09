using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;
using WhispersAndTales.Dictionary;
using Condition = WhispersAndTales.Model.Condition;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class ConditionsParser : IXmlSubElementParser
    {
        private List<Condition> _parsedConditions = new();

        public void Parse(XElement parentElement)
        {
            var conditionsElement = parentElement.Element("Conditions");
            if (conditionsElement == null) return;

            foreach (var conditionElement in conditionsElement.Elements("Condition"))
            {
                var condition = new Condition();

                // Dodaj wszystkie atrybuty XML jako właściwości
                foreach (var attr in conditionElement.Attributes())
                {
                    condition.Properties[attr.Name.LocalName] = new Property(
                    attr.Name.LocalName,
                    attr.Value,
                    "string"
                );
                }

                _parsedConditions.Add(condition);
            }
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is GameAction action)
            {
                action.Conditions.AddRange(_parsedConditions);
            }
        }
    }
}
