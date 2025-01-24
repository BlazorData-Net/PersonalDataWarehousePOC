using CSScriptLib;
using PersonalDataWarehousePOC.Services;
using Renci.SshNet.Messages;
using System.Data;
using System.Linq;
public class Dataloader
{
    public async Task<IEnumerable<IDictionary<string, object>>> LoadParquet(string TableName)
    {
        IEnumerable<IDictionary<string, object>> response = new List<IDictionary<string, object>>();

        // Seperate the table name from the database
        DataService dataService = new DataService();
        var (Database, Table) = dataService.ExtractDatabaseAndTable(TableName);

        // Load the DataTable
        String parquetFolder = $"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)}/PersonalDataWarehouse/Databases/{Database}/Parquet";
        var fileName = Path.Combine(parquetFolder, $"{Table}.parquet");

        if (System.IO.File.Exists(fileName))
        {
            DataService objDataService = new DataService();
            var CurrentDataTable = await objDataService.ReadParquetFileAsync(fileName);

            // Convert the DataTable to a List of Dictionaries
            response = CurrentDataTable.AsEnumerable()
                .Select(row => CurrentDataTable.Columns
                .Cast<DataColumn>()
                .ToDictionary(column => column.ColumnName, column => row[column]))
                .ToList();

            return response;
        }

        return response;
    }

    public async Task<IEnumerable<IDictionary<string, object>>> RunDynamicCode(string paramCode)
    {
        dynamic script = CSScript.Evaluator.LoadMethod(paramCode);

        var result = await script.ReturnResult();

        return result as IEnumerable<IDictionary<string, object>>;
    }
}