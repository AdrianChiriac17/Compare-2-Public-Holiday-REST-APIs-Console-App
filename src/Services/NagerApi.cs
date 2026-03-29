using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Models;

namespace HolidayApiComparer.Services;

public class NagerApi : IHolidayProvider
{
    private readonly string _apiUrl;
    private readonly HttpClient _httpClient;

    public NagerApi()
    {
        _apiUrl = Environment.GetEnvironmentVariable("NAGER_API_URL") 
                  ?? throw new ArgumentNullException("NAGER_API_URL environment variable is missing.");
        _httpClient = new HttpClient();
    }

    public async Task<List<Holiday>> GetNatHolidaysForCountryForYear(string countryCode, int year)
    {
        Console.WriteLine($"[NagerApi] Fetching holidays for {countryCode} in {year} using {_apiUrl}...");
        
        bool isRegionalRequest = countryCode.Length > 2;
        var apiCountryCode = isRegionalRequest ? countryCode.Split('-')[0] : countryCode;
        var requestUri = $"{_apiUrl}/PublicHolidays/{year}/{apiCountryCode}";

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

            var apiHolidays = JsonSerializer.Deserialize<List<NagerHolidayDto>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiHolidays == null || !apiHolidays.Any())
            {
                return new List<Holiday>();
            }

            var nationalHolidays = apiHolidays
                .Where(dto => 
                {
                    // Global applies to the whole country regardless of region
                    if (dto.Global)
                    {
                        return true;
                    }

                    // If they want plain GB, they only get Global. They do NOT get local counties.
                    if (!isRegionalRequest)
                    {
                        return false;
                    }

                    // If they explicitly requested a region (e.g. GB-ENG), check counties
                    return dto.Counties != null && dto.Counties.Contains(countryCode, StringComparer.OrdinalIgnoreCase);
                })
                .Select(dto => new Holiday
                {
                    CountryCode = countryCode,
                    Date = dto.Date,
                    Description = dto.Name ?? dto.LocalName ?? "Unknown Holiday"
                })
                .ToList();
            
            return nationalHolidays;
        }
        catch (Exception)
        {
            return new List<Holiday>();
        }
    }

    public async Task<List<Holiday>> GetNatHolidaysForCountryForYearNoWeekends(string countryCode, int year)
    {
        Console.WriteLine($"[NagerApi] Fetching holidays for {countryCode} in {year} (No Weekends)...");
        var holidays = await GetNatHolidaysForCountryForYear(countryCode, year);

        return holidays
            .Where(h => h.Date.DayOfWeek != DayOfWeek.Saturday && h.Date.DayOfWeek != DayOfWeek.Sunday)
            .ToList();
    }

    private class NagerHolidayDto
    {
        public DateTime Date { get; set; }
        public string? LocalName { get; set; }
        public string? Name { get; set; }
        public string[]? Counties { get; set; }
        public bool Global { get; set; }
    }
}
