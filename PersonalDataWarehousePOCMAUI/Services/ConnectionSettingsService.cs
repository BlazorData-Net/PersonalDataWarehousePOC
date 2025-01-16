using Microsoft.Extensions.Options;
using PersonalDataWarehousePOCMAUI.Models;

namespace PersonalDataWarehousePOC.Services
{
    public class ConnectionSettingsService
    {
        public SQLServerSettings SQLServer { get; }
        public FabricSettings Fabric { get; }
        public AzureStorageSettings AzureStorage { get; }

        public ConnectionSettingsService(IOptions<ConnectionSettings> options)
        {
            var settings = options.Value;
            SQLServer = settings.SQLServer;
            Fabric = settings.Fabric;
            AzureStorage = settings.AzureStorage;
        }
    }
}
