using Newtonsoft.Json;
using OpenAI.Files;

namespace PersonalDataWarehousePOCMAUI.Services
{
    public class SettingsService
    {
        // Properties
        public string Organization { get; set; }
        public string ApiKey { get; set; }
        public string AIModel { get; set; }
        public string AIType { get; set; }
        public string Endpoint { get; set; }
        public string AIEmbeddingModel { get; set; }
        public string ApiVersion { get; set; }

        // Constructor
        public SettingsService()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            // Get OpenAI API key from appsettings.json
            // PersonalDataWarehouse Directory
            String folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/PersonalDataWarehouse";
            var PersonalDataWarehouseSettingsPath = $"{folderPath}/PersonalDataWarehouseSettings.config";

            string PersonalDataWarehouseSettings = "";

            // Open the file to get existing content
            using (var streamReader = new StreamReader(PersonalDataWarehouseSettingsPath))
            {
                PersonalDataWarehouseSettings = streamReader.ReadToEnd();
            }

            // Convert the JSON to a dynamic object
            dynamic PersonalDataWarehouseSettingsObject = JsonConvert.DeserializeObject(PersonalDataWarehouseSettings);

            if (PersonalDataWarehouseSettingsObject.ApplicationSettings.AIType == null || PersonalDataWarehouseSettingsObject.ApplicationSettings.AIType == "")
            {
                PersonalDataWarehouseSettingsObject.ApplicationSettings.AIType = "OpenAI";
            }

            Organization = PersonalDataWarehouseSettingsObject.OpenAIServiceOptions.Organization;
            ApiKey = PersonalDataWarehouseSettingsObject.OpenAIServiceOptions.ApiKey;
            AIModel = PersonalDataWarehouseSettingsObject.ApplicationSettings.AIModel;
            AIType = PersonalDataWarehouseSettingsObject.ApplicationSettings.AIType;
            Endpoint = PersonalDataWarehouseSettingsObject.ApplicationSettings.Endpoint;
            ApiVersion = PersonalDataWarehouseSettingsObject.ApplicationSettings.ApiVersion;
            AIEmbeddingModel = PersonalDataWarehouseSettingsObject.ApplicationSettings.AIEmbeddingModel;
        }

        public async Task SaveSettings(string paramOrganization, string paramApiKey, string paramAIModel, string paramAIType, string paramEndpoint, string paramApiVersion, string paramAIEmbeddingModel)
        {
            // Get OpenAI API key from appsettings.json
            // PersonalDataWarehouse Directory
            var PersonalDataWarehouseSettingsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/PersonalDataWarehouse/PersonalDataWarehouseSettings.config";

            string PersonalDataWarehouseSettings = "";

            // Open the file to get existing content
            using (var streamReader = new StreamReader(PersonalDataWarehouseSettingsPath))
            {
                PersonalDataWarehouseSettings = streamReader.ReadToEnd();
            }

            // Convert the JSON to a dynamic object
            dynamic PersonalDataWarehouseSettingsObject = JsonConvert.DeserializeObject(PersonalDataWarehouseSettings);

            // Update the dynamic object
            PersonalDataWarehouseSettingsObject.OpenAIServiceOptions.Organization = paramOrganization;
            PersonalDataWarehouseSettingsObject.OpenAIServiceOptions.ApiKey = paramApiKey;
            PersonalDataWarehouseSettingsObject.ApplicationSettings.AIModel = paramAIModel;
            PersonalDataWarehouseSettingsObject.ApplicationSettings.AIType = paramAIType;
            PersonalDataWarehouseSettingsObject.ApplicationSettings.Endpoint = paramEndpoint;
            PersonalDataWarehouseSettingsObject.ApplicationSettings.ApiVersion = paramApiVersion;
            PersonalDataWarehouseSettingsObject.ApplicationSettings.AIEmbeddingModel = paramAIEmbeddingModel;

            // Convert the dynamic object back to JSON
            PersonalDataWarehouseSettings = JsonConvert.SerializeObject(PersonalDataWarehouseSettingsObject, Formatting.Indented);

            // Write the JSON back to the file
            using (var streamWriter = new StreamWriter(PersonalDataWarehouseSettingsPath))
            {
                await streamWriter.WriteAsync(PersonalDataWarehouseSettings);
            }

            // Update the properties
            Organization = paramOrganization;
            ApiKey = paramApiKey;
            AIModel = paramAIModel;
            AIType = paramAIType;
            Endpoint = paramEndpoint;
            ApiVersion = paramApiVersion;
            AIEmbeddingModel = paramAIEmbeddingModel;
        }
    }
}