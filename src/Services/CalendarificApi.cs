using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Models;

namespace HolidayApiComparer.Services;

public class CalendarificApi : IHolidayProvider
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public CalendarificApi()
    {
        _apiUrl = Environment.GetEnvironmentVariable("CALENDARIFIC_API_URL") 
                  ?? throw new ArgumentNullException("CALENDARIFIC_API_URL environment variable is missing.");
        _apiKey = Environment.GetEnvironmentVariable("CALENDARIFIC_API_KEY") 
                  ?? throw new ArgumentNullException("CALENDARIFIC_API_KEY environment variable is missing.");
        _httpClient = new HttpClient();
    }

    public async Task<List<Holiday>> GetNatHolidaysForCountryForYear(string countryCode, int year)
    {
        Console.WriteLine($"[CalendarificApi] Fetching holidays for {countryCode} in {year} using {_apiUrl}...");
        
        bool isRegionalRequest = countryCode.Length > 2;
        var apiCountryCode = isRegionalRequest ? countryCode.Substring(0, 2) : countryCode;
        var requestUri = $"{_apiUrl}/holidays?&api_key={_apiKey}&country={apiCountryCode}&year={year}";

        try
        {
            var response = await _httpClient.GetAsync(requestUri);

            if (!response.IsSuccessStatusCode)
            {
                return new List<Holiday>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return new List<Holiday>();
            }

            var apiResponse = JsonSerializer.Deserialize<CalendarificResponseDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiResponse?.Response?.Holidays == null || !apiResponse.Response.Holidays.Any())
            {
                return new List<Holiday>();
            }

            var _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var holidays = apiResponse.Response.Holidays
                .Where(dto => dto.Type != null && (dto.Type.Contains("National holiday") || dto.Type.Contains("Local holiday") || dto.Type.Contains("Common local holiday")))
                .Where(dto => 
                {
                    bool isAllStates = dto.StatesElement.ValueKind == JsonValueKind.String && dto.StatesElement.GetString() == "All";

                    // Always include holidays that explicitly apply to 'All' states (fully national)
                    if (isAllStates)
                    {
                        return true;
                    }

                    // If the request was for base country (e.g. GB instead of GB-ENG),
                    // we do NOT want local/regional holidays.
                    if (!isRegionalRequest)
                    {
                        return false;
                    }
                    
                    // Filter down local holidays to ensure they match our specific requested region
                    if (dto.StatesElement.ValueKind == JsonValueKind.Array)
                    {
                        var states = JsonSerializer.Deserialize<List<StateDto>>(dto.StatesElement.GetRawText(), _jsonOptions);

                        return states != null && states.Any(s => s.Iso != null && s.Iso.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    return false;
                })
                .Select(dto => new Holiday
                {
                    CountryCode = countryCode,
                    Date = DateTime.TryParse(dto.Date?.Iso, out var parsedDate) ? parsedDate : default,
                    Description = dto.Name ?? "Unknown Holiday"
                })
                .Where(h => h.Date != default)
                .ToList();

            return holidays;
        }
        catch (Exception)
        {
            return new List<Holiday>();
        }
    }

    public async Task<List<Holiday>> GetNatHolidaysForCountryForYearNoWeekends(string countryCode, int year)
    {
        Console.WriteLine($"[CalendarificApi] Fetching holidays for {countryCode} in {year} (No Weekends)...");
        var holidays = await GetNatHolidaysForCountryForYear(countryCode, year);

        return holidays
            .Where(h => h.Date.DayOfWeek != DayOfWeek.Saturday && h.Date.DayOfWeek != DayOfWeek.Sunday)
            .ToList();
    }

    private class CalendarificResponseDto
    {
        public MetaDto? Meta { get; set; }
        public ResponseDataDto? Response { get; set; }
    }

    private class MetaDto
    {
        public int Code { get; set; }
    }

    private class ResponseDataDto
    {
        public List<CalendarificHolidayDto>? Holidays { get; set; }
    }

    private class CalendarificHolidayDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateDto? Date { get; set; }
        public List<string>? Type { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("states")]
        public JsonElement StatesElement { get; set; }
    }

    private class StateDto
    {
        public int Id { get; set; }
        public string? Abbrev { get; set; }
        public string? Name { get; set; }
        public string? Iso { get; set; }
    }

    private class DateDto
    {
        public string? Iso { get; set; }
        public DatetimeDto? Datetime { get; set; }
    }

    private class DatetimeDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
    }
}
