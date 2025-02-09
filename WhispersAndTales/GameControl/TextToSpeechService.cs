using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public static class TextToSpeechService
    {
        private static readonly SemaphoreSlim _queueSemaphore = new SemaphoreSlim(1, 1);
        private static readonly Queue<(string text, SpeechOptions options)> _queue = new Queue<(string, SpeechOptions)>();
        private static readonly SemaphoreSlim _processingLock = new SemaphoreSlim(1, 1);

        // Flaga informująca, czy następuje wywoływanie mowy
        public static bool IsSpeaking { get; private set; } = false;

        public static async Task SpeakAsync(string text, string locale = "PL", float pitch = 1.0f, float volume = 1.0f)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Log.Write("Empty text in TTS");
                return;
            }

            // Pobierz dostępne języki i wybierz właściwy
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var selectedLocale = locales.FirstOrDefault(l => l.Country == locale);
            if (selectedLocale == null)
            {
                Log.Write("TTS - Language location not found");
                return;
            }

            // Ustaw opcje TTS
            var options = new SpeechOptions
            {
                Locale = selectedLocale,
                Pitch = pitch,
                Volume = volume
            };

            // Dodaj żądanie do kolejki
            await _queueSemaphore.WaitAsync();
            try
            {
                _queue.Enqueue((text, options));
            }
            finally
            {
                _queueSemaphore.Release();
            }

            // Przetwórz kolejkę (jeśli nie jest już przetwarzana)
            await ProcessQueueAsync();
        }

        private static async Task ProcessQueueAsync()
        {
            // Gwarantujemy, że tylko jeden wątek przetwarza kolejkę
            await _processingLock.WaitAsync();
            try
            {
                while (true)
                {
                    (string text, SpeechOptions options) speakRequest;

                    // Sprawdź, czy kolejka zawiera elementy
                    await _queueSemaphore.WaitAsync();
                    try
                    {
                        if (_queue.Count == 0)
                            break;
                        speakRequest = _queue.Dequeue();
                    }
                    finally
                    {
                        _queueSemaphore.Release();
                    }

                    IsSpeaking = true;
                    try
                    {
                        // Odtwarzaj tekst
                        await TextToSpeech.Default.SpeakAsync(speakRequest.text, speakRequest.options);
                        Log.Write("TTS narrator: " + speakRequest.text);
                    }
                    catch (Exception ex)
                    {
                        Log.Write($"Text-to-Speech error: {ex.Message}");
                    }
                }
            }
            finally
            {
                // Wszystkie żądania zostały przetworzone – ustaw flagę na false
                IsSpeaking = false;
                _processingLock.Release();
            }
        }
    }

}
