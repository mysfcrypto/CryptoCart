using Advanced_Ecommerce.Models;
using Advanced_Ecommerce.Models.Dto;
using Advanced_Ecommerce.Work.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advanced_Ecommerce.Work.Managers
{
    public class CategoryManager
    {
        private ICategoryRepository _categoryRepository;

        public CategoryManager(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IList<CategoryDto>> GetCategories()
        {
            IList<Category> categories = await _categoryRepository.GetCategoryList();
            var resultList = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                CategoryName = c.Name,
                CategoryImage = c.ImageCategory,
                ProductCount = c.Products.Count()
            }).ToList();

            return resultList;
        }

        public async Task CreateCategory(Category category)
        {
            await _categoryRepository.Add(category);
        }

        public async Task DeleteCategory(int id)
        {
            await _categoryRepository.Delete(id);
        }

        public async Task<Category> GetCategoryById(int id)
        {
            return await _categoryRepository.GetById(id);
        }

        public async Task EditCategory(Category category)
        {
            await _categoryRepository.Update(category);
        }

        public async Task<IList<Category>> GetCategoriesForHomePage()
        {
            var categories = await _categoryRepository.GetCategoryList();
            return categories.Skip(Math.Max(0, categories.Count() - 5)).ToList();
        }
    }
}
