import * as signalR from '@microsoft/signalr'
import { http } from './http.js'

const HUB_URL = '/hubs/chat'

class ChatClient {
  constructor() {
    this.connection = null
    this.handlers = {}
  }

  async connect() {
    if (this.connection) return
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, { accessTokenFactory: () => localStorage.getItem('auth_token') || '' })
      .withAutomaticReconnect()
      .build()

    this.connection.on('ReceiveMessage', (msg) => {
      if (this.handlers['message']) this.handlers['message'](msg)
    })

    await this.connection.start()
  }

  on(event, cb) {
    this.handlers[event] = cb
  }

  async joinConversation(conversationId) {
    if (!this.connection) await this.connect()
    await this.connection.invoke('JoinConversation', conversationId)
  }

  async sendMessage(conversationId, content, messageType = 'Text', mediaUrl = null) {
    if (!this.connection) await this.connect()
    await this.connection.invoke('SendMessage', conversationId, content, messageType, mediaUrl)
  }

  async uploadImage(file) {
    const form = new FormData()
    form.append('file', file)
    // 使用 vite 代理前缀：'/api' 是代理到后端的路径；使用完整路径避免被 http helper 二次拼接
    const resp = await fetch('/api/chat/messages/upload', {
      method: 'POST',
      body: form,
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('auth_token') || ''}`
      }
    })
    const data = await resp.json()
    if (!resp.ok || !data.success) throw new Error(data.message || '上传失败')
    return data.data.url
  }

  /**
   * 根据电话号码创建对话
   * @param {string} phoneNumber - 对方电话号码
   * @returns {Promise<Object>} 创建的对话信息
   */
  async createConversationByPhone(phoneNumber) {
    const resp = await fetch('/api/chat/create-by-phone', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('auth_token') || ''}`
      },
      body: JSON.stringify({ phoneNumber })
    })
    const data = await resp.json()
    if (!resp.ok || !data.success) throw new Error(data.message || '创建对话失败')
    return data.data
  }
}

export const chatClient = new ChatClient()

