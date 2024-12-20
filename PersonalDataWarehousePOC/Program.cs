using BlazorDatasheet.Extensions;
using PersonalDataWarehousePOC.Components;
using PersonalDataWarehousePOC.Services;

namespace PersonalDataWarehousePOC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Blazor Datasheet
        builder.Services.AddBlazorDatasheet();

        // DataService Service
        builder.Services.AddSingleton<DataService>();

        // This is required by Excel service to parse strings in binary BIFF2-5 Excel documents
        // encoded with DOS-era code pages.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
