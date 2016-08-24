using System.Net;
using System.Threading.Tasks;

namespace YouTackTimeReportsWeb.Services
{
    public interface IConnection
    {
        Task<bool> Authenticate(NetworkCredential credentials);
        void Logout();
        Task<string> Get(string request, object values);
        Task<T> GetJson<T>(string request, object values);
        Task<string> PostJson(string request, object data);
        Task<string> Delete(string request);
    }
}
