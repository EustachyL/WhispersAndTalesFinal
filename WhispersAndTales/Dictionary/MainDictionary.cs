using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Model;
using WhispersAndTales.Model.Interfaces;
using Attribute = WhispersAndTales.Model.Attribute;
using Location = WhispersAndTales.Model.Location;

namespace WhispersAndTales.Dictionary
{
    static class MainDictionary
    {
        public static List<ITaged> Objectlist { get; set; } = [];
        public static Dictionary<string, GameCommand> CommandList { get; set; } = [];
        public static List<ITaged> Types { get; set; } = new List<ITaged>()
        {
           new Attribute(),
           new CharacterClass(),
           new Event(),
           new GameAction(),
           new Item(),
           new Structure(),
           new Location(),
           new Modifier(),
           new NonPlayerCharacter(),
           new Scenario(),
        };
    }
}
