using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 订单管理服务实现
    /// 提供订单管理和评价相关的业务逻辑
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public OrderService(
            ApplicationDbContext context,
            IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// 获取用户发布的订单列表
        /// </summary>
        public async Task<List<MutualOrder>> GetUserOrdersAsync(string userId, OrderFilters filters)
        {
            var query = _context.MutualOrders
                .Include(o => o.Owner)
                .Where(o => o.OwnerId == userId);

            if (filters.Status.HasValue)
            {
                query = query.Where(o => o.Status == filters.Status.Value);
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取待评价的订单列表
        /// </summary>
        public async Task<List<MutualOrder>> GetOrdersToEvaluateAsync(string userId)
        {
            // 这里简化实现，假设用户可以评价已完成的订单
            // 实际应该根据评价关系来确定
            return await _context.MutualOrders
                .Include(o => o.Owner)
                .Where(o => o.OwnerId == userId && o.ExecutionStatus == OrderExecutionStatus.Completed)
                .OrderByDescending(o => o.CompletedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 获取已完成的订单列表（服务者视角）
        /// </summary>
        public async Task<List<MutualOrder>> GetCompletedOrdersAsync(string sitterId)
        {
            // 这里简化实现，实际应该有字段记录服务者
            // 暂时返回所有已完成的订单
            return await _context.MutualOrders
                .Include(o => o.Owner)
                .Where(o => o.ExecutionStatus == OrderExecutionStatus.Completed)
                .OrderByDescending(o => o.CompletedAt)
                .ToListAsync();
        }

        /// <summary>
        /// 获取订单的评价信息
        /// </summary>
        public async Task<OrderEvaluation?> GetOrderEvaluationAsync(string orderId)
        {
            return await _context.OrderEvaluations
                .Include(e => e.Evaluator)
                .Include(e => e.EvaluatedUser)
                .FirstOrDefaultAsync(e => e.OrderId == orderId);
        }

        /// <summary>
        /// 获取用户的信誉评分
        /// </summary>
        public async Task<UserReputation> GetUserReputationAsync(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            var evaluations = await _context.OrderEvaluations
                .Where(e => e.EvaluatedUserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .ToListAsync();

            var positiveEvaluations = evaluations.Count(e => e.Score >= 4);
            // 暂时只计算作为发布者的已完成订单
            var totalCompletedOrders = await _context.MutualOrders
                .CountAsync(o => o.OwnerId == userId && o.ExecutionStatus == OrderExecutionStatus.Completed);

            return new UserReputation
            {
                UserId = user.Id,
                Username = user.Username,
                ReputationScore = user.ReputationScore,
                ReputationLevel = "",
                TotalCompletedOrders = totalCompletedOrders,
                OrdersAsRequester = await _context.MutualOrders.CountAsync(o => o.OwnerId == userId && o.ExecutionStatus == OrderExecutionStatus.Completed),
                OrdersAsHelper = 0, // 暂时设为0，实际需要根据服务者记录计算
                TotalEvaluations = evaluations.Count,
                PositiveEvaluations = positiveEvaluations,
                PositiveRate = evaluations.Count > 0 ? (double)positiveEvaluations / evaluations.Count * 100 : 0,
                AverageScore = evaluations.Count > 0 ? evaluations.Average(e => e.Score) : 0,
                RecentEvaluations = evaluations
            };
        }

        /// <summary>
        /// 提交订单评价
        /// </summary>
        public async Task<OrderEvaluation> SubmitEvaluationAsync(string evaluatorId, string orderId, EvaluationData evaluation)
        {
            var order = await _context.MutualOrders
                .Include(o => o.Owner)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new ArgumentException("订单不存在");
            }

            if (order.ExecutionStatus != OrderExecutionStatus.Completed)
            {
                throw new InvalidOperationException("只能评价已完成的订单");
            }

            // 检查是否已经评价过
            var existingEvaluation = await _context.OrderEvaluations
                .FirstOrDefaultAsync(e => e.OrderId == orderId && e.EvaluatorId == evaluatorId);

            if (existingEvaluation != null)
            {
                throw new InvalidOperationException("该订单已评价过");
            }

            // 确定评价类型和被评价者
            string evaluatedUserId;
            string evaluationType;

            if (order.OwnerId == evaluatorId)
            {
                // 宠物主人评价服务者
                // 这里简化，实际需要从订单记录中获取服务者ID
                evaluatedUserId = "sitter-id"; // 暂时设为占位符
                evaluationType = "requester_to_helper";
            }
            else
            {
                // 服务者评价宠物主人
                evaluatedUserId = order.OwnerId;
                evaluationType = "helper_to_requester";
            }

            var orderEvaluation = new OrderEvaluation
            {
                OrderId = orderId,
                EvaluatorId = evaluatorId,
                EvaluatedUserId = evaluatedUserId,
                EvaluationType = evaluationType,
                Score = evaluation.Score,
                Content = evaluation.Content,
                CreatedAt = DateTime.Now
            };

            _context.OrderEvaluations.Add(orderEvaluation);

            // 不再维护全局信誉分，改用订单评价模块记录评分数据（OrderEvaluations）

            await _context.SaveChangesAsync();

            return orderEvaluation;
        }

        /// <summary>
        /// 更新订单评价
        /// </summary>
        public async Task<OrderEvaluation> UpdateEvaluationAsync(string evaluatorId, string orderId, EvaluationData evaluation)
        {
            var existingEvaluation = await _context.OrderEvaluations
                .FirstOrDefaultAsync(e => e.OrderId == orderId && e.EvaluatorId == evaluatorId);

            if (existingEvaluation == null)
            {
                throw new ArgumentException("评价不存在");
            }

            // 这里简化实现，实际应该撤销之前的信誉变化，然后应用新的变化
            existingEvaluation.Score = evaluation.Score;
            existingEvaluation.Content = evaluation.Content;

            await _context.SaveChangesAsync();

            return existingEvaluation;
        }

        /// <summary>
        /// 完成订单
        /// </summary>
        public async Task<MutualOrder> CompleteOrderAsync(string userId, string orderId)
        {
            var order = await _context.MutualOrders.FindAsync(orderId);
            if (order == null)
            {
                throw new ArgumentException("订单不存在");
            }

            // 检查权限：只有订单参与者可以完成订单
            if (order.OwnerId != userId /* && 不是服务者 */)
            {
                throw new UnauthorizedAccessException("无权限完成此订单");
            }

            if (order.ExecutionStatus != OrderExecutionStatus.Accepted && order.ExecutionStatus != OrderExecutionStatus.InProgress)
            {
                throw new InvalidOperationException("只有进行中的订单才能完成");
            }

            order.ExecutionStatus = OrderExecutionStatus.Completed;
            order.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return order;
        }

        /// <summary>
        /// 获取信誉变化日志
        /// </summary>
        public async Task<List<ReputationLog>> GetReputationLogsAsync(string userId, int page = 1, int pageSize = 10)
        {
            return await _context.ReputationLogs
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取用户的最新评价列表
        /// </summary>
        public async Task<List<OrderEvaluation>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10)
        {
            return await _context.OrderEvaluations
                .Include(e => e.Evaluator)
                .Include(e => e.Order)
                .Where(e => e.EvaluatedUserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取信誉变化趋势
        /// </summary>
        public async Task<List<ReputationTrend>> GetReputationTrendAsync(string userId, int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days);

            var logs = await _context.ReputationLogs
                .Where(r => r.UserId == userId && r.CreatedAt >= startDate)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            var trends = new List<ReputationTrend>();

            foreach (var log in logs)
            {
                trends.Add(new ReputationTrend
                {
                    Date = log.CreatedAt.Date,
                    Score = log.NewScore,
                    Change = log.NewScore - log.OldScore,
                    Reason = log.Reason
                });
            }

            return trends;
        }

        /// <summary>
        /// 获取当前信誉分数
        /// </summary>
        private async Task<int> GetCurrentScoreAsync(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            return user?.ReputationScore ?? 0;
        }
    }
}
