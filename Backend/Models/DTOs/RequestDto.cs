using System.ComponentModel.DataAnnotations;

namespace petpal.API.Models.DTOs
{
    /// <summary>
    /// 需求数据传输对象
    /// 用于审核列表显示，避免JSON序列化时的循环引用问题
    /// </summary>
    public class RequestDto
    {
        /// <summary>
        /// 订单ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 订单标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 宠物类型
        /// </summary>
        public string PetType { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public string ServiceType { get; set; }

        /// <summary>
        /// 服务开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 服务结束时间
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 服务描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 宠物主人信息（简易版，避免循环引用）
        /// </summary>
        public UserSimpleDto User { get; set; }

        /// <summary>
        /// 服务经度
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// 服务纬度
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// 与用户的距离（公里）
        /// </summary>
        public double? Distance { get; set; }
    }

    /// <summary>
    /// 用户简易数据传输对象
    /// 只包含审核列表所需的基本用户信息
    /// </summary>
    public class UserSimpleDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// 姓名（兼容前端统一使用 name 字段）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public UserRole Role { get; set; }

        /// <summary>
        /// 信誉分数
        /// </summary>
        public int ReputationScore { get; set; }
    }
}
