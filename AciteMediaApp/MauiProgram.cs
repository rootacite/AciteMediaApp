using AciteMediaApp.Models;
using AciteMediaApp.Services.Interfaces;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

namespace AciteMediaApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
    		builder.Logging.AddDebug();
    		builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                {
                    return true;
                }
            };

            builder.Services.AddSingleton<HttpClient>(sp => new HttpClient(handler)
            {
                BaseAddress = new Uri(MetadataService.BaseUrl),
            });
            builder.Services.AddSingleton<IServiceProvider, ServiceProvider>();
            builder.Services.AddSingleton<ConnectivityService>();
            builder.Services.AddSingleton<DownloaderService>();

            builder.Services.AddSingleton<IComicService, ComicService>();
            builder.Services.AddSingleton<IMetadataService, MetadataService>();

            builder.Services.AddKeyedSingleton<IPageCache, MemoryPageCache>("mc");
            builder.Services.AddKeyedSingleton<IPageCache, DiskPageCache>("dc");
            builder.Services.AddKeyedSingleton<IPageCache, WebPageCache>("wc");

            builder.Services.AddSingleton<MainPageModel>();
            builder.Services.AddSingleton<DownloaderPageModel>();
            builder.Services.AddSingleton<VideoPageModel>();

            builder.Services.AddTransientWithShellRoute<ComicViewer, ComicViewerModel>("comic_reader");
            builder.Services.AddTransientWithShellRoute<DetailPage, DetailPageModel>("detail_page");
            builder.Services.AddTransientWithShellRoute<VideoViewer, VideoViewerModel>("video_viewer");
            // builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

            return builder.Build();
        }
    }
}
