using Microsoft.Extensions.DependencyInjection;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Services;
using DotNetEnv;

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

// Test call
var nagerHolidays = await nagerService.GetNatHolidaysForCountryForYear("RO", 2026);
Console.WriteLine($"\n--- Nager API Holidays for RO in 2026 ({nagerHolidays.Count}) ---");
foreach (var holiday in nagerHolidays)
{
    Console.WriteLine($"- {holiday.Date:yyyy-MM-dd}: {holiday.Description}");
}

var calendarificHolidays = await calendarificService.GetNatHolidaysForCountryForYear("RO", 2026);
Console.WriteLine($"\n--- Calendarific API Holidays for US in 2026 ({calendarificHolidays.Count}) ---");
foreach (var holiday in calendarificHolidays)
{
    Console.WriteLine($"- {holiday.Date:yyyy-MM-dd}: {holiday.Description}");
}
