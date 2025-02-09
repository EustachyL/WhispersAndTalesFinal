using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Provider;
using Uri = Android.Net.Uri;
using Environment = Android.OS.Environment;
using WhispersAndTales.Services;
using static Android.App.Application;
using Android.App;

namespace WhispersAndTales
{
    public class AndroidPermissionChecker : IPermission
    {
        private TaskCompletionSource<bool> _tcsPermission;
        private ActivityLifecycleListener _lifecycleListener;

        public async Task<bool> CheckAndRequestExternalStoragePermission()
        {
            // Jeśli wersja Android jest niższa niż R (Android 11) lub pozwolenie jest już przyznane
            if (Build.VERSION.SdkInt < BuildVersionCodes.R || Environment.IsExternalStorageManager)
            {
                return true;
            }

            _tcsPermission = new TaskCompletionSource<bool>();
            _lifecycleListener = new ActivityLifecycleListener(OnActivityResumed);
            Platform.CurrentActivity.Application.RegisterActivityLifecycleCallbacks(_lifecycleListener);

            try
            {
                var intent = new Intent(Settings.ActionManageAppAllFilesAccessPermission);
                var uri = Uri.Parse("package:" + AppInfo.Current.PackageName);
                intent.SetData(uri);
                Platform.CurrentActivity.StartActivity(intent);

                // Dodaj timeout na wypadek, gdyby użytkownik nie wrócił do aplikacji
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30)); // 30 sekund timeout
                var completedTask = await Task.WhenAny(_tcsPermission.Task, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    // Użytkownik nie wrócił w wyznaczonym czasie
                    _tcsPermission.TrySetResult(false);
                }

                return await _tcsPermission.Task;
            }
            finally
            {
                // Sprzątanie
                Platform.CurrentActivity.Application.UnregisterActivityLifecycleCallbacks(_lifecycleListener);
                _lifecycleListener = null;
            }
        }

        private void OnActivityResumed()
        {
            // Sprawdź ponownie pozwolenie, gdy użytkownik wróci do aplikacji
            var hasPermission = Build.VERSION.SdkInt < BuildVersionCodes.R || Environment.IsExternalStorageManager;
            _tcsPermission?.TrySetResult(hasPermission);
        }

        private class ActivityLifecycleListener : Java.Lang.Object, IActivityLifecycleCallbacks
        {
            private readonly Action _onResume;

            public ActivityLifecycleListener(Action onResume)
            {
                _onResume = onResume;
            }

            public void OnActivityResumed(Activity activity)
            {
                _onResume?.Invoke();
            }

            // Pozostałe metody cyklu życia - puste implementacje
            public void OnActivityCreated(Activity activity, Bundle? savedInstanceState) { }
            public void OnActivityStarted(Activity activity) { }
            public void OnActivityPaused(Activity activity) { }
            public void OnActivityStopped(Activity activity) { }
            public void OnActivitySaveInstanceState(Activity activity, Bundle outState) { }
            public void OnActivityDestroyed(Activity activity) { }
        }
    }
}
