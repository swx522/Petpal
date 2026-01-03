using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 审核材料类型枚举
    /// </summary>
    public enum AuditMaterialType
    {
        IdCard,             // 身份证
        PetCertificate,     // 宠物健康证明
        ServiceEnvironment, // 服务环境照片
        QualificationCert,  // 专业资质证书
        Other               // 其他
    }

    /// <summary>
    /// 审核材料状态枚举
    /// </summary>
    public enum AuditMaterialStatus
    {
        Pending,    // 待审核
        Approved,   // 已通过
        Rejected    // 已拒绝
    }

    /// <summary>
    /// Sitter审核材料实体类
    /// 存储Sitter提交的审核资料信息
    /// </summary>
    public class AuditMaterial
    {
        /// <summary>
        /// 材料唯一标识符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 关联的Sitter用户ID
        /// </summary>
        [Required]
        public string SitterId { get; set; }

        /// <summary>
        /// 材料类型
        /// </summary>
        [Required]
        public AuditMaterialType MaterialType { get; set; }

        /// <summary>
        /// 材料名称
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string MaterialName { get; set; }

        /// <summary>
        /// 文件路径或URL
        /// </summary>
        [Required]
        public string FilePath { get; set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 文件类型（如：image/jpeg）
        /// </summary>
        [MaxLength(50)]
        public string? ContentType { get; set; }

        /// <summary>
        /// 审核状态
        /// </summary>
        public AuditMaterialStatus Status { get; set; } = AuditMaterialStatus.Pending;

        /// <summary>
        /// 审核意见
        /// </summary>
        [MaxLength(500)]
        public string? ReviewComment { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// 关联的Sitter用户
        /// </summary>
        [ForeignKey("SitterId")]
        public virtual User Sitter { get; set; }
    }

    /// <summary>
    /// 服务者资格申请实体类
    /// 存储服务者提交的资格申请信息
    /// </summary>
    public class SitterApplication
    {
        /// <summary>
        /// 申请唯一标识符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 申请人用户ID
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// 申请人真实姓名
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string RealName { get; set; }

        /// <summary>
        /// 身份证号
        /// </summary>
        [Required]
        [MaxLength(18)]
        public string IdCardNumber { get; set; }

        /// <summary>
        /// 加入社区的原因
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string JoinReason { get; set; }

        /// <summary>
        /// 申请状态
        /// </summary>
        public SitterAuditStatus Status { get; set; } = SitterAuditStatus.Pending;

        /// <summary>
        /// 审核意见
        /// </summary>
        [MaxLength(500)]
        public string? ReviewComment { get; set; }

        /// <summary>
        /// 申请提交时间
        /// </summary>
        public DateTime AppliedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 审核完成时间
        /// </summary>
        public DateTime? ReviewedAt { get; set; }

        /// <summary>
        /// 关联的用户
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
