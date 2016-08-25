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
            _connection = new Connection(parameters.Host, parameters.Port, parameters.IsSsl, parameters.Path);
            try
            {
                NetworkCredential credentials = new NetworkCredential
                {
                    UserName = parameters.Login,
                    Password = parameters.Password
                };
                bool isAuthenticated = await _connection.Authenticate(credentials);
                if(isAuthenticated)
                {
                    _report = new ReportService(_connection);
                    var finalReport = await _report.GetReports(parameters);
                    return Json(new { status = 1, report = finalReport });
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
