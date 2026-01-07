using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace petpal.API.Models
{
    public enum MessageType
    {
        Text,
        Image
    }

    public class Message
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; }

        public string? Content { get; set; }

        public MessageType MessageType { get; set; } = MessageType.Text;

        // 图片等媒体的 URL（相对或绝对）
        public string? MediaUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}


