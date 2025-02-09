using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.Services;

namespace WhispersAndTales.Dictionary
{
    public class GameCommand
    {
        public bool Allowed { get; set; }
        public bool isTwoStage { get; set; } = false;
        public string StageIntro { get; set; } = "";
        public string Keyword { get; set; }
        public List<IHasProperties> Targets { get; set; }
        private Action<ITaged> _action;

        private List<string> tempCommandKeys = new List<string>();
        private Dictionary<string, GameCommand> allowedCommands;

        public GameCommand(string keyword, Action action, bool allowed = true)
        {
            Allowed = allowed;
            Keyword = keyword;
            _action = (x) => action();
        }
        public GameCommand(string keyword, Action<ITaged> action, bool allowed = true)
        {
            Allowed = allowed;
            Keyword = keyword;
            _action = action;
        }
        public void Execute()
        {
            if (isTwoStage)
            {
                StartTwoStageCommand();
                return;
            }

            if (Allowed)
            {
                _action.Invoke(null);
            }
            else
            {
                TextToSpeechService.SpeakAsync($"Komenda '{Keyword}' jest niedozwolona.");
            }
        }

        private void StartTwoStageCommand()
        {
            try
            {
                if (Targets == null || Targets.Count == 0)
                {
                    TextToSpeechService.SpeakAsync($"Brak celów dla komendy");
                    return;
                }

                allowedCommands = MainDictionary.CommandList.Where(c => c.Value.Allowed).ToDictionary(c => c.Key, c => c.Value);
                foreach (var cmd in allowedCommands)
                {
                    if (cmd.Key != "pomoc")
                        cmd.Value.Allowed = false;
                }

                TextToSpeechService.SpeakAsync(StageIntro);
                foreach (var target in Targets)
                {
                    if (target.Properties.TryGetValue("Name", out Property prop))
                    {
                        string targetName = prop.Value.ToString().ToLower();
                        MainDictionary.CommandList[targetName] = new GameCommand(targetName, () =>
                        { _action.Invoke((ITaged)target); RestoreCommands(); });
                        tempCommandKeys.Add(targetName);
                    }
                }

                MainDictionary.CommandList["wróć"] = new GameCommand("wróć", () => { TextToSpeechService.SpeakAsync(" cofnąłeś wybór "); RestoreCommands(); });
                tempCommandKeys.Add("wróć");
                foreach (var key in tempCommandKeys)
                {
                    TextToSpeechService.SpeakAsync(key);
                }
            }
            catch (Exception ex)
            {
                TextToSpeechService.SpeakAsync($"bład w komendzie: " + Keyword + "błędny cel");
            }


        }

        private void RestoreCommands()
        {
            foreach (var key in tempCommandKeys)
            {
                MainDictionary.CommandList.Remove(key);
            }
            tempCommandKeys.Clear();

            foreach (var cmd in allowedCommands)
            {
                if (cmd.Value != null)
                    cmd.Value.Allowed = true;
            }
        }
    }


}
