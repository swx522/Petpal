using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 信誉变化日志（保留但不强依赖声誉计算服务）
    /// </summary>
    public class ReputationLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = "";

        public int OldScore { get; set; }
        public int NewScore { get; set; }
        public string Reason { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}


