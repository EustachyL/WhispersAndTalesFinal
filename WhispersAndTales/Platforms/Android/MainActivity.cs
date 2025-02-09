using Android.App;
using Android.Content.PM;
using Android.OS;

namespace WhispersAndTales
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static event Action OnResumedFromSettings;
        protected override void OnResume()
        {
            base.OnResume();
            OnResumedFromSettings?.Invoke();
        }
    }
}
