using AmuleRemoteControl.Components.Interfaces;
using Microsoft.Extensions.Logging;

namespace AmuleRemoteControl.Components.Service
{
    public class NetworkHelper : INetworkHelper
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NetworkHelper> _logger;
        private readonly IUtilityServices _utilityServices;

        public NetworkHelper(HttpClient httpClient, ILogger<NetworkHelper> logger, IUtilityServices utilityServices)
        {
            _httpClient = httpClient;
            _logger = logger;
            _utilityServices = utilityServices;
        }

        public async Task<string> SendRequest()
        {
            try
            {
                var url = _utilityServices.ApiUrl;

                if (url.Contains("myLocalaMuleAddress"))
                {
                    return "Default Network Setting";
                }

                using var req = new HttpRequestMessage(HttpMethod.Get, url);

                var result = await _httpClient.SendAsync(req);

                if (result.IsSuccessStatusCode)
                {
                    return result.StatusCode.ToString();
                }

                _logger.LogWarning($"Get request response: {result.StatusCode}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Request error: {ex.Message}");
                return string.Empty;
            }

        }

        public async Task<string> SendRequest(string page)
        {
            var url = _utilityServices.ApiUrl + page;

            var result = await _httpClient.GetStringAsync(url);
            return result;
        }

        public async Task<string> SendRequest(string page, Dictionary<string, string> parameters)
        {
            //var url = _utilityServices.ApiUrl + page;

            //var result = await httpClient.GetStringAsync(url);
            //return result;

            try
            {
                var url = _utilityServices.ApiUrl + page;
                using var req = new HttpRequestMessage(HttpMethod.Get, url) { Content = new FormUrlEncodedContent(parameters) };
                var result = await _httpClient.SendAsync(req);

                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }

                _logger.LogWarning($"Get command response: {result.StatusCode}");
                return string.Empty;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error get command: {ex.Message}");
                return string.Empty;

            }
        }

        public async Task<string> PostRequest(string page, Dictionary<string, string> parameters)
        {
            try
            {
                var url = _utilityServices.ApiUrl + page;
                using var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(parameters) };
                var result = await _httpClient.SendAsync(req);

                if (result.IsSuccessStatusCode)
                {
                    return await result.Content.ReadAsStringAsync();
                }

                _logger.LogWarning($"Post command response: {result.StatusCode}");
                return string.Empty;

            }
            catch (Exception ex)
            {
                _logger.LogError($"Error post command: {ex.Message}");
                return string.Empty;

            }

        }

    }
}
