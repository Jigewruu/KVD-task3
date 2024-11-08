using HtmlAgilityPack;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CurrencyRateScraper
{
    private const string Url = "https://www.hsbc.com.my/investments/products/foreign-exchange/currency-rate/";

    public async Task<List<CurrencyRate>> GetExchangeRatesAsync()
    {
        var rates = new List<CurrencyRate>();
        var web = new HtmlWeb();
        var doc = await web.LoadFromWebAsync(Url);

        // Select rows under the <tbody> element of the table
        var rows = doc.DocumentNode.SelectNodes("//tbody/tr");

        if (rows != null)
        {
            foreach (var row in rows)
            {
                var cells = row.SelectNodes("th|td");

                if (cells.Count >= 4) // Ensure at least 4 columns (Currency Code, Currency Name, TT Sell, TT Buy)
                {
                    var buyRateText = cells[2].InnerText.Trim();
                    var sellRateText = cells[3].InnerText.Trim();

                    // Parse sell and buy rates, handling "N/A" cases
                    decimal? sellRate = sellRateText == "N/A" ? (decimal?)null : decimal.Parse(sellRateText);
                    decimal? buyRate = buyRateText == "N/A" ? (decimal?)null : decimal.Parse(buyRateText);

                    var rate = new CurrencyRate
                    {
                        CurrencyCode = cells[0].InnerText.Trim(),         // Currency code (e.g., USD)
                        SellRate = sellRate,                              // TT Sell rate or null if "N/A"
                        BuyRate = buyRate                                 // TT Buy rate or null if "N/A"
                    };
                    rates.Add(rate);
                }
            }
        }

        return rates;
    }
}

public class CurrencyRate
{
    public string CurrencyCode { get; set; }
    public decimal? SellRate { get; set; } // Nullable to handle "N/A"
    public decimal? BuyRate { get; set; }  // Nullable to handle "N/A"
}
