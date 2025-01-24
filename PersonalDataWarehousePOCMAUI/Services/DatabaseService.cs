namespace PersonalDataWarehousePOC.Services
{
    using System;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using Parquet;
    using Parquet.Schema;
    using Parquet.Data;
    using System.Threading.Tasks;
    using DataColumn = Parquet.Data.DataColumn;
    using ClosedXML.Excel;
    using System.Text;
    using Newtonsoft.Json;
    using PersonalDataWarehousePOCMAUI.Model;

    public class DatabaseService
    {
        public string RootFolder { get; set; }
        private LogService _logService;

        public DatabaseService(LogService logService)
        {
            _logService = logService;
            RootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PersonalDataWarehouse", "Databases");
        }

        public DatabaseService()
        {
            RootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PersonalDataWarehouse", "Databases");
        }

        #region public void CreateDatabase(string DatabaseName)
        public void CreateDatabase(string DatabaseName)
        {
            // Create the Parquet folder
            string ParquetPath = Path.Combine(RootFolder, DatabaseName, "Parquet");
            if (!Directory.Exists(ParquetPath))
            {
                Directory.CreateDirectory(ParquetPath);
            }

            // Create the Views folder
            string ViewsPath = Path.Combine(RootFolder, DatabaseName, "Views");
            if (!Directory.Exists(ViewsPath))
            {
                Directory.CreateDirectory(ViewsPath);
            }

            // Create the Classes folder
            string ClassesPath = Path.Combine(RootFolder, DatabaseName, "Classes");
            if (!Directory.Exists(ClassesPath))
            {
                Directory.CreateDirectory(ClassesPath);
            }

            // Create the Reports folder
            string ReportsPath = Path.Combine(RootFolder, DatabaseName, "Reports");
            if (!Directory.Exists(ReportsPath))
            {
                Directory.CreateDirectory(ReportsPath);
            }

            // Create the Reports/Data folder
            string ReportsDataPath = Path.Combine(ReportsPath, "Data");
            if (!Directory.Exists(ReportsDataPath))
            {
                Directory.CreateDirectory(ReportsDataPath);
            }
        }
        #endregion

        #region public void UpdateDatabase(string OldDatabaseName, string OldNewDatabaseName)
        public void UpdateDatabase(string OldDatabaseName, string NewDatabaseName)
        {
            string OldDatabasePath = Path.Combine(RootFolder, OldDatabaseName);
            string NewDatabasePath = Path.Combine(RootFolder, NewDatabaseName);
            if (Directory.Exists(OldDatabasePath))
            {
                Directory.Move(OldDatabasePath, NewDatabasePath);
            }
        }
        #endregion

        #region public void DeleteDatabase(string DatabaseName)
        public void DeleteDatabase(string DatabaseName)
        {
            string DatabasePath = Path.Combine(RootFolder, DatabaseName);

            if (Directory.Exists(DatabasePath))
            {
                Directory.Delete(DatabasePath, true);
            }
        }
        #endregion

        #region public byte[] ExportDatabase(string DatabaseName)
        public byte[] ExportDatabase(string DatabaseName)
        {
            try
            {
                string DatabasePath = Path.Combine(RootFolder, DatabaseName);

                // Create _TempZip
                string TempZipPath =
                    $"{RootFolder}/_TempZip";

                if (!Directory.Exists(TempZipPath))
                {
                    Directory.CreateDirectory(TempZipPath);
                }
                else
                {
                    // Delete the temp directory
                    Directory.Delete(TempZipPath, true);

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(TempZipPath))
                    {
                        Directory.CreateDirectory(TempZipPath);
                    }
                }

                string ExportFilePath = Path.Combine(TempZipPath, $"{DatabaseName}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.pdw");

                // Zip the files
                ZipFile.CreateFromDirectory(DatabasePath, ExportFilePath);

                // Read the Zip file into a byte array
                byte[] ExportFileBytes = File.ReadAllBytes(ExportFilePath);

                // Delete the temp directory
                Directory.Delete(TempZipPath, true);

                return ExportFileBytes;
            }
            catch (Exception ex)
            {
                // Log error
                _logService.WriteToLog("ExportFile: " + ex.Message + " " + ex.StackTrace ?? "" + " " + ex.InnerException.StackTrace ?? "");

                return null;
            }
        }
        #endregion

        #region public string ImportDatabase(string DatabaseName, byte[] DatabaseFile)
        public string ImportDatabase(string DatabaseName, byte[] DatabaseFile)
        {
            string strResponse = string.Empty;

            try
            {
                string DatabasePath = Path.Combine(RootFolder, DatabaseName);

                #region Create Temp Directories
                // Create _Temp
                string TempPath =
                    $"{RootFolder}/_Temp";

                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }
                else
                {
                    // Delete the temp directory
                    Directory.Delete(TempPath, true);

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(TempPath))
                    {
                        Directory.CreateDirectory(TempPath);
                    }
                }
                #endregion

                // Save the file to the TempPath directory
                string ImportFilePath = $"{TempPath}/{DatabaseName}.pdw";
                File.WriteAllBytes(ImportFilePath, DatabaseFile);

                // Extract the files to the destination directory
                ZipFile.ExtractToDirectory(ImportFilePath, DatabasePath, true);

                // Delete the temp directories
                Directory.Delete(TempPath, true);

                // Log
                _logService.WriteToLog($"Database {DatabaseName} imported.");

                strResponse = $"Database {DatabaseName} imported.";
            }
            catch (Exception ex)
            {
                // Log error
                _logService.WriteToLog("ImportDatabase: " + ex.Message + " " + ex.StackTrace ?? "" + " " + ex.InnerException.StackTrace ?? "");

                return ex.Message;
            }

            return strResponse;
        }
        #endregion

        #region public async Task<List<string>> GetAllTablesAsync()
        public async Task<List<string>> GetAllTablesAsync()
        {
            var result = new List<string>();

            // Find all subdirectories named "Parquet" (recursively)
            var parquetDirs = Directory.EnumerateDirectories(RootFolder, "Parquet", SearchOption.AllDirectories);

            foreach (var parquetDir in parquetDirs)
            {
                // Get the parent folder name of the "Parquet" directory
                string parentFolder = Path.GetFileName(Path.GetDirectoryName(parquetDir));

                // Find all .parquet files in this "Parquet" directory (no further recursion)
                var parquetFiles = Directory.EnumerateFiles(parquetDir, "*.parquet", SearchOption.TopDirectoryOnly);

                foreach (var file in parquetFiles)
                {
                    // Extract just the filename
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Format: "ParentFolder/ParquetFileName"
                    result.Add($"{parentFolder}/{fileName}");
                }
            }

            // Sort so that folders beginning with "Default" come first, then everything else in alphabetical order
            result = result
                // Items whose parent folder starts with "Default" come first
                .OrderBy(item => item.Split('/')[0].StartsWith("Default", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                // Then, sort alphabetically by the full string
                .ThenBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return await Task.FromResult(result);
        }
        #endregion

        #region public async Task<List<string>> GetAllDatabasesAsync()
        public async Task<List<string>> GetAllDatabasesAsync()
        {
            // Use a HashSet to avoid duplicates and perform case-insensitive comparisons.
            var databases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Find all subdirectories named "Parquet" under the root folder (recursively).
            var parquetDirs = Directory.EnumerateDirectories(RootFolder, "Parquet", SearchOption.AllDirectories);

            foreach (var parquetDir in parquetDirs)
            {
                // The parent folder of "Parquet" is considered the database name.
                string parentFolder = Path.GetFileName(Path.GetDirectoryName(parquetDir));

                // Add it to our set of databases.
                databases.Add(parentFolder);
            }

            // Sort so that folders beginning with "Default" come first, 
            // then everything else in alphabetical order.
            var result = databases
                .OrderBy(db => db.StartsWith("Default", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(db => db, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Return via Task to match the async signature.
            return await Task.FromResult(result);
        }
        #endregion

        #region public async Task<List<string>> GetAllViewsAsync()
        public async Task<List<string>> GetAllViewsAsync()
        {
            var result = new List<string>();

            // Find all subdirectories named "Views" (recursively)
            var viewsDirs = Directory.EnumerateDirectories(RootFolder, "Views", SearchOption.AllDirectories);

            foreach (var viewDir in viewsDirs)
            {
                // Get the parent folder name of the "Views" directory
                string parentFolder = Path.GetFileName(Path.GetDirectoryName(viewDir));

                // Find all .view files in this "Views" directory (no further recursion)
                var viewFiles = Directory.EnumerateFiles(viewDir, "*.view", SearchOption.TopDirectoryOnly);

                foreach (var file in viewFiles)
                {
                    // Extract just the filename
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Format: "ParentFolder/ViewFileName"
                    result.Add($"{parentFolder}/{fileName}");
                }
            }

            // Sort so that folders beginning with "Default" come first, then everything else in alphabetical order
            result = result
                // Items whose parent folder starts with "Default" come first
                .OrderBy(item => item.Split('/')[0].StartsWith("Default", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                // Then, sort alphabetically by the full string
                .ThenBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return await Task.FromResult(result);
        }
        #endregion

        #region public async Task<List<string>> GetAllReportsAsync()
        public async Task<List<string>> GetAllReportsAsync()
        {
            var result = new List<string>();

            // Find all subdirectories named "Reports" (recursively)
            var ReportsDirs = Directory.EnumerateDirectories(RootFolder, "Reports", SearchOption.AllDirectories);

            foreach (var viewDir in ReportsDirs)
            {
                // Get the parent folder name of the "Reports" directory
                string parentFolder = Path.GetFileName(Path.GetDirectoryName(viewDir));

                // Find all .view files in this "Reports" directory (no further recursion)
                var viewFiles = Directory.EnumerateFiles(viewDir, "*.view", SearchOption.TopDirectoryOnly);

                foreach (var file in viewFiles)
                {
                    // Extract just the filename
                    string fileName = Path.GetFileNameWithoutExtension(file);

                    // Format: "ParentFolder/ViewFileName"
                    result.Add($"{parentFolder}/{fileName}");
                }
            }

            // Sort so that folders beginning with "Default" come first, then everything else in alphabetical order
            result = result
                // Items whose parent folder starts with "Default" come first
                .OrderBy(item => item.Split('/')[0].StartsWith("Default", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                // Then, sort alphabetically by the full string
                .ThenBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return await Task.FromResult(result);
        }
        #endregion

        // Utililty

        #region public string RemoveSpacesSpecialCharacters(string input)
        public string RemoveSpacesSpecialCharacters(string input)
        {
            StringBuilder sb = new StringBuilder();

            bool lastWasSpace = false;
            bool IsFirstLetter = true;

            // Remove spaces
            input = input.Trim();

            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c)) // Only allow letters and digits
                {
                    if (IsFirstLetter)
                    {
                        // Capitalize first letter
                        if (char.IsLetter(c))
                        {
                            sb.Append(char.ToUpper(c));
                        }

                        IsFirstLetter = false;
                    }
                    else
                    {
                        if (lastWasSpace)
                        {
                            // If the previous character was a space, capitalize the current letter
                            if (char.IsLetter(c))
                            {
                                sb.Append(char.ToUpper(c));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            lastWasSpace = false;
                        }
                        else
                        {
                            sb.Append(c);
                        }
                    }
                }
                else if (char.IsWhiteSpace(c))
                {
                    lastWasSpace = true; // Mark space to handle capitalization
                }
                // Special characters are ignored
            }

            return sb.ToString();
        }
        #endregion

        #region public (string Database, string Table) ExtractDatabaseAndTable(string paramTable)
        /// <summary>
        /// Splits the input string by '/' into two parts: Database and Table.
        /// Expected input format: "Database/Table"
        /// </summary>
        /// <param name="paramTable">The string containing the database and table, separated by '/'</param>
        /// <returns>A tuple (Database, Table)</returns>
        /// <exception cref="ArgumentException">Thrown if the input string is null/empty or not in the correct format</exception>
        public (string Database, string Table) ExtractDatabaseAndTable(string paramTable)
        {
            if (string.IsNullOrWhiteSpace(paramTable))
            {
                throw new ArgumentException("Input cannot be null or empty.", nameof(paramTable));
            }

            string[] parts = paramTable.Split('/');
            if (parts.Length < 2)
            {
                throw new ArgumentException($"Input must be in the format 'Database/Table'. Received: {paramTable}", nameof(paramTable));
            }

            return (parts[0], parts[1]);
        } 
        #endregion
    }
}