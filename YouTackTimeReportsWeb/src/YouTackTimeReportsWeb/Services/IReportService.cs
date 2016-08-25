using System.Threading.Tasks;
using YouTackTimeReportsWeb.Models;

namespace YouTackTimeReportsWeb.Services
{
    public interface IReportService
    {
        Task<FinalReport> GetReports(SearchParams parameters);
    }
}
