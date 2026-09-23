using System.Text.Json;
using NASA_APOD_Gallery.Models;
using NASA_APOD_Gallery.Repositories;

namespace NASA_APOD_Gallery.Services
{
    public class NasaApodService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ApodRepository _apodRepository;

        public NasaApodService(
            HttpClient httpClient,
            IConfiguration configuration,
            ApodRepository apodRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apodRepository = apodRepository;
        }

        public async Task<List<ApodDto>> GetApodAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var apiKey = _configuration["NasaApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new Exception(
                    "NASA API key was not found. Check User Secrets.");
            }

            if (startDate > endDate)
            {
                throw new Exception(
                    "Start date cannot be after end date.");
            }

            var url =
                $"planetary/apod" +
                $"?api_key={apiKey}" +
                $"&start_date={startDate:yyyy-MM-dd}" +
                $"&end_date={endDate:yyyy-MM-dd}";

            // First attempt: try the range request with simple retries for transient issues.
            HttpResponseMessage? rangeResponse = null;
            var maxAttempts = 2;

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    rangeResponse = await _httpClient.GetAsync(url);

                    if (rangeResponse.IsSuccessStatusCode)
                    {
                        var rangeJson = await rangeResponse.Content.ReadAsStringAsync();
                        var rangeData = JsonSerializer.Deserialize<List<ApodDto>>(
                            rangeJson,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        return rangeData ?? new List<ApodDto>();
                    }

                    // If server error (5xx) and we can retry, back off and retry once.
                    var status = (int)rangeResponse.StatusCode;
                    if (status >= 500 && attempt < maxAttempts)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                        continue;
                    }

                    // Non-retryable or final attempt failed — capture response content.
                    var errorContent = await rangeResponse.Content.ReadAsStringAsync();
                    // If response is not JSON, treat as server-side issue and fall back below.
                    // For 4xx errors, surface immediately.
                    if (status >= 400 && status < 500)
                    {
                        throw new Exception(
                            $"NASA API returned an error.\n\nStatus Code: {status}\nStatus: {rangeResponse.StatusCode}\n\nResponse:\n{errorContent}");
                    }

                    // For 5xx on final attempt, break to fallback to per-day fetch.
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw new Exception(
                        "The NASA API request was canceled or timed out. Try a smaller date range or check your network connection.");
                }
                catch (HttpRequestException) when (attempt < maxAttempts)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                    continue;
                }
            }

            // If we reach here, the range request either failed with 5xx or returned HTML.
            // Fallback: fetch each date individually so we can save partial results.
            var results = new List<ApodDto>();
            var current = startDate.Date;

            while (current <= endDate.Date)
            {
                var singleUrl = $"planetary/apod?api_key={apiKey}&date={current:yyyy-MM-dd}";

                try
                {
                    HttpResponseMessage singleResp = await _httpClient.GetAsync(singleUrl);
                    if (!singleResp.IsSuccessStatusCode)
                    {
                        // Skip this date on server error but continue with others.
                        current = current.AddDays(1);
                        continue;
                    }

                    var singleJson = await singleResp.Content.ReadAsStringAsync();

                    // APOD single-date endpoint returns a single object, not an array.
                    var singleApod = JsonSerializer.Deserialize<ApodDto>(
                        singleJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (singleApod != null)
                    {
                        results.Add(singleApod);
                    }
                }
                catch
                {
                    // On transient failure for this date, just skip and continue.
                }

                // Small delay to avoid hammering the API.
                await Task.Delay(250);
                current = current.AddDays(1);
            }

            return results;
        }

        public async Task FetchAndSaveApodAsync(
            DateTime startDate,
            DateTime endDate)
        {
            var apodData =
                await GetApodAsync(startDate, endDate);

            foreach (var apod in apodData)
            {
                await _apodRepository.SaveApodAsync(apod);
            }
        }
    }
}