using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Advanced_Ecommerce.Models;
using Advanced_Ecommerce.Models.ViewModels;
using Advanced_Ecommerce.Work.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;

namespace Advanced_Ecommerce.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ShoppingCart _shoppingCart;
        private readonly IProductRepository _productRepository;
        private HttpClient _httpClient;
        public ShoppingCartController(ShoppingCart shoppingCart, IProductRepository productRepository)
        {
            _shoppingCart = shoppingCart;
            _productRepository = productRepository;
        }

        public async Task<IActionResult> Index(bool isValidAmount=true, string returnUrl="/")
        {
            _shoppingCart.GetShoppingCartItems();
            var model = new ShoppingCartIndexViewModel
            {
                ShoppingCart = _shoppingCart,
                ShoppingCartTotal = _shoppingCart.GetShoppingCartTotal(),
                ReturnUrl = returnUrl
            };
            IEnumerable<CurrencyViewModel> currencies = await GetCurrenciesAsync();
            OrderViewIndexModel orderViewIndexModel = new OrderViewIndexModel
            {
                ShoppingCartIndexViewModel = model,
                Currencies = currencies
            };
            if (!isValidAmount)
            {
                ViewBag.InvalidAmountText = "*There were not enough items in stock to add*";
            }
            string view = ".\\Views\\Order\\Index.cshtml";
            return View(view,orderViewIndexModel);
        }

        [HttpPost]
        [Route("/ShoppingCart/Add/{id}/{returnUrl?}")]
        public async Task<IActionResult> Add(int id, int? amount=1, string returnUrl=null)
        {
            Product product = await _productRepository.GetById(id);

            returnUrl = returnUrl.Replace("%2F", "/");
            bool isValidAmount = false;

            if(product != null)
            {
                isValidAmount = _shoppingCart.AddToCart(product, amount.Value);
            }

            return await Index(isValidAmount, returnUrl);
        }

        public async Task<IActionResult> Remove(int prodId)
        {
            Product product = await _productRepository.GetById(prodId);

            if (product != null)
            {
                _shoppingCart.RemoveFromCart(product);
            }
            return RedirectToAction("Index");
        }

        public IActionResult Back(string returnUrl = "/")
        {
            return Redirect(returnUrl);
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
    }
}
