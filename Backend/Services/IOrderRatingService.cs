using petpal.API.Models;

namespace petpal.API.Services
{
    public interface IOrderRatingService
    {
        /// <summary>
        /// 为订单打分（1-5），使用现有的 OrderEvaluation 实体
        /// </summary>
        Task<OrderEvaluation> RateOrderAsync(string orderId, string raterId, string evaluatedUserId, string evaluationType, int score, string? content);

        /// <summary>
        /// 获取某订单的所有评分
        /// </summary>
        Task<List<OrderEvaluation>> GetRatingsByOrderAsync(string orderId);
    }
}


