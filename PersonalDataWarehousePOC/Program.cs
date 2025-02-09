using Blazor.Monaco;
using BlazorDatasheet.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using PersonalDataWarehousePOC.Components;
using PersonalDataWarehousePOC.Services;
using Radzen;
using System.Reflection;

namespace PersonalDataWarehousePOC;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Load appsettings.json and UserSecrets
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings{builder.Environment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddUserSecrets(Assembly.GetExecutingAssembly(), true);

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

        builder.Services.AddBlazorMonacoComponents();
        builder.Services.AddRadzenComponents();

        /// **** ODATA START ****
        var modelBuilder = new ODataConventionModelBuilder();

        ODataSupport.AddDynamicEntitySets(modelBuilder);

        builder.Services.AddControllers().AddOData(
            options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
                "odata",
                modelBuilder.GetEdmModel()));
        /// **** ODATA END ****

        var app = builder.Build();

        /// **** ODATA START ****
        app.UseRouting();
        /// **** ODATA END ****

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

        /// **** ODATA START ****
        app.MapControllers();
        /// **** ODATA END ****
  
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}