using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WhispersAndTales.Services;

namespace WhispersAndTales.Model
{
    public class DiceRoller
    {
        private static Random _random = new Random();

        public static int RollDie(int sides)
        {
            if (sides < 2)
                throw new ArgumentException("Kość musi mieć co najmniej dwie ścianki.");

            return _random.Next(1, sides + 1); // Rzut kością, wynik w przedziale 1..sides
        }

        public static int RollMultipleDice(int sides, int numberOfDice)
        {
            if (sides < 2)
                throw new ArgumentException("Kość musi mieć co najmniej dwie ścianki.");
            if (numberOfDice < 1)
                throw new ArgumentException("Należy rzucić co najmniej jedną kością.");
            if (numberOfDice > 1)
                TextToSpeechService.SpeakAsync("Rzucasz kośćią " + sides + " stronną, " + numberOfDice + " razy");
            else
                TextToSpeechService.SpeakAsync("Rzucasz kośćią " + sides + " stronną, " + numberOfDice + " raz");
            int total = 0;
            for (int i = 0; i < numberOfDice; i++)
            {
                total += RollDie(sides);
            }

            return total;
        }

        public static int ParseAndRoll(string diceNotation)
        {
            var regex = new Regex(@"^(\d+)D(\d+)$", RegexOptions.IgnoreCase);
            var match = regex.Match(diceNotation);

            if (!match.Success)
                throw new ArgumentException("Niepoprawny format rzutu kością.");

            // Parsowanie liczby kości i liczby ścianek
            int numberOfDice = int.Parse(match.Groups[1].Value);
            int sides = int.Parse(match.Groups[2].Value);

            // Wykonanie rzutu
            return RollMultipleDice(sides, numberOfDice);
        }
    }
}
