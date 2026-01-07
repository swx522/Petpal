// src/services/sitter.js
import { http } from '@/utils/http.js'

class SitterService {
  // ===============================
  // 服务者资质管理（新增）
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
   * 提交服务者资格申请
   * @param {Object} applicationData - 申请数据
   */
  async submitApplication(applicationData) {
    try {
      const response = await http.post('/sitter/application', applicationData)
      return response
    } catch (error) {
      console.error('提交申请失败:', error)
      throw error
    }
  }

  /**
   * 获取我的申请记录
   */
  async getMyApplication() {
    try {
      const response = await http.get('/sitter/application')
      return response
    } catch (error) {
      console.error('获取申请记录失败:', error)
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
  // 接单相关功能（保持不变）
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
      const response = await http.get(`/sitter/requests/detail/${requestId}`)
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
  async getMyOrders(options = {}) {
    try {
      const params = {}
      if (options.page) params.page = options.page
      if (options.pageSize) params.pageSize = options.pageSize
      if (options.status) params.status = options.status
      if (options.executionStatus) params.executionStatus = options.executionStatus

      // 使用统一的订单查询接口
      const response = await http.get('/user/orders', params)
      return response
    } catch (error) {
      console.error('获取我的订单失败:', error)
      throw error
    }
  }

  // 保留原方法名以向后兼容，但调用新的接口
  async getFinishedOrders(options = {}) {
    return this.getMyOrders({ ...options, executionStatus: 'Completed' })
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
   * 获取审核状态映射
   */
  getAuditStatusMapping() {
    return {
      NotApplied: { 
        label: "未申请", 
        icon: "📝", 
        color: "#6b7280",
        description: "您尚未提交服务者资质申请"
      },
      Pending: { 
        label: "审核中", 
        icon: "⏳", 
        color: "#f59e0b",
        description: "管理员正在审核您的申请资料"
      },
      Approved: { 
        label: "已通过", 
        icon: "✅", 
        color: "#10b981",
        description: "恭喜！您已成为认证服务者"
      },
      Rejected: { 
        label: "已拒绝", 
        icon: "❌", 
        color: "#ef4444",
        description: "申请未通过审核，请修改后重新提交"
      },
      Resubmitted: { 
        label: "重新提交", 
        icon: "🔄", 
        color: "#8b5cf6",
        description: "您的补充资料正在审核中"
      }
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
   * 格式化审核状态数据
   * @param {Object} auditData - 后端返回的审核状态数据
   */
  formatAuditData(auditData) {
    const statusMap = this.getAuditStatusMapping()
    const status = auditData.auditStatus
    const statusInfo = statusMap[status] || statusMap.NotApplied
    
    return {
      sitterId: auditData.sitterId,
      user: auditData.user,
      auditStatus: status,
      statusInfo: statusInfo,
      stageDescription: auditData.stageDescription,
      estimatedCompletion: auditData.estimatedCompletion,
      progress: auditData.progress || 0,
      appliedAt: auditData.appliedAt,
      lastAuditAt: auditData.lastAuditAt,
      reviewComment: auditData.reviewComment
    }
  }

  /**
   * 格式化申请记录数据
   * @param {Object} applicationData - 后端返回的申请记录数据
   */
  formatApplicationData(applicationData) {
    const statusMap = this.getAuditStatusMapping()
    const status = applicationData.status
    const statusInfo = statusMap[status] || statusMap.NotApplied
    
    return {
      id: applicationData.id,
      realName: applicationData.realName,
      idCardNumber: this.maskIdCard(applicationData.idCardNumber),
      joinReason: applicationData.joinReason,
      status: status,
      statusInfo: statusInfo,
      appliedAt: applicationData.appliedAt,
      reviewedAt: applicationData.reviewedAt,
      reviewComment: applicationData.reviewComment
    }
  }

  /**
   * 隐藏身份证中间部分（用于显示）
   * @param {string} idCard - 身份证号码
   */
  maskIdCard(idCard) {
    if (!idCard || idCard.length < 8) return idCard
    const firstFour = idCard.substring(0, 4)
    const lastFour = idCard.substring(idCard.length - 4)
    return `${firstFour}********${lastFour}`
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

  /**
   * 格式化审核时间（更详细）
   * @param {string} timeString - 时间字符串
   */
  formatAuditTime(timeString) {
    if (!timeString) return '--'
    const date = new Date(timeString)
    const now = new Date()
    const diffMs = now - date
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
    
    if (diffDays === 0) {
      return '今天 ' + date.toLocaleTimeString('zh-CN', { 
        hour: '2-digit', 
        minute: '2-digit' 
      })
    } else if (diffDays === 1) {
      return '昨天 ' + date.toLocaleTimeString('zh-CN', { 
        hour: '2-digit', 
        minute: '2-digit' 
      })
    } else if (diffDays < 7) {
      return `${diffDays}天前`
    } else {
      return date.toLocaleDateString('zh-CN', { 
        month: '2-digit', 
        day: '2-digit' 
      })
    }
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
      if (error.message.includes('已是审核通过的服务者')) {
        return '您已经是认证服务者了'
      }
      if (error.message.includes('待审核的申请')) {
        return '您已有待审核的申请，请耐心等待'
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

  /**
   * 获取审核状态详情
   */
  async getAuditStatusDetail() {
    try {
      const response = await this.getAuditStatus()
      if (response.success) {
        return this.formatAuditData(response.data)
      }
      return null
    } catch (error) {
      console.error('获取审核状态详情失败:', error)
      return null
    }
  }

  /**
   * 获取申请记录详情
   */
  async getApplicationDetail() {
    try {
      const response = await this.getMyApplication()
      if (response.success) {
        return this.formatApplicationData(response.data)
      }
      return null
    } catch (error) {
      console.error('获取申请记录详情失败:', error)
      return null
    }
  }

  /**
   * 验证申请数据
   * @param {Object} applicationData - 申请数据
   */
  validateApplication(applicationData) {
    const errors = []
    
    if (!applicationData.realName || applicationData.realName.trim().length < 2) {
      errors.push('真实姓名至少需要2个字符')
    }
    
    if (!applicationData.idCardNumber || !this.isValidIdCard(applicationData.idCardNumber)) {
      errors.push('请输入有效的18位身份证号码')
    }
    
    if (!applicationData.joinReason || applicationData.joinReason.trim().length < 10) {
      errors.push('申请原因至少需要10个字符')
    }
    
    return {
      isValid: errors.length === 0,
      errors: errors
    }
  }

  /**
   * 验证身份证号码（简单验证）
   * @param {string} idCard - 身份证号码
   */
  isValidIdCard(idCard) {
    if (!idCard || typeof idCard !== 'string') return false
    
    // 移除空格
    const cleanedId = idCard.trim()
    
    // 检查长度（15位旧版或18位新版）
    if (cleanedId.length !== 15 && cleanedId.length !== 18) {
      return false
    }
    
    // 简单格式检查
    if (cleanedId.length === 18) {
      const pattern = /^[1-9]\d{5}(19|20)\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$/
      return pattern.test(cleanedId)
    }
    
    if (cleanedId.length === 15) {
      const pattern = /^[1-9]\d{7}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\d{3}$/
      return pattern.test(cleanedId)
    }
    
    return false
  }
}

// 创建单例实例
const sitterService = new SitterService()

export default sitterService