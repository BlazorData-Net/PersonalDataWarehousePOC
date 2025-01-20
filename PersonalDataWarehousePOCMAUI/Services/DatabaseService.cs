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

    public class DatabaseService
    {
        public string RootFolder { get; set; }  
        public DatabaseService()
        {
            RootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PersonalDataWarehouse");
        }

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
    }
}