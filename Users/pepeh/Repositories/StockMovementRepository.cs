using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEstoqueRoupas.Repositories
{
    public class StockMovementRepository : IStockMovementRepository
    {
        private readonly AppDbContext _context;

        public StockMovementRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StockMovement> AddAsync(StockMovement movement)
        {
            _context.StockMovements.Add(movement);
            await _context.SaveChangesAsync();
            return movement;
        }

        public async Task<List<StockMovement>> GetByProductAsync(int productId)
        {
            return await _context.StockMovements
                .Where(m => m.ProductId == productId)
                .OrderByDescending(m => m.Date)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<StockMovement>> GetAllAsync(MovementType? type, int take = 100)
        {
            var query = _context.StockMovements.AsQueryable();
            if (type.HasValue)
                query = query.Where(m => m.Type == type.Value);

            return await query
                .OrderByDescending(m => m.Date)
                .Take(take)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<StockMovement>> GetTodayAsync()
        {
            var today = DateTime.Today;
            return await _context.StockMovements
                .Where(m => m.Date >= today && m.Date < today.AddDays(1))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
