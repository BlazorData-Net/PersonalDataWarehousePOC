using BlazorDatasheet.Extensions;
using PersonalDataWarehousePOCWeb.Client.Pages;
using PersonalDataWarehousePOCWeb.Components;
using Radzen;
using System.Reflection;

namespace PersonalDataWarehousePOCWeb
{
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
                .AddInteractiveWebAssemblyComponents();

            builder.Services.AddBlazorDatasheet();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<HttpClient>(sp =>
            {
                // Attempt to read the current request's host
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var request = httpContextAccessor.HttpContext?.Request;

                var scheme = request?.Scheme ?? "https";
                var host = request?.Host.ToString() ?? null;

                if (string.IsNullOrEmpty(host))
                {
                    throw new InvalidOperationException("Host is null.");
                }

                var baseAddress = $"{scheme}://{host}/";

                return new HttpClient { BaseAddress = new Uri(baseAddress) };
            });

            builder.Services.AddControllers();

            builder.Services.AddRadzenComponents();

            var app = builder.Build();

            app.MapDefaultEndpoints();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();

            app.MapControllers();

            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.Run();
        }
    }
}
