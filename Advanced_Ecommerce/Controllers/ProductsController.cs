using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Advanced_Ecommerce.Data;
using Advanced_Ecommerce.Models;
using Advanced_Ecommerce.Models.ViewModels;
using Advanced_Ecommerce.Work.Repository;
using System.Data;
using Microsoft.ML;
using Microsoft.ML.Trainers;

namespace Advanced_Ecommerce.Controllers
{
    public class ProductsController : Controller
    {
        private IProductRepository _productRepository;
        private ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;
        private static MLContext mlContext = new MLContext();

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }

        public async Task<IActionResult> ProductPage(int? id)
        {
            if (id == null)
            {
                return null;
            }

            var product = await _productRepository.GetById((int)id);
            var relatedProductIds = GetRelatedProducts(product.Id, _productRepository.GetProducts().Result, 5);
            List<Product> products = new List<Product>();

            foreach (var _id in relatedProductIds)
            {
                products.Add(await _productRepository.GetById(_id));
            }

            var model = new ProductIndexViewModel
            {
                Id = product.Id,
                ShortDescrip = product.ShortDescription,
                LongDescrip = product.LongDescription,
                Price = product.Price,
                Image = product.PreviewIage,
                inStock = product.inStock,
                Category = product.Category,
                RelatedProducts = products,
            };
            
            return View(model);
        }
        public List<int> GetRelatedProducts(int productId, IList<Product> data, int numberOfRecommendations)
        {
            List<int> recommendation = new List<int>();

            #region Initialization
            // Use System.Random to shuffle the product IDs
            var random = new Random();
            recommendation = _productRepository.GetProducts().Result
                .Select(x => x.Id)
                .OrderBy(x => random.Next())
                .Take(numberOfRecommendations)
                .ToList();
            #endregion Initialization

            if(recommendation.Count > 0)
            {
                return recommendation;
            }
            // Convert the data to an IDataView
            IDataView dataView = mlContext.Data.LoadFromEnumerable(data);

            // Set up a matrix factorization trainer
            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(Product.CategoryId),
                MatrixRowIndexColumnName = nameof(productId),
                LabelColumnName = nameof(Product.ShortDescription),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var trainer = mlContext.Recommendation().Trainers.MatrixFactorization(options);

            // Train the model
            var model = trainer.Fit(dataView);

            // Create prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<Product, ProductPrediction>(model);

            var allProducts = data.Select(p => p.Id).Distinct().ToList();
            List<Tuple<int, float>> scores = new List<Tuple<int, float>>();

            foreach (var otherProductId in allProducts)
            {
                if (otherProductId != productId)
                {
                    // Predict the affinity between the specified product and other products
                    var prediction = predictionEngine.Predict(new Product()
                    {
                        Id = productId, // Simulate the product as a user
                        CategoryId = otherProductId
                    });

                    scores.Add(new Tuple<int, float>(otherProductId, prediction.Score));
                }
            }

            // Order by score and take the top N recommendations
            recommendation = scores.OrderByDescending(s => s.Item2)
                                            .Take(numberOfRecommendations)
                                            .Select(r => r.Item1).ToList();
            return recommendation;
        }
        // GET: Products
        public async Task<IActionResult> Index()
        {
            return View(await _productRepository.GetProducts());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetById((int)id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (Request.Form.Files.Count > 0)
            {
                var file = Request.Form.Files.FirstOrDefault();
                using (var dataStream = new MemoryStream())
                {
                    await file.CopyToAsync(dataStream);
                    product.PreviewIage = Convert.ToBase64String(dataStream.ToArray());
                }
            }
            await _productRepository.Add(product);
            return RedirectToAction(nameof(Index));
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetById((int)id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ShortDescription,LongDescription,Price,PreviewIage,inStock,CategoryId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                   await _productRepository.Update(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _productRepository.GetById((int)id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetById(id);
            await _productRepository.Delete(product);
            return RedirectToAction(nameof(Index));
        }

    }
    public class ProductPrediction
    {
        public float Label;
        public float Score; // Predicted score or affinity
    }
}
