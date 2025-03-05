using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PersonalDataWarehousePOCMAUI.Services
{
    public class ReportController : ControllerBase
    {
        [HttpGet("/api/Status")]
        public IActionResult GetStatus()
        {
            return Ok(new { Status = "Working" });
        }

        // Method that responds to: https://localhost:5000/api/GetData?database={database}&datasource={Datasource}
        [HttpGet("/api/GetData")]
        public IActionResult GetData(string database, string datasource)
        {
            return Ok(new { Database = database, Datasource = datasource });
        }
    }
}
