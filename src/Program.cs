using Microsoft.Extensions.DependencyInjection;
using HolidayApiComparer.Interfaces;
using HolidayApiComparer.Services;
using System;
using System.Threading.Tasks;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

// Setup Dependency Injection
var serviceProvider = new ServiceCollection()
    // We register them as Keyed Services so you can choose which one to call by name
    .AddKeyedTransient<IHolidayProvider, NagerApi>("nager")
    .AddKeyedTransient<IHolidayProvider, CalendarificApi>("calendarific")
    .BuildServiceProvider();

// Example magic: getting them out of the DI container
var nagerService = serviceProvider.GetRequiredKeyedService<IHolidayProvider>("nager");
var calendarificService = serviceProvider.GetRequiredKeyedService<IHolidayProvider>("calendarific");

// Test call
await nagerService.GetNatHolidaysForCountryForYear("US", 2026);
await calendarificService.GetNatHolidaysForCountryForYearNoWeekends("US", 2026);
