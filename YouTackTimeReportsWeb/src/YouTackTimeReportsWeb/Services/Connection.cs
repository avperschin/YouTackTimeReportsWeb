using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Dynamic;
using System.Net;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace YouTackTimeReportsWeb.Services
{
    public class Connection : IConnection
    {
        readonly IUriConstructor _uriConstructor;
        FlurlClient _client;
        public bool IsAuthenticated { get; private set; }

        public Connection(string host, int port = 80, bool useSsl = false, string path = null)
        {
            var protocol = "http";


            if (useSsl)
            {
                protocol = "https";
            }

            _uriConstructor = new UriConstructor(protocol, host, port, path);
        }
        public async Task<string> PostJson(string request, object data)
        {
            try
            {
                var baseUri = _uriConstructor.ConstructBaseUri(request);
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var response = await baseUri.WithClient(_client).PostJsonAsync(data);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                throw new Exception(response.ReasonPhrase);
            }
            catch (FlurlHttpException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task<T> GetJson<T>(string request, object values)
        {
            try
            {
                var baseUri = _uriConstructor.ConstructBaseUri(request);
                if (values != null)
                {
                    baseUri.SetQueryParams(values);
                }
                var response = await baseUri.WithClient(_client).GetJsonAsync<T>();
                return response;
            }
            catch (FlurlHttpException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task<string> Delete(string request)
        {
            try
            {
                var baseUri = _uriConstructor.ConstructBaseUri(request);
                var response = await baseUri.WithClient(_client).DeleteAsync();
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (FlurlHttpException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task<string> Get(string request, object values)
        {
            try
            {
                var baseUri = _uriConstructor.ConstructBaseUri(request);
                if (values != null)
                {
                    baseUri.SetQueryParams(values);
                }
                var response = await baseUri.WithClient(_client).GetAsync();
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
            catch (FlurlHttpException exception)
            {
                throw new Exception(exception.Message);
            }
        }

        public async Task<bool> Authenticate(NetworkCredential credentials)
        {
            return await Authenticate(credentials.UserName, credentials.Password);
        }
        private async Task<bool> Authenticate(string username, string password)
        {
            IsAuthenticated = false;

            dynamic credentials = new ExpandoObject();

            credentials.login = username;
            credentials.password = password;

            try
            {
                var baseUri = _uriConstructor.ConstructBaseUri("user/login");
                _client = new FlurlClient().EnableCookies();

                var response = await baseUri.WithClient(_client).PostUrlEncodedAsync(new
                {
                    credentials.login, credentials.password
                });
                if (response.IsSuccessStatusCode)
                {
                    string message = await response.Content.ReadAsStringAsync();
                    if (message.Contains("ok"))
                    {
                        IsAuthenticated = true;
                        return IsAuthenticated;
                    }
                    else
                    {
                        return IsAuthenticated;
                    }
                }
                else
                {
                    throw new AuthenticationException(response.ReasonPhrase);
                }
            }
            catch (FlurlHttpException exception)
            {
                throw new AuthenticationException(exception.Message);
            }
        }

        public void Logout()
        {
            IsAuthenticated = false;
            _client?.Dispose();
        }
    }
}
