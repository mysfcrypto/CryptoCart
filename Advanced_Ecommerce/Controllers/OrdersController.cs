using Microsoft.AspNetCore.Mvc;
using Advanced_Ecommerce.Models;
using Advanced_Ecommerce.Work.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advanced_Ecommerce.Models.ViewModels;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Advanced_Ecommerce.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ShoppingCart _shoppingCart;
        private readonly ProductManager _productManager;
        private HttpClient _httpClient;
        private readonly string _apiBase = "https://api.nowpayments.io/v1";

        public OrdersController()
        {
        }
        public async Task<IActionResult> Index(ShoppingCartIndexViewModel model)
        {
            IEnumerable<CurrencyViewModel> currencies = await GetCurrenciesAsync();
            string view = ".\\Views\\Order\\Index.cshtml";
            return View(view,currencies);
        }

        public async Task<List<CurrencyViewModel>> GetCurrenciesAsync()
        {
            try
            {
                _httpClient = new HttpClient
                {
                    // Set the base address and headers for the HttpClient
                    BaseAddress = new Uri("https://api.nowpayments.io/v1/")
                };
                _httpClient.DefaultRequestHeaders.Accept.Clear();
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               //API Key to connect through Aquib's Wallet (NowPayment.io)
                _httpClient.DefaultRequestHeaders.Add("x-api-key", "ZE8YCBV-50H4YWJ-PQZ9A22-V8PMAVJ");

                // Make the API call
                HttpResponseMessage response = await _httpClient.GetAsync("currencies?fixed_rate=true");
                response.EnsureSuccessStatusCode(); // Throw an error if not successful

                // Deserialize the JSON response into the CurrencyResponse object
                string json = await response.Content.ReadAsStringAsync();
                CurrencyResponse result = JsonSerializer.Deserialize<CurrencyResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result.Currencies;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
        public async Task<IActionResult> CreateInvoice(string paymentCurrency, decimal totalSum)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.nowpayments.io/v1/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-api-key", "ZE8YCBV-50H4YWJ-PQZ9A22-V8PMAVJ");
            var minPayment = await _httpClient.GetAsync($"{_apiBase}/min-amount?currency_from={paymentCurrency}&currency_to=usd&fiat_equivalent=usd&is_fixed_rate=False&is_fee_paid_by_user=True");
            var minAmount = await minPayment.Content.ReadAsStringAsync();

            var estimateResponse = await _httpClient.GetAsync($"estimate?amount={Convert.ToInt32(totalSum / 90)}&currency_from=usd&currency_to={paymentCurrency}");
            
            var res =  await estimateResponse.Content.ReadAsStringAsync();
            
            if (!estimateResponse.IsSuccessStatusCode)
            {
                return View("Error", "Failed to estimate crypto amount.");
            }
            var estimatedCrypto = await estimateResponse.Content.ReadAsStringAsync(); // Assuming the response JSON has the value directly or you need to adjust it based on actual response structure
            CurrencyConversion conversion = JsonSerializer.Deserialize<CurrencyConversion>(estimatedCrypto);
            // Create invoice
            var invoiceData = new
            {
                price_amount = conversion.EstimatedAmount,
                price_currency = "usd",
                order_id = "ORDER123",  // Example order ID
                order_description = "Product Description Here",
                ipn_callback_url = "https://yourwebsite.com/ipn",
                success_url = "https://yourwebsite.com/success",
                cancel_url = "https://yourwebsite.com/cancel",
                partially_paid_url = "https://yourwebsite.com/partial",
                is_fixed_rate = true,
                is_fee_paid_by_user = true
            };

            var invoiceResponse = await _httpClient.PostAsJsonAsync("invoice", invoiceData);
            if (!invoiceResponse.IsSuccessStatusCode)
            {
                return View("Error", "Failed to create invoice.");
            }

            var invoiceResult = await invoiceResponse.Content.ReadAsStringAsync(); // Assuming this returns a URL or some other data to redirect to
            InvoiceDetails invoiceDetails = JsonSerializer.Deserialize<InvoiceDetails>(invoiceResult);
            // Optionally handle the result to extract URL or relevant data to redirect or show to the user
            // Assuming the invoice URL is directly returned or you may need to parse JSON to get the URL
            return Redirect(invoiceDetails.InvoiceUrl);
        }

        // Display payment status
        public async Task<IActionResult> PaymentStatus()
        {
            var status = await _httpClient.GetAsync($"{_apiBase}/payment-status");
            var paymentStatus = await status.Content.ReadAsStringAsync();

            return View("PaymentStatus", paymentStatus);
        }

    }
    public class CurrencyConversion
    {
        [JsonPropertyName("currency_from")]
        public string CurrencyFrom { get; set; }

        [JsonPropertyName("amount_from")]
        public decimal AmountFrom { get; set; }

        [JsonPropertyName("currency_to")]
        public string CurrencyTo { get; set; }

        [JsonPropertyName("estimated_amount")]
        public string EstimatedAmount { get; set; }
    }
    public class InvoiceDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("token_id")]
        public string TokenId { get; set; }

        [JsonPropertyName("order_id")]
        public string OrderId { get; set; }

        [JsonPropertyName("order_description")]
        public string OrderDescription { get; set; }

        [JsonPropertyName("price_amount")]
        public string PriceAmount { get; set; }

        [JsonPropertyName("price_currency")]
        public string PriceCurrency { get; set; }

        [JsonPropertyName("pay_currency")]
        public string PayCurrency { get; set; }

        [JsonPropertyName("ipn_callback_url")]
        public string IpnCallbackUrl { get; set; }

        [JsonPropertyName("invoice_url")]
        public string InvoiceUrl { get; set; }

        [JsonPropertyName("success_url")]
        public string SuccessUrl { get; set; }

        [JsonPropertyName("cancel_url")]
        public string CancelUrl { get; set; }

        [JsonPropertyName("partially_paid_url")]
        public string PartiallyPaidUrl { get; set; }

        [JsonPropertyName("payout_currency")]
        public string PayoutCurrency { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonPropertyName("is_fixed_rate")]
        public bool IsFixedRate { get; set; }

        [JsonPropertyName("is_fee_paid_by_user")]
        public bool IsFeePaidByUser { get; set; }
    }
}
