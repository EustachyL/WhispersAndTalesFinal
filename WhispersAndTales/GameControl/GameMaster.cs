using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Dictionary;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;
using Location = WhispersAndTales.Model.Location;
namespace WhispersAndTales.Services
{
    public class GameMaster
    {
        private readonly ISpeechToText _speechToText;
        private readonly CultureInfo _language;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _paused = false;
        private Scenario ActiveScenario;

        //private TextToSpeechService _tts = new TextToSpeechService();

        public event Action<string> OnRecognizedText; // Wydarzenie, które przekazuje rozpoznany tekst
        public event Action<string> OnPartialText;    // Wydarzenie dla częściowych wyników rozpoznawania
        public event Action<string> OnError;         // Wydarzenie w przypadku błędu
        public Dictionary<string, GameCommand> commands = [];

        public GameMaster(ISpeechToText speechToText, string language = "pl-PL")
        {
            _speechToText = speechToText ?? throw new ArgumentNullException(nameof(speechToText));
            _language = CultureInfo.GetCultureInfo(language);
            CreateBasicCommands();
            LoadCommands();
            TextToSpeechService.SpeakAsync("Aplikacja gotowa");
        }


        // Zatrzymanie nasłuchiwania
        public void StopListening()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        // Nasłuchiwanie tekstu na bieżąco
        public async Task ProcessSpeechRecognition(CancellationToken cancellationToken)
        {
            var progress = new Progress<string>(text =>
            {
                OnRecognizedText?.Invoke(text);
                CheckCommands(text);
            });
            try
            {
                // Rozpoznanie pojedynczej wypowiedzi
                await _speechToText.StartContinuousListeningAsync(_language, progress, cancellationToken);

            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Error during continuous recognition: {ex.Message}");
            }

        }
        public void CheckCommands(string text)
        {
            LoadCommands();
            if (TextToSpeechService.IsSpeaking)
            {
                Log.Write("TTS is talking, recognition blocked");
                return;
            }
            text = text.Trim().ToLower(); // Normalizacja tekstu

            foreach (var commandEntry in commands.Where(x => x.Value.Allowed))
            {
                string keyword = commandEntry.Key.ToLower();
                GameCommand command = commandEntry.Value;
                var wc = new WordChecker(text);
                if (wc.ContainsWord(keyword))
                {
                    command.Execute();
                    return; // Przerwij przetwarzanie po znalezieniu pierwszej pasującej komendy
                }
                else if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    command.Execute();
                    return;
                }
            }
        }
        public void BeginScenario()
        {
            LoadFiles();
            try
            {
                ActiveScenario = (Scenario)MainDictionary.Objectlist.First(x => x.Type == typeof(Scenario));
                ActiveScenario.StartGame();
                MainDictionary.CommandList["start"].Allowed = false;
                MainDictionary.CommandList["koniec"].Allowed = true;
                MainDictionary.CommandList["wyjdź"].Allowed = true;
            }
            catch (Exception ex)
            {
                TextToSpeechService.SpeakAsync("Nie udało się wczytać scenariusza - brak zdefiniowanych scenariuszy");
            }

        }
        public void LoadFiles()
        {
            var t = FileLoader.BuildFromFileAsync("adventure_scenario.xml");
            t.Wait();

        }
        private void Exit()
        {
            Application.Current?.Quit();
        }
        public void LoadCommands()
        {
            commands = MainDictionary.CommandList.Where(x => x.Value.Allowed == true).ToDictionary();
        }
        private void Pause()
        {
            TextToSpeechService.SpeakAsync("Pauza");
            MainDictionary.CommandList["wznów"].Allowed = true;
            _paused = true;
        }
        private void UnPause()
        {
            TextToSpeechService.SpeakAsync("Wzowiono");
            MainDictionary.CommandList["wznów"].Allowed = false;
            _paused = false;
        }
        private void EndScenario()
        {
            TextToSpeechService.SpeakAsync("Zamykam scenariusz.");
            ActiveScenario = null;
            MainDictionary.Objectlist = new();
            MainDictionary.CommandList = new();
            LoadFiles();
            CreateBasicCommands();
        }
        private void Help()
        {
            TextToSpeechService.SpeakAsync("Dostępne aktualnie polecenia to");
            foreach (var command in commands)
            {
                TextToSpeechService.SpeakAsync(command.Key);
            }
        }
        public void CreateBasicCommands()
        {
            MainDictionary.CommandList.Add("start", new GameCommand("start", BeginScenario));
            MainDictionary.CommandList.Add("pauza", new GameCommand("pauza", Pause));
            MainDictionary.CommandList.Add("wznów", new GameCommand("wznów", UnPause, false));
            MainDictionary.CommandList.Add("koniec", new GameCommand("koniec", EndScenario, false));
            MainDictionary.CommandList.Add("pomoc", new GameCommand("pomoc", Help));
            MainDictionary.CommandList.Add("wyjdź", new GameCommand("wyjdź", Exit));
        }
    }
}
