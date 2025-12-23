using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 信誉日志实体类
    /// 记录用户信誉分变化的历史
    /// </summary>
    public class ReputationLog
    {
        /// <summary>
        /// 日志唯一标识符
        /// </summary>
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 用户ID
        /// 信誉分发生变化的用户
        /// </summary>
        [Required]
        public string UserId { get; set; }

        /// <summary>
        /// 变化前的信誉分数
        /// </summary>
        [Required]
        public int OldScore { get; set; }

        /// <summary>
        /// 变化后的信誉分数
        /// </summary>
        [Required]
        public int NewScore { get; set; }

        /// <summary>
        /// 变化原因
        /// 描述信誉分变化的原因
        /// </summary>
        [Required]
        [MaxLength(200, ErrorMessage = "原因描述长度不能超过200个字符")]
        public string Reason { get; set; }

        /// <summary>
        /// 创建时间
        /// 信誉分变化发生的时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 关联的用户信息
        /// 导航属性
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
