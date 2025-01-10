﻿using BlazorDatasheet.Extensions;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Parquet.Schema;
using PersonalDataWarehousePOC.Services;
using PersonalDataWarehousePOCMAUI.Services;
using PersonalDataWarehousePOCMAUI.Models;
using Radzen;
using PersonalDataWarehousePOCMAUI.Model;
using PersonalDataWarehouse.AI;

namespace PersonalDataWarehousePOCMAUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            // Add services to the container.
            AppMetadata appMetadata = new AppMetadata() { Version = "01.02.00" };
            builder.Services.AddSingleton(appMetadata);

            // Blazor Datasheet
            builder.Services.AddBlazorDatasheet();

            // Services
            builder.Services.AddSingleton<DataService>();
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddSingleton<LogService>();
            builder.Services.AddSingleton<OrchestratorMethods>();

            // This is required by Excel service to parse strings in binary BIFF2-5 Excel documents
            // encoded with DOS-era code pages.
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            builder.Services.AddRadzenComponents();

            // Data Directory
            String folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/PersonalDataWarehouse";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // PersonalDataWarehouseLog.csv
            string PersonalDataWarehouseLogFilePath = Path.Combine(folderPath, "PersonalDataWarehouseLog.csv");
            if (!File.Exists(PersonalDataWarehouseLogFilePath))
            {
                using (var streamWriter = new StreamWriter(PersonalDataWarehouseLogFilePath))
                {
                    streamWriter.WriteLine("Date,Time,Event");
                }
            }

            // PersonalDataWarehouse.config
            string PersonalDataWarehouseFilePath = Path.Combine(folderPath, "PersonalDataWarehouse.config");
            if (!File.Exists(PersonalDataWarehouseFilePath))
            {
                using (var streamWriter = new StreamWriter(PersonalDataWarehouseFilePath))
                {
                    streamWriter.WriteLine(
                    """
                        {
                          "OpenAIServiceOptions": {
                            "Organization": "",
                            "ApiKey": ""
                          },
                          "ApplicationSettings": {
                            "AIModel": "gpt-4o"
                          }
                        }
                        """
                    );
                }
            }

            return builder.Build();
        }
    }
}
