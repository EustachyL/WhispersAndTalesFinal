using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class ItemsParser : IXmlSubElementParser
    {
        private List<string> itemTags = new();

        public void Parse(XElement parentElement)
        {
            var inventoryElement = parentElement.Element("Inventory");
            if (inventoryElement == null) return;

            // Zbieranie tagów przedmiotów z XML
            itemTags = inventoryElement.Elements("Item")
                .Select(x => x.Attribute("tag")?.Value)
                .Where(tag => !string.IsNullOrEmpty(tag))
                .ToList();
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is not CharacterClass characterClass) return;

            // Wyszukiwanie przedmiotów w głównym słowniku
            var items = MainDictionary.Objectlist
                .OfType<Item>()
                .Where(item => itemTags.Contains(item.Tag))
                .ToList();

            // Dodawanie przedmiotów do ekwipunku klasy postaci
            characterClass.Inventory.AddRange(items);
        }
    }
}
