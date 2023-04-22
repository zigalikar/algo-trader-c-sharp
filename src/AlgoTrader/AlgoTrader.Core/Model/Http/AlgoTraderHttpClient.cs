using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace AlgoTrader.Core.Model.Http
{
    public class AlgoTraderHttpClient
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private HttpClient _client;

        public AlgoTraderHttpClient()
        {
            _client = new HttpClient();
        }

        public void SetBearerToken(string token)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void SetHeader(string key, string value)
        {
            _client.DefaultRequestHeaders.Remove(key);
            _client.DefaultRequestHeaders.Add(key, value);
        }

        public async Task<T> GetAsync<T>(string url, IDictionary<string, string> headers = null) where T : class
        {
            try
            {
                var req = CreateHttpRequestMessage(HttpMethod.Get, url, headers);
                using (var res = await _client.SendAsync(req))
                {
                    var content = await res.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, "Exception in GetAsync");
                return null;
            }
        }

        //public Task<Stream> GetStreamAsync(string url)
        //{
        //    try
        //    {
        //        return _client.GetStreamAsync(url);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        logger.Error(ex, "Exception in GetStreamAsync");
        //        return null;
        //    }
        //}

        public Task PostAsync(string url) => PostAsync<object>(url, null);

        public Task<R> PostAsync<R>(string url, object body) where R : class => PostAsync<object, R>(url, body);

        public async Task<R> PostAsync<T, R>(string url, T body, IDictionary<string, string> headers = null) where T : class where R : class
        {
            try
            {
                var content = GetJsonHttpContent(body);
                var req = CreateHttpRequestMessage(HttpMethod.Post, url, headers, content);
                using (var res = await _client.SendAsync(req))
                    return await ProcessResponse<R>(res);
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, $"Exception in {nameof(PostAsync)}");
                return default;
            }
        }

        public Task PutAsync(string url) => PutAsync<object>(url, null);

        public Task<R> PutAsync<R>(string url, object body) where R : class => PutAsync<object, R>(url, body);

        public async Task<R> PutAsync<T, R>(string url, T body, IDictionary<string, string> headers = null) where T : class where R : class
        {
            try
            {
                var content = GetJsonHttpContent(body);
                var req = CreateHttpRequestMessage(HttpMethod.Put, url, headers, content);
                using (var res = await _client.SendAsync(req))
                    return await ProcessResponse<R>(res);
            }
            catch (System.Exception ex)
            {
                logger.Error(ex, $"Exception in {nameof(PostAsync)}");
                return default;
            }
        }

        private HttpContent GetJsonHttpContent(object body)
        {
            HttpContent content = null;
            if (body != null)
            {
                content = new StringContent(JsonConvert.SerializeObject(body, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.None }), Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            return content;
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string url, IDictionary<string, string> headers, HttpContent content = null)
        {
            var req = new HttpRequestMessage(method, url);
            if (headers != null)
            {
                foreach (var header in headers)
                    req.Headers.Add(header.Key, header.Value);
            }

            if (content != null)
                req.Content = content;

            return req;
        }

        private async Task<R> ProcessResponse<R>(HttpResponseMessage res) where R : class
        {
            if (res == null)
                return default;

            if (res.IsSuccessStatusCode)
            {
                var str = await res.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<R>(str);
            }
            else
                return default;
        }
    }
}
