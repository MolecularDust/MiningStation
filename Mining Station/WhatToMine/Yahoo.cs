using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mining_Station
{
    public class Yahoo
    {
        private const string AllCurrenciesUrl = "https://finance.yahoo.com/webservice/v1/symbols/allcurrencies/quote?format=json";

        public static async Task<SortedDictionary<string, decimal>> GetAllCurrencies(CancellationToken token = default(CancellationToken))
        {
            using (var client = new HttpClient())
            {
                string json = null;
                var currencies = new SortedDictionary<string, decimal>();
                try
                {
                    var response = await client.GetAsync(AllCurrenciesUrl, token);
                    response.EnsureSuccessStatusCode();
                    json = await response.Content.ReadAsStringAsync();
                    if (json == null)
                        throw new Exception();
                    var rawData = JsonConverter.ConvertFromJson<Dictionary<string, object>>(json, false);
                    var list = rawData["list"] as Dictionary<string, object>;
                    var resources = list["resources"] as ArrayList;
                    foreach (var resource in resources)
                    {
                        var content = ((Dictionary<string, object>)resource)["resource"] as Dictionary<string, object>;
                        var fields = content["fields"] as Dictionary<string, object>;
                        var name = ((string)fields["symbol"]).Split('=')[0];
                        if (name == "USD")
                            continue;
                        var price = Convert.ToDecimal((string)fields["price"], CultureInfo.InvariantCulture);
                        currencies[name] = price;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                return currencies;
            }
        }
    }
}
