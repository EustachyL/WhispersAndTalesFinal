using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Model;
using WhispersAndTales.Services;

namespace WhispersAndTales.FileBuilders.Parsers
{
    public class EventTypeParser : IXmlSubElementParser
    {
        private string _eventType;

        public void Parse(XElement element)
        {
            _eventType = element.Attribute("type")?.Value?.ToLower();
        }

        public void ApplyTo<T>(T instance)
        {
            if (instance is Event eventInstance)
            {
                var actions = new Event.EventActions();

                switch (_eventType)
                {
                    case "narrate":
                        eventInstance.ActionType = actions.Narrate;
                        break;

                    case "addconnection":
                        eventInstance.ActionType = actions.AddConnection;
                        break;

                    case "endscenario":
                        eventInstance.ActionType = actions.EndScenario;
                        break;

                    default:
                        Log.Write($"Nieznany typ zdarzenia: {_eventType}");
                        TextToSpeechService.SpeakAsync($"Nieznany typ zdarzenia: {_eventType}");
                        break;
                }
            }
        }
    }
}
