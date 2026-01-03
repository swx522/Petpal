// src/utils/user.js
import { http } from '@/utils/http.js'

// 用户API服务
export const userAPI = {
async joinCommunity(data) {
  try {
    return await http.post('/community/join', {
      communityId: data.communityId
    })
  } catch (error) {
    console.error('加入社区失败:', error)
    return {
      success: false,
      message: error.message || '加入社区失败'
    }
  }
},

  async getLocation() {
  try {
    return await http.get('/user/location')
  } catch (error) {
    console.error('获取位置失败:', error)
    return {
      success: false,
      message: error.message || '获取位置失败'
    }
  }
},

/**
 * 更新用户位置
 * @param {Object} data - 位置数据 {latitude, longitude}
 */
async updateLocation(data) {
  try {
    return await http.post('/user/location', {
      latitude: data.latitude,
      longitude: data.longitude
    })
  } catch (error) {
    console.error('更新位置失败:', error)
    return {
      success: false,
      message: error.message || '更新位置失败'
    }
  }
},
  // ============ 社区相关API ============
  
  /**
   * 获取我的社区信息
   */
  async getMyCommunity() {
    return http.get('/community/my')
  },

  /**
   * 根据经纬度查找社区
   * @param {number} longitude - 经度
   * @param {number} latitude - 纬度
   */
  async findCommunity(longitude, latitude) {
    return http.get(`/community/find?longitude=${longitude}&latitude=${latitude}`)
  },

  /**
   * 获取社区内的服务列表
   * @param {number} communityId - 社区ID
   * @param {number} userLat - 用户纬度
   * @param {number} userLng - 用户经度
   */
  async getCommunityServices(communityId, userLat, userLng) {
    return http.get(`/community/services/${communityId}?userLat=${userLat}&userLng=${userLng}`)
  },

  /**
   * 获取跨社区的附近服务
   * @param {number} userLat - 用户纬度
   * @param {number} userLng - 用户经度
   * @param {number} radiusKm - 半径（公里）
   * @param {number} excludeCommunityId - 排除的社区ID（可选）
   */
  async getNearbyServices(userLat, userLng, radiusKm = 3, excludeCommunityId = null) {
    let url = `/community/services/nearby?userLat=${userLat}&userLng=${userLng}&radiusKm=${radiusKm}`
    if (excludeCommunityId !== null) {
      url += `&excludeCommunityId=${excludeCommunityId}`
    }
    return http.get(url)
  },
  
  // 用户注册
  async register(userData) {
    return http.post('/auth/register', userData)
  },

  // 用户登录
  async login(credentials) {
    return http.post('/auth/login', credentials)
  },

  // 重置密码
  async resetPassword(data) {
    return http.post('/auth/reset-password', data)
  },

  // 获取用户信息
  async getUserInfo() {
    return http.get('/user/profile')
  },

  // 更新用户信息
  async updateUserInfo(data) {
    return http.put('/user/profile', {
      username: data.username,
      email: data.email,
      phone: data.phone
    })
  },

  // 修改密码
  async changePassword(data) {
    return http.put('/user/password', {
      oldPassword: data.oldPassword,
      newPassword: data.newPassword
    })
  },

  // 删除账户
  async deleteAccount(confirmation) {
    return http.delete('/user/delete', {
      data: { confirmation }
    })
  },

  // 退出登录
  async logout() {
    try {
      await http.post('/auth/logout')
    } finally {
      // 无论API调用成功与否，都清除本地存储
      this.clearLocalStorage()
    }
  },

  // 检查登录状态
  isAuthenticated() {
    return !!this.getToken()
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
  },

  // 获取token
  getToken() {
    return localStorage.getItem('auth_token')
  },

  // 保存token
  saveToken(token) {
    localStorage.setItem('auth_token', token)
  },

  // 清除所有本地存储
  clearLocalStorage() {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('user_id')
    localStorage.removeItem('user_info')
    localStorage.removeItem('petpal_userRole')
    localStorage.removeItem('petpal_isLoggedIn')
  },

  // 保存登录状态
  saveLoginState(token, userId, userInfo) {
    // 保存token和用户ID
    this.saveToken(token)
    localStorage.setItem('user_id', userId)

    // 确保userInfo包含所有必要字段
    const fullUserInfo = {
      userId: userId,
      username: userInfo.username || userInfo.name || '',
      email: userInfo.email || '',
      phone: userInfo.phone || '',
      role: userInfo.role || '',
      createdAt: userInfo.createdAt || new Date().toISOString(),
      reputationScore: userInfo.reputationScore || 0,
      reputationLevel: userInfo.reputationLevel || '新手',
      isRealNameCertified: userInfo.isRealNameCertified || false,
      isPetCertified: userInfo.isPetCertified || false
    }
    
    // 保存完整的用户信息
    this.saveUserInfo(fullUserInfo)
    
    // 专门保存角色信息，供Profile.vue和Layout.vue使用
    if (fullUserInfo.role) {
      localStorage.setItem('petpal_userRole', fullUserInfo.role)
      localStorage.setItem('petpal_isLoggedIn', 'true')
    }
    
    // 触发storage事件，让其他页面知道状态变化
    window.dispatchEvent(new StorageEvent('storage', {
      key: 'user_info',
      newValue: JSON.stringify(fullUserInfo)
    }))
  },

  // 从本地存储获取角色
  getUserRole() {
    return localStorage.getItem('petpal_userRole') || 'user'
  },

  // 检查是否已登录
  isLoggedIn() {
    return localStorage.getItem('petpal_isLoggedIn') === 'true'
  },

  // 获取用户ID
  getUserId() {
    return localStorage.getItem('user_id')
  },

  // 更新本地用户信息（部分更新）
  updateLocalUserInfo(updates) {
    const currentUser = this.getCurrentUser()
    if (currentUser) {
      const updatedUser = { ...currentUser, ...updates }
      this.saveUserInfo(updatedUser)
      return updatedUser
    }
    return null
  }
}