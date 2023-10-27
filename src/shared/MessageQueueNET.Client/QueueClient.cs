using MessageQueueNET.Client.Extensions;
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
        private readonly JsonSerializerOptions _jsonOptions;

        public QueueClient(string serverUrl, string queueName,
                           string clientId = "", string clientSecret = "",
                           HttpClient? httpClient = null)
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

                _serverUrl = _serverUrl.Replace($"{userInfo}@", "");
            }
            else
            {
                _clientId = clientId;
                _clientSecret = clientSecret;
            }

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        async public Task<InfoResult> ApiInfoAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/info"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<InfoResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<MessagesResult> DequeueAsync(int count = 1, bool register = false)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/dequeue/{_queueName}?count={count}&register={register}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<MessagesResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<ApiResult> ConfirmDequeueAsync(Guid messageId)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/confirmdequeue/{_queueName}?messageId={messageId}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<ApiResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<ApiResult> EnqueueAsync(IEnumerable<string> messages)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, $"{_serverUrl}/queue/enqueue/{_queueName}"))
            {
                ModifyHttpRequest(requestMessage);

                requestMessage.Content = new StringContent(
                    JsonSerializer.Serialize(messages),
                    Encoding.UTF8,
                    "application/json");

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<ApiResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<MessagesResult> AllMessagesAsync(int max = 0, bool unconfirmedOnly = false)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/all/{_queueName}?max={max}&unconfirmedOnly={unconfirmedOnly}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<MessagesResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<QueueLengthResult> LengthAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/length/{_queueName}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<QueueLengthResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);;
                }
            }
        }

        async public Task<ApiResult> RemoveAsync(RemoveType removeType = RemoveType.Queue)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/remove/{_queueName}?removeType={removeType}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<ApiResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<QueuePropertiesResult> RegisterAsync(int? lifetimeSeconds = null,
                                                               int? itemLifetimeSeconds = null,
                                                               int? confirmationPeriodSeconds = null,
                                                               int? maxUnconfirmedItems = null,
                                                               bool? suspendEnqueue = null,
                                                               bool? suspendDequeue = null)
        {
            List<string> urlParameters = new List<string>();

            if (lifetimeSeconds.HasValue)
            {
                urlParameters.Add($"lifetimeSeconds={lifetimeSeconds.Value}");
            }
            if (itemLifetimeSeconds.HasValue)
            {
                urlParameters.Add($"itemLifetimeSeconds={itemLifetimeSeconds.Value}");
            }
            if (confirmationPeriodSeconds.HasValue)
            {
                urlParameters.Add($"confirmationPeriodSeconds={confirmationPeriodSeconds.Value}");
            }
            if (maxUnconfirmedItems.HasValue)
            {
                urlParameters.Add($"maxUnconfirmedItems={maxUnconfirmedItems.Value}");
            }
            if (suspendEnqueue.HasValue)
            {
                urlParameters.Add($"suspendEnqueue={suspendEnqueue.Value}");
            }
            if (suspendDequeue.HasValue)
            {
                urlParameters.Add($"suspendDequeue={suspendDequeue.Value}");
            }

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/register/{_queueName}?{String.Join("&", urlParameters)}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<QueuePropertiesResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        async public Task<QueuePropertiesResult> PropertiesAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/properties/{_queueName}"))
            {
                ModifyHttpRequest(requestMessage);

                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    string jsonResponse = await httpResponse.Content.ReadAsStringAsync();

                    return Result.Deserialize<QueuePropertiesResult>(jsonResponse, _jsonOptions);
                }
            }
        }

        async public Task<QueueNamesResult> QueueNamesAsync()
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_serverUrl}/queue/queuenames"))
            {
                ModifyHttpRequest(requestMessage);
                using (var httpResponse = await _httpClient.SendAsync(requestMessage))
                {
                    CheckHttpResponse(httpResponse);

                    return Result.Deserialize<QueueNamesResult>(await httpResponse.Content.ReadAsStringAsync(), _jsonOptions);
                }
            }
        }

        #region Helper

        private void ModifyHttpRequest(HttpRequestMessage requestMessage)
        {
            if (!String.IsNullOrEmpty(_clientId) && !String.IsNullOrEmpty(_clientSecret))
            {
                // Add Basic Auth
                var authenticationString = $"{_clientId}:{_clientSecret}";
                var base64EncodedAuthenticationString = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(authenticationString));

                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
            }
        }

        private void CheckHttpResponse(HttpResponseMessage httpResponse)
        {
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Error connecting with queue message service. Status code: {httpResponse.StatusCode}");
            }
        }

        #endregion
    }
}
