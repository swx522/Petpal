// src/services/sitter.js
import { http } from '@/utils/http.js'

class SitterService {
  // ===============================
  // 服务者资质管理
  // ===============================

  /**
   * 获取服务者审核状态
   */
  async getAuditStatus() {
    try {
      const response = await http.get('/sitter/audit/status')
      return response
    } catch (error) {
      console.error('获取审核状态失败:', error)
      throw error
    }
  }

  /**
   * 提交审核资料
   * @param {Object} materials - 审核资料对象
   */
  async submitAuditMaterials(materials) {
    try {
      const response = await http.post('/sitter/audit/materials', materials)
      return response
    } catch (error) {
      console.error('提交审核资料失败:', error)
      throw error
    }
  }

  /**
   * 获取已提交的审核资料
   */
  async getAuditMaterials() {
    try {
      const response = await http.get('/sitter/audit/materials')
      return response
    } catch (error) {
      console.error('获取审核资料失败:', error)
      throw error
    }
  }

  // ===============================
  // 接单相关功能
  // ===============================

  /**
   * 获取可接单的需求列表
   * @param {Object} filters - 筛选条件
   * @param {string} filters.type - 服务类型筛选
   * @param {number} filters.page - 页码
   * @param {number} filters.pageSize - 每页数量
   */
  async getAvailableRequests(filters = {}) {
    try {
      const params = {
        type: filters.type || '',
        page: filters.page || 1,
        pageSize: filters.pageSize || 10
      }

      // 使用服务者专用的API端点，避免循环序列化问题
      const response = await http.get('/sitter/requests/available', params)
      return response
    } catch (error) {
      console.error('获取可接单需求失败:', error)
      throw error
    }
  }

  /**
   * 获取单个需求的详情
   * @param {string} requestId - 需求ID
   */
  async getRequestDetail(requestId) {
    try {
      const response = await http.get(`/requests/detail/${requestId}`)
      return response
    } catch (error) {
      console.error('获取需求详情失败:', error)
      throw error
    }
  }

  /**
   * 接受需求（接单）
   * @param {string} requestId - 需求ID
   */
  async acceptRequest(requestId) {
    try {
      // 使用服务者专用的API端点
      const response = await http.post(`/sitter/requests/accept/${requestId}`)
      return response
    } catch (error) {
      console.error('接受需求失败:', error)
      throw error
    }
  }

  /**
   * 计算与需求发布者的距离
   * @param {string} requestId - 需求ID
   */
  async calculateDistance(requestId) {
    try {
      const response = await http.get('/location/distance', { requestId })
      return response
    } catch (error) {
      console.error('计算距离失败:', error)
      throw error
    }
  }

  // ===============================
  // 订单管理
  // ===============================

  /**
   * 获取服务者已完成的订单列表
   * @param {Object} options - 分页选项
   * @param {number} options.page - 页码
   * @param {number} options.pageSize - 每页数量
   */
  async getFinishedOrders(options = {}) {
    try {
      const params = {
        page: options.page || 1,
        pageSize: options.pageSize || 10
      }
      
      const response = await http.get('/orders/finished', params)
      return response
    } catch (error) {
      console.error('获取已完成订单失败:', error)
      throw error
    }
  }

  /**
   * 获取单个订单的评价反馈
   * @param {string} orderId - 订单ID
   */
  async getOrderFeedback(orderId) {
    try {
      const response = await http.get(`/orders/feedback/${orderId}`)
      return response
    } catch (error) {
      console.error('获取订单评价失败:', error)
      throw error
    }
  }

  // ===============================
  // 个人资料管理（统一使用 UserService）
  // ===============================

  // 注意：个人资料和密码修改已统一到 UserService
  // 相关功能请使用 user.js 中的方法

  // ===============================
  // 实用工具方法
  // ===============================

  /**
   * 获取服务类型映射
   */
  getServiceTypeMapping() {
    return {
      walk: { label: "遛狗服务 🚶", color: "#3b82f6" },
      feed: { label: "喂食照顾 🍽️", color: "#10b981" },
      medical: { label: "就医陪伴 🏥", color: "#ef4444" },
      groom: { label: "美容护理 ✂️", color: "#8b5cf6" },
      other: { label: "其他服务 🐾", color: "#6b7280" }
    }
  }

  /**
   * 获取宠物类型映射
   */
  getPetTypeMapping() {
    return {
      dog: { label: "狗狗", emoji: "🐶" },
      cat: { label: "猫咪", emoji: "🐱" },
      rabbit: { label: "兔兔", emoji: "🐰" },
      bird: { label: "鸟鸟", emoji: "🐦" },
      other: { label: "其他", emoji: "🐾" }
    }
  }

  /**
   * 格式化后端返回的需求数据
   * @param {Object} requestData - 后端返回的需求数据
   */
  formatRequestData(requestData) {
    const serviceTypeMap = this.getServiceTypeMapping()
    const petTypeMap = this.getPetTypeMapping()
    
    return {
      id: requestData.id,
      title: requestData.title,
      petType: requestData.petType,
      petTypeName: petTypeMap[requestData.petType]?.label || "宠物",
      petEmoji: petTypeMap[requestData.petType]?.emoji || "🐾",
      type: requestData.serviceType,
      typeName: serviceTypeMap[requestData.serviceType]?.label || "服务",
      typeColor: serviceTypeMap[requestData.serviceType]?.color || "#6b7280",
      description: requestData.description,
      distance: requestData.distance || 0,
      location: requestData.communityName || "未知位置",
      publisher: requestData.user?.name || requestData.user?.username || "匿名用户",
      startTime: requestData.startTime,
      endTime: requestData.endTime,
      createdAt: requestData.createdAt,
      rewardPoints: 0, // 根据后端实际字段调整
      matchRate: 0,    // 根据后端实际字段调整
      urgent: false    // 根据实际业务逻辑确定
    }
  }

  /**
   * 格式化后端返回的反馈数据
   * @param {Object} feedbackData - 后端返回的反馈数据
   */
  formatFeedbackData(feedbackData) {
    const petTypeMap = this.getPetTypeMapping()
    
    return {
      id: feedbackData.orderId,
      orderId: feedbackData.orderId,
      serviceType: feedbackData.orderTitle,
      petType: "dog", // 需要根据实际数据调整
      userName: feedbackData.evaluations[0]?.evaluator?.username || "用户",
      userRating: feedbackData.averageScore,
      rating: feedbackData.averageScore,
      comment: feedbackData.evaluations[0]?.content || "暂无评价内容",
      location: "未知位置", // 需要根据实际数据调整
      completedTime: feedbackData.completedAt,
      // 示例数据，实际应从订单数据中获取
      petTypeName: "宠物",
      petEmoji: "🐾"
    }
  }

  /**
   * 格式化时间
   * @param {string} timeString - 时间字符串
   */
  formatTime(timeString) {
    if (!timeString) return ''
    const date = new Date(timeString)
    return date.toLocaleString('zh-CN', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  /**
   * 格式化日期
   * @param {string} dateString - 日期字符串
   */
  formatDate(dateString) {
    if (!dateString) return ''
    const date = new Date(dateString)
    return date.toLocaleDateString('zh-CN', { 
      month: 'short', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    })
  }

  // ===============================
  // 错误处理
  // ===============================

  /**
   * 处理API错误
   * @param {Error} error - 错误对象
   */
  handleApiError(error) {
    const defaultMessage = '请求失败，请稍后重试'
    
    if (error.response?.data?.message) {
      return error.response.data.message
    }
    
    if (error.message) {
      // 处理常见的错误信息
      if (error.message.includes('未认证')) {
        return '请先登录'
      }
      if (error.message.includes('权限')) {
        return '权限不足，请确认账户类型'
      }
      if (error.message.includes('审核')) {
        return '请先完成服务者资质审核'
      }
      return error.message
    }
    
    return defaultMessage
  }

  /**
   * 检查用户是否为审核通过的服务者
   */
  async checkSitterStatus() {
    try {
      const auditStatus = await this.getAuditStatus()
      return auditStatus.data?.auditStatus === 'Approved'
    } catch (error) {
      console.error('检查服务者状态失败:', error)
      return false
    }
  }
}

// 创建单例实例
const sitterService = new SitterService()

export default sitterService