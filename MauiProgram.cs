using System.Net;
using AmuleRemoteControl.Components.Data;
using AmuleRemoteControl.Components.Interfaces;
using AmuleRemoteControl.Components.Service;
using AmuleRemoteControl.Components.Service.Parsers;
using Microsoft.Extensions.Logging;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;
using Radzen;
using Serilog;

namespace AmuleRemoteControl;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton(typeof(IFingerprint), CrossFingerprint.Current);

        // Register CultureProvider as singleton to manage application culture
        builder.Services.AddSingleton<ICultureProvider, CultureProvider>();

        // Register Radzen services (UI components)
        builder.Services.AddScoped<SessionStorageAccessor>();
        builder.Services.AddScoped<DialogService>();
        builder.Services.AddScoped<NotificationService>();
        builder.Services.AddScoped<TooltipService>();
        builder.Services.AddScoped<ContextMenuService>();

        // Register core application services
        builder.Services.AddScoped<IUtilityServices, UtilityServices>();
        builder.Services.AddScoped<IAmuleRemoteServices, aMuleRemoteService>();
        builder.Services.AddScoped<IAccessService, AccessService>();
        builder.Services.AddScoped<IEd2kUrlParser, Ed2kUrlParser>(); // Ed2k URL parser for deep linking
        builder.Services.AddSingleton<IDeepLinkService, DeepLinkService>(); // Event-based deep link service

        // Register HTML parsing infrastructure (Sprint 7-8: Phase 3)
        builder.Services.AddSingleton<XPathConfiguration>(); // XPath configuration for version-aware parsing

        // Register parser implementations (Sprint 8-9)
        builder.Services.AddScoped<IDownloadParser, DownloadParser>();
        builder.Services.AddScoped<IUploadParser, UploadParser>();
        builder.Services.AddScoped<IServerParser, ServerParser>();
        builder.Services.AddScoped<IStatsParser, StatsParser>();
        builder.Services.AddScoped<ISearchParser, SearchParser>();
        builder.Services.AddScoped<IPreferencesParser, PreferencesParser>();

        // Register version detector (Sprint 10+)
        builder.Services.AddSingleton<IAmuleVersionDetector, AmuleVersionDetector>();

        // Configure HttpClient with IHttpClientFactory for NetworkHelper
        builder.Services.AddHttpClient<INetworkHelper, NetworkHelper>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        var _cachePath = FileSystem.Current.CacheDirectory;

        var _appData = FileSystem.AppDataDirectory;

        var logFileName = Path.Combine(_cachePath, "serilog_.log");

        Log.Logger = new LoggerConfiguration()
                     .Enrich.FromLogContext()
                     .WriteTo.Debug(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3} {Username} {Message:lj}{Exception}{NewLine}")
                     .WriteTo.File(logFileName, rollingInterval: RollingInterval.Day,
                     retainedFileCountLimit: 5,
                     outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {CorrelationId} {Level:u3} {Username} {Message:lj}{Exception}{NewLine}")
                     .CreateLogger();

        builder.Services.AddLogging(logging =>
        {
            logging.AddSerilog(dispose: true);
        });
        return builder.Build();
    }
}



