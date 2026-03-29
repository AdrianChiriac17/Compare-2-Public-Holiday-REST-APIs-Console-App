using Microsoft.Extensions.DependencyInjection;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Services;
using DotNetEnv;
using HolidayApiComparer.Models;

// Load environment variables from .env file
DotNetEnv.Env.TraversePath().Load();

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    // We register them as Keyed Services so you can choose which one to call by name
    .AddKeyedTransient<IHolidayProvider, NagerApi>("nager")
    .AddKeyedTransient<IHolidayProvider, CalendarificApi>("calendarific")
    .BuildServiceProvider();

var nagerService = serviceProvider.GetRequiredKeyedService<IHolidayProvider>("nager");
var calendarificService = serviceProvider.GetRequiredKeyedService<IHolidayProvider>("calendarific");

List<Holiday> englishHolidays = await calendarificService.GetNatHolidaysForCountryForYearNoWeekends("GB-ENG", 2026);
foreach( Holiday holiday in englishHolidays )
{
    Console.WriteLine($"Holiday: {holiday.Description} on {holiday.Date.ToShortDateString()}");
}

var comparer = new ApiComparerService(nagerService, calendarificService);
await comparer.RunComparisonsAsync(2026);
