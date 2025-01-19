using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PersonalDataWarehousePOCMAUI.Services
{
    public class SettingsService
    {
        // Nested Configuration Classes
        public class ApplicationSettings
        {
            public string AIModel { get; set; }
            public string AIType { get; set; }
            public string Endpoint { get; set; }
            public string ApiVersion { get; set; }
            public string AIEmbeddingModel { get; set; }
        }

        public class SQLServerSettings
        {
            public string DatabaseName { get; set; }
            public string DatabaseUsername { get; set; }
            public string IntegratedSecurityDisplay { get; set; }
            public string ServerName { get; set; }
        }

        public class FabricSettings
        {
            public string DatabaseName { get; set; }
            public string DatabaseUsername { get; set; }
            public string IntegratedSecurityDisplay { get; set; }
            public string ServerName { get; set; }
        }

        public class AzureStorageSettings
        {
            public string StorageAccountName { get; set; }
            public string ContainerName { get; set; }
        }

        public class Configuration
        {
            public ApplicationSettings ApplicationSettings { get; set; }
            public SQLServerSettings SQLServerSettings { get; set; }
            public FabricSettings FabricSettings { get; set; }
            public AzureStorageSettings AzureStorageSettings { get; set; }
        }

        // Configuration Property
        public Configuration Settings { get; private set; }

        // Path to the settings file
        private readonly string _settingsPath;

        // Constructor
        public SettingsService()
        {
            // Construct the path to the settings file using Path.Combine for cross-platform compatibility
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PersonalDataWarehouse");
            _settingsPath = Path.Combine(folderPath, "PersonalDataWarehouse.config");

            LoadSettings();
        }

        /// <summary>
        /// Loads the settings from the configuration file.
        /// </summary>
        public async void LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    await InitializeDefaultSettingsAsync();
                }

                // Read the content of the settings file
                string settingsContent;
                using (var streamReader = new StreamReader(_settingsPath))
                {
                    settingsContent = streamReader.ReadToEnd();
                }

                // Deserialize the JSON content into the Configuration object
                Settings = JsonConvert.DeserializeObject<Configuration>(settingsContent);

                if (Settings == null)
                {
                    throw new InvalidDataException("Failed to deserialize the settings file.");
                }

                // Set default values for specific settings if necessary
                if (string.IsNullOrWhiteSpace(Settings.ApplicationSettings.AIType))
                {
                    Settings.ApplicationSettings.AIType = "OpenAI";
                }

                // You can add more default value assignments here if needed
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed (e.g., logging)
                Console.WriteLine($"Error loading settings: {ex.Message}");
                throw; // Re-throw the exception if you want to handle it further up the call stack
            }
        }

        /// <summary>
        /// Saves the entire configuration to the settings file.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveSettingsAsync()
        {
            try
            {
                // Ensure the directory exists
                string folderPath = Path.GetDirectoryName(_settingsPath);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Serialize the Configuration object back to JSON
                string updatedSettings = JsonConvert.SerializeObject(Settings, Formatting.Indented);

                // Write the updated JSON back to the file asynchronously
                using (var streamWriter = new StreamWriter(_settingsPath, false))
                {
                    await streamWriter.WriteAsync(updatedSettings);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed (e.g., logging)
                Console.WriteLine($"Error saving settings: {ex.Message}");
                throw; // Re-throw the exception if you want to handle it further up the call stack
            }
        }

        /// <summary>
        /// Updates specific sections of the configuration and saves the changes.
        /// </summary>
        /// <param name="applicationSettings">New application settings.</param>
        /// <param name="sqlServerSettings">New SQL Server settings.</param>
        /// <param name="fabricSettings">New Fabric settings.</param>
        /// <param name="azureStorageSettings">New Azure Storage settings.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task UpdateSettingsAsync(
            ApplicationSettings applicationSettings = null,
            SQLServerSettings sqlServerSettings = null,
            FabricSettings fabricSettings = null,
            AzureStorageSettings azureStorageSettings = null)
        {
            // Update the settings if new values are provided
            if (applicationSettings != null)
            {
                Settings.ApplicationSettings = applicationSettings;
            }

            if (sqlServerSettings != null)
            {
                Settings.SQLServerSettings = sqlServerSettings;
            }

            if (fabricSettings != null)
            {
                Settings.FabricSettings = fabricSettings;
            }

            if (azureStorageSettings != null)
            {
                Settings.AzureStorageSettings = azureStorageSettings;
            }

            // Save the updated settings to the file
            await SaveSettingsAsync();
        }

        /// <summary>
        /// Initializes default settings and saves them to the configuration file.
        /// This can be used when the configuration file does not exist.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task InitializeDefaultSettingsAsync()
        {
            try
            {
                // Initialize default settings
                Settings = new Configuration
                {
                    ApplicationSettings = new ApplicationSettings
                    {
                        AIModel = "DefaultAIModel",
                        AIType = "OpenAI",
                        Endpoint = "https://api.openai.com",
                        ApiVersion = "v1",
                        AIEmbeddingModel = "DefaultEmbeddingModel"
                    },
                    SQLServerSettings = new SQLServerSettings
                    {
                        DatabaseName = "DefaultDB",
                        DatabaseUsername = "sa",
                        IntegratedSecurityDisplay = "False",
                        ServerName = "localhost"
                    },
                    FabricSettings = new FabricSettings
                    {
                        DatabaseName = "FabricDB",
                        DatabaseUsername = "fabricUser",
                        IntegratedSecurityDisplay = "False",
                        ServerName = "fabricServer"
                    },
                    AzureStorageSettings = new AzureStorageSettings
                    {
                        StorageAccountName = "DefaultStorageAccount",
                        ContainerName = "default-container"
                    }
                };

                // Save the default settings to the file
                await SaveSettingsAsync();
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed (e.g., logging)
                Console.WriteLine($"Error initializing default settings: {ex.Message}");
                throw;
            }
        }
    }
}