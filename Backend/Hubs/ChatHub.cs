using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using petpal.API.Data;
using petpal.API.Models;
using System.Security.Claims;

namespace petpal.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetUserId() => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

        public async Task JoinConversation(string conversationId)
        {
            var userId = GetUserId();
            var conv = await _context.Conversations.FindAsync(conversationId);
            if (conv == null) return;
            if (conv.ParticipantAId != userId && conv.ParticipantBId != userId) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendMessage(string conversationId, string content, string messageType = "Text", string mediaUrl = null)
        {
            var userId = GetUserId();
            var conv = await _context.Conversations.FindAsync(conversationId);
            if (conv == null) throw new HubException("会话不存在");
            if (conv.ParticipantAId != userId && conv.ParticipantBId != userId) throw new HubException("无权限发送消息");

            var msg = new Message
            {
                ConversationId = conversationId,
                SenderId = userId,
                Content = content,
                MediaUrl = mediaUrl,
                MessageType = messageType == "Image" ? MessageType.Image : MessageType.Text,
                CreatedAt = DateTime.Now
            };

            _context.Messages.Add(msg);
            conv.LastMessageAt = DateTime.Now;
            await _context.SaveChangesAsync();

            var dto = new
            {
                id = msg.Id,
                conversationId = msg.ConversationId,
                senderId = msg.SenderId,
                content = msg.Content,
                mediaUrl = msg.MediaUrl,
                messageType = msg.MessageType.ToString(),
                createdAt = msg.CreatedAt
            };

            await Clients.Group(conversationId).SendAsync("ReceiveMessage", dto);
        }
    }
}


