namespace WhispersAndTales
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }
        protected override void OnSleep()
        {
            MessagingCenter.Send(this, "StopSpeechRecognition");
        }
        protected override void OnResume()
        {
            MessagingCenter.Send(this, "RestartSpeechRecognition");
        }
    }
}
