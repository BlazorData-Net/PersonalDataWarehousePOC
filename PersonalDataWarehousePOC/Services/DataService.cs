namespace PersonalDataWarehousePOC.Services
{
    using System;
    using System.Data;
    using System.IO;
    using Parquet;
    using Parquet.Schema;

    public class DataService
    {
        #region public async Task<DataTable> ReadParquetFileAsync(string filename)
        public async Task<DataTable> ReadParquetFileAsync(string filename)
        {
            var dt = new DataTable($"{filename}");

            // Open the file stream
            using var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create the reader asynchronously
            using (var reader = await ParquetReader.CreateAsync(fileStream))
            {
                DataField[] dataFields = reader.Schema.GetDataFields();

                for (int rgIndex = 0; rgIndex < reader.RowGroupCount; rgIndex++)
                {
                    using (var rowGroupReader = reader.OpenRowGroupReader(rgIndex))
                    {
                        // Add columns to the DataTable
                        foreach (var field in dataFields)
                        {
                            dt.Columns.Add(field.Name, field.ClrType);
                        }

                        // Read each column asynchronously from this row group
                        Parquet.Data.DataColumn[] parquetColumns = new Parquet.Data.DataColumn[dataFields.Length];
                        for (int colIndex = 0; colIndex < dataFields.Length; colIndex++)
                        {
                            parquetColumns[colIndex] = await rowGroupReader.ReadColumnAsync(dataFields[colIndex]);
                        }

                        long rowCount = rowGroupReader.RowCount;

                        // Build rows from the column data
                        for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                        {
                            object[] rowValues = new object[dataFields.Length];
                            for (int colIndex = 0; colIndex < dataFields.Length; colIndex++)
                            {
                                rowValues[colIndex] = parquetColumns[colIndex].Data.GetValue(rowIndex);
                            }
                            dt.Rows.Add(rowValues);
                        }
                    }
                }
            }

            return dt;
        } 
        #endregion
    }
}
