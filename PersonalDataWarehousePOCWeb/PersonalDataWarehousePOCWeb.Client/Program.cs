using BlazorDatasheet.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PersonalDataWarehousePOCWeb.Client.Services;
using Radzen;

namespace PersonalDataWarehousePOCWeb.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddBlazorDatasheet();
            builder.Services.AddRadzenComponents();

            // DataService Service
            builder.Services.AddSingleton<DataService>();

            // <-- This line registers HttpClient in DI
            builder.Services.AddScoped(sp =>
                new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
