using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Models;

namespace HolidayApiComparer.Services;

public class NagerApi : IHolidayProvider
{
    private readonly string _apiUrl;

    public NagerApi()
    {
        _apiUrl = Environment.GetEnvironmentVariable("NAGER_API_URL") 
                  ?? throw new ArgumentNullException("NAGER_API_URL environment variable is missing.");
    }

    public Task<List<Holiday>> GetNatHolidaysForCountryForYear(string countryCode, int year)
    {
        // TODO: Implement actual Nager API call using _apiUrl
        Console.WriteLine($"[NagerApi] Fetching holidays for {countryCode} in {year} using {_apiUrl}...");
        return Task.FromResult(new List<Holiday>());
    }

    public Task<List<Holiday>> GetNatHolidaysForCountryForYearNoWeekends(string countryCode, int year)
    {
        // TODO: Implement actual Nager API call and filter weekends
        Console.WriteLine($"[NagerApi] Fetching holidays for {countryCode} in {year} (No Weekends)...");
        return Task.FromResult(new List<Holiday>());
    }
}
