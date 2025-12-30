using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Models;
using petpal.API.Services;
using System.Security.Claims;

namespace petpal.API.Controllers
{
    /// <summary>
    /// 订单评分控制器
    /// POST /api/orders/{orderId}/rating  - 提交评分
    /// GET  /api/orders/{orderId}/ratings - 获取订单的所有评分
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderRatingController : ControllerBase
    {
        private readonly IOrderRatingService _orderRatingService;

        public OrderRatingController(IOrderRatingService orderRatingService)
        {
            _orderRatingService = orderRatingService;
        }

        /// <summary>
        /// 为订单提交评分（1-5）
        /// </summary>
        [HttpPost("{orderId}/rating")]
        public async Task<IActionResult> RateOrder(string orderId, [FromBody] RateOrderRequest request)
        {
            var raterId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(raterId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "用户未认证" });
            }

            try
            {
                var evaluation = await _orderRatingService.RateOrderAsync(
                    orderId,
                    raterId,
                    request.EvaluatedUserId,
                    request.EvaluationType ?? "unknown",
                    request.Score,
                    request.Content);

                return Ok(new ApiResponse { Success = true, Data = evaluation, Message = "评分提交成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// 获取订单的评分列表
        /// </summary>
        [HttpGet("{orderId}/ratings")]
        public async Task<IActionResult> GetRatings(string orderId)
        {
            try
            {
                var ratings = await _orderRatingService.GetRatingsByOrderAsync(orderId);
                return Ok(new ApiResponse { Success = true, Data = ratings });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }
    }
}


