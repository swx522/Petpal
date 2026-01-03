using System.ComponentModel.DataAnnotations;

namespace petpal.API.Models.DTOs
{
    /// <summary>
    /// 用户数据传输对象
    /// 用于避免JSON序列化时的循环引用问题
    /// </summary>
    public class UserDto
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
        /// 手机号码
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 用户角色
        /// </summary>
        public UserRole Role { get; set; }
        /// <summary>
        /// 角色编号（数值），便于前端按数字判断（如 Admin = 2）
        /// </summary>
        public int RoleId { get; set; }
        /// <summary>
        /// 角色名称（字符串形式，供前端兼容使用）
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// 信誉分数
        /// </summary>
        public int ReputationScore { get; set; }

        /// <summary>
        /// 信誉等级
        /// </summary>
        public string ReputationLevel { get; set; }

        /// <summary>
        /// 实名认证状态
        /// </summary>
        // 认证字段已移除

        /// <summary>
        /// Sitter审核状态
        /// </summary>
        public SitterAuditStatus SitterAuditStatus { get; set; }

        /// <summary>
        /// 看护简介
        /// </summary>
        public string CareIntroduction { get; set; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public string ServiceTypes { get; set; }

        /// <summary>
        /// 用户所在社区（简易信息，避免循环引用）
        /// </summary>
        public CommunitySimpleDto Community { get; set; }

        /// <summary>
        /// 用户当前位置经度
        /// </summary>
        public decimal? Longitude { get; set; }

        /// <summary>
        /// 用户当前位置纬度
        /// </summary>
        public decimal? Latitude { get; set; }

        /// <summary>
        /// 账户创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginAt { get; set; }
    }
}
