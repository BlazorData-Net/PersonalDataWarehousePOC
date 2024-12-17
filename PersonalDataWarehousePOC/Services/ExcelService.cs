namespace PersonalDataWarehousePOC.Services
{
    using System.Data;
    using System.IO;
    using ExcelDataReader;

    public class ExcelService
    {
        public async Task<DataSet> ReadExcelFileAsync(Stream fileStream)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var reader = ExcelReaderFactory.CreateReader(memoryStream);
            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });

            return result;
        }
    }
}
