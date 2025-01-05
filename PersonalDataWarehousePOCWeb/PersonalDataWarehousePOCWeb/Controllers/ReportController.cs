using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Data;
using Microsoft.Reporting.NETCore;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.ReportingServices.ReportProcessing.ReportObjectModel;

public class MyReportRequest
{
    public byte[]? RdlcBytes { get; set; }
    public string? Title { get; set; }
    public List<Dictionary<string, object>>? Rows { get; set; }
}

[ApiController]
[Route("[controller]")]
public class ReportsController : ControllerBase
{
    [HttpPost("GenerateReport")]
    public IActionResult GenerateReport([FromBody] MyReportRequest request)
    {
        // 1. Validate request
        if (request.RdlcBytes == null || request.RdlcBytes.Length == 0)
            return BadRequest("No RDLC data received.");

        if (request.Rows == null || request.Rows.Count == 0)
            return BadRequest("No row data received.");

        // 2. Convert rows to a DataTable
        var dataTable = ConvertToDataTable(request.Rows);

        // 3. Create the LocalReport
        using var report = new LocalReport();

        // 4. Load the RDLC definition from the byte array
        using var ms = new MemoryStream(request.RdlcBytes);
        using var sr = new StreamReader(ms);
        report.LoadReportDefinition(sr);

        // 5. Add the data source (the name "Items" must match your RDLC dataset)
        report.DataSources.Clear();
        report.DataSources.Add(new ReportDataSource("Items", dataTable));    

        // 6. Set the report parameter (if your RDLC uses "Title")
        if (!string.IsNullOrEmpty(request.Title))
        {
            report.SetParameters(new ReportParameter("Title", request.Title));
        }

        // 7. Render the report into PDF
        byte[] pdfBytes = report.Render("PDF");

        // 8. Return the PDF to the client
        return File(pdfBytes, "application/pdf");
    }

    private DataTable ConvertToDataTable(List<Dictionary<string, object>> rows)
    {
        // Create a DataTable
        DataTable dt = new DataTable("MyDataTable");

        if (rows.Count == 0) return dt;

        // Get all fields from the first row
        var fields = rows[0].Keys;

        // Add columns for all fields (replace spaces with underscores)
        foreach (var field in fields)
        {
            string columnName = field;
            dt.Columns.Add(columnName, typeof(object));
        }

        // Fill the DataTable rows
        foreach (var item in rows)
        {
            DataRow newRow = dt.NewRow();

            foreach (var field in fields)
            {
                string columnName = field;
                newRow[columnName] = (item[field] != null) ? item[field].ToString() : null;
            }

            dt.Rows.Add(newRow);
        }

        return dt;
    }
}