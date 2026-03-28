using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Models;

namespace HolidayApiComparer.Services;

public class CalendarificApi : IHolidayProvider
{
    private readonly string _apiUrl;
    private readonly string _apiKey;

    public CalendarificApi()
    {
        _apiUrl = Environment.GetEnvironmentVariable("CALENDARIFIC_API_URL") 
                  ?? throw new ArgumentNullException("CALENDARIFIC_API_URL environment variable is missing.");
        _apiKey = Environment.GetEnvironmentVariable("CALENDARIFIC_API_KEY") 
                  ?? throw new ArgumentNullException("CALENDARIFIC_API_KEY environment variable is missing.");
    }

    public Task<List<Holiday>> GetNatHolidaysForCountryForYear(string countryCode, int year)
    {
        // TODO: Implement actual Calendarific API call using _apiUrl and _apiKey
        Console.WriteLine($"[CalendarificApi] Fetching holidays for {countryCode} in {year} using {_apiUrl}...");
        return Task.FromResult(new List<Holiday>());
    }

    public Task<List<Holiday>> GetNatHolidaysForCountryForYearNoWeekends(string countryCode, int year)
    {
        // TODO: Implement actual Calendarific API call and filter weekends
        Console.WriteLine($"[CalendarificApi] Fetching holidays for {countryCode} in {year} (No Weekends)...");
        return Task.FromResult(new List<Holiday>());
    }
}
