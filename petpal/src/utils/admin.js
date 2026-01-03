// src/utils/admin.js
import { http } from '@/utils/http.js'

// 管理员API服务
export const adminAPI = {
  // ============ 社区统计相关API ============
  
  /**
   * 获取社区数据概览统计
   */
  async getCommunityStats() {
    try {
      return await http.get('/admin/community/stats')
    } catch (error) {
      console.error('获取社区统计失败:', error)
      return {
        success: false,
        message: error.message || '获取社区统计失败'
      }
    }
  },

  /**
   * 获取成员分布数据
   */
  async getMemberDistribution() {
    try {
      return await http.get('/admin/community/distribution')
    } catch (error) {
      console.error('获取成员分布失败:', error)
      return {
        success: false,
        message: error.message || '获取成员分布失败'
      }
    }
  },

  /**
   * 获取社区活跃度趋势数据
   * @param {number} days - 天数，默认7天
   */
  async getActivityTrend(days = 7) {
    try {
      return await http.get(`/admin/community/activity?days=${days}`)
    } catch (error) {
      console.error('获取活跃度趋势失败:', error)
      return {
        success: false,
        message: error.message || '获取活跃度趋势失败'
      }
    }
  },

  // ============ 成员管理相关API ============
  
  /**
   * 获取社区成员列表
   * @param {Object} filters - 筛选条件
   * @param {string} filters.role - 角色筛选: 'User'/'Sitter'
   * @param {string} filters.auditStatus - 审核状态筛选: 'Pending'/'Approved'/'Rejected'
   * @param {number} filters.page - 页码
   * @param {number} filters.pageSize - 每页数量
   */
  async getCommunityMembers(filters = {}) {
    try {
      const params = {
        page: filters.page || 1,
        pageSize: filters.pageSize || 12,
        ...(filters.role && { role: filters.role }),
        ...(filters.auditStatus && { auditStatus: filters.auditStatus })
      }
      return await http.get('/admin/community/members', params)
    } catch (error) {
      console.error('获取成员列表失败:', error)
      return {
        success: false,
        message: error.message || '获取成员列表失败'
      }
    }
  },

  /**
   * 搜索社区成员
   * @param {string} keyword - 搜索关键词
   * @param {Object} filters - 筛选条件
   */
  async searchMembers(keyword, filters = {}) {
    try {
      if (!keyword || keyword.trim() === '') {
        return {
          success: false,
          message: '搜索关键词不能为空'
        }
      }
      
      const params = {
        keyword: keyword.trim(),
        page: filters.page || 1,
        pageSize: filters.pageSize || 12,
        ...(filters.role && { role: filters.role }),
        ...(filters.auditStatus && { auditStatus: filters.auditStatus })
      }
      
      return await http.get('/admin/community/members/search', params)
    } catch (error) {
      console.error('搜索成员失败:', error)
      return {
        success: false,
        message: error.message || '搜索成员失败'
      }
    }
  },

  /**
   * 修改成员角色
   * @param {string} memberId - 成员ID
   * @param {string} newRole - 新角色: 'User'/'Sitter'
   */
  async changeMemberRole(memberId, newRole) {
    try {
      if (!memberId || !newRole) {
        return {
          success: false,
          message: '成员ID和新角色不能为空'
        }
      }
      
      return await http.put('/admin/community/members/role', {
        memberId,
        newRole
      })
    } catch (error) {
      console.error('修改成员角色失败:', error)
      return {
        success: false,
        message: error.message || '修改成员角色失败'
      }
    }
  },

  /**
   * 移除社区成员
   * @param {string} memberId - 成员ID
   */
  async removeMember(memberId) {
    try {
      if (!memberId) {
        return {
          success: false,
          message: '成员ID不能为空'
        }
      }
      
      return await http.delete(`/admin/community/members/remove/${memberId}`, {
      })
    } catch (error) {
      console.error('移除成员失败:', error)
      return {
        success: false,
        message: error.message || '移除成员失败'
      }
    }
  },

  /**
   * 审核通过服务者资质
   * @param {string} memberId - 成员ID
   */
  async approveQualification(memberId) {
    try {
      if (!memberId) {
        return {
          success: false,
          message: '成员ID不能为空'
        }
      }
      
      return await http.put('/admin/community/members/approve', {
        memberId
      })
    } catch (error) {
      console.error('审核通过失败:', error)
      return {
        success: false,
        message: error.message || '审核通过失败'
      }
    }
  },

  /**
   * 拒绝服务者资质
   * @param {string} memberId - 成员ID
   * @param {string} reason - 拒绝原因
   */
  async rejectQualification(memberId, reason) {
    try {
      if (!memberId || !reason) {
        return {
          success: false,
          message: '成员ID和拒绝原因不能为空'
        }
      }
      
      return await http.put('/admin/community/members/reject', {
        memberId,
        reason
      })
    } catch (error) {
      console.error('拒绝审核失败:', error)
      return {
        success: false,
        message: error.message || '拒绝审核失败'
      }
    }
  },

  /**
   * 允许服务者重新提交资质审核
   * @param {string} memberId - 成员ID
   */
  async allowResubmitQualification(memberId) {
    try {
      if (!memberId) {
        return {
          success: false,
          message: '成员ID不能为空'
        }
      }
      
      return await http.put('/admin/community/members/allow-resubmit', {
        memberId
      })
    } catch (error) {
      console.error('允许重审失败:', error)
      return {
        success: false,
        message: error.message || '允许重审失败'
      }
    }
  },

  /**
   * 重新审核服务者资质
   * @param {string} memberId - 成员ID
   */
  async reReviewQualification(memberId) {
    try {
      if (!memberId) {
        return {
          success: false,
          message: '成员ID不能为空'
        }
      }
      
      return await http.put('/admin/community/members/rereview', {
        memberId
      })
    } catch (error) {
      console.error('重新审核失败:', error)
      return {
        success: false,
        message: error.message || '重新审核失败'
      }
    }
  },

  // ============ 需求审核相关API ============
  
  /**
   * 获取需求审核列表
   * @param {Object} filters - 筛选条件
   * @param {string} filters.status - 状态筛选: 'pending'/'approved'/'rejected'
   * @param {string} filters.serviceType - 服务类型筛选
   * @param {number} filters.page - 页码
   * @param {number} filters.pageSize - 每页数量
   */
  async getReviewList(filters = {}) {
    try {
      // 将前端状态转换为后端状态
      let backendStatus = 'Pending'
      if (filters.status === 'approved') backendStatus = 'Approved'
      if (filters.status === 'rejected') backendStatus = 'Rejected'
      
      const params = {
        page: filters.page || 1,
        pageSize: filters.pageSize || 10,
        ...(backendStatus && { status: backendStatus }),
        ...(filters.serviceType && filters.serviceType !== 'all' && { serviceType: filters.serviceType })
      }
      
      return await http.get('/admin/requests/review/list', params)
    } catch (error) {
      console.error('获取审核列表失败:', error)
      return {
        success: false,
        message: error.message || '获取审核列表失败'
      }
    }
  },

  /**
   * 获取单个需求的审核详情
   * @param {string} requestId - 需求ID
   */
  async getReviewDetail(requestId) {
    try {
      if (!requestId) {
        return {
          success: false,
          message: '需求ID不能为空'
        }
      }
      
      return await http.get(`/admin/requests/review/detail/${requestId}`)
    } catch (error) {
      console.error('获取审核详情失败:', error)
      return {
        success: false,
        message: error.message || '获取审核详情失败'
      }
    }
  },

  /**
   * 审核通过需求
   * @param {string} requestId - 需求ID
   */
  async approveRequest(requestId) {
    try {
      if (!requestId) {
        return {
          success: false,
          message: '需求ID不能为空'
        }
      }
      
      return await http.put('/admin/requests/review/pass', {
        requestId
      })
    } catch (error) {
      console.error('审核通过失败:', error)
      return {
        success: false,
        message: error.message || '审核通过失败'
      }
    }
  },

  /**
   * 审核拒绝需求
   * @param {string} requestId - 需求ID
   * @param {string} reason - 拒绝原因
   */
  async rejectRequest(requestId, reason) {
    try {
      if (!requestId || !reason) {
        return {
          success: false,
          message: '需求ID和拒绝原因不能为空'
        }
      }
      
      return await http.put('/admin/requests/review/reject', {
        requestId,
        reason
      })
    } catch (error) {
      console.error('审核拒绝失败:', error)
      return {
        success: false,
        message: error.message || '审核拒绝失败'
      }
    }
  },

  /**
   * 重新审核需求
   * @param {string} requestId - 需求ID
   */
  async recheckRequest(requestId) {
    try {
      if (!requestId) {
        return {
          success: false,
          message: '需求ID不能为空'
        }
      }
      
      return await http.put('/admin/requests/review/recheck', {
        requestId
      })
    } catch (error) {
      console.error('重新审核失败:', error)
      return {
        success: false,
        message: error.message || '重新审核失败'
      }
    }
  },

  /**
   * 删除审核记录
   * @param {string} requestId - 需求ID
   */
  async deleteReviewRecord(requestId) {
    try {
      if (!requestId) {
        return {
          success: false,
          message: '需求ID不能为空'
        }
      }
      
      return await http.delete('/admin/requests/review/delete', {
        requestId
      })
    } catch (error) {
      console.error('删除审核记录失败:', error)
      return {
        success: false,
        message: error.message || '删除审核记录失败'
      }
    }
  },

  /**
   * 编辑需求内容
   * @param {string} requestId - 需求ID
   * @param {Object} updates - 更新内容
   */
  async editRequest(requestId, updates) {
    try {
      if (!requestId) {
        return {
          success: false,
          message: '需求ID不能为空'
        }
      }
      
      return await http.put('/admin/requests/edit', {
        requestId,
        ...updates
      })
    } catch (error) {
      console.error('编辑需求失败:', error)
      return {
        success: false,
        message: error.message || '编辑需求失败'
      }
    }
  },

  // ============ 社区设置相关API ============
  
  /**
   * 获取社区设置信息
   */
  async getCommunitySettings() {
    try {
      return await http.get('/admin/community/settings')
    } catch (error) {
      console.error('获取社区设置失败:', error)
      return {
        success: false,
        message: error.message || '获取社区设置失败'
      }
    }
  },

  /**
   * 更新社区设置
   * @param {Object} settings - 社区设置
   */
  async updateCommunitySettings(settings) {
    try {
      if (!settings) {
        return {
          success: false,
          message: '社区设置不能为空'
        }
      }
      
      return await http.put('/admin/community/settings', settings)
    } catch (error) {
      console.error('更新社区设置失败:', error)
      return {
        success: false,
        message: error.message || '更新社区设置失败'
      }
    }
  },

  /**
   * 重置社区设置为默认值
   */
  async resetCommunitySettings() {
    try {
      const defaultSettings = {
        name: 'PetPal 社区',
        description: '宠物互助社区',
        requireApproval: true,
        autoFlagSensitive: true,
        urgentReviewTime: '4',
        rejectTemplates: `联系方式不清晰\n地址信息不完整\n需求描述不明确\n内容涉及违规\n服务时间不合理\n宠物信息不全`
      }
      
      return await this.updateCommunitySettings(defaultSettings)
    } catch (error) {
      console.error('重置社区设置失败:', error)
      return {
        success: false,
        message: error.message || '重置社区设置失败'
      }
    }
  },

  // ============ 工具方法 ============
  
  /**
   * 验证管理员权限
   */
async verifyAdminPermission() {
  try {
    // 检查本地存储是否有管理员标识
    const userRole = localStorage.getItem('petpal_userRole')
    console.log('当前用户角色:', userRole)
    
    // 打印更多调试信息
    console.log('localStorage内容:', {
      userRole: userRole,
      auth_token: localStorage.getItem('auth_token'),
      userId: localStorage.getItem('petpal_userId')
    })
    
    // 检查是否为管理员（2）或版主（admin）
    if (userRole !== '2' && userRole !== 'admin') {
      console.warn('权限不足：当前角色 =', userRole, '，需要2或admin')
      return {
        success: false,
        message: '需要管理员权限'
      }
    }
    
    return {
      success: true,
      message: '权限验证通过'
    }
  } catch (error) {
    console.error('权限验证失败:', error)
    return {
      success: false,
      message: error.message || '权限验证失败'
    }
  }
},

  /**
   * 统一错误处理
   * @param {Error} error - 错误对象
   * @param {string} action - 操作名称
   */
  handleError(error, action = '操作') {
    console.error(`${action}失败:`, error)
    
    let message = `${action}失败`
    if (error.response) {
      // HTTP错误响应
      switch (error.response.status) {
        case 401:
          message = '未授权，请重新登录'
          break
        case 403:
          message = '权限不足，需要管理员权限'
          break
        case 404:
          message = '资源不存在'
          break
        case 500:
          message = '服务器内部错误'
          break
        default:
          message = `服务器错误: ${error.response.status}`
      }
    } else if (error.request) {
      // 网络错误
      message = '网络连接失败，请检查网络设置'
    } else {
      // 其他错误
      message = error.message || `${action}失败`
    }
    
    return {
      success: false,
      message,
      error: error
    }
  },

  /**
   * 显示成功提示
   * @param {string} message - 成功消息
   */
  showSuccess(message) {
    // 这里可以集成UI框架的通知系统
    if (typeof window !== 'undefined' && window.alert) {
      alert(message)
    }
    console.log('操作成功:', message)
  },

  /**
   * 显示错误提示
   * @param {string} message - 错误消息
   */
  showError(message) {
    // 这里可以集成UI框架的通知系统
    if (typeof window !== 'undefined' && window.alert) {
      alert(message)
    }
    console.error('操作失败:', message)
  },

  /**
   * 确认对话框
   * @param {string} message - 确认消息
   * @returns {Promise<boolean>} - 用户是否确认
   */
  async confirmDialog(message) {
    if (typeof window !== 'undefined' && window.confirm) {
      return window.confirm(message)
    }
    return false
  },

  /**
   * 格式化时间
   * @param {string|Date} date - 日期
   * @returns {string} - 格式化后的时间
   */
  formatTime(date) {
    if (!date) return '未设置'
    const dateObj = new Date(date)
    return dateObj.toLocaleString('zh-CN', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  },

  /**
   * 格式化日期
   * @param {string|Date} date - 日期
   * @returns {string} - 格式化后的日期
   */
  formatDate(date) {
    if (!date) return ''
    const dateObj = new Date(date)
    return dateObj.toLocaleDateString('zh-CN', { 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  },

  /**
   * 获取宠物类型emoji
   * @param {string} petType - 宠物类型
   * @returns {string} - emoji
   */
  getPetEmoji(petType) {
    const emojiMap = {
      dog: '🐶',
      cat: '🐱',
      rabbit: '🐰',
      bird: '🐦',
      other: '🐾'
    }
    return emojiMap[petType] || '🐾'
  },

  /**
   * 获取宠物类型名称
   * @param {string} petType - 宠物类型
   * @returns {string} - 类型名称
   */
  getPetTypeName(petType) {
    const typeMap = {
      dog: '狗狗',
      cat: '猫咪',
      rabbit: '兔兔',
      bird: '鸟鸟',
      other: '其他宠物'
    }
    return typeMap[petType] || '宠物'
  },

  /**
   * 获取服务类型颜色
   * @param {string} type - 服务类型
   * @returns {string} - 颜色代码
   */
  getTypeColor(type) {
    const colorMap = {
      walk: '#3b82f6',    // 蓝色
      feed: '#10b981',    // 绿色
      medical: '#ef4444', // 红色
      groom: '#8b5cf6',   // 紫色
      other: '#6b7280'    // 灰色
    }
    return colorMap[type] || '#6b7280'
  },

  /**
   * 获取服务类型名称
   * @param {string} type - 服务类型
   * @returns {string} - 类型名称
   */
  getTypeName(type) {
    const typeMap = {
      walk: '遛狗服务',
      feed: '喂食照顾',
      medical: '就医陪伴',
      groom: '美容护理',
    }
    return typeMap[type] || '其他服务'
  }
}

// 导出默认实例
export default adminAPI