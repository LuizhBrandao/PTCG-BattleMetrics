using Android.App;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;

namespace PTCGBattleMetrics.Android;

[Activity(
    Label = "PTCG Metrics",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.KeyboardHidden,
    Theme = "@android:style/Theme.DeviceDefault.NoActionBar")]
public class MainActivity : Activity
{
    private WebView? _webView;
    
    // Endereço público do GitHub Pages (PWA 100% Gratuito)
    public const string DefaultAppUrl = "https://luizhbrandao.github.io/PTCG-BattleMetrics/";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Estilização Dark Theme imersiva para o status bar e navigation bar
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        {
            Window?.SetStatusBarColor(Color.ParseColor("#0B0F19"));
            Window?.SetNavigationBarColor(Color.ParseColor("#0B0F19"));
        }

        _webView = new WebView(this);
        _webView.SetBackgroundColor(Color.ParseColor("#0B0F19"));

        var settings = _webView.Settings;
        settings.JavaScriptEnabled = true;
        settings.DomStorageEnabled = true;
        settings.DatabaseEnabled = true;
        settings.SetSupportZoom(false);
        settings.BuiltInZoomControls = false;
        settings.DisplayZoomControls = false;
        settings.LoadWithOverviewMode = true;
        settings.UseWideViewPort = true;
        settings.CacheMode = CacheModes.Default;
        settings.AllowFileAccess = true;
        settings.AllowContentAccess = true;

        _webView.SetWebViewClient(new BattleMetricsWebViewClient());
        _webView.SetWebChromeClient(new WebChromeClient());

        SetContentView(_webView);

        _webView.LoadUrl(DefaultAppUrl);
    }

    public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
    {
        if (keyCode == Keycode.Back && _webView != null && _webView.CanGoBack())
        {
            _webView.GoBack();
            return true;
        }
        return base.OnKeyDown(keyCode, e);
    }

    private class BattleMetricsWebViewClient : WebViewClient
    {
        public override bool ShouldOverrideUrlLoading(WebView? view, IWebResourceRequest? request)
        {
            return false; // Mantém a navegação dentro do app
        }
    }
}
