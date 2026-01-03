using petpal.API.Models;
using petpal.API.Models.DTOs;

namespace petpal.API.Services
{
    /// <summary>
    /// 社区管理服务接口
    /// 定义社区相关的业务逻辑方法
    /// </summary>
    public interface ICommunityService
    {
        /// <summary>
        /// 获取用户所属社区信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>社区信息</returns>
        Task<Community?> GetUserCommunityAsync(string userId);

        /// <summary>
        /// 获取社区数据概览统计
        /// </summary>
        /// <returns>社区统计数据</returns>
        Task<CommunityStats> GetCommunityStatsAsync();

        /// <summary>
        /// 获取社区成员分布数据
        /// </summary>
        /// <returns>成员分布数据</returns>
        Task<MemberDistribution> GetMemberDistributionAsync();

        /// <summary>
        /// 获取社区活跃度趋势数据
        /// </summary>
        /// <param name="days">天数</param>
        /// <returns>活跃度数据</returns>
        Task<List<ActivityData>> GetActivityTrendAsync(int days = 7);

        /// <summary>
        /// 获取社区成员列表
        /// </summary>
        /// <param name="filters">筛选条件</param>
        /// <returns>成员列表</returns>
        Task<List<User>> GetCommunityMembersAsync(MemberFilters filters);

        /// <summary>
        /// 搜索社区成员
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <param name="filters">筛选条件</param>
        /// <returns>搜索结果</returns>
        Task<List<User>> SearchMembersAsync(string keyword, MemberFilters filters);

        /// <summary>
        /// 修改成员角色
        /// </summary>
        /// <param name="adminUserId">管理员ID</param>
        /// <param name="memberId">成员ID</param>
        /// <param name="newRole">新角色</param>
        Task ChangeMemberRoleAsync(string adminUserId, string memberId, UserRole newRole);

        /// <summary>
        /// 移除社区成员
        /// </summary>
        /// <param name="adminUserId">管理员ID</param>
        /// <param name="memberId">成员ID</param>
        Task RemoveMemberAsync(string adminUserId, string memberId);

        /// <summary>
        /// 获取社区设置信息
        /// </summary>
        /// <returns>社区设置</returns>
        Task<CommunitySettings> GetCommunitySettingsAsync();

        /// <summary>
        /// 更新社区设置
        /// </summary>
        /// <param name="adminUserId">管理员ID</param>
        /// <param name="settings">新设置</param>
        Task UpdateCommunitySettingsAsync(string adminUserId, CommunitySettings settings);
    }

    /// <summary>
    /// 社区统计数据
    /// </summary>
    public class CommunityStats
    {
        public int TotalMembers { get; set; }
        public int PetOwners { get; set; }
        public int ServiceProviders { get; set; }
        public int PendingRequests { get; set; }
    }

    /// <summary>
    /// 成员分布数据
    /// </summary>
    public class MemberDistribution
    {
        public int TotalMembers { get; set; }
        public int PetOwners { get; set; }
        public int ServiceProviders { get; set; }
        public int Admins { get; set; }
    }

    /// <summary>
    /// 活跃度数据
    /// </summary>
    public class ActivityData
    {
        public DateTime Date { get; set; }
        public int ActiveUsers { get; set; }
        public int NewRequests { get; set; }
        public int CompletedOrders { get; set; }
    }

    /// <summary>
    /// 成员筛选条件
    /// </summary>
    public class MemberFilters
    {
        public UserRole? Role { get; set; }
        public bool? IsCertified { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// 社区设置
    /// </summary>
    public class CommunitySettings
    {
        public string Rules { get; set; } = "";
        public string Announcement { get; set; } = "";
        public bool AllowGuestRequests { get; set; } = false;
        public int MaxRequestDistance { get; set; } = 3000;
    }
}
