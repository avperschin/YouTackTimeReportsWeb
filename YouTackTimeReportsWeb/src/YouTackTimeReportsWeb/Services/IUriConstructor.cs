using Flurl;

namespace YouTackTimeReportsWeb.Services
{
    public interface IUriConstructor
    {
        Url ConstructBaseUri(string request);
    }
}
