using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebUtil.HttpClient
{
    public static class HttpClientUtil
    {
        public static void SetDefaultHeader(this System.Net.Http.HttpClient client, HttpRequestMessage request, string userId)
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public static async Task<T> Request<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            Action<HttpRequestMessage> preAction = null)
        {
            using var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url);
            preAction?.Invoke(request);
            var response = await client.SendAsync(request);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public static async Task<T> RequestJson<T>(this IHttpClientFactory httpClientFactory,
                HttpMethod httpMethod,
                string url,
                string body)
        {
            using var client = httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(httpMethod, url);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public static async Task<T> RequestJson<T>(this IHttpClientFactory httpClientFactory,
            HttpMethod httpMethod,
            string url,
            object body)
        {
            return await httpClientFactory.RequestJson<T>(httpMethod, url, JsonConvert.SerializeObject(body));
        }
    }
}