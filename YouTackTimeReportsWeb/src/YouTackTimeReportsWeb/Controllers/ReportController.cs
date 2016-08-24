using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;
using YouTackTimeReportsWeb.Models;
using YouTackTimeReportsWeb.Services;

namespace YouTackTimeReportsWeb.Controllers
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private IConnection _connection;
        private IReportService _report;
        [HttpGet]
        public async Task<IActionResult> Get(SearchParams parameters)
        {
            _connection = new Connection(parameters.host, parameters.port, parameters.isssl, parameters.path);
            try
            {
                NetworkCredential _credentials = new NetworkCredential();
                _credentials.UserName = parameters.login;
                _credentials.Password = parameters.password;
                bool IsAuthenticated = await _connection.Authenticate(_credentials);
                if(IsAuthenticated)
                {
                    _report = new ReportService(_connection);
                    var report = await _report.GetReports(parameters);
                    return Json(new { status = 1, report = report });
                }
                else
                {
                    return Json(new { status = 0, error = "Не удачная попытка авторизации на сервере! Повторите позже." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = 0, error = ex.Message });
            }
            finally
            {
                _connection.Logout();
            }
        }
    }
}
