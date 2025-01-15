﻿namespace PersonalDataWarehousePOC.Services
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
    using OfficeOpenXml;

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

        #region public async Task<DataSet> ConvertParquetTableToDataSetAsync(Parquet.Rows.Table table)
        public async Task<DataSet> ConvertParquetTableToDataSetAsync(Parquet.Rows.Table table, string filename)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException("Filename cannot be null or whitespace.", nameof(filename));

            var dt = new DataTable(filename);

            // Retrieve schema information
            DataField[] dataFields = table.Schema.GetDataFields();

            // Add columns to the DataTable once
            foreach (var field in dataFields)
            {
                // Handle nullable types
                Type columnType = Nullable.GetUnderlyingType(field.ClrType) ?? field.ClrType;
                dt.Columns.Add(field.Name, columnType);
            }

            // Optimize DataTable loading
            dt.BeginLoadData();

            // Convert table to stream
            using var ms = new MemoryStream();
            await table.WriteAsync(ms);

            try
            {
                using (var reader = await ParquetReader.CreateAsync(ms))
                {
                    for (int rgIndex = 0; rgIndex < reader.RowGroupCount; rgIndex++)
                    {
                        using (var rowGroupReader = reader.OpenRowGroupReader(rgIndex))
                        {
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
            }
            finally
            {
                // End optimized loading
                dt.EndLoadData();
            }

            // Convert DataTable to DataSet
            var ds = new DataSet();
            ds.Tables.Add(dt);

            return ds;
        }
        #endregion

        #region public async Task WriteDataTableToParquetAsync(DataTable CurrentDataTable, string CurrentTableName)
        public async Task WriteDataTableToParquetAsync(DataTable CurrentDataTable, string CurrentTableName)
        {
            if (CurrentDataTable == null) throw new ArgumentNullException(nameof(CurrentDataTable));

            int columnCount = CurrentDataTable.Columns.Count;

            // Prepare fields for all columns
            var parquetFields = new List<Field>(columnCount);

            for (int i = 0; i < columnCount; i++)
            {
                string columnName = CurrentDataTable.Columns[i].ColumnName;
                parquetFields.Add(new DataField<string>(columnName));
            }

            ParquetSchema parquetSchema = new ParquetSchema(parquetFields);

            var parquetTable = new Parquet.Rows.Table(parquetSchema);

            foreach (DataRow dataRow in CurrentDataTable.Rows)
            {
                // Initialize the row with a pre-sized string array
                var row = new Parquet.Rows.Row(new string[columnCount]);

                for (int j = 0; j < columnCount; j++)
                {
                    string value = dataRow[j]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Using chained Replace calls for clarity and performance
                        value = value.Replace("\r\n", " ")
                                     .Replace("\t", " ")
                                     .Replace("\r", " ")
                                     .Replace("\n", " ")
                                     .Trim();
                    }
                    row[j] = value;
                }

                parquetTable.Add(row);
            }

            using var ms = new MemoryStream();

            await parquetTable.WriteAsync(ms); // Await for async write

            ms.Position = 0;

            // Data Directory
            String folderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/PersonalDataWarehouse/Parquet";
            
            string fileName = $"{folderPath}/{CurrentTableName}.parquet";

            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                ms.CopyTo(fileStream);
            }
        }
        #endregion

        #region public async Task<byte[]> ExportDataTableToParquetAsync(DataTable CurrentDataTable, string CurrentTableName)
        public async Task<byte[]> ExportDataTableToParquetAsync(DataTable CurrentDataTable, string CurrentTableName)
        {
            if (CurrentDataTable == null) throw new ArgumentNullException(nameof(CurrentDataTable));
            int columnCount = CurrentDataTable.Columns.Count;

            // Prepare fields for all columns
            var parquetFields = new List<Field>(columnCount);
            for (int i = 0; i < columnCount; i++)
            {
                string columnName = CurrentDataTable.Columns[i].ColumnName;
                parquetFields.Add(new DataField<string>(columnName));
            }

            ParquetSchema parquetSchema = new ParquetSchema(parquetFields);

            var parquetTable = new Parquet.Rows.Table(parquetSchema);

            foreach (DataRow dataRow in CurrentDataTable.Rows)
            {
                // Initialize the row with a pre-sized string array
                var row = new Parquet.Rows.Row(new string[columnCount]);
                for (int j = 0; j < columnCount; j++)
                {
                    string value = dataRow[j]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Using chained Replace calls for clarity and performance
                        value = value.Replace("\r\n", " ")
                                     .Replace("\t", " ")
                                     .Replace("\r", " ")
                                     .Replace("\n", " ")
                                     .Trim();
                    }
                    row[j] = value;
                }
                parquetTable.Add(row);
            }

            using var ms = new MemoryStream();

            await parquetTable.WriteAsync(ms);

            return ms.ToArray();
        } 
        #endregion

        public async Task<byte[]> ExportDataTableToExcelAsync(DataTable CurrentDataTable, string CurrentTableName)
        {
            //if (CurrentDataTable == null) throw new ArgumentNullException(nameof(CurrentDataTable));
            //var excelPackage = new ExcelPackage();

            //var ws = excelPackage.Workbook.Worksheets.Add(CurrentTableName);

            //// Add the headers
            //for (int i = 0; i < CurrentDataTable.Columns.Count; i++)
            //{
            //    ws.Cells[1, i + 1].Value = CurrentDataTable.Columns[i].ColumnName;
            //}

            //// Add the data
            //for (int i = 0; i < CurrentDataTable.Rows.Count; i++)
            //{
            //    for (int j = 0; j < CurrentDataTable.Columns.Count; j++)
            //    {
            //        ws.Cells[i + 2, j + 1].Value = CurrentDataTable.Rows[i][j];
            //    }
            //}

            //return excelPackage.GetAsByteArray();
        }

        // Utility

        #region public static string FirstCharToUpper(string input)
        public static string FirstCharToUpper(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        } 
        #endregion
    }
}