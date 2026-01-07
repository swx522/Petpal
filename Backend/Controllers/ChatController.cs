using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;

namespace petpal.API.Controllers
{
    // 请求模型
    public class CreateConversationByPhoneRequest
    {
        public string PhoneNumber { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ChatController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private string GetUserId() => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";

        // 获取或创建会话（针对订单调用）
        [HttpGet("conversations")]
        public async Task<IActionResult> GetOrCreateConversation([FromQuery] string orderId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(orderId)) return BadRequest(new { success = false, message = "orderId 必须提供" });

            var order = await _context.MutualOrders.FindAsync(orderId);
            if (order == null) return NotFound(new { success = false, message = "订单不存在" });

            // 只有 Owner 或 Sitter 可以获取会话
            if (order.OwnerId != userId && order.SitterId != userId) return Forbid();

            var conv = await _context.Conversations.FirstOrDefaultAsync(c => c.OrderId == orderId);
            if (conv == null)
            {
                conv = new Conversation
                {
                    OrderId = orderId,
                    ParticipantAId = order.OwnerId,
                    ParticipantBId = order.SitterId ?? order.OwnerId,
                    CreatedAt = DateTime.Now
                };
                _context.Conversations.Add(conv);
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, data = conv });
        }

        // 分页获取消息
        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(string id, [FromQuery] int page = 1, [FromQuery] int pageSize = 30)
        {
            var userId = GetUserId();
            var conv = await _context.Conversations.FindAsync(id);
            if (conv == null) return NotFound(new { success = false, message = "会话不存在" });
            if (conv.ParticipantAId != userId && conv.ParticipantBId != userId) return Forbid();

            var query = _context.Messages.Where(m => m.ConversationId == id).OrderByDescending(m => m.CreatedAt);
            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).OrderBy(m => m.CreatedAt).ToListAsync();

            return Ok(new { success = true, data = new { total, items } });
        }

        // 获取当前用户的会话列表（包含最后一条消息和未读数）
        [HttpGet("myconversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var userId = GetUserId();
            var convs = await _context.Conversations
                .Where(c => c.ParticipantAId == userId || c.ParticipantBId == userId)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            var result = new List<object>();
            foreach (var c in convs)
            {
                var lastMsg = await _context.Messages
                    .Where(m => m.ConversationId == c.Id)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefaultAsync();

                var unreadCount = await _context.Messages
                    .Where(m => m.ConversationId == c.Id && !m.IsRead && m.SenderId != userId)
                    .CountAsync();

                // 获取对方用户简要信息
                var otherId = c.ParticipantAId == userId ? c.ParticipantBId : c.ParticipantAId;
                var other = await _context.Users.FindAsync(otherId);

                result.Add(new
                {
                    id = c.Id,
                    orderId = c.OrderId,
                    otherUser = other != null ? new { id = other.Id, username = other.Username, role = other.Role } : null,
                    lastMessage = lastMsg != null ? new { id = lastMsg.Id, content = lastMsg.Content, mediaUrl = lastMsg.MediaUrl, messageType = lastMsg.MessageType.ToString(), createdAt = lastMsg.CreatedAt } : null,
                    unreadCount
                });
            }

            return Ok(new { success = true, data = result });
        }

        // 上传图片
        [HttpPost("messages/upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            var userId = GetUserId();
            if (file == null || file.Length == 0) return BadRequest(new { success = false, message = "文件为空" });

            // 限制大小（示例：限制 5MB）
            if (file.Length > 5 * 1024 * 1024) return BadRequest(new { success = false, message = "文件过大" });

            // 限制类型，仅允许 jpg/png/webp
            var permittedExt = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !permittedExt.Contains(ext))
            {
                return BadRequest(new { success = false, message = "仅允许上传 jpg/png/webp 格式的图片" });
            }

            // 可选：检查 ContentType
            var permittedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!permittedTypes.Contains(file.ContentType))
            {
                // 不严格依赖 ContentType，但给出警告
                // return BadRequest(new { success = false, message = "不支持的文件类型" });
            }

            var uploads = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", "chat");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploads, fileName);

            using (var stream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/chat/{fileName}";
            return Ok(new { success = true, data = new { url } });
        }

        // 根据电话号码创建或获取对话
        [HttpPost("create-by-phone")]
        public async Task<IActionResult> CreateConversationByPhone([FromBody] CreateConversationByPhoneRequest request)
        {
            var userId = GetUserId();

            // 查找目标用户
            var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Phone == request.PhoneNumber);
            if (targetUser == null)
            {
                return NotFound(new { success = false, message = "该电话号码对应的用户不存在" });
            }

            // 不能与自己创建对话
            if (targetUser.Id == userId)
            {
                return BadRequest(new { success = false, message = "不能与自己创建对话" });
            }

            // 检查是否已有对话（非订单相关的对话）
            var existingConversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.OrderId == null && // 只查找非订单对话
                    ((c.ParticipantAId == userId && c.ParticipantBId == targetUser.Id) ||
                     (c.ParticipantAId == targetUser.Id && c.ParticipantBId == userId)));

            if (existingConversation != null)
            {
                return Ok(new { success = true, data = existingConversation, message = "对话已存在" });
            }

            // 创建新对话
            var newConversation = new Conversation
            {
                OrderId = null, // 非订单相关对话
                ParticipantAId = userId,
                ParticipantBId = targetUser.Id,
                CreatedAt = DateTime.Now,
                LastMessageAt = DateTime.Now
            };

            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                data = newConversation,
                message = $"已成功与 {targetUser.Username} 创建对话"
            });
        }

        // 标记已读（简化版）
        [HttpPost("conversations/{id}/mark-read")]
        public async Task<IActionResult> MarkRead(string id)
        {
            var userId = GetUserId();
            var conv = await _context.Conversations.FindAsync(id);
            if (conv == null) return NotFound(new { success = false, message = "会话不存在" });
            if (conv.ParticipantAId != userId && conv.ParticipantBId != userId) return Forbid();

            var unread = await _context.Messages.Where(m => m.ConversationId == id && !m.IsRead && m.SenderId != userId).ToListAsync();
            foreach (var m in unread) m.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }
}


