using EMart.Data;
using EMart.DTOs;
using EMart.Models;
using Microsoft.EntityFrameworkCore;

namespace EMart.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto?> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<Product> SaveProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<List<Product>> SearchProductsAsync(string q);
    }

    public class ProductService : IProductService
    {
        private readonly EMartDbContext _context;

        public ProductService(EMartDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            return await _context.Products
                .Select(p => new ProductDto(p.Id, p.ProdName, p.ProdImagePath, p.ProdShortDesc, p.ProdLongDesc, p.MrpPrice, p.CardholderPrice, p.PointsToBeRedeem, p.CategoryId))
                .ToListAsync();
        }

        public async Task<ProductDto?> GetProductByIdAsync(int id)
        {
            var p = await _context.Products.FindAsync(id);
            if (p == null) return null;
            return new ProductDto(p.Id, p.ProdName, p.ProdImagePath, p.ProdShortDesc, p.ProdLongDesc, p.MrpPrice, p.CardholderPrice, p.PointsToBeRedeem, p.CategoryId);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Select(p => new ProductDto(p.Id, p.ProdName, p.ProdImagePath, p.ProdShortDesc, p.ProdLongDesc, p.MrpPrice, p.CardholderPrice, p.PointsToBeRedeem, p.CategoryId))
                .ToListAsync();
        }

        public async Task<Product> SaveProductAsync(Product product)
        {
            if (product.Id > 0)
            {
                _context.Products.Update(product);
            }
            else
            {
                _context.Products.Add(product);
            }
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Product>> SearchProductsAsync(string q)
        {
            return await _context.Products
                .Where(p => p.ProdName != null && p.ProdName.ToLower().Contains(q.ToLower()))
                .ToListAsync();
        }
    }
}
