using PersonalDataWarehousePOC.Services;
using System.Data;
using System.Linq;
public class Dataloader
{
    public async Task<IEnumerable<IDictionary<string, object>>> LoadParquet(string TableName)
    {
        IEnumerable<IDictionary<string, object>> response = new List<IDictionary<string, object>>();

        // Load the DataTable
        var parquetFolder = Path.Combine("Data", "Parquet");
        var fileName = Path.Combine(parquetFolder, $"{TableName}.parquet");

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
}