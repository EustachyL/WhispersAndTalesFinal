using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.Services;

namespace WhispersAndTales.Model
{
    public class CombatSystem
    {
        private readonly ICanFight _player;
        private readonly ICanFight _enemies;
        private bool _combatEnded;

        public CombatSystem(ICanFight player, ICanFight enemies)
        {
            _player = player;
            _enemies = enemies;

            // Subskrypcja eventów śmierci
            _player.OnDeath += HandlePlayerDeath;
            _enemies.OnDeath += HandleEnemyDeath;

        }

        public void StartCombat()
        {
            TextToSpeechService.SpeakAsync("Rozpoczyna się walka!");
            _combatEnded = false;

            if (!_combatEnded && _player.IsAlive && _enemies.IsAlive)
            {
                MainDictionary.CommandList.Add("atakuj", new GameCommand("atakuj", PlayerTurn));
            }
            else
                EndCombat();

        }
        private void ContinueCombat()
        {
            var targets = _enemies.IsAlive;
            if (!targets) EndCombat();

            PlayerTurn();
        }
        private void PlayerTurn()
        {
            MainDictionary.CommandList.Remove("atakuj");
            var targets = _enemies;
            TextToSpeechService.SpeakAsync($"rzuć kośćią aby uzyskać obrażenia, powiedz rzut");
            var target = targets;
            MainDictionary.CommandList["rzut"] = new GameCommand("rzut", () => { ExecuteAttack(_player, target); NPCTurn(); });
        }

        private void NPCTurn()
        {
            var targets = _enemies.IsAlive;
            if (!targets) { EndCombat(); return; }
            ExecuteAttack(_enemies, _player);

            ContinueCombat();
        }

        private void ExecuteAttack(ICanFight attacker, ICanFight target)
        {
            int damage;
            try
            {
                damage = attacker.GetDamage();
            }
            catch (Exception ex)
            {
                damage = 5;
            }

            target.DealDamage(damage);
            if (_player == attacker)
                TextToSpeechService.SpeakAsync($"atakujesz {GetName(target)} zadając {damage} obrażeń!");
            if (_player == target)
                TextToSpeechService.SpeakAsync($"jesteś atakowany przez {GetName(attacker)} zadaje ci {damage} obrażeń!");

            if (!target.IsAlive)
            {
                TextToSpeechService.SpeakAsync($"{GetName(target)} został pokonany!");
            }
        }

        private string GetName(ICanFight fighter)
        {
            if (fighter is IHasProperties tagged)
            {
                return tagged.Properties.TryGetValue("Name", out Property prop)
                    ? prop.Value.ToString()
                    : "Nieznany";
            }
            return "Nieznany";
        }

        private void HandlePlayerDeath()
        {
            TextToSpeechService.SpeakAsync("Gracz został pokonany!");
            _combatEnded = true;
        }

        private void HandleEnemyDeath()
        {
            if (!_enemies.IsAlive)
            {
                _combatEnded = true;
            }
        }

        private void EndCombat()
        {
            MainDictionary.CommandList.Remove("rzut");
            if (_player.IsAlive)
            {
                TextToSpeechService.SpeakAsync("Walka zakończona zwycięstwem!");

            }
            else
            {
                var endEvent = (Event)MainDictionary.Objectlist.FirstOrDefault(x => x.Tag == "Defeat");
                if (endEvent is Event)
                    endEvent.Trigger((ITaged)_player);
            }
        }
    }
}
