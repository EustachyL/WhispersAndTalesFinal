using System.Globalization;
using WhispersAndTales.FileBuilders;
using WhispersAndTales.Services;
using static Microsoft.Maui.ApplicationModel.Permissions;
using IPermission = WhispersAndTales.Services.IPermission;

namespace WhispersAndTales
{
    public partial class MainPage : ContentPage
    {
        private ISpeechToText speechToText;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();
        GameMaster gameMaster;
        private IPermission _permissionChecker;

        public Command ListenCommand { get; set; }
        public Command ListenCancelCommand { get; set; }
        public string RecognitionText { get; set; }

        public MainPage(ISpeechToText speechToText, IPermission permissions)
        {
            InitializeComponent();
            _permissionChecker = permissions;
            Log.Initialize(this);
            this.speechToText = speechToText;
            CheckPermissions();
        }
        private async void CheckPermissions()
        {
            var hasPermission = await _permissionChecker.CheckAndRequestExternalStoragePermission();

            for (int i = 0; !hasPermission; i++)
            {
                await Task.Run(async () =>
                {
                    await Task.Delay(1000);

                });
                if (i > 7)
                    break;
            }
            hasPermission = await _permissionChecker.CheckAndRequestExternalStoragePermission();
            if (!hasPermission)
            {
                Application.Current?.Quit();
            }
            else
            {
                _ = LoadMauiAsset();
                FileLoader.TryCopyFileToDocumentsAsync();
                ListenCommand = new Command(Listen);
                ListenCancelCommand = new Command(ListenCancel);
                BindingContext = this;
                gameMaster = new GameMaster(speechToText);
                gameMaster.OnRecognizedText += text => Log.Write($"Recognized: {text}");
                gameMaster.OnPartialText += text => Log.Write($"Partial: {text}");
                gameMaster.OnError += error => Log.Write($"Error: {error}");
                Task task = gameMaster.ProcessSpeechRecognition(tokenSource.Token);
            }
        }
        void InitApp()
        {


        }
        private async void Listen()
        {
            var isAuthorized = await speechToText.RequestPermissions();
            if (isAuthorized)
            {
                // await TTS.SpeakAsync("rozpoczynam słuchanie");
                try
                {
                    RecognitionText = await speechToText.Listen(CultureInfo.GetCultureInfo("pl-PL"),
                        new Progress<string>(partialText =>
                        {
                            if (DeviceInfo.Platform == DevicePlatform.Android)
                            {
                                RecognitionText = partialText;
                            }
                            else
                            {
                                RecognitionText += partialText + " ";
                            }

                            OnPropertyChanged(nameof(RecognitionText));
                        }), tokenSource.Token);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Permission Error", "No microphone access", "OK");
            }
        }

        private void ListenCancel()
        {
            tokenSource?.Cancel();
            //TTS.SpeakAsync("koniec słuchania");
        }
        private void OnCounterClicked(object sender, EventArgs e)
        {
            TextDisplay.IsVisible = !TextDisplay.IsVisible;

            SemanticScreenReader.Announce(CounterBtn.Text);
        }
        public void AddText(string newText)
        {
            if (!string.IsNullOrWhiteSpace(TextDisplay.Text))
            {
                TextDisplay.Text += Environment.NewLine;
            }
            TextDisplay.Text += newText;
        }
        async Task LoadMauiAsset()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("adventure_scenario.xml");
            if (stream != null)
            {
                using var reader = new StreamReader(stream);

                var contents = reader.ReadToEnd();
                Log.Write("załadowano plik zasobów" + "adventure_scenario.xml");
            }

        }
    }

}
