using petpal.API.Data;
using Microsoft.EntityFrameworkCore;
using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 地理位置服务实现
    /// 提供地理位置计算和验证功能
    /// </summary>
    public class GeolocationService : IGeolocationService
    {
        private readonly ApplicationDbContext _context;
        private const double EarthRadius = 6371000; // 地球半径，单位：米

        public GeolocationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 计算两点间的距离
        /// 使用Haversine公式计算球面距离
        /// </summary>
        /// <param name="lat1">第一个点的纬度</param>
        /// <param name="lon1">第一个点的经度</param>
        /// <param name="lat2">第二个点的纬度</param>
        /// <param name="lon2">第二个点的经度</param>
        /// <returns>距离（米）</returns>
        public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            // 输入验证
            if (!IsValidCoordinate(lat1, lon1) || !IsValidCoordinate(lat2, lon2))
            {
                throw new ArgumentException("无效的坐标值");
            }

            // 将角度转换为弧度
            double lat1Rad = ToRadians(lat1);
            double lon1Rad = ToRadians(lon1);
            double lat2Rad = ToRadians(lat2);
            double lon2Rad = ToRadians(lon2);

            // 计算差值
            double deltaLat = lat2Rad - lat1Rad;
            double deltaLon = lon2Rad - lon1Rad;

            // Haversine公式
            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                      Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                      Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        /// <summary>
        /// 验证坐标是否在中国境内
        /// 使用简化的边界检查
        /// </summary>
        public bool IsInChina(double latitude, double longitude)
        {
            const double minLat = 3.86;
            const double maxLat = 53.55;
            const double minLon = 73.66;
            const double maxLon = 135.05;

            return latitude >= minLat && latitude <= maxLat &&
                   longitude >= minLon && longitude <= maxLon;
        }

        /// <summary>
        /// 验证坐标是否在合理的范围内
        /// </summary>
        public bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 &&
                   longitude >= -180 && longitude <= 180 &&
                   !double.IsNaN(latitude) && !double.IsNaN(longitude) &&
                   !double.IsInfinity(latitude) && !double.IsInfinity(longitude);
        }

        /// <summary>
        /// 获取附近的实体
        /// </summary>
        public async Task<List<T>> GetNearbyEntities<T>(double centerLat, double centerLon, double radius, Func<IQueryable<T>, IQueryable<T>> query) where T : class
        {
            var baseQuery = query(_context.Set<T>().AsQueryable());
            return await baseQuery.ToListAsync();
        }

        /// <summary>
        /// 将角度转换为弧度
        /// </summary>
        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// 根据经纬度查找对应的社区
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>社区信息，如果没有找到则返回null</returns>
        public async Task<Community?> FindCommunityByLocationAsync(decimal longitude, decimal latitude)
        {
            // 直接在数据库中查询匹配的社区
            // 注意：这里使用简单的矩形范围查询，生产环境中可能需要更复杂的地理查询
            var community = await _context.Communities
                .Where(c => c.IsActive &&
                           c.MinLng <= longitude && c.MaxLng >= longitude &&
                           c.MinLat <= latitude && c.MaxLat >= latitude)
                .FirstOrDefaultAsync();

            return community;
        }

        /// <summary>
        /// 获取社区内的服务列表（按距离排序）
        /// </summary>
        /// <param name="communityId">社区ID</param>
        /// <param name="userLat">用户纬度</param>
        /// <param name="userLng">用户经度</param>
        /// <returns>按距离排序的服务列表</returns>
        public async Task<List<MutualOrder>> GetServicesInCommunityAsync(int communityId, double userLat, double userLng)
        {
            // 获取当前用户的ID
            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Latitude == (decimal)userLat && u.Longitude == (decimal)userLng);

            var services = await _context.MutualOrders
                .Include(o => o.Owner)
                .Include(o => o.Community)
                .Where(o => o.Status == OrderStatus.Approved &&
                           o.ExecutionStatus == OrderExecutionStatus.Open &&
                           o.CommunityId == communityId)
                .ToListAsync();

            // 排除自己的订单
            if (currentUser != null)
            {
                services = services.Where(o => o.OwnerId != currentUser.Id).ToList();
            }

            // 计算距离并排序（基于社区中心点）
            foreach (var service in services)
            {
                if (service.Community != null)
                {
                    var center = service.Community.GetCenter();
                    // CalculateDistance 返回米，MutualOrder.Distance 的注释表示单位为公里
                    // 因此这里要转换为公里
                    var distanceMeters = CalculateDistance(userLat, userLng, (double)center.CenterLat, (double)center.CenterLng);
                    service.Distance = Math.Round(distanceMeters / 1000.0, 3); // 保留到千分位，前端显示时通常保留一位
                }
                else
                {
                    service.Distance = 0; // 如果没有社区，距离设为0
                }
            }

            return services.OrderBy(s => s.Distance).ToList();
        }

        /// <summary>
        /// 获取跨社区的附近服务（按距离排序）
        /// </summary>
        /// <param name="userLat">用户纬度</param>
        /// <param name="userLng">用户经度</param>
        /// <param name="radiusKm">搜索半径（公里）</param>
        /// <param name="excludeCommunityId">排除的社区ID</param>
        /// <returns>按距离排序的服务列表</returns>
        public async Task<List<MutualOrder>> GetNearbyServicesAcrossCommunitiesAsync(double userLat, double userLng, double radiusKm, int? excludeCommunityId = null)
        {
            // 首先获取所有待接单的服务（排除指定社区）
            var query = _context.MutualOrders
                .Include(o => o.Owner)
                .Include(o => o.Community)
                .Where(o => o.Status == OrderStatus.Approved &&
                           o.ExecutionStatus == OrderExecutionStatus.Open);

            if (excludeCommunityId.HasValue)
            {
                query = query.Where(o => o.CommunityId != excludeCommunityId.Value);
            }

            var services = await query.ToListAsync();

            // 过滤出在距离范围内的服务（基于社区中心点）
            var nearbyServices = new List<MutualOrder>();
            foreach (var service in services)
            {
                if (service.Community != null)
                {
                    var center = service.Community.GetCenter();
                    // CalculateDistance 返回米，参数 radiusKm 是公里，需要统一单位为公里比较
                    var distanceMeters = CalculateDistance(userLat, userLng, (double)center.CenterLat, (double)center.CenterLng);
                    var distanceKm = distanceMeters / 1000.0;
                    if (distanceKm <= radiusKm)
                    {
                        service.Distance = Math.Round(distanceKm, 3);
                        nearbyServices.Add(service);
                    }
                }
            }

            return nearbyServices.OrderBy(s => s.Distance).ToList();
        }
    }
}