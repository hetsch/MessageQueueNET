using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using System.Text;

namespace MessageQueueNET.Client
{
    public class QueueClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly string _queueName;

        public QueueClient(string serverUrl, string indexName, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _serverUrl = serverUrl;
            _queueName = indexName;
        }

        async public Task<IEnumerable<string>> Dequeue(int count = 1)
        {
            using (var httpResponse = await _httpClient.GetAsync($"{ _serverUrl }/queue/dequeue/{ _queueName }?count={ count }"))
            {
                return JsonSerializer.Deserialize<IEnumerable<string>>(await httpResponse.Content.ReadAsStringAsync());
            }
        }

        async public Task<bool> Enqueue(IEnumerable<string> messages)
        {
            HttpContent postContent = new StringContent(
                    JsonSerializer.Serialize(messages),
                    Encoding.UTF8,
                    "application/json");

            using (var httpResponse = await _httpClient.PutAsync($"{ _serverUrl }/queue/enqueue/{ _queueName }", postContent))
            {
                return Convert.ToBoolean(await httpResponse.Content.ReadAsStringAsync());
            }
        }

        async public Task<IEnumerable<string>> AllMessages()
        {
            using (var httpResponse = await _httpClient.GetAsync($"{ _serverUrl }/queue/all/{ _queueName }"))
            {
                return JsonSerializer.Deserialize<IEnumerable<string>>(await httpResponse.Content.ReadAsStringAsync());
            }
        }

        async public Task<int> Count()
        {
            using (var httpResponse = await _httpClient.GetAsync($"{ _serverUrl }/queue/length/{ _queueName }"))
            {
                return Convert.ToInt32(await httpResponse.Content.ReadAsStringAsync());
            }
        }

        async public Task<bool> Remove()
        {
            using (var httpResponse = await _httpClient.GetAsync($"{ _serverUrl }/queue/remove/{ _queueName }"))
            {
                return JsonSerializer.Deserialize<bool>(await httpResponse.Content.ReadAsStringAsync());
            }
        }

        async public Task<bool> Register()
        {
            using (var httpResponse = await _httpClient.GetAsync($"{ _serverUrl }/queue/register/{ _queueName }"))
            {
                return JsonSerializer.Deserialize<bool>(await httpResponse.Content.ReadAsStringAsync());
            }
        }

        async public Task<IEnumerable<string>> QueueNames()
        {
            using (var httpResponse = await _httpClient.GetAsync($"{ _serverUrl }/queue/queuenames"))
            {
                return JsonSerializer.Deserialize<IEnumerable<string>>(await httpResponse.Content.ReadAsStringAsync());
            }
        }
    }
}
