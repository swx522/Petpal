using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 互助类型枚举
    /// 定义平台支持的互助服务类型
    /// </summary>
    public enum HelpType
    {
        Foster,     // 寄养服务
        Feed,       // 喂养服务
        Accompany,  // 陪伴服务
        Share       // 分享经验
    }

    /// <summary>
    /// 订单状态枚举
    /// 描述互助订单在生命周期中的各个状态
    /// </summary>
    public enum OrderStatus
    {
        Pending,    // 待接单：订单已发布，等待帮助者接受
        Accepted,   // 已接单：有帮助者接受了订单
        InProgress, // 进行中：服务正在执行
        Completed,  // 已完成：服务已完成，等待评价
        Cancelled   // 已取消：订单被取消
    }

    /// <summary>
    /// 互助订单实体类
    /// 表示一个宠物互助服务的需求和执行记录
    /// </summary>
    public class MutualOrder
    {
        /// <summary>
        /// 订单唯一标识符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 委托人ID
        /// 发布互助需求的用户
        /// </summary>
        [Required]
        public string RequesterId { get; set; }

        /// <summary>
        /// 帮助者ID
        /// 可为空，表示尚未有帮助者接受订单
        /// </summary>
        public string? HelperId { get; set; }

        /// <summary>
        /// 宠物ID
        /// 需要提供服务的宠物
        /// </summary>
        [Required]
        public string PetId { get; set; }

        /// <summary>
        /// 互助类型
        /// 确定提供何种类型的服务
        /// </summary>
        [Required]
        public HelpType HelpType { get; set; }

        /// <summary>
        /// 订单状态
        /// 默认为待接单状态
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// 服务开始时间
        /// 互助服务开始的时间点
        /// </summary>
        [Required(ErrorMessage = "开始时间不能为空")]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 服务结束时间
        /// 互助服务结束的时间点
        /// </summary>
        [Required(ErrorMessage = "结束时间不能为空")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// 服务地点经度
        /// 用于地理位置计算和附近订单查询
        /// </summary>
        [Required]
        [Range(-180, 180, ErrorMessage = "经度范围无效")]
        public double Longitude { get; set; }

        /// <summary>
        /// 服务地点纬度
        /// </summary>
        [Required]
        [Range(-90, 90, ErrorMessage = "纬度范围无效")]
        public double Latitude { get; set; }

        /// <summary>
        /// 订单备注
        /// 委托人的特殊要求或说明
        /// </summary>
        [MaxLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string Remark { get; set; }

        /// <summary>
        /// 订单附加图片
        /// 用户上传的宠物照片或其他相关图片
        /// </summary>
        public string? OrderImages { get; set; }

        /// <summary>
        /// 服务所在社区ID
        /// 关联到Community表，表示服务发生的地理区域
        /// </summary>
        public int? CommunityId { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 订单接单时间
        /// 帮助者接受订单的时间
        /// </summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>
        /// 订单完成时间
        /// 服务执行完成的时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 委托人信息
        /// 导航属性：关联到User实体
        /// </summary>
        [ForeignKey("RequesterId")]
        public virtual User Requester { get; set; }

        /// <summary>
        /// 帮助者信息
        /// 导航属性：可为空，表示未分配帮助者
        /// </summary>
        [ForeignKey("HelperId")]
        public virtual User? Helper { get; set; }

        /// <summary>
        /// 服务所在社区
        /// 导航属性：关联到Community实体
        /// </summary>
        [ForeignKey("CommunityId")]
        public virtual Community? Community { get; set; }

        /// <summary>
        /// 与用户的距离（公里）
        /// 运行时计算，不存储在数据库中
        /// </summary>
        [NotMapped]
        public double? Distance { get; set; }

        /// <summary>
        /// 宠物信息
        /// 导航属性：关联到Pet实体
        /// </summary>
        [ForeignKey("PetId")]
        public virtual Pet Pet { get; set; }

        /// <summary>
        /// 订单评价列表
        /// 导航属性：订单完成后的评价记录
        /// </summary>
        public virtual ICollection<OrderEvaluation> Evaluations { get; set; } = new List<OrderEvaluation>();
    }

    /// <summary>
    /// 订单评价实体类
    /// 记录订单完成后的评价信息
    /// </summary>
    public class OrderEvaluation
    {
        /// <summary>
        /// 评价唯一标识符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 关联的订单ID
        /// </summary>
        [Required]
        public string OrderId { get; set; }

        /// <summary>
        /// 评价者ID
        /// 可能是委托人评价帮助者，或帮助者评价委托人
        /// </summary>
        [Required]
        public string EvaluatorId { get; set; }

        /// <summary>
        /// 被评价者ID
        /// </summary>
        [Required]
        public string EvaluatedUserId { get; set; }

        /// <summary>
        /// 评价类型
        /// "requester_to_helper" 或 "helper_to_requester"
        /// </summary>
        [Required]
        public string EvaluationType { get; set; }

        /// <summary>
        /// 评分
        /// 1-5星评分
        /// </summary>
        [Required]
        [Range(1, 5, ErrorMessage = "评分必须在1-5之间")]
        public int Score { get; set; }

        /// <summary>
        /// 评价内容
        /// 详细的评价描述，可选
        /// </summary>
        [MaxLength(500, ErrorMessage = "评价内容长度不能超过500个字符")]
        public string Content { get; set; }

        /// <summary>
        /// 评价时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 关联的订单
        /// 导航属性
        /// </summary>
        [ForeignKey("OrderId")]
        public virtual MutualOrder Order { get; set; }

        /// <summary>
        /// 评价者信息
        /// 导航属性
        /// </summary>
        [ForeignKey("EvaluatorId")]
        public virtual User Evaluator { get; set; }

        /// <summary>
        /// 被评价者信息
        /// 导航属性
        /// </summary>
        [ForeignKey("EvaluatedUserId")]
        public virtual User EvaluatedUser { get; set; }
    }
}
