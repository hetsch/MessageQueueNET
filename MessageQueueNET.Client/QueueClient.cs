using MessageQueueNET.Client.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageQueueNET.Client
{
    public class QueueClient
    {
        private static HttpClient ReuseableHttpClient = new HttpClient();

        private readonly HttpClient _httpClient;
        private readonly string _serverUrl;
        private readonly string _queueName;

        private readonly string _clientId, _clientSecret;

        public QueueClient(string serverUrl, string queueName,
                           string clientId = "", string clientSecret = "",
                           HttpClient httpClient = null)
        {
            if (httpClient == null && ReuseableHttpClient == null)
            {
                ReuseableHttpClient = new HttpClient();
            }

            _httpClient = httpClient ?? ReuseableHttpClient;
            _serverUrl = serverUrl;
            _queueName = queueName;

            var uri = new Uri(_serverUrl);
            var userInfo = uri.UserInfo;

            if (String.IsNullOrEmpty(clientId) &&
                String.IsNullOrEmpty(clientSecret) &&
                !String.IsNullOrEmpty(userInfo) && userInfo.Contains(":"))
            {
                _clientId = userInfo.Substring(0, userInfo.IndexOf(':'));
                _clientSecret = userInfo.Substring(userInfo.IndexOf(':') + 1);

                _serverUrl = _serverUrl.Replace($"{ userInfo }@", "");
            }
            else
            {
                _clientId = clientId;
                _clientSecret = clientSecret;
            }
        }

        async public Task<IEnumerable<string>> DequeueAsync(int count = 1, bool register = false)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/dequeue/{ _queueName }?count={ count }&register={ register }"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    return JsonSerializer.Deserialize<IEnumerable<string>>(await httpResponse.Content.ReadAsStringAsync());
                }
            }
        }

        async public Task<bool> EnqueueAsync(IEnumerable<string> messages)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"{ _serverUrl }/queue/enqueue/{ _queueName }"))
            {
                ModifyHttpRequest(requestMessage);

                requestMessage.Content = new StringContent(
                    JsonSerializer.Serialize(messages),
                    Encoding.UTF8,
                    "application/json");

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    return Convert.ToBoolean(await httpResponse.Content.ReadAsStringAsync());
                }
            }
        }

        async public Task<IEnumerable<string>> AllMessagesAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/all/{ _queueName }"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    return JsonSerializer.Deserialize<IEnumerable<string>>(await httpResponse.Content.ReadAsStringAsync());
                }
            }
        }

        async public Task<int> LengthAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/length/{ _queueName }"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    var response = await httpResponse.Content.ReadAsStringAsync();
                    return Convert.ToInt32(response);
                }
            }
        }

        async public Task<bool> RemoveAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/remove/{ _queueName }"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    return JsonSerializer.Deserialize<bool>(await httpResponse.Content.ReadAsStringAsync());
                }
            }
        }

        async public Task<bool> RegisterAsync(int lifetimeSeconds = 0, int itemLifetimeSeconds = 0)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/register/{ _queueName }?lifetimeSeconds={ lifetimeSeconds }&itemLifetimeSeconds={ itemLifetimeSeconds }"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    return JsonSerializer.Deserialize<bool>(await httpResponse.Content.ReadAsStringAsync());
                }
            }
        }

        async public Task<QueueProperties> PropertiesAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/properties/{ _queueName }"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    string jsonResponse = await httpResponse.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<QueueProperties>(jsonResponse,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
            }
        }

        async public Task<IEnumerable<string>> QueueNamesAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{ _serverUrl }/queue/queuenames"))
            {
                ModifyHttpRequest(requestMessage);
                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);
                    return JsonSerializer.Deserialize<IEnumerable<string>>(await httpResponse.Content.ReadAsStringAsync());
                }
            }
        }

        #region Helper

        private void ModifyHttpRequest(HttpRequestMessage requestMessage)
        {
            if (!String.IsNullOrEmpty(_clientId) && !String.IsNullOrEmpty(_clientSecret))
            {
                // Add Basic Auth
                var authenticationString = $"{ _clientId }:{ _clientSecret }";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            }
        }

        private void CheckHttpResponse(HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Error connecting with queue message service. Status code: { httpResponse.StatusCode }");
            }
        }

        #endregion
    }
}
