<template>
  <div class="chats-page">
    <!-- 对话列表 -->
    <div class="chats-list">
      <!-- 头部 -->
      <div class="chats-header">
        <h3>💬 我的消息</h3>
      </div>

      <!-- 新建对话按钮 -->
      <div class="new-conversation-section">
        <button @click="showCreateDialog = true" class="btn-new-chat">
          ➕ 新建对话
        </button>
      </div>

      <!-- 对话列表 -->
      <div class="conversations-container">
        <div
          v-for="conv in conversations"
          :key="conv.id"
          class="conv-item"
          :class="{ active: conv.id === activeConvId }"
          @click="openConversation(conv)"
        >
          <div class="conv-title">{{ conv.otherUser?.username || '系统消息' }}</div>
          <div class="conv-last">
            {{ conv.lastMessage?.content || (conv.lastMessage?.messageType === 'Image' ? '📷 [图片]' : '暂无消息') }}
          </div>
          <div class="conv-unread" v-if="conv.unreadCount > 0">{{ conv.unreadCount }}</div>
        </div>
      </div>
    </div>

    <!-- 聊天面板 -->
    <div class="chat-panel" v-if="activeConvId">
      <!-- 消息区域 -->
      <div class="messages" ref="messagesEl">
        <div
          v-for="m in messages"
          :key="m.id"
          class="msg"
          :class="{ me: m.senderId === currentUserId }"
        >
          <!-- 对方头像 -->
          <div v-if="m.senderId !== currentUserId" class="msg-avatar">
            <div class="avatar-placeholder">👤</div>
          </div>

          <!-- 消息内容和时间 -->
          <div class="msg-wrapper">
            <div class="msg-content">
              <template v-if="m.messageType === 'Image'">
                <img :src="m.mediaUrl" class="msg-image" @click="previewImage(m.mediaUrl)" />
              </template>
              <template v-else>
                {{ m.content }}
              </template>
            </div>
            <div class="msg-time">{{ formatDate(m.createdAt) }}</div>
          </div>

          <!-- 自己头像 -->
          <div v-if="m.senderId === currentUserId" class="msg-avatar">
            <div class="avatar-placeholder">👨</div>
          </div>
        </div>
      </div>

      <!-- 输入区域 -->
      <div class="chat-actions">
        <div class="chat-input-container">
          <textarea
            v-model="input"
            class="chat-input"
            @keyup.enter.exact="sendText"
            @keyup.ctrl.enter="input += '\n'"
            placeholder="输入消息，按 Enter 发送，Ctrl+Enter 换行..."
            rows="1"
          ></textarea>
        </div>
        <button
          class="file-input-btn"
          @click="$refs.fileInput.click()"
          title="发送图片"
        >
          📎
        </button>
        <input
          type="file"
          ref="fileInput"
          class="file-input"
          @change="onFileChange"
          accept="image/*"
        />
        <button
          @click="sendText"
          :disabled="!input.trim() && !isUploading"
          class="send-btn"
        >
          <span v-if="isUploading">⏳</span>
          <span v-else>📤 发送</span>
        </button>
      </div>
    </div>

    <!-- 空状态 -->
    <div class="empty" v-else>
      <div class="empty-icon">💬</div>
      <h4>开始聊天吧！</h4>
      <p>选择一个对话或新建对话开始交流</p>
    </div>

    <!-- 新建对话弹窗 -->
    <div v-if="showCreateDialog" class="modal-overlay" @click="showCreateDialog = false">
      <div class="modal-content" @click.stop>
        <h3>📞 新建对话</h3>
        <div class="form-group">
          <label>对方电话号码</label>
          <input
            v-model="phoneNumber"
            type="tel"
            placeholder="请输入对方电话号码"
            @keyup.enter="createConversationByPhone"
            ref="phoneInput"
          />
        </div>
        <div class="modal-actions">
          <button @click="showCreateDialog = false" class="btn-cancel">取消</button>
          <button
            @click="createConversationByPhone"
            :disabled="!phoneNumber.trim()"
            class="btn-confirm"
          >
            创建对话
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { http } from '@/utils/http.js'
import { chatClient } from '@/utils/chat.js'

const conversations = ref([])
const activeConvId = ref(null)
const messages = ref([])
const input = ref('')
const currentUserId = localStorage.getItem('user_id') || ''
const messagesEl = ref(null)

// 新建对话相关
const showCreateDialog = ref(false)
const phoneNumber = ref('')
const isUploading = ref(false)

// 文件输入引用
const fileInput = ref(null)

const route = useRoute()
const router = useRouter()

const loadConversations = async () => {
  const res = await http.get('/chat/myconversations')
  if (res.success) conversations.value = res.data
}

const openConversation = async (conv) => {
  activeConvId.value = conv.id
  // mark read
  await http.post(`/chat/conversations/${conv.id}/mark-read`, {})
  const res = await http.get(`/chat/conversations/${conv.id}/messages?page=1&pageSize=100`)
  if (res.success) messages.value = res.data.items
  await chatClient.joinConversation(conv.id)
  scrollToBottom()
}

const sendText = async () => {
  if (!input.value.trim() || !activeConvId.value) return
  const content = input.value.trim()
  input.value = ''
  await chatClient.sendMessage(activeConvId.value, content, 'Text', null)
}

const onFileChange = async (e) => {
  const file = e.target.files[0]
  if (!file) return

  isUploading.value = true
  try {
    const url = await chatClient.uploadImage(file)
    await chatClient.sendMessage(activeConvId.value, '', 'Image', url)
  } catch (error) {
    console.error('发送图片失败:', error)
    alert('发送图片失败: ' + (error.message || '网络错误'))
  } finally {
    isUploading.value = false
    e.target.value = ''
  }
}

chatClient.on('message', (msg) => {
  // If message belongs to active conv append, else update unread
  if (msg.conversationId === activeConvId.value) {
    messages.value.push(msg)
    nextTick(scrollToBottom)
    // mark read
    http.post(`/chat/conversations/${msg.conversationId}/mark-read`, {})
  } else {
    const conv = conversations.value.find(c => c.id === msg.conversationId)
    if (conv) conv.unreadCount = (conv.unreadCount || 0) + 1
  }
})

const scrollToBottom = () => {
  if (!messagesEl.value) return
  messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}

// 根据电话号码创建对话
const createConversationByPhone = async () => {
  if (!phoneNumber.value.trim()) return

  try {
    const response = await http.post('/chat/create-by-phone', {
      phoneNumber: phoneNumber.value.trim()
    })

    if (response.success) {
      // 关闭弹窗，清空输入
      showCreateDialog.value = false
      phoneNumber.value = ''

      // 重新加载对话列表
      await loadConversations()

      // 自动打开新创建的对话
      if (response.data) {
        const newConv = conversations.value.find(c => c.id === response.data.id)
        if (newConv) {
          openConversation(newConv)
        }
      }

      alert(response.message || '对话创建成功')
    } else {
      alert(response.message || '创建对话失败')
    }
  } catch (error) {
    console.error('创建对话失败:', error)
    alert('创建对话失败: ' + (error.message || '网络错误'))
  }
}

// 图片预览
const previewImage = (url) => {
  window.open(url, '_blank')
}

// 自动调整输入框高度
const adjustTextareaHeight = () => {
  const textarea = document.querySelector('.chat-input')
  if (textarea) {
    textarea.style.height = 'auto'
    textarea.style.height = Math.min(textarea.scrollHeight, 120) + 'px'
  }
}

// 监听输入变化，自动调整高度
watch(input, adjustTextareaHeight)

const formatDate = (d) => new Date(d).toLocaleString()

onMounted(async () => {
  await loadConversations()
  await chatClient.connect()
  // 若有 convId 查询参数，则自动打开
  const convId = route.query.convId || route.query.conversationId
  if (convId) {
    const conv = conversations.value.find(c => c.id === convId)
    if (conv) openConversation(conv)
    else {
      // reload convs and try again
      await loadConversations()
      const conv2 = conversations.value.find(c => c.id === convId)
      if (conv2) openConversation(conv2)
      else {
        // fallback: navigate without param
        router.replace({ path: '/chats' })
      }
    }
  }
})
</script>

<style scoped>
/* 聊天页面主容器 */
.chats-page {
  display: flex;
  height: 100%;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.05);
}

/* 对话列表区域 */
.chats-list {
  width: 340px;
  background: white;
  border-right: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

/* 对话列表头部 */
.chats-header {
  padding: 20px;
  border-bottom: 1px solid #f1f5f9;
  background: linear-gradient(135deg, #f8fafc 0%, #f1f5f9 100%);
}

.chats-header h3 {
  margin: 0;
  font-size: 18px;
  font-weight: 600;
  color: #334155;
  display: flex;
  align-items: center;
  gap: 8px;
}

/* 新建对话按钮 */
.new-conversation-section {
  padding: 16px 20px;
  border-bottom: 1px solid #f1f5f9;
}

.btn-new-chat {
  width: 100%;
  padding: 12px 16px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);
}

.btn-new-chat:hover {
  background: linear-gradient(135deg, #16a34a, #15803d);
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(34, 197, 94, 0.3);
}

/* 对话列表容器 */
.conversations-container {
  flex: 1;
  overflow-y: auto;
  padding: 8px 0;
}

/* 对话项 */
.conv-item {
  padding: 16px 20px;
  cursor: pointer;
  position: relative;
  margin: 0 8px 4px 8px;
  border-radius: 12px;
  transition: all 0.2s;
  border: 1px solid transparent;
}

.conv-item:hover {
  background: #f8fafc;
  border-color: #e2e8f0;
  transform: translateX(2px);
}

.conv-item.active {
  background: linear-gradient(135deg, #dbeafe 0%, #f0f9ff 100%);
  border-color: #93c5fd;
  box-shadow: 0 2px 8px rgba(59, 130, 246, 0.15);
}

.conv-title {
  font-size: 15px;
  font-weight: 600;
  color: #334155;
  margin-bottom: 6px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.conv-title::before {
  content: '💬';
  font-size: 16px;
}

.conv-last {
  color: #64748b;
  font-size: 13px;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.conv-unread {
  position: absolute;
  right: 16px;
  top: 16px;
  background: linear-gradient(135deg, #ef4444, #dc2626);
  color: white;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  box-shadow: 0 2px 4px rgba(239, 68, 68, 0.3);
  min-width: 20px;
  text-align: center;
}

/* 聊天面板 */
.chat-panel {
  flex: 1;
  display: flex;
  flex-direction: column;
  background: white;
  border-radius: 0 12px 12px 0;
  overflow: hidden;
}

/* 消息区域 */
.messages {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  background: linear-gradient(135deg, #fafbfc 0%, #f8fafc 100%);
}

/* 消息样式 */
.msg {
  margin-bottom: 20px;
  display: flex;
  align-items: flex-end;
  gap: 8px;
  padding: 0 20px;
}

.msg.me {
  flex-direction: row-reverse;
  justify-content: flex-start;
}

.msg:not(.me) {
  justify-content: flex-start;
}

/* 消息头像 */
.msg-avatar {
  flex-shrink: 0;
}

.avatar-placeholder {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, #e2e8f0, #cbd5e1);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

/* 消息内容包装器 */
.msg-wrapper {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  max-width: 60%;
  flex: 1;
}

.msg.me .msg-wrapper {
  align-items: flex-end;
}

/* 消息内容容器 */
.msg-content {
  max-width: 300px;
  padding: 12px 16px;
  border-radius: 18px;
  font-size: 14px;
  line-height: 1.4;
  position: relative;
  word-wrap: break-word;
  white-space: pre-wrap;
}

/* 自己的消息 - 右侧绿色 */
.msg.me .msg-content {
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border-bottom-right-radius: 6px;
  margin-left: auto;
}

/* 对方消息 - 左侧白色 */
.msg:not(.me) .msg-content {
  background: white;
  color: #334155;
  border: 1px solid #e2e8f0;
  border-bottom-left-radius: 6px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  margin-right: auto;
}

/* 图片消息样式 */
.msg-image {
  max-width: 250px;
  border-radius: 12px;
  display: block;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  transition: transform 0.2s;
  cursor: pointer;
}

.msg-image:hover {
  transform: scale(1.02);
}

/* 时间戳样式 */
.msg-time {
  font-size: 11px;
  color: #94a3b8;
  margin-top: 4px;
  white-space: nowrap;
  min-width: fit-content;
}

/* 自己的消息时间戳右对齐 */
.msg.me .msg-time {
  text-align: right;
  margin-left: auto;
  margin-right: 0;
  padding-right: 4px;
}

/* 对方消息时间戳左对齐 */
.msg:not(.me) .msg-time {
  text-align: left;
  margin-left: 0;
  margin-right: auto;
  padding-left: 4px;
}

/* 聊天操作区域 */
.chat-actions {
  padding: 20px;
  background: white;
  border-top: 1px solid #f1f5f9;
  display: flex;
  gap: 12px;
  align-items: flex-end;
}

.chat-input-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.chat-input {
  width: 100%;
  padding: 14px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 14px;
  resize: none;
  min-height: 44px;
  max-height: 120px;
  font-family: inherit;
  line-height: 1.4;
  transition: all 0.2s;
}

.chat-input:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.send-btn {
  padding: 12px 20px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border: none;
  border-radius: 12px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 6px;
  box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);
  min-width: 80px;
  justify-content: center;
}

.send-btn:hover:not(:disabled) {
  background: linear-gradient(135deg, #16a34a, #15803d);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
}

.send-btn:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.file-input-btn {
  padding: 12px;
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.file-input-btn:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.file-input {
  display: none;
}

/* 空状态 */
.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  color: #94a3b8;
  text-align: center;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.empty h4 {
  margin: 0 0 8px 0;
  color: #64748b;
  font-size: 18px;
  font-weight: 500;
}

.empty p {
  margin: 0;
  font-size: 14px;
}

/* 模态框样式 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  backdrop-filter: blur(4px);
}

.modal-content {
  background: white;
  padding: 32px;
  border-radius: 16px;
  width: 90%;
  max-width: 420px;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
  border: 1px solid #e2e8f0;
}

.modal-content h3 {
  margin: 0 0 24px 0;
  color: #334155;
  font-size: 20px;
  font-weight: 600;
  text-align: center;
}

.form-group {
  margin-bottom: 24px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  font-weight: 500;
  color: #475569;
  font-size: 14px;
}

.form-group input {
  width: 100%;
  padding: 14px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  transition: all 0.2s;
  font-family: inherit;
}

.form-group input:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  margin-top: 32px;
}

.btn-cancel {
  padding: 12px 24px;
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s;
}

.btn-confirm {
  padding: 12px 24px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.3s;
  box-shadow: 0 2px 4px rgba(34, 197, 94, 0.2);
  min-width: 100px;
}

.btn-confirm:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.btn-confirm:hover:not(:disabled) {
  background: linear-gradient(135deg, #16a34a, #15803d);
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(34, 197, 94, 0.3);
}

.btn-cancel:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .chats-page {
    flex-direction: column;
  }

  .chats-list {
    width: 100%;
    height: 200px;
    border-right: none;
    border-bottom: 1px solid #e2e8f0;
  }

  .chat-panel {
    border-radius: 0;
  }

  .conv-item {
    max-width: none;
  }

  .chat-actions {
    flex-direction: column;
    gap: 8px;
  }

  .send-btn {
    width: 100%;
  }
}
</style>

