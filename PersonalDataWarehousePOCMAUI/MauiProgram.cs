using BlazorDatasheet.Extensions;
using Microsoft.Extensions.Logging;
using PersonalDataWarehousePOC.Services;
using Radzen;

namespace PersonalDataWarehousePOCMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            // Blazor Datasheet
            builder.Services.AddBlazorDatasheet();

            // DataService Service
            builder.Services.AddSingleton<DataService>();

            // This is required by Excel service to parse strings in binary BIFF2-5 Excel documents
            // encoded with DOS-era code pages.
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            builder.Services.AddRadzenComponents();

            return builder.Build();
        }
    }
}
