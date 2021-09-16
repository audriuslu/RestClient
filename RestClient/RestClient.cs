using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RestClient
{
    public class RestClient
    {
        private static HttpClient _client;
        private X509Certificate _certificate;
        private string _baseAddress;
        private string _requestContentType;
        private NetworkCredential _networkCredentials;

        public static RestClient New()
        {
            return new RestClient();
        }

        public RestClient Build()
        {
            if (_certificate != null || _networkCredentials != null)
            {
                var requestHandler = new HttpClientHandler();
                if (_certificate != null)
                {
                    requestHandler.ClientCertificates.Add(_certificate);
                }

                if (_networkCredentials != null)
                {
                    requestHandler.Credentials = _networkCredentials;
                }

                _client = new HttpClient(requestHandler);
            }
            else
            {
                _client = new HttpClient();
            }

            _client.BaseAddress = new Uri(_baseAddress);

            if (!string.IsNullOrWhiteSpace(_requestContentType))
            {
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_requestContentType));
            }

            return this;
        }

        public void SetBearerRequestToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }


        public RestClient WithCertificate(X509Certificate certificate)
        {
            _certificate = certificate;
            return this;
        }

        public RestClient WithUserAndPassword(string userName, string password)
        {
            _networkCredentials = new NetworkCredential(userName, password);
            return this;
        }

        public RestClient WithBaseAddress(string baseAddress)
        {
            _baseAddress = baseAddress;
            return this;
        }

        public RestClient WithRequestContentType(string contentType)
        {
            _requestContentType = contentType;
            return this;
        }

        public async Task<HttpResponseMessage> GetAsync(string apiMethod)
        {
            return await MakeRequestAsync(HttpMethod.Get, apiMethod, null).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PostAsync(string apiMethod, object content)
        {
            return await MakeRequestAsync(HttpMethod.Post, apiMethod, content).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> PutAsync(string apiMethod, object content)
        {
            return await MakeRequestAsync(HttpMethod.Put, apiMethod, content).ConfigureAwait(false);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string apiMethod)
        {
            return await MakeRequestAsync(HttpMethod.Delete, apiMethod, null).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string apiMethod)
        {
            return await MakeRequestAsync<T>(HttpMethod.Get, apiMethod, null).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string apiMethod, object content)
        {
            return await MakeRequestAsync<T>(HttpMethod.Post, apiMethod, content).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(string apiMethod, object content)
        {
            return await MakeRequestAsync<T>(HttpMethod.Put, apiMethod, content).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string apiMethod)
        {
            return await MakeRequestAsync<T>(HttpMethod.Delete, apiMethod, null).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> MakeRequestAsync(HttpMethod httpMethod, string apiMethod, object content)
        {
            using (var requestMessage = RequestMessageBuilder.New()
                .WithHttpMethod(httpMethod)
                .WithUri(new Uri(string.Concat(_baseAddress, apiMethod)))
                .WithContentType(_requestContentType)
                .WithContent(content)
                .Build())
            {
                return await _client.SendAsync(requestMessage).ConfigureAwait(false);
            }
        }

        private async Task<T> MakeRequestAsync<T>(HttpMethod httpMethod, string apiMethod, object content = null)
        {
            using (var requestMessage = RequestMessageBuilder.New()
                .WithHttpMethod(httpMethod)
                .WithUri(new Uri(string.Concat(_baseAddress, apiMethod)))
                .WithContentType(_requestContentType)
                .WithContent(content)
                .Build())
            {
                using (var response = await _client.SendAsync(requestMessage).ConfigureAwait(false))
                {
                    return await ProcessResponse<T>(response).ConfigureAwait(false);
                }
            }
        }

        private async Task<T> ProcessResponse<T>(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.Format("Error occurred while calling API: {0}", response.ReasonPhrase));
            }

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(result);
        }
    }
}
