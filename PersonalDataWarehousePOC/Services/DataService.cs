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

        public async Task WriteDataTableToParquetAsync(DataTable currentDataTable, string outputPath)
        {
            if (currentDataTable == null) throw new ArgumentNullException(nameof(currentDataTable));

            int rowCount = currentDataTable.Rows.Count;
            int columnCount = currentDataTable.Columns.Count;

            // 1) Define the schema
            //    Here, we assume everything is a string column for simplicity.
            DataField[] dataFields = new DataField[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                string columnName = currentDataTable.Columns[i].ColumnName;
                dataFields[i] = new DataField<string>(columnName);
            }

            var schema = new ParquetSchema(dataFields);

            // 2) Build each DataColumn from the DataTable
            //    We'll gather the values for each column into a string[] array
            //    and apply any "cleanup" of newlines, tabs, etc.
            DataColumn[] parquetColumns = new DataColumn[columnCount];
            for (int colIndex = 0; colIndex < columnCount; colIndex++)
            {
                // Prepare an array to hold all row values for this column
                var columnValues = new string[rowCount];

                for (int rowIndex = 0; rowIndex < rowCount; rowIndex++)
                {
                    object cellValue = currentDataTable.Rows[rowIndex][colIndex];
                    string value = cellValue?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.Replace("\r\n", " ")
                                     .Replace("\t", " ")
                                     .Replace("\r", " ")
                                     .Replace("\n", " ")
                                     .Trim();
                    }

                    columnValues[rowIndex] = value;
                }

                // Create a DataColumn object associated with the schema's DataField
                parquetColumns[colIndex] = new DataColumn(schema.DataFields[colIndex], columnValues);
            }

            // 3) Write to Parquet using an async pattern, row-group style
            using (Stream fileStream = File.OpenWrite(outputPath))
            {
                // CreateAsync can be awaited
                using (ParquetWriter parquetWriter = await ParquetWriter.CreateAsync(schema, fileStream))
                {
                    // Optional: set compression to Gzip with optimal level
                    parquetWriter.CompressionMethod = CompressionMethod.Gzip;
                    parquetWriter.CompressionLevel = CompressionLevel.Optimal;

                    // Create a row group and write each column
                    using (ParquetRowGroupWriter groupWriter = parquetWriter.CreateRowGroup())
                    {
                        foreach (DataColumn col in parquetColumns)
                        {
                            await groupWriter.WriteColumnAsync(col);
                        }
                    }
                }
            }
        }
    }
}