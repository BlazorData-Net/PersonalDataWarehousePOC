using BlazorDatasheet.Extensions;
using PersonalDataWarehousePOCWeb.Client.Pages;
using PersonalDataWarehousePOCWeb.Components;

namespace PersonalDataWarehousePOCWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            var app = builder.Build();

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
