using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEstoqueRoupas.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetLowStockAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Quantity <= p.ReorderThreshold)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product> AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing is null) return false;

            existing.Name = product.Name;
            existing.CategoryId = product.CategoryId;
            existing.Quantity = product.Quantity;
            existing.ReorderThreshold = product.ReorderThreshold;
            existing.Price = product.Price;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
