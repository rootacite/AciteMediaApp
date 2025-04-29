using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AciteMediaApp.Services
{
    public class ConnectivityService(HttpClient httpClient)
    {
        private HttpClient _httpClient = httpClient;

        public async Task<ConnectivityResult> CheckUrlWithTimeoutAsync(
            string url,
            int timeoutMilliseconds = 2000,
            CancellationToken cancellationToken = default)
        {
            // First check basic network connectivity
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                return new ConnectivityResult(false, "No internet connection");
            }

            try
            {
                using var timeoutCts = new CancellationTokenSource(timeoutMilliseconds);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    timeoutCts.Token,
                    cancellationToken);

                var stopwatch = Stopwatch.StartNew();

                // Use HEAD method to minimize data transfer
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                var response = await _httpClient.SendAsync(request, linkedCts.Token);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    return new ConnectivityResult(
                        true,
                        $"Success ({(int)response.StatusCode} {response.StatusCode})",
                        stopwatch.ElapsedMilliseconds);
                }

                return new ConnectivityResult(
                    false,
                    $"Request failed ({(int)response.StatusCode} {response.StatusCode})",
                    stopwatch.ElapsedMilliseconds);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new ConnectivityResult(false, "Connection timeout", timeoutMilliseconds);
            }
            catch (HttpRequestException ex)
            {
                return new ConnectivityResult(false, $"Request error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return new ConnectivityResult(false, $"Error: {ex.Message}");
            }
        }
    }

    public record ConnectivityResult(
        bool IsConnected,
        string Message,
        long? ResponseTimeMs = null);
}
