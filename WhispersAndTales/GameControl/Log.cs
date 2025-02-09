using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public static class Log
    {
        // Pole przechowujące odniesienie do strony głównej
        private static MainPage _mainPage;

        // Metoda inicjalizująca odniesienie do MainPage
        public static void Initialize(MainPage mainPage)
        {
            _mainPage = mainPage;
        }

        // Metoda logująca tekst
        public static void Write(string text)
        {
            if (_mainPage != null)
            {
                _mainPage.AddText(DateTime.Now+" - " + text+"\n");
            }
            else
            {
                throw new InvalidOperationException("MainPage is not initialized. Call Log.Initialize() first.");
            }
        }
    }
}
