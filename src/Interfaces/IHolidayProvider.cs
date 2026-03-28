using System.Collections.Generic;
using System.Threading.Tasks;
using HolidayApiComparer.Models;

namespace HolidayApiComparer.Interfaces;

public interface IHolidayProvider
{
    /// <summary>
    /// Gets the national holidays for a specific country and year, excluding weekends.
    /// </summary>
    Task<List<Holiday>> GetNatHolidaysForCountryForYearNoWeekends(string countryCode, int year);

    /// <summary>
    /// Gets all national holidays for a specific country and year.
    /// </summary>
    Task<List<Holiday>> GetNatHolidaysForCountryForYear(string countryCode, int year);
}
