using Advanced_Ecommerce.Enums;
using Advanced_Ecommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advanced_Ecommerce.Work.Repository
{
    public interface IOrderRepository
    {
		Task CreateOrder(Order order);
		Task<Order> GetById(int orderId);
		Task<IList<Order>> GetByUserId(string userId);
		Task<IList<Order>> GetAll();
		Task<IList<Order>> GetUserLatestOrders(int count, string userId);
		Task<IList<Product>> GetUserMostPopularFoods(string id);
		Task<IList<Order>> GetFilteredOrders(
			string userId = null,
			OrderBy orderBy = OrderBy.None,
			int offset = 0,
			int limit = 10,
			decimal? minimalPrice = null,
			decimal? maximalPrice = null,
			DateTime? minDate = null,
			DateTime? maxDate = null,
			string zipCode = null
			);
	}
}
