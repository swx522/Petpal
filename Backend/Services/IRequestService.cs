using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 需求管理服务接口
    /// 定义需求发布、接单、审核相关的业务逻辑方法
    /// </summary>
    public interface IRequestService
    {
        /// <summary>
        /// 获取宠物类型列表
        /// </summary>
        /// <returns>宠物类型列表</returns>
        Task<List<PetType>> GetPetTypesAsync();

        /// <summary>
        /// 获取服务类型列表
        /// </summary>
        /// <returns>服务类型列表</returns>
        Task<List<ServiceCategory>> GetServiceCategoriesAsync();

        /// <summary>
        /// 创建宠物信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="petInfo">宠物信息</param>
        /// <returns>创建的宠物信息</returns>
        Task<Pet> CreatePetProfileAsync(string userId, CreatePetRequest petInfo);

        /// <summary>
        /// 发布宠物服务需求
        /// </summary>
        /// <param name="userId">发布者ID</param>
        /// <param name="request">需求信息</param>
        /// <returns>创建的需求</returns>
        Task<MutualOrder> CreateRequestAsync(string userId, CreateRequestData request);

        /// <summary>
        /// 获取可接单的需求列表
        /// </summary>
        /// <param name="sitterId">服务者ID</param>
        /// <param name="filters">筛选条件</param>
        /// <returns>需求列表</returns>
        Task<List<MutualOrder>> GetAvailableRequestsAsync(string sitterId, RequestFilters filters);

        /// <summary>
        /// 获取需求的详细信息
        /// </summary>
        /// <param name="requestId">需求ID</param>
        /// <param name="userId">查看者ID</param>
        /// <returns>需求详情</returns>
        Task<RequestDetail> GetRequestDetailAsync(string requestId, string userId);

        /// <summary>
        /// 接受需求（接单）
        /// </summary>
        /// <param name="sitterId">服务者ID</param>
        /// <param name="requestId">需求ID</param>
        /// <returns>接单结果</returns>
        Task<MutualOrder> AcceptRequestAsync(string sitterId, string requestId);

        /// <summary>
        /// 获取待审核的需求列表
        /// </summary>
        /// <param name="filters">筛选条件</param>
        /// <returns>待审核需求列表</returns>
        Task<List<MutualOrder>> GetPendingReviewsAsync(ReviewFilters filters);

        /// <summary>
        /// 获取审核详情
        /// </summary>
        /// <param name="requestId">需求ID</param>
        /// <returns>审核详情</returns>
        Task<ReviewDetail> GetReviewDetailAsync(string requestId);

        /// <summary>
        /// 审核通过需求
        /// </summary>
        /// <param name="adminId">管理员ID</param>
        /// <param name="requestId">需求ID</param>
        /// <returns>审核结果</returns>
        Task<MutualOrder> ApproveRequestAsync(string adminId, string requestId);

        /// <summary>
        /// 审核拒绝需求
        /// </summary>
        /// <param name="adminId">管理员ID</param>
        /// <param name="requestId">需求ID</param>
        /// <param name="reason">拒绝原因</param>
        /// <returns>审核结果</returns>
        Task<MutualOrder> RejectRequestAsync(string adminId, string requestId, string reason);

        /// <summary>
        /// 重新审核需求
        /// </summary>
        /// <param name="adminId">管理员ID</param>
        /// <param name="requestId">需求ID</param>
        /// <returns>审核结果</returns>
        Task<MutualOrder> RecheckRequestAsync(string adminId, string requestId);

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="userId1">用户1 ID</param>
        /// <param name="userId2">用户2 ID</param>
        /// <returns>距离（米）</returns>
        Task<double> CalculateDistanceAsync(string userId1, string userId2);
    }

    /// <summary>
    /// 宠物类型
    /// </summary>
    public class PetType
    {
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// 服务类型
    /// </summary>
    public class ServiceCategory
    {
        public string Value { get; set; } = "";
        public string Label { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// 创建宠物请求
    /// </summary>
    public class CreatePetRequest
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Breed { get; set; } = "";
        public int Age { get; set; }
        public bool IsVaccinated { get; set; }
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// 创建需求数据
    /// </summary>
    public class CreateRequestData
    {
        public string PetId { get; set; } = "";
        public string ServiceType { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }

    /// <summary>
    /// 需求筛选条件
    /// </summary>
    public class RequestFilters
    {
        public string? ServiceType { get; set; }
        public double? MaxDistance { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 需求详情
    /// </summary>
    public class RequestDetail
    {
        public MutualOrder Request { get; set; } = new();
        public Pet? Pet { get; set; }
        public User Owner { get; set; } = new();
        public double Distance { get; set; }
    }

    /// <summary>
    /// 审核筛选条件
    /// </summary>
    public class ReviewFilters
    {
        public string? ServiceType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 审核详情
    /// </summary>
    public class ReviewDetail
    {
        public MutualOrder Request { get; set; } = new();
        public Pet? Pet { get; set; }
        public User Owner { get; set; } = new();
        public List<AuditMaterial> Materials { get; set; } = new();
    }
}
