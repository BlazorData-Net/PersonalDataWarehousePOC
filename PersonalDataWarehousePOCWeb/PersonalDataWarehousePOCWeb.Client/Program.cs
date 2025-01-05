using BlazorDatasheet.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace PersonalDataWarehousePOCWeb.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            builder.Services.AddBlazorDatasheet();

            // <-- This line registers HttpClient in DI
            builder.Services.AddScoped(sp =>
                new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            await builder.Build().RunAsync();
        }
    }
}
