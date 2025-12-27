using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 订单管理服务接口
    /// 定义订单管理和评价相关的业务逻辑方法
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// 获取用户发布的订单列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="filters">筛选条件</param>
        /// <returns>订单列表</returns>
        Task<List<MutualOrder>> GetUserOrdersAsync(string userId, OrderFilters filters);

        /// <summary>
        /// 获取待评价的订单列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>待评价订单列表</returns>
        Task<List<MutualOrder>> GetOrdersToEvaluateAsync(string userId);

        /// <summary>
        /// 获取已完成的订单列表（服务者视角）
        /// </summary>
        /// <param name="sitterId">服务者ID</param>
        /// <returns>已完成订单列表</returns>
        Task<List<MutualOrder>> GetCompletedOrdersAsync(string sitterId);

        /// <summary>
        /// 获取订单的评价信息
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>评价信息</returns>
        Task<OrderEvaluation?> GetOrderEvaluationAsync(string orderId);

        /// <summary>
        /// 获取用户的信誉评分
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>信誉信息</returns>
        Task<UserReputation> GetUserReputationAsync(string userId);

        /// <summary>
        /// 提交订单评价
        /// </summary>
        /// <param name="evaluatorId">评价者ID</param>
        /// <param name="orderId">订单ID</param>
        /// <param name="evaluation">评价内容</param>
        /// <returns>评价结果</returns>
        Task<OrderEvaluation> SubmitEvaluationAsync(string evaluatorId, string orderId, EvaluationData evaluation);

        /// <summary>
        /// 更新订单评价
        /// </summary>
        /// <param name="evaluatorId">评价者ID</param>
        /// <param name="orderId">订单ID</param>
        /// <param name="evaluation">更新的评价内容</param>
        /// <returns>更新结果</returns>
        Task<OrderEvaluation> UpdateEvaluationAsync(string evaluatorId, string orderId, EvaluationData evaluation);

        /// <summary>
        /// 完成订单
        /// </summary>
        /// <param name="userId">操作者ID</param>
        /// <param name="orderId">订单ID</param>
        /// <returns>完成结果</returns>
        Task<MutualOrder> CompleteOrderAsync(string userId, string orderId);

        /// <summary>
        /// 获取信誉变化日志
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>信誉日志列表</returns>
        Task<List<ReputationLog>> GetReputationLogsAsync(string userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// 获取用户的最新评价列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <returns>评价列表</returns>
        Task<List<OrderEvaluation>> GetUserReviewsAsync(string userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// 获取信誉变化趋势
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="days">天数</param>
        /// <returns>趋势数据</returns>
        Task<List<ReputationTrend>> GetReputationTrendAsync(string userId, int days = 30);
    }

    /// <summary>
    /// 订单筛选条件
    /// </summary>
    public class OrderFilters
    {
        public OrderStatus? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 用户信誉信息
    /// </summary>
    public class UserReputation
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public int ReputationScore { get; set; }
        public string ReputationLevel { get; set; } = "";
        public int TotalCompletedOrders { get; set; }
        public int OrdersAsRequester { get; set; }
        public int OrdersAsHelper { get; set; }
        public int TotalEvaluations { get; set; }
        public int PositiveEvaluations { get; set; }
        public double PositiveRate { get; set; }
        public double AverageScore { get; set; }
        public List<OrderEvaluation> RecentEvaluations { get; set; } = new();
    }

    /// <summary>
    /// 评价数据
    /// </summary>
    public class EvaluationData
    {
        public int Score { get; set; }
        public string Content { get; set; } = "";
        public List<string>? Images { get; set; }
    }

    /// <summary>
    /// 信誉趋势数据
    /// </summary>
    public class ReputationTrend
    {
        public DateTime Date { get; set; }
        public int Score { get; set; }
        public int Change { get; set; }
        public string Reason { get; set; } = "";
    }
}
