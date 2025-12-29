using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Interfaces;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Fingerprint;

namespace AmuleRemoteControl;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
        | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
        LaunchMode = LaunchMode.SingleInstance)]
    [IntentFilter(
        new[] { Intent.ActionSend, Intent.ActionView },
        Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
        DataScheme = "ed2k",
        AutoVerify = true)]
    public class MainActivity : MauiAppCompatActivity
    {

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            CrossFingerprint.SetCurrentActivityResolver(() => this);

            //if (Intent?.Data != null)
            //{
            //    Console.WriteLine("OnCreate-Bundle: Intent present: " + Intent.ActionView.ToString() +" - " + Intent.Data.ToString() );
            //}
            //else
            //{
            //    Console.WriteLine("OnCreate-Bundle: Intent is null");
            //}

            var action = Intent?.Action;
            var data = Intent?.Data?.ToString();

            if (action == Intent.ActionView && data is not null)
            {
                HandleAppLink(data);
            }
            else
            {
                // No action needed for deep link service here
            }

        }


        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            Intent = intent;
            var action = Intent?.Action;
            var data = Intent?.Data?.ToString();

            if (action == Intent.ActionView && data is not null)
            {
                HandleAppLink(data);
            }
        }


        void HandleAppLink(string url)
        {
            var deepLinkService = IPlatformApplication.Current?.Services.GetService<IDeepLinkService>();
            if (deepLinkService != null)
            {
                // Notify the service about the received link
                deepLinkService.NotifyLinkReceived(url, DeepLinkSource.ExternalApp);
            }
        }

    }