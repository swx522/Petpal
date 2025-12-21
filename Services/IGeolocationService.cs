using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 地理位置服务接口
    /// 提供地理位置计算和验证功能
    /// </summary>
    public interface IGeolocationService
    {
        /// <summary>
        /// 计算两点间的距离
        /// </summary>
        /// <param name="lat1">第一个点的纬度</param>
        /// <param name="lon1">第一个点的经度</param>
        /// <param name="lat2">第二个点的纬度</param>
        /// <param name="lon2">第二个点的经度</param>
        /// <returns>距离（米）</returns>
        double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

        /// <summary>
        /// 验证坐标是否在中国境内
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <returns>是否在中国境内</returns>
        bool IsInChina(double latitude, double longitude);

        /// <summary>
        /// 验证坐标是否在合理的范围内
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <returns>坐标是否有效</returns>
        bool IsValidCoordinate(double latitude, double longitude);

        /// <summary>
        /// 获取附近的实体
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="centerLat">中心点纬度</param>
        /// <param name="centerLon">中心点经度</param>
        /// <param name="radius">搜索半径（米）</param>
        /// <param name="query">查询表达式</param>
        /// <returns>附近的实体列表</returns>
        Task<List<T>> GetNearbyEntities<T>(double centerLat, double centerLon, double radius, Func<IQueryable<T>, IQueryable<T>> query) where T : class;

        /// <summary>
        /// 根据经纬度查找对应的社区
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>社区信息，如果没有找到则返回null</returns>
        Task<Community?> FindCommunityByLocationAsync(decimal longitude, decimal latitude);

        /// <summary>
        /// 获取社区内的服务列表（按距离排序）
        /// </summary>
        /// <param name="communityId">社区ID</param>
        /// <param name="userLat">用户纬度</param>
        /// <param name="userLng">用户经度</param>
        /// <returns>按距离排序的服务列表</returns>
        Task<List<MutualOrder>> GetServicesInCommunityAsync(int communityId, double userLat, double userLng);

        /// <summary>
        /// 获取跨社区的附近服务（按距离排序）
        /// </summary>
        /// <param name="userLat">用户纬度</param>
        /// <param name="userLng">用户经度</param>
        /// <param name="radiusKm">搜索半径（公里）</param>
        /// <param name="excludeCommunityId">排除的社区ID</param>
        /// <returns>按距离排序的服务列表</returns>
        Task<List<MutualOrder>> GetNearbyServicesAcrossCommunitiesAsync(double userLat, double userLng, double radiusKm, int? excludeCommunityId = null);
    }
}
