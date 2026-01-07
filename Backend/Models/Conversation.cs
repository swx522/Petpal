using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    /// <summary>
    /// 聊天会话：订单相关的双方会话（Owner <-> Sitter）
    /// </summary>
    public class Conversation
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // 关联订单（可为空）
        public string? OrderId { get; set; }

        // 参与者 A / B 为用户 ID（Owner / Sitter）
        [Required]
        public string ParticipantAId { get; set; }

        [Required]
        public string ParticipantBId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastMessageAt { get; set; }
    }
}


