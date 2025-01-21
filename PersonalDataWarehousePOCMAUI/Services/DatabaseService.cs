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

        #region public List<string> GetDatabaseList()
        public List<string> GetDatabaseList()
        {
            List<string> DatabaseList = new List<string>();

            string[] DatabaseFolders = Directory.GetDirectories(RootFolder);

            foreach (string DatabaseFolder in DatabaseFolders)
            {
                DatabaseList.Add(Path.GetFileName(DatabaseFolder));
            }

            return DatabaseList;
        }
        #endregion

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
    }
}