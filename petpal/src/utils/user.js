// src/api/user.js
import { http } from '@/utils/http.js'

// 用户API服务
export const userAPI = {
  // 用户注册
  async register(userData) {
    return http.post('/users/register', userData)
  },

  // 用户登录
  async login(credentials) {
    return http.post('/users/login', credentials)
  },

  // 发送验证码
  async sendCaptcha(phone) {
    return http.post('/users/send-captcha', { phone })
  },

  // 重置密码
  async resetPassword(data) {
    return http.post('/users/reset-password', data)
  },

  // 获取用户信息
  async getUserInfo(userId) {
    return http.get(`/users/${userId}`)
  },

  // 更新用户信息
  async updateUserInfo(userId, data) {
    return http.put(`/users/${userId}`, data)
  },

  // 退出登录（客户端操作，无需API）
  logout() {
    // 清除本地存储的认证信息
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user_id')
    localStorage.removeItem('user_info')
  },

  // 检查登录状态
  isAuthenticated() {
    return !!localStorage.getItem('auth_token')
  },

  // 获取当前用户信息（从本地存储）
  getCurrentUser() {
    try {
      const userInfo = localStorage.getItem('user_info')
      return userInfo ? JSON.parse(userInfo) : null
    } catch (error) {
      console.error('解析用户信息失败:', error)
      return null
    }
  },

  // 保存用户信息到本地存储
  saveUserInfo(userInfo) {
    localStorage.setItem('user_info', JSON.stringify(userInfo))
  }
}