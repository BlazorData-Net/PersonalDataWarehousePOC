using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace PersonalDataWarehousePOCMAUI.Services
{
    public class LocalhostOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var remoteIp = context.HttpContext.Connection.RemoteIpAddress;

            // Check both IPv4 and IPv6 loopback addresses
            if (!IPAddress.IsLoopback(remoteIp))
            {
                context.Result = new ForbidResult();
            }
        }
    }

    [LocalhostOnly]
    public class ReportController : ControllerBase
    {
        [HttpGet("/api/GetStatus")]
        public IActionResult GetStatus()
        {
            return Ok(new { Status = "Working" });
        }

        [HttpGet("/api/GetData")]
        public IActionResult GetData(string database, string datasource)
        {
            return Ok(new { Database = database, Datasource = datasource });
        }
    }
}
