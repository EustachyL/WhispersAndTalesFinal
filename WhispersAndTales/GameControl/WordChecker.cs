using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public class WordChecker
    {
        private readonly HashSet<string> _tokenizedWords;

        // Konstruktor przyjmujący tekst do tokenizacji
        public WordChecker(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                text = "";

            _tokenizedWords = new HashSet<string>(TokenizeText(text), StringComparer.OrdinalIgnoreCase);
        }

        // Metoda sprawdzająca, czy podane słowo znajduje się w tokenizowanym tekście
        public bool ContainsWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return false;

            return _tokenizedWords.Contains(word.ToLower());
        }

        // Prywatna metoda do tokenizacji tekstu (rozdzielanie na słowa)
        private IEnumerable<string> TokenizeText(string text)
        {
            // Użycie wyrażenia regularnego do wyodrębnienia słów
            return Regex.Matches(text, @"\b\w+\b")
                        .Cast<Match>()
                        .Select(match => match.Value.ToLower());
        }
    }
}
