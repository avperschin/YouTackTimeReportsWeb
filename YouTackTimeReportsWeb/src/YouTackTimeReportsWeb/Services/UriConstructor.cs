using Flurl;
using System;

namespace YouTackTimeReportsWeb.Services
{
    public class UriConstructor : IUriConstructor
    {
        readonly string _host;
        readonly string _path;
        readonly int _port;
        readonly string _protocol;

        public UriConstructor(string protocol, string host, int port, string path)
        {
            _protocol = protocol;
            _port = port;
            _host = host;
            if (!String.IsNullOrEmpty(path))
            {
                _path = AddPrefixBar(path);
            }
            else
            {
                _path = null;
            }
        }


        /// <summary>
        /// Создает базовый Url для сервера, содержащий хост, порт и заданный запрос
        /// </summary>
        /// <param name="request">Заданный запрос</param>
        /// <returns>Url</returns>
        public Url ConstructBaseUri(string request)
        {
            var url = new Url(String.Format("{0}://{1}:{2}", _protocol, _host, _port));
            if (!String.IsNullOrEmpty(_path)) url.AppendPathSegment(_path);
            url.AppendPathSegment("rest");
            url.AppendPathSegment(request);
            return url;
        }

        string AddPrefixBar(string path)
        {
            if (path.Length > 0)
            {
                if (path[0] != '/')
                {
                    return '/' + path;
                }
            }
            return path;
        }
    }
}
