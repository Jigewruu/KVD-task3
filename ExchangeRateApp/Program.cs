using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
public class Program
{
    private static List<CurrencyRate> _rates = new List<CurrencyRate>();
    private static System.Timers.Timer _refreshTimer;
    private static System.Timers.Timer _paginationTimer;
    private static int _pageIndex = 0;
    private const int RowsPerPage = 10;

    public static async Task Main(string[] args)
    {
        // Initial data fetch
        await FetchRatesAsync();

        // Set up a timer to refresh the data every hour
        var hourlyRefreshTimer = new System.Timers.Timer(TimeSpan.FromHours(1).TotalMilliseconds);
        hourlyRefreshTimer.Elapsed += async (sender, e) => await FetchRatesAsync();
        hourlyRefreshTimer.Start();

        // Set up a timer to display the rates in pages every 2 minutes
        _paginationTimer = new System.Timers.Timer(TimeSpan.FromMinutes(2).TotalMilliseconds);
        _paginationTimer.Elapsed += DisplayPagedRates;
        _paginationTimer.Start();

        // Keep the application running
        Console.ReadLine();
    }

    private static async Task FetchRatesAsync()
    {
        var scraper = new CurrencyRateScraper();
        _rates = await scraper.GetExchangeRatesAsync();

        // Reset pagination index and show first page
        _pageIndex = 0;
        DisplayPagedRates(null, null);

        // Display last update time
        Console.WriteLine($"Last Update: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    private static void DisplayPagedRates(object sender, ElapsedEventArgs e)
    {
        // Clear console for a fresh display
        Console.Clear();
        Console.WriteLine("Currency Exchange Rates:");
        Console.WriteLine("Country Name | Buy Rate | Sell Rate");

        // Paginate and display 10 items at a time
        var pagedRates = _rates.Skip(_pageIndex * RowsPerPage).Take(RowsPerPage).ToList();
        foreach (var rate in pagedRates)
        {
            Console.WriteLine($"{rate.CurrencyCode} | " +
                              $"{(rate.BuyRate.HasValue ? rate.BuyRate.Value.ToString("F4") : "N/A")} | " +
                              $"{(rate.SellRate.HasValue ? rate.SellRate.Value.ToString("F4") : "N/A")}");
        }

        // Move to the next page; reset if reached the end
        _pageIndex = (_pageIndex + 1) % (_rates.Count / RowsPerPage + (_rates.Count % RowsPerPage > 0 ? 1 : 0));
    }
}
