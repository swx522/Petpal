using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 社区实体类
    /// 表示地理位置上的社区区域，用于本地化服务
    /// </summary>
    public class Community
    {
        /// <summary>
        /// 社区唯一标识符
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 社区名称
        /// 如：XX小区、XX街道等
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 社区范围的最小经度
        /// </summary>
        [Required]
        [Range(-180, 180)]
        public decimal MinLng { get; set; }

        /// <summary>
        /// 社区范围的最大经度
        /// </summary>
        [Required]
        [Range(-180, 180)]
        public decimal MaxLng { get; set; }

        /// <summary>
        /// 社区范围的最小纬度
        /// </summary>
        [Required]
        [Range(-90, 90)]
        public decimal MinLat { get; set; }

        /// <summary>
        /// 社区范围的最大纬度
        /// </summary>
        [Required]
        [Range(-90, 90)]
        public decimal MaxLat { get; set; }

        /// <summary>
        /// 社区描述
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// 社区创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 社区状态
        /// true: 活跃, false: 停用
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// 该社区的用户列表
        /// 导航属性：一个社区可以有多个用户
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// 该社区的服务订单列表
        /// 导航属性：一个社区可以有多个服务订单
        /// </summary>
        public virtual ICollection<MutualOrder> Orders { get; set; } = new List<MutualOrder>();

        /// <summary>
        /// 检查给定的经纬度是否在社区范围内
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>是否在社区范围内</returns>
        public bool ContainsLocation(decimal longitude, decimal latitude)
        {
            return longitude >= MinLng && longitude <= MaxLng &&
                   latitude >= MinLat && latitude <= MaxLat;
        }

        /// <summary>
        /// 获取社区中心点经纬度
        /// </summary>
        /// <returns>(经度, 纬度)</returns>
        public (decimal CenterLng, decimal CenterLat) GetCenter()
        {
            return ((MinLng + MaxLng) / 2, (MinLat + MaxLat) / 2);
        }
    }
}
