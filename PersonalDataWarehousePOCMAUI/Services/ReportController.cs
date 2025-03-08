using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace PersonalDataWarehousePOCMAUI.Services
{
    // This attribute restricts access to localhost only
    // It is applied to the entire controller 
    // It checks the remote IP address of the request and returns a 403 Forbidden response if it's not localhost
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
            var reportText = """"                
                <Query>
                <XmlData>
                <Customers xmlns="http://www.adventure-works.com">  
                   <Customer ID="11">  
                      <FirstName>Bobby</FirstName>  
                      <LastName>Moore</LastName>  
                      <Orders>  
                         <Order ID="1" Qty="6">Chair</Order>  
                         <Order ID="2" Qty="1">Table</Order>  
                      </Orders>  
                      <Returns>  
                         <Return ID="1" Qty="2">Chair</Return>  
                      </Returns>  
                   </Customer>  
                   <Customer ID="20">  
                      <FirstName>Crystal</FirstName>  
                      <LastName>Hu</LastName>  
                      <Orders>  
                         <Order ID="8" Qty="2">Sofa</Order>  
                      </Orders>  
                      <Returns/>  
                   </Customer>  
                   <Customer ID="33">  
                      <FirstName>Wyatt</FirstName>  
                      <LastName>Diaz</LastName>  
                      <Orders>  
                         <Order ID="15" Qty="2">EndTables</Order>  
                      </Orders>  
                      <Returns/>  
                   </Customer>  
                </Customers>  
                </XmlData>
                </Query>
                """";

            return Ok(reportText);
        }
    }
}
