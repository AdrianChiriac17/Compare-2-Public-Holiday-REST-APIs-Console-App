using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Models;

namespace HolidayApiComparer.Services;

public class ApiComparerService
{
    private readonly IHolidayProvider _nager;
    private readonly IHolidayProvider _calendarific;

    public ApiComparerService(IHolidayProvider nager, IHolidayProvider calendarific)
    {
        _nager = nager;
        _calendarific = calendarific;
    }

    public async Task RunComparisonsAsync(int year)
    {
        string[] targetCountries = { "GB-ENG", "RO", "MK", "IN", "CR", "BO", "CO" };

        Console.WriteLine($"\n===== STARTING COMPARISON FOR YEAR {year} =====\n");

        foreach (var countryCode in targetCountries)
        {
            // Always Call NoWeekends
            var nagerHolidays = await _nager.GetNatHolidaysForCountryForYearNoWeekends(countryCode, year);
            var calendarificHolidays = await _calendarific.GetNatHolidaysForCountryForYearNoWeekends(countryCode, year);

            var hardcoded = GetHardcodedHolidays(countryCode, year);

            if (hardcoded != null)
            {
                // Filter weekends for hardcoded to match the NoWeekends constraint
                hardcoded = hardcoded
                    .Where(h => h.Date.DayOfWeek != DayOfWeek.Saturday && h.Date.DayOfWeek != DayOfWeek.Sunday)
                    .ToList();

                Console.WriteLine($"\n================== {countryCode} ==================");
                Console.WriteLine($"Comparing APIs against HARDCODED list (Expected: {hardcoded.Count} weekdays)");
                
                CompareLists("Hardcoded", "Nager", hardcoded, nagerHolidays);
                CompareLists("Hardcoded", "Calendarific", hardcoded, calendarificHolidays);
            }
            else
            {
                Console.WriteLine($"\n================== {countryCode} ==================");
                Console.WriteLine($"Comparing Nager vs Calendarific");
                CompareLists("Nager", "Calendarific", nagerHolidays, calendarificHolidays);
            }
        }
        
        Console.WriteLine($"\n===== COMPARISON FINISHED =====\n");
    }

    private void CompareLists(string name1, string name2, List<Holiday> list1, List<Holiday> list2)
    {
        Console.WriteLine($"\n -> {name1} count: {list1.Count} | {name2} count: {list2.Count}");

        var dates1 = list1.Select(h => h.Date).ToHashSet();
        var dates2 = list2.Select(h => h.Date).ToHashSet();

        var missingIn2 = list1.Where(h => !dates2.Contains(h.Date)).ToList();
        var extraIn2 = list2.Where(h => !dates1.Contains(h.Date)).ToList();

        if (missingIn2.Count == 0 && extraIn2.Count == 0)
        {
            Console.WriteLine("    [MATCH] Both sources contain the exact same dates.");
        }
        else
        {
            Console.WriteLine("    [DIFFERENCE DETECTED]");
            if (missingIn2.Any())
            {
                Console.WriteLine($"    ! Days in {name1} but missing in {name2}:");
                foreach (var d in missingIn2)
                {
                    Console.WriteLine($"       - {d.Date:yyyy-MM-dd} ({d.Description})");
                }
            }
            
            if (extraIn2.Any())
            {
                Console.WriteLine($"    ! Days in {name2} but missing in {name1}:");
                foreach (var d in extraIn2)
                {
                    Console.WriteLine($"       - {d.Date:yyyy-MM-dd} ({d.Description})");
                }
            }
        }
    }

    private List<Holiday>? GetHardcodedHolidays(string countryCode, int year)
    {
        return countryCode switch
        {
            "IN" => GetIndiaNationalHolidays(year),
            "CR" => GetCostaRicaNationalHolidays(year),
            "BO" => GetBoliviaNationalHolidays(year),
            "CO" => GetColumbiaNationalHolidays(year),
            _ => null
        };
    }

    private static List<Holiday> GetIndiaNationalHolidays(int year) =>
        new List<Holiday>
        {
            new() { CountryCode = "IN", Date = new DateTime(year, 1, 1), Description = "New year" },
            new() { CountryCode = "IN", Date = new DateTime(year, 1, 14), Description = "Makara Sankranti" },
            new() { CountryCode = "IN", Date = new DateTime(year, 3, 31), Description = "Khutub-e-Ramzan" },
            new() { CountryCode = "IN", Date = new DateTime(year, 4, 18), Description = "Good Friday" },
            new() { CountryCode = "IN", Date = new DateTime(year, 5, 1), Description = "May Day" },
            new() { CountryCode = "IN", Date = new DateTime(year, 8, 15), Description = "Independence Day" },
            new() { CountryCode = "IN", Date = new DateTime(year, 8, 27), Description = "Varasiddhi Vinayaka Vrata" },
            new() { CountryCode = "IN", Date = new DateTime(year, 10, 1), Description = "Mahanavami" },
            new() { CountryCode = "IN", Date = new DateTime(year, 10, 2), Description = "Gandhi Jayanthi" },
            new() { CountryCode = "IN", Date = new DateTime(year, 10, 20), Description = "Diwali" },
            new() { CountryCode = "IN", Date = new DateTime(year, 10, 21), Description = "2nd day of Diwali" },
            new() { CountryCode = "IN", Date = new DateTime(year, 12, 25), Description = "Christmas" }
        };

    private static List<Holiday> GetCostaRicaNationalHolidays(int year) =>
        new List<Holiday>
        {
            new() { CountryCode = "CR", Date = new DateTime(year, 1, 1), Description = "New Year's Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 4, 11), Description = "Juan Santamaria Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 4, 17), Description = "Maundy Thursday" },
            new() { CountryCode = "CR", Date = new DateTime(year, 4, 18), Description = "Good Friday" },
            new() { CountryCode = "CR", Date = new DateTime(year, 5, 1), Description = "Labour Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 7, 25), Description = "Annexation of Nicoya" },
            new() { CountryCode = "CR", Date = new DateTime(year, 8, 2), Description = "Our Lady of Los Ángeles Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 8, 15), Description = "Mother's Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 8, 31), Description = "Day of the Black Person and Afro-Costa Rican Culture" },
            new() { CountryCode = "CR", Date = new DateTime(year, 9, 15), Description = "Independence Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 12, 1), Description = "Abolition of the Army Day" },
            new() { CountryCode = "CR", Date = new DateTime(year, 12, 25), Description = "Christmas Day" }
        };

    private static List<Holiday> GetBoliviaNationalHolidays(int year) =>
        new List<Holiday>
        {
            new() { CountryCode = "BO", Date = new DateTime(year, 1, 1), Description = "New Year's Day" },
            new() { CountryCode = "BO", Date = new DateTime(year, 1, 22), Description = "Plurinational State Foundation" },
            new() { CountryCode = "BO", Date = new DateTime(year, 3, 3), Description = "Carnival day 1" },
            new() { CountryCode = "BO", Date = new DateTime(year, 3, 4), Description = "Carnival day 2" },
            new() { CountryCode = "BO", Date = new DateTime(year, 4, 18), Description = "Holy Friday" },
            new() { CountryCode = "BO", Date = new DateTime(year, 5, 1), Description = "Labour Day" },
            new() { CountryCode = "BO", Date = new DateTime(year, 6, 19), Description = "Corpus Christi" },
            new() { CountryCode = "BO", Date = new DateTime(year, 6, 21), Description = "New Year - Aymara" },
            new() { CountryCode = "BO", Date = new DateTime(year, 8, 6), Description = "Bolivian Independence Day" },
            new() { CountryCode = "BO", Date = new DateTime(year, 9, 14), Description = "Cochabamba's Day" },
            new() { CountryCode = "BO", Date = new DateTime(year, 11, 2), Description = "All Saints Day" },
            new() { CountryCode = "BO", Date = new DateTime(year, 12, 25), Description = "Christmas" }
        };

    private static List<Holiday> GetColumbiaNationalHolidays(int year) =>
        new List<Holiday>
        {
            new() { CountryCode = "CO", Date = new DateTime(year, 1, 1), Description = "New Year's Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 1, 6), Description = "Epiphany (Three Kings' Day)" },
            new() { CountryCode = "CO", Date = new DateTime(year, 3, 24), Description = "Saint Joseph's Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 4, 16), Description = "Family Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 4, 17), Description = "Maundy Thursday" },
            new() { CountryCode = "CO", Date = new DateTime(year, 4, 18), Description = "Good Friday" },
            new() { CountryCode = "CO", Date = new DateTime(year, 5, 1), Description = "Labour Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 6, 2), Description = "Ascension Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 6, 23), Description = "Corpus Christi" },
            new() { CountryCode = "CO", Date = new DateTime(year, 6, 30), Description = "Saint Peter and Saint Paul Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 8, 7), Description = "Battle of Boyacá (Colombian Independence)" },
            new() { CountryCode = "CO", Date = new DateTime(year, 8, 18), Description = "Assumption of the Virgin Mary" },
            new() { CountryCode = "CO", Date = new DateTime(year, 10, 13), Description = "Columbus Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 11, 3), Description = "All Saint's Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 11, 17), Description = "Independence of Cartagena" },
            new() { CountryCode = "CO", Date = new DateTime(year, 12, 8), Description = "Immaculate Conception Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 12, 25), Description = "Christmas Day" },
            new() { CountryCode = "CO", Date = new DateTime(year, 12, 31), Description = "Family Day" }
        };
}