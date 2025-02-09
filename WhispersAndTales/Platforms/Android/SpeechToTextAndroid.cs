using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Speech;
using Android.Test;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Services;

namespace WhispersAndTales
{
    public class SpeechRecognitionListener : Java.Lang.Object, IRecognitionListener
    {
        public Action<SpeechRecognizerError> Error { get; set; }
        public Action<string> PartialResults { get; set; }
        public Action<string> Results { get; set; }

        public Action RestartListening { get; set; }
        public void OnBeginningOfSpeech()
        {
        }
        public void OnBufferReceived(byte[] buffer) { }

        public void OnEndOfSpeech()
        {
        }

        public void OnError([GeneratedEnum] SpeechRecognizerError error)
        {
            Error?.Invoke(error);
            RestartListening?.Invoke();
        }

        public void OnEvent(int eventType, Bundle @params) { }

        public void OnPartialResults(Bundle partialResults)
        {
            SendResults(partialResults, PartialResults);
        }

        public void OnReadyForSpeech(Bundle @params) { }

        public void OnResults(Bundle results)
        {
            SendResults(results, Results);
            RestartListening?.Invoke();
        }

        public void OnRmsChanged(float rmsdB) { }

        void SendResults(Bundle bundle, Action<string> action)
        {
            var matches = bundle?.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            if (matches == null || matches.Count == 0)
            {
                return;
            }

            action?.Invoke(matches.First());
        }
    }

    public class SpeechToTextImplementation : ISpeechToText
    {
        private SpeechRecognitionListener listener;
        private SpeechRecognizer speechRecognizer;
        bool wait = false;
        public async Task<string> Listen(CultureInfo culture, IProgress<string> recognitionResult, CancellationToken cancellationToken)
        {

            Log.Write("TTS Listen start");
            var taskResult = new TaskCompletionSource<string>();
            listener = new SpeechRecognitionListener
            {

                Error = ex => taskResult.TrySetException(new Exception("Failure in speech engine - " + ex)),
                PartialResults = sentence =>
                {
                    recognitionResult?.Report(sentence);
                },
                Results = sentence => taskResult.TrySetResult(sentence)

            };

            speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);

            if (speechRecognizer is null)
            {
                throw new ArgumentException("Speech recognizer is not available");
            }

            speechRecognizer.SetRecognitionListener(listener);
            speechRecognizer.StartListening(CreateSpeechIntent(culture));

            await using (cancellationToken.Register(() =>
            {
                StopRecording();
                taskResult.TrySetCanceled();
            }))
            {
                return await taskResult.Task;
            }
        }
        public async Task StartContinuousListeningAsync(CultureInfo culture, IProgress<string> recognitionResult, CancellationToken cancellationToken)
        {
            MessagingCenter.Subscribe<App>(this, "StopSpeechRecognition", (sender) =>
            {
                wait = true;
                speechRecognizer?.StopListening();
            });
            MessagingCenter.Subscribe<App>(this, "RestartSpeechRecognition", (sender) =>
            {
                wait = false;
                speechRecognizer?.StartListening(CreateSpeechIntent(culture));
            });
            Log.Write("TTS Continuous Listen start");

            var isAuthorized = await RequestPermissions();
            if (!isAuthorized)
            {
                throw new UnauthorizedAccessException("Speech recognizer permissions denied");
            }

            listener = new SpeechRecognitionListener
            {
                PartialResults = sentence => recognitionResult?.Report(sentence),
                Results = sentence =>
                {
                    recognitionResult?.Report(sentence);
                },
                Error = ex =>
                {
                    Log.Write($"Speech recognizer error: {ex}");
                },
                RestartListening = async () =>
                {
                    if (!wait)
                        if (!cancellationToken.IsCancellationRequested)
                        {

                            Log.Write("Restarting speech recognition");
                            await Task.Delay(500);
                            speechRecognizer.StartListening(CreateSpeechIntent(culture));
                        }
                }
            };

            speechRecognizer = SpeechRecognizer.CreateSpeechRecognizer(Android.App.Application.Context);
            if (speechRecognizer == null)
            {
                throw new ArgumentException("Speech recognizer is not available");
            }

            speechRecognizer.SetRecognitionListener(listener);

            // Rozpoczynamy pierwszy nasłuch
            speechRecognizer.StartListening(CreateSpeechIntent(culture));


            // Blokujemy, aby metoda nie kończyła się natychmiast
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        private void StopRecording()
        {
            Log.Write("TTS Listen end");
            speechRecognizer?.StopListening();
            speechRecognizer?.Destroy();
        }

        private Intent CreateSpeechIntent(CultureInfo culture)
        {
            var intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);

            // Wymuś użycie języka polskiego
            var javaLocale = Java.Util.Locale.ForLanguageTag(culture.Name);
            intent.PutExtra(RecognizerIntent.ExtraLanguagePreference, javaLocale);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, javaLocale);

            // Dodatkowe ustawienia językowe
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

            // Wymagania dla urządzenia
            intent.PutExtra(RecognizerIntent.ExtraCallingPackage, Android.App.Application.Context.PackageName);

            // Włączanie wyników częściowych
            intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
            // Ustawienie długiego czasu ciszy
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 15000);
            intent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 15000);



            return intent;
        }


        public async Task<bool> RequestPermissions()
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            var isAvailable = SpeechRecognizer.IsRecognitionAvailable(Android.App.Application.Context);
            return status == PermissionStatus.Granted && isAvailable;

        }
    }
}
