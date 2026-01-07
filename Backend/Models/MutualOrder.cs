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
    /// 订单审核状态枚举
    /// 仅描述管理员审核的结果状态
    /// </summary>
    public enum OrderStatus
    {
        Pending,    // 待审核：订单已发布，等待管理员审核
        Approved,   // 已审核通过：管理员审核通过，可以接单
        Rejected    // 已审核拒绝：管理员审核拒绝
    }

    /// <summary>
    /// 订单执行状态枚举
    /// 描述订单从接单到完成的状态流转
    /// </summary>
    public enum OrderExecutionStatus
    {
        Open,       // 待接单：审核通过后等待帮助者接单
        Accepted,   // 已接单：有帮助者接受了订单，服务正在执行
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
        /// 宠物主人ID
        /// 发布互助需求的用户
        /// </summary>
        [Required]
        public string OwnerId { get; set; }

        /// <summary>
        /// 服务者ID
        /// 接受并执行订单的服务者
        /// </summary>
        public string? SitterId { get; set; }

        /// <summary>
        /// 订单标题
        /// </summary>
        [Required]
        [MaxLength(200, ErrorMessage = "标题长度不能超过200个字符")]
        public string Title { get; set; }

        /// <summary>
        /// 宠物类型
        /// 如：Dog（狗）、Cat（猫）等
        /// </summary>
        [Required]
        [MaxLength(50, ErrorMessage = "宠物类型长度不能超过50个字符")]
        public string PetType { get; set; }

        /// <summary>
        /// 服务类型
        /// 如：Foster（寄养）、Feed（喂养）、Accompany（陪伴）等
        /// </summary>
        [Required]
        [MaxLength(50, ErrorMessage = "服务类型长度不能超过50个字符")]
        public string ServiceType { get; set; }

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
        /// 服务描述
        /// 详细的服务需求描述
        /// </summary>
        [MaxLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 订单审核状态
        /// 默认为待审核状态
        /// </summary>
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// 订单执行状态
        /// 默认为待接单状态
        /// </summary>
        public OrderExecutionStatus ExecutionStatus { get; set; } = OrderExecutionStatus.Open;

        /// <summary>
        /// 服务所在社区ID
        /// 关联到Community表，表示服务发生的地理区域
        /// </summary>
        public int? CommunityId { get; set; }

        /// <summary>
        /// 服务经度
        /// 用于地理位置定位和距离计算
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// 服务纬度
        /// 用于地理位置定位和距离计算
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// 订单创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 接单时间
        /// </summary>
        public DateTime? AcceptedAt { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 宠物主人信息
        /// 导航属性：关联到User实体
        /// </summary>
        [ForeignKey("OwnerId")]
        public virtual User Owner { get; set; }

        /// <summary>
        /// 服务者信息
        /// 导航属性：关联到User实体
        /// </summary>
        [ForeignKey("SitterId")]
        public virtual User? Sitter { get; set; }

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
