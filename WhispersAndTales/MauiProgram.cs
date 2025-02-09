using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using WhispersAndTales.Services;

namespace WhispersAndTales
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            }).UseMauiCommunityToolkit();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddTransient<MainPage>();
#if ANDROID
            builder.Services.AddSingleton<ISpeechToText, SpeechToTextImplementation>();
            builder.Services.AddSingleton<IPermission, AndroidPermissionChecker>();
#else
            builder.Services.AddSingleton<ISpeechToText, SpeechToTextStub>();
            builder.Services.AddSingleton<IPermission, PermissionStub>();
#endif
            return builder.Build();
        }
    }
}