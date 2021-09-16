using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace RestClient
{
    public class RequestMessageBuilder
    {
        private HttpMethod _httpMethod;
        private string _contentType;
        private object _content;
        private HttpRequestMessage _httpRequestMessage;
        private Uri _uri;

        public static RequestMessageBuilder New()
        {
            return new RequestMessageBuilder();
        }

        public RequestMessageBuilder WithHttpMethod(HttpMethod httpMethod)
        {
            _httpMethod = httpMethod;
            return this;
        }

        public RequestMessageBuilder WithContentType(string contentType)
        {
            _contentType = contentType;
            return this;
        }

        public RequestMessageBuilder WithContent(object content)
        {
            _content = content;
            return this;
        }

        public RequestMessageBuilder WithUri(Uri uri)
        {
            _uri = uri;
            return this;
        }

        public HttpRequestMessage Build()
        {
            _httpRequestMessage = new HttpRequestMessage(_httpMethod, _uri);
            if (_content != null)
            {
                if (_content is FormUrlEncodedContent)
                {
                    _httpRequestMessage.Content = (FormUrlEncodedContent)_content;
                }
                else
                {
                    _httpRequestMessage.Content = SerializeContent(_content, _contentType);
                }
            }

            return _httpRequestMessage;
        }

        private static ByteArrayContent SerializeContent(object content, string contentType)
        {
            var jsonContent = JsonConvert.SerializeObject(content);
            var buffer = Encoding.UTF8.GetBytes(jsonContent);
            var byteContent = new ByteArrayContent(buffer);

            if (!string.IsNullOrWhiteSpace(contentType))
            {
                byteContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            }

            return byteContent;
        }
    }
}
