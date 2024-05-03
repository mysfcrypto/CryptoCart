using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Advanced_Ecommerce.Models.ViewModels
{
    public class CurrencyViewModel
    {
        [JsonProperty("min_amount")]
        public double MinAmount { get; set; }

        [JsonProperty("max_amount")]
        public double MaxAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }


    public class CurrencyResponse
    {
        [JsonPropertyName("currencies")]
        public List<CurrencyViewModel> Currencies { get; set; }
    }

    public class OrderViewIndexModel
    {
        public IEnumerable<CurrencyViewModel> Currencies { get; set; }
        public ShoppingCartIndexViewModel ShoppingCartIndexViewModel { get; set; }
    }
}
