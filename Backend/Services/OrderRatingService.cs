using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;

namespace petpal.API.Services
{
    public class OrderRatingService : IOrderRatingService
    {
        private readonly ApplicationDbContext _context;

        public OrderRatingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderEvaluation> RateOrderAsync(string orderId, string raterId, string evaluatedUserId, string evaluationType, int score, string? content)
        {
            if (score < 1 || score > 5) throw new Exception("评分必须在1到5之间");

            var evaluation = new OrderEvaluation
            {
                OrderId = orderId,
                EvaluatorId = raterId,
                EvaluatedUserId = evaluatedUserId,
                EvaluationType = evaluationType,
                Score = score,
                Content = content ?? ""
            };

            _context.OrderEvaluations.Add(evaluation);
            await _context.SaveChangesAsync();
            return evaluation;
        }

        public async Task<List<OrderEvaluation>> GetRatingsByOrderAsync(string orderId)
        {
            return await _context.OrderEvaluations
                                 .Where(r => r.OrderId == orderId)
                                 .ToListAsync();
        }
    }
}


