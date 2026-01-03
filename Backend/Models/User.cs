using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 用户角色枚举
    /// 定义用户在系统中的角色类型
    /// </summary>
    public enum UserRole
    {
        User,       // 普通用户
        Sitter,     // 服务提供者
        // Moderator,  // 社区管理员 (已弃用，统一使用Admin)
        Admin       // 系统管理员
    }

    /// <summary>
    /// 用户状态枚举
    /// 表示用户账户的当前状态
    /// </summary>
    public enum UserStatus
    {
        Active,     // 活跃状态
        Inactive,   // 非活跃状态
        Banned      // 被封禁状态
    }

    /// <summary>
    /// Sitter审核状态枚举
    /// 表示服务提供者资质审核的当前状态
    /// </summary>
    public enum SitterAuditStatus
    {
        NotApplied,     // 未申请
        Pending,        // 待审核
        Approved,       // 已通过
        Rejected,       // 已拒绝
        Resubmitted     // 已重新提交
    }

    /// <summary>
    /// 用户实体类
    /// 表示宠物互助平台中的用户信息
    /// </summary>
    public class User
    {
        /// <summary>
        /// 用户唯一标识符
        /// 使用GUID作为主键，确保全局唯一性
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 用户名
        /// 用户在平台上的显示名称
        /// </summary>
        [Required(ErrorMessage = "用户名不能为空")]
        [MaxLength(50, ErrorMessage = "用户名长度不能超过50个字符")]
        public string Username { get; set; }

        /// <summary>
        /// 密码哈希
        /// 使用SHA256算法加密后的密码字符串
        /// </summary>
        [Required(ErrorMessage = "密码不能为空")]
        [MaxLength(100)]
        public string PasswordHash { get; set; }

        /// <summary>
        /// 手机号码
        /// 用户注册时使用的手机号码，用于登录和接收通知
        /// </summary>
        [Required(ErrorMessage = "手机号码不能为空")]
        [MaxLength(20, ErrorMessage = "手机号码格式不正确")]
        [Phone(ErrorMessage = "手机号码格式不正确")]
        public string Phone { get; set; }

        /// <summary>
        /// 邮箱地址
        /// 用户的电子邮箱，可选字段
        /// </summary>
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [MaxLength(100)]
        public string Email { get; set; }

        /// <summary>
        /// 用户角色
        /// 默认为普通用户，只有管理员可以修改
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>
        /// 用户状态
        /// 控制用户是否可以正常使用平台功能
        /// </summary>
        public UserStatus Status { get; set; } = UserStatus.Active;

        /// <summary>
        /// 信誉分数
        /// 基于用户历史行为计算的信誉评分，初始值为100
        /// </summary>
        public int ReputationScore { get; set; } = 100;

        /// <summary>
        /// 信誉等级
        /// 根据信誉分数计算得出的等级称号
        /// </summary>
        public string ReputationLevel { get; set; } = "新手";

        /// <summary>
        /// 实名认证状态
        /// 表示用户是否已完成实名身份验证
        /// </summary>
        // 认证字段已移除：IsRealNameCertified, IsPetCertified

        /// <summary>
        /// Sitter审核状态
        /// 仅对Sitter角色用户有效，表示资质审核状态
        /// </summary>
        public SitterAuditStatus SitterAuditStatus { get; set; } = SitterAuditStatus.NotApplied;

        /// <summary>
        /// 看护简介
        /// Sitter的自我介绍，包含养宠经验、服务类型等信息
        /// </summary>
        [MaxLength(1000, ErrorMessage = "看护简介长度不能超过1000个字符")]
        public string? CareIntroduction { get; set; }

        /// <summary>
        /// 服务类型
        /// Sitter提供的服务类型，如寄养、喂食、陪伴等
        /// </summary>
        [MaxLength(500, ErrorMessage = "服务类型描述长度不能超过500个字符")]
        public string? ServiceTypes { get; set; }

        /// <summary>
        /// 资质资料
        /// Sitter提交的资质证明文件路径或URL
        /// </summary>
        public string? QualificationDocuments { get; set; }

        /// <summary>
        /// 审核时间
        /// 管理员审核Sitter资质的时间
        /// </summary>
        public DateTime? AuditAt { get; set; }

        /// <summary>
        /// 审核管理员ID
        /// 执行审核操作的管理员ID
        /// </summary>
        public string? AuditBy { get; set; }

        /// <summary>
        /// 审核意见
        /// 管理员审核时的意见说明
        /// </summary>
        [MaxLength(500, ErrorMessage = "审核意见长度不能超过500个字符")]
        public string? AuditComment { get; set; }

        /// <summary>
        /// 拒绝原因
        /// 当审核被拒绝时，提供给Sitter的具体原因
        /// </summary>
        [MaxLength(500, ErrorMessage = "拒绝原因长度不能超过500个字符")]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// 账户创建时间
        /// 记录用户注册的时间戳
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后登录时间
        /// 记录用户最近一次成功登录的时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// 用户所在社区ID
        /// 关联到Community表
        /// </summary>
        public int? CommunityId { get; set; }

        /// <summary>
        /// 用户当前位置经度
        /// </summary>
        [Range(-180, 180)]
        public decimal? Longitude { get; set; }

        /// <summary>
        /// 用户当前位置纬度
        /// </summary>
        [Range(-90, 90)]
        public decimal? Latitude { get; set; }

        /// <summary>
        /// 用户最后定位更新时间
        /// </summary>
        public DateTime? LocationUpdatedAt { get; set; }

        /// <summary>
        /// 用户所在社区
        /// 导航属性：关联到Community实体
        /// </summary>
        [ForeignKey("CommunityId")]
        public virtual Community? Community { get; set; }

        /// <summary>
        /// 用户的宠物列表
        /// 导航属性：一个用户可以拥有多只宠物
        /// </summary>
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

        /// <summary>
        /// 用户作为委托人的订单列表
        /// 导航属性：用户发布的互助需求订单
        /// </summary>
        public virtual ICollection<MutualOrder> OrdersAsRequester { get; set; } = new List<MutualOrder>();

        /// <summary>
        /// 用户作为帮助者的订单列表
        /// 导航属性：用户接受的互助订单
        /// </summary>
        public virtual ICollection<MutualOrder> OrdersAsHelper { get; set; } = new List<MutualOrder>();
    }

    /// <summary>
    /// 宠物实体类
    /// 表示用户拥有的宠物信息
    /// </summary>
    public class Pet
    {
        /// <summary>
        /// 宠物唯一标识符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 宠物主人ID
        /// 外键，关联到User表
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// 宠物姓名
        /// </summary>
        [Required(ErrorMessage = "宠物姓名不能为空")]
        [MaxLength(50, ErrorMessage = "宠物姓名长度不能超过50个字符")]
        public string Name { get; set; }

        /// <summary>
        /// 宠物类型
        /// 如：Dog（狗）、Cat（猫）、Other（其他）
        /// </summary>
        [Required(ErrorMessage = "宠物类型不能为空")]
        public string Type { get; set; }

        /// <summary>
        /// 宠物品种
        /// 可选字段，详细描述宠物的品种
        /// </summary>
        [MaxLength(50)]
        public string Breed { get; set; }

        /// <summary>
        /// 宠物年龄（岁）
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// 是否已接种疫苗
        /// 重要的健康信息，用于互助服务匹配
        /// </summary>
        public bool IsVaccinated { get; set; } = false;

        /// <summary>
        /// 宠物描述
        /// 详细介绍宠物的性格、习惯等信息
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// 宠物信息创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 宠物主人
        /// 导航属性：关联到User实体
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User Owner { get; set; }
    }
}
