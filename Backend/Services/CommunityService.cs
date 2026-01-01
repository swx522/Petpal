using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using petpal.API.Models.DTOs;

namespace petpal.API.Services
{
    /// <summary>
    /// 社区管理服务实现
    /// 提供社区相关的业务逻辑
    /// </summary>
    public class CommunityService : ICommunityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public CommunityService(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        /// <summary>
        /// 获取用户所属社区信息
        /// </summary>
        public async Task<Community?> GetUserCommunityAsync(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Community)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.Community;
        }

        /// <summary>
        /// 获取社区数据概览统计
        /// </summary>
        public async Task<CommunityStats> GetCommunityStatsAsync()
        {
            var users = await _context.Users.ToListAsync();
            var pendingRequests = await _context.MutualOrders
                .CountAsync(o => o.Status == OrderStatus.Pending);

            return new CommunityStats
            {
                TotalMembers = users.Count,
                PetOwners = users.Count(u => u.Role == UserRole.User),
                ServiceProviders = users.Count(u => u.Role == UserRole.Sitter),
                PendingRequests = pendingRequests
            };
        }

        /// <summary>
        /// 获取社区成员分布数据
        /// </summary>
        public async Task<MemberDistribution> GetMemberDistributionAsync()
        {
            var users = await _context.Users.ToListAsync();

            return new MemberDistribution
            {
                TotalMembers = users.Count,
                PetOwners = users.Count(u => u.Role == UserRole.User),
                ServiceProviders = users.Count(u => u.Role == UserRole.Sitter),
                Moderators = users.Count(u => u.Role == UserRole.Moderator || u.Role == UserRole.Admin)
            };
        }

        /// <summary>
        /// 获取社区活跃度趋势数据
        /// </summary>
        public async Task<List<ActivityData>> GetActivityTrendAsync(int days = 7)
        {
            var startDate = DateTime.Now.AddDays(-days);

            var activityData = new List<ActivityData>();

            for (int i = 0; i < days; i++)
            {
                var date = DateTime.Now.AddDays(-i);
                var nextDate = date.AddDays(1);

                var activeUsers = await _context.Users
                    .CountAsync(u => u.LastLoginAt >= date && u.LastLoginAt < nextDate);

                var newRequests = await _context.MutualOrders
                    .CountAsync(o => o.CreatedAt >= date && o.CreatedAt < nextDate);

                var completedOrders = await _context.MutualOrders
                    .CountAsync(o => o.CompletedAt >= date && o.CompletedAt < nextDate);

                activityData.Add(new ActivityData
                {
                    Date = date.Date,
                    ActiveUsers = activeUsers,
                    NewRequests = newRequests,
                    CompletedOrders = completedOrders
                });
            }

            return activityData.OrderBy(d => d.Date).ToList();
        }

        /// <summary>
        /// 获取社区成员列表
        /// </summary>
        public async Task<List<User>> GetCommunityMembersAsync(MemberFilters filters)
        {
            var query = _context.Users
                .Include(u => u.Community)
                .AsQueryable();

            if (filters.Role.HasValue)
            {
                query = query.Where(u => u.Role == filters.Role.Value);
            }

            if (filters.IsCertified.HasValue)
            {
                query = query.Where(u => u.IsRealNameCertified && u.IsPetCertified);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();
            return users;
        }

        /// <summary>
        /// 搜索社区成员
        /// </summary>
        public async Task<List<User>> SearchMembersAsync(string keyword, MemberFilters filters)
        {
            var query = _context.Users
                .Include(u => u.Community)
                .Where(u => u.Username.Contains(keyword) ||
                           u.Phone.Contains(keyword) ||
                           u.Email.Contains(keyword));

            if (filters.Role.HasValue)
            {
                query = query.Where(u => u.Role == filters.Role.Value);
            }

            if (filters.IsCertified.HasValue)
            {
                query = query.Where(u => u.IsRealNameCertified && u.IsPetCertified);
            }

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();
            return users;
        }

        /// <summary>
        /// 修改成员角色
        /// </summary>
        public async Task ChangeMemberRoleAsync(string adminUserId, string memberId, UserRole newRole)
        {
            var admin = await _context.Users.FindAsync(adminUserId);
            if (admin == null || (admin.Role != UserRole.Admin && admin.Role != UserRole.Moderator))
            {
                throw new UnauthorizedAccessException("无权限修改成员角色");
            }

            var member = await _context.Users.FindAsync(memberId);
            if (member == null)
            {
                throw new ArgumentException("成员不存在");
            }

            // 防止管理员被降级
            if (member.Role == UserRole.Admin && newRole != UserRole.Admin)
            {
                throw new InvalidOperationException("不能修改管理员角色");
            }

            member.Role = newRole;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 移除社区成员
        /// </summary>
        public async Task RemoveMemberAsync(string adminUserId, string memberId)
        {
            var admin = await _context.Users.FindAsync(adminUserId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("只有管理员可以移除成员");
            }

            var member = await _context.Users.FindAsync(memberId);
            if (member == null)
            {
                throw new ArgumentException("成员不存在");
            }

            // 防止移除管理员
            if (member.Role == UserRole.Admin)
            {
                throw new InvalidOperationException("不能移除管理员");
            }

            // 设置社区ID为null，实际上是移除出社区
            member.CommunityId = null;
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 获取社区设置信息
        /// </summary>
        public async Task<CommunitySettings> GetCommunitySettingsAsync()
        {
            // 这里简化实现，实际应该从数据库或配置文件中读取
            // 暂时返回默认设置
            return new CommunitySettings
            {
                Rules = "社区互助守则：\n1. 保持诚信，文明交流\n2. 按约定时间提供服务\n3. 服务完成后及时评价\n4. 遇到问题及时沟通",
                Announcement = "欢迎加入宠物互助社区！\n我们致力于为宠物主人和服务者搭建桥梁，提供便捷的宠物照顾服务。",
                AllowGuestRequests = false,
                MaxRequestDistance = 3000
            };
        }

        /// <summary>
        /// 更新社区设置
        /// </summary>
        public async Task UpdateCommunitySettingsAsync(string adminUserId, CommunitySettings settings)
        {
            var admin = await _context.Users.FindAsync(adminUserId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("只有管理员可以修改社区设置");
            }

            // 这里简化实现，实际应该保存到数据库或配置文件中
            // 暂时只做权限验证，具体实现需要根据实际需求扩展

            await Task.CompletedTask;
        }
    }
}
