using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PersonalDataWarehousePOCMAUI.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        private IHost _webHost;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Start the web host in the background
            _webHost = CreateWebHost();
            Task.Run(() => _webHost.Run());
        }

        private IHost CreateWebHost()
        {
            var builder = WebApplication.CreateBuilder();

            // Configure Kestrel to listen on a specific port (e.g., 5000)
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000);
            });

            // Add controllers or minimal API endpoints
            builder.Services.AddControllers(); // or builder.Services.AddEndpointsApiExplorer(); if using minimal APIs

            var app = builder.Build();

            // Map your API endpoints
            app.MapControllers();
            // For minimal API endpoints, you might add:
            // app.MapGet("/hello", () => "Hello from MAUI API!");

            return app;
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}
