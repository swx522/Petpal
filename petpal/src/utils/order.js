// src/utils/order.js
import { http } from '@/utils/http.js'

// ============ 单独导出的常用函数（保持向后兼容） ============

// 宠物信息管理
export const getPetTypes = async () => {
  try {
    return await http.get('/pet/types')
  } catch (error) {
    console.error('获取宠物类型失败:', error)
    return {
      success: false,
      message: error.message || '获取宠物类型失败'
    }
  }
}

export const getServiceCategories = async () => {
  try {
    return await http.get('/service/categories')
  } catch (error) {
    console.error('获取服务类型失败:', error)
    return {
      success: false,
      message: error.message || '获取服务类型失败'
    }
  }
}

// 评价管理
export const submitEvaluation = async (data) => {
  try {
    return await http.post('/evaluate/submit', {
      orderId: data.orderId,
      score: data.score,
      content: data.content
    })
  } catch (error) {
    console.error('提交评价失败:', error)
    return {
      success: false,
      message: error.message || '提交评价失败'
    }
  }
}

export const updateEvaluation = async (data) => {
  try {
    return await http.put('/evaluate/update', {
      evaluationId: data.evaluationId,
      score: data.score,
      content: data.content
    })
  } catch (error) {
    console.error('更新评价失败:', error)
    return {
      success: false,
      message: error.message || '更新评价失败'
    }
  }
}

// 宠物主人订单管理
export const getMyOrders = async (options = {}) => {
  try {
    const params = {
      page: options.page || 1,
      pageSize: options.pageSize || 10
    }
    // 只有当status存在且不为undefined时才传递
    if (options.status !== undefined && options.status !== null) {
      params.status = options.status
    }
    return await http.get('/user/orders', params)
  } catch (error) {
    console.error('获取订单列表失败:', error)
    return {
      success: false,
      message: error.message || '获取订单列表失败'
    }
  }
}

export const getOrdersToEvaluate = async () => {
  try {
    return await http.get('/order/to-evaluate')
  } catch (error) {
    console.error('获取待评价订单失败:', error)
    return {
      success: false,
      message: error.message || '获取待评价订单失败'
    }
  }
}

// 服务者订单管理
export const getFinishedOrders = async (options = {}) => {
  try {
    // 使用新的统一订单接口
    return await http.get('/user/orders', {
      page: options.page || 1,
      pageSize: options.pageSize || 10,
      executionStatus: 'Completed' // 只获取已完成的订单
    })
  } catch (error) {
    console.error('获取已完成订单失败:', error)
    return {
      success: false,
      message: error.message || '获取已完成订单失败'
    }
  }
}

export const getOrderFeedback = async (orderId) => {
  try {
    return await http.get(`/orders/feedback/${orderId}`)
  } catch (error) {
    console.error('获取订单评价失败:', error)
    return {
      success: false,
      message: error.message || '获取订单评价失败'
    }
  }
}

// 需求发布管理
export const createRequest = async (data) => {
  try {
    return await http.post('/request/create', {
      title: data.title,
      petType: data.petType,
      serviceType: data.serviceType,
      startTime: data.startTime,
      endTime: data.endTime,
      description: data.description
    })
  } catch (error) {
    console.error('发布需求失败:', error)
    return {
      success: false,
      message: error.message || '发布需求失败'
    }
  }
}

export const setSchedule = async (data) => {
  try {
    return await http.post('/schedule/set', {
      startTime: data.startTime,
      endTime: data.endTime
    })
  } catch (error) {
    console.error('设置时间失败:', error)
    return {
      success: false,
      message: error.message || '设置时间失败'
    }
  }
}

// 服务者接单管理
export const getAvailableRequests = async (options = {}) => {
  try {
    return await http.get('/requests/available', {
      type: options.type,
      page: options.page || 1,
      pageSize: options.pageSize || 10
    })
  } catch (error) {
    console.error('获取可接单需求失败:', error)
    return {
      success: false,
      message: error.message || '获取可接单需求失败'
    }
  }
}

export const getRequestDetail = async (requestId) => {
  try {
    return await http.get(`/requests/detail/${requestId}`)
  } catch (error) {
    console.error('获取需求详情失败:', error)
    return {
      success: false,
      message: error.message || '获取需求详情失败'
    }
  }
}

export const acceptRequest = async (requestId) => {
  try {
    return await http.post(`/requests/accept/${requestId}`)
  } catch (error) {
    console.error('接受需求失败:', error)
    return {
      success: false,
      message: error.message || '接受需求失败'
    }
  }
}

export const calculateDistance = async (requestId) => {
  try {
    return await http.get('/location/distance', {
      requestId: requestId
    })
  } catch (error) {
    console.error('计算距离失败:', error)
    return {
      success: false,
      message: error.message || '计算距离失败'
    }
  }
}

export const deleteOrder = async (orderId) => {
  try {
    return await http.delete(`/order/${orderId}`)
  } catch (error) {
    console.error('删除订单失败:', error)
    return {
      success: false,
      message: error.message || '删除订单失败'
    }
  }
}

// 管理员审核管理
export const getReviewList = async (options = {}) => {
  try {
    return await http.get('/requests/review/list', {
      status: options.status,
      serviceType: options.serviceType,
      page: options.page || 1,
      pageSize: options.pageSize || 10
    })
  } catch (error) {
    console.error('获取审核列表失败:', error)
    return {
      success: false,
      message: error.message || '获取审核列表失败'
    }
  }
}

export const getReviewDetail = async (requestId) => {
  try {
    return await http.get(`/requests/review/detail/${requestId}`)
  } catch (error) {
    console.error('获取审核详情失败:', error)
    return {
      success: false,
      message: error.message || '获取审核详情失败'
    }
  }
}

export const approveRequest = async (data) => {
  try {
    return await http.put('/requests/review/pass', {
      requestId: data.requestId,
      comment: data.comment
    })
  } catch (error) {
    console.error('审核通过失败:', error)
    return {
      success: false,
      message: error.message || '审核通过失败'
    }
  }
}

export const rejectRequest = async (data) => {
  try {
    return await http.put('/requests/review/reject', {
      requestId: data.requestId,
      comment: data.comment,
      rejectionReason: data.rejectionReason
    })
  } catch (error) {
    console.error('拒绝需求失败:', error)
    return {
      success: false,
      message: error.message || '拒绝需求失败'
    }
  }
}

export const recheckRequest = async (data) => {
  try {
    return await http.put('/requests/review/recheck', {
      requestId: data.requestId
    })
  } catch (error) {
    console.error('重新审核失败:', error)
    return {
      success: false,
      message: error.message || '重新审核失败'
    }
  }
}

// 地理位置接口
export const getLocation = async () => {
  try {
    return await http.get('/area/location')
  } catch (error) {
    console.error('获取位置信息失败:', error)
    return {
      success: false,
      message: error.message || '获取位置信息失败'
    }
  }
}

// 订单评分接口
export const rateOrder = async (orderId, data) => {
  try {
    return await http.post(`/orders/${orderId}/rating`, {
      evaluatedUserId: data.evaluatedUserId,
      evaluationType: data.evaluationType,
      score: data.score,
      content: data.content
    })
  } catch (error) {
    console.error('提交评分失败:', error)
    return {
      success: false,
      message: error.message || '提交评分失败'
    }
  }
}

export const getOrderRatings = async (orderId) => {
  try {
    return await http.get(`/orders/${orderId}/ratings`)
  } catch (error) {
    console.error('获取评分列表失败:', error)
    return {
      success: false,
      message: error.message || '获取评分列表失败'
    }
  }
}

// 宠物信息提交
export const submitPetProfile = async (data) => {
  try {
    return await http.post('/pet/profile', {
      name: data.name,
      type: data.type,
      breed: data.breed,
      age: data.age,
      isVaccinated: data.isVaccinated,
      description: data.description
    })
  } catch (error) {
    console.error('提交宠物信息失败:', error)
    return {
      success: false,
      message: error.message || '提交宠物信息失败'
    }
  }
}

// ============ 工具函数（单独导出） ============

export const calculateTimeInterval = (startTime, endTime) => {
  try {
    const start = new Date(startTime)
    const end = new Date(endTime)
    const diffMs = end - start
    
    if (diffMs < 0) return '时间错误'
    
    const diffHours = diffMs / (1000 * 60 * 60)
    
    if (diffHours < 1) {
      const minutes = Math.round(diffHours * 60)
      return `${minutes}分钟`
    } else if (diffHours < 24) {
      const hours = Math.round(diffHours)
      return `${hours}小时`
    } else {
      const days = Math.round(diffHours / 24)
      return `${days}天`
    }
  } catch (error) {
    return '未知'
  }
}

export const formatDateTime = (dateTime, format = 'short') => {
  try {
    const date = new Date(dateTime)
    
    if (isNaN(date.getTime())) {
      return '无效日期'
    }
    
    const options = {
      short: {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      },
      long: {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
      },
      dateOnly: {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      },
      timeOnly: {
        hour: '2-digit',
        minute: '2-digit'
      }
    }
    
    return date.toLocaleDateString('zh-CN', options[format] || options.short)
  } catch (error) {
    return '日期格式错误'
  }
}

export const formatOrderStatus = (status) => {
  const OrderStatus = {
    PENDING: 'Pending',
    ACCEPTED: 'Accepted',
    IN_PROGRESS: 'InProgress',
    COMPLETED: 'Completed',
    CANCELLED: 'Cancelled'
  }
  
  const statusMap = {
    [OrderStatus.PENDING]: { 
      text: '待接单', 
      color: '#fbbf24', 
      bgColor: '#fef3c7',
      icon: '⏳',
      badgeColor: 'warning'
    },
    [OrderStatus.ACCEPTED]: { 
      text: '已接单', 
      color: '#3b82f6', 
      bgColor: '#dbeafe',
      icon: '👥',
      badgeColor: 'info'
    },
    [OrderStatus.IN_PROGRESS]: { 
      text: '进行中', 
      color: '#8b5cf6', 
      bgColor: '#ede9fe',
      icon: '🚀',
      badgeColor: 'primary'
    },
    [OrderStatus.COMPLETED]: { 
      text: '已完成', 
      color: '#10b981', 
      bgColor: '#d1fae5',
      icon: '✅',
      badgeColor: 'success'
    },
    [OrderStatus.CANCELLED]: { 
      text: '已取消', 
      color: '#ef4444', 
      bgColor: '#fee2e2',
      icon: '❌',
      badgeColor: 'error'
    }
  }
  
  return statusMap[status] || { 
    text: status, 
    color: '#64748b', 
    bgColor: '#f1f5f9',
    icon: '📝',
    badgeColor: 'default'
  }
}

export const formatPetType = (petType) => {
  const PetType = {
    DOG: 'dog',
    CAT: 'cat',
    RABBIT: 'rabbit',
    BIRD: 'bird',
    OTHER: 'other'
  }
  
  const petTypeMap = {
    [PetType.DOG]: { 
      label: '狗狗', 
      icon: '🐶', 
      color: '#f59e0b',
      bgColor: '#fef3c7'
    },
    [PetType.CAT]: { 
      label: '猫咪', 
      icon: '🐱', 
      color: '#8b5cf6',
      bgColor: '#ede9fe'
    },
    [PetType.RABBIT]: { 
      label: '兔兔', 
      icon: '🐰', 
      color: '#ec4899',
      bgColor: '#fce7f3'
    },
    [PetType.BIRD]: { 
      label: '鸟鸟', 
      icon: '🐦', 
      color: '#06b6d4',
      bgColor: '#cffafe'
    },
    [PetType.OTHER]: { 
      label: '其他', 
      icon: '🐾', 
      color: '#64748b',
      bgColor: '#f1f5f9'
    }
  }
  
  return petTypeMap[petType] || { 
    label: petType, 
    icon: '🐾', 
    color: '#64748b',
    bgColor: '#f1f5f9'
  }
}

export const formatServiceType = (serviceType) => {
  const ServiceType = {
    WALK: 'walk',
    FEED: 'feed',
    MEDICAL: 'medical',
    GROOM: 'groom',
    OTHER: 'other'
  }
  
  const serviceTypeMap = {
    [ServiceType.WALK]: { 
      label: '遛狗服务', 
      icon: '🚶', 
      color: '#10b981',
      bgColor: '#d1fae5'
    },
    [ServiceType.FEED]: { 
      label: '喂食照顾', 
      icon: '🍽️', 
      color: '#f59e0b',
      bgColor: '#fef3c7'
    },
    [ServiceType.MEDICAL]: { 
      label: '就医陪伴', 
      icon: '🏥', 
      color: '#ef4444',
      bgColor: '#fee2e2'
    },
    [ServiceType.GROOM]: { 
      label: '美容护理', 
      icon: '✂️', 
      color: '#8b5cf6',
      bgColor: '#ede9fe'
    },
    [ServiceType.OTHER]: { 
      label: '其他服务', 
      icon: '🐾', 
      color: '#64748b',
      bgColor: '#f1f5f9'
    }
  }
  
  return serviceTypeMap[serviceType] || { 
    label: serviceType, 
    icon: '🐾', 
    color: '#64748b',
    bgColor: '#f1f5f9'
  }
}

export const generateOrderNumber = (orderId, createdAt) => {
  try {
    const date = new Date(createdAt)
    const dateStr = date.toISOString().slice(0, 10).replace(/-/g, '')
    const idPrefix = orderId ? orderId.substring(0, 4).toUpperCase() : 'XXXX'
    return `OD${dateStr}${idPrefix}`
  } catch (error) {
    return `OD${Date.now().toString().slice(-8)}`
  }
}

export const validateRequirementData = (data) => {
  const errors = []
  const fieldErrors = {}
  
  if (!data.petType) {
    errors.push('请选择宠物类型')
    fieldErrors.petType = '请选择宠物类型'
  }
  
  if (!data.serviceType) {
    errors.push('请选择服务类型')
    fieldErrors.serviceType = '请选择服务类型'
  }
  
  if (!data.title || data.title.trim().length === 0) {
    errors.push('请填写需求标题')
    fieldErrors.title = '请填写需求标题'
  } else if (data.title.trim().length < 3) {
    errors.push('标题至少需要3个字符')
    fieldErrors.title = '标题至少需要3个字符'
  } else if (data.title.length > 100) {
    errors.push('标题不能超过100个字符')
    fieldErrors.title = '标题不能超过100个字符'
  }
  
  if (!data.startTime) {
    errors.push('请选择开始时间')
    fieldErrors.startTime = '请选择开始时间'
  }
  
  if (!data.endTime) {
    errors.push('请选择结束时间')
    fieldErrors.endTime = '请选择结束时间'
  }
  
  if (data.startTime && data.endTime) {
    const start = new Date(data.startTime)
    const end = new Date(data.endTime)
    const now = new Date()
    
    if (start >= end) {
      errors.push('开始时间必须早于结束时间')
      fieldErrors.startTime = '开始时间必须早于结束时间'
      fieldErrors.endTime = '结束时间必须晚于开始时间'
    }
    
    if (start <= now) {
      errors.push('开始时间必须晚于当前时间')
      fieldErrors.startTime = '开始时间必须晚于当前时间'
    }
  }
  
  if (!data.description || data.description.trim().length === 0) {
    errors.push('请填写需求描述')
    fieldErrors.description = '请填写需求描述'
  } else if (data.description.trim().length < 10) {
    errors.push('描述至少需要10个字符')
    fieldErrors.description = '描述至少需要10个字符'
  } else if (data.description.length > 500) {
    errors.push('描述不能超过500个字符')
    fieldErrors.description = '描述不能超过500个字符'
  }
  
  return {
    isValid: errors.length === 0,
    errors,
    fieldErrors
  }
}

export const calculateReputationScore = (evaluations) => {
  if (!evaluations?.length) {
    return {
      totalEvaluations: 0,
      averageScore: 0,
      positiveRate: 0,
      starRating: 0
    }
  }
  
  const total = evaluations.length
  const totalScore = evaluations.reduce((sum, evaluation) => sum + (evaluation.score || 0), 0)
  const positiveCount = evaluations.filter(evaluation => (evaluation.score || 0) >= 4).length
  const averageScore = totalScore / total
  const positiveRate = (positiveCount / total) * 100
  
  const starRating = Math.round((averageScore / 5) * 5)
  
  return {
    totalEvaluations: total,
    averageScore: parseFloat(averageScore.toFixed(1)),
    positiveRate: parseFloat(positiveRate.toFixed(1)),
    starRating: Math.min(starRating, 5)
  }
}

export const filterOrders = (orders, filters) => {
  if (!Array.isArray(orders)) return []
  
  return orders.filter(order => {
    if (filters?.status && order.status !== filters.status) {
      return false
    }
    
    if (filters?.serviceType && order.serviceType !== filters.serviceType) {
      return false
    }
    
    if (filters?.petType && order.petType !== filters.petType) {
      return false
    }
    
    if (filters?.startDate) {
      const orderDate = new Date(order.createdAt || order.startTime)
      const filterDate = new Date(filters.startDate)
      if (orderDate < filterDate) return false
    }
    
    if (filters?.endDate) {
      const orderDate = new Date(order.createdAt || order.endTime)
      const filterDate = new Date(filters.endDate)
      if (orderDate > filterDate) return false
    }
    
    if (filters?.keyword) {
      const keyword = filters.keyword.toLowerCase()
      const searchFields = [
        order.title,
        order.description,
        order.orderNumber,
        order.petName
      ].filter(Boolean).map(field => field.toLowerCase())
      
      if (!searchFields.some(field => field.includes(keyword))) {
        return false
      }
    }
    
    return true
  })
}

// ============ 枚举常量（单独导出） ============

export const OrderStatus = {
  PENDING: 'Pending',
  ACCEPTED: 'Accepted',
  IN_PROGRESS: 'InProgress',
  COMPLETED: 'Completed',
  CANCELLED: 'Cancelled'
}

export const EvaluationType = {
  user_TO_HELPER: 'user_to_helper',
  HELPER_TO_user: 'helper_to_user'
}

export const PetType = {
  DOG: 'dog',
  CAT: 'cat',
  RABBIT: 'rabbit',
  BIRD: 'bird',
  OTHER: 'other'
}

export const ServiceType = {
  WALK: 'walk',
  FEED: 'feed',
  MEDICAL: 'medical',
  GROOM: 'groom',
  OTHER: 'other'
}

// ============ 订单API服务（整合所有功能） ============

export const orderAPI = {
  // 枚举常量
  OrderStatus,
  EvaluationType,
  PetType,
  ServiceType,
  
  // API方法（引用单独导出的函数）
  getPetTypes,
  getServiceCategories,
  submitEvaluation,
  updateEvaluation,
  getMyOrders,
  getOrdersToEvaluate,
  getFinishedOrders,
  deleteOrder,
  getOrderFeedback,
  createRequest,
  setSchedule,
  getAvailableRequests,
  getRequestDetail,
  acceptRequest,
  calculateDistance,
  getReviewList,
  getReviewDetail,
  approveRequest,
  rejectRequest,
  recheckRequest,
  getLocation,
  rateOrder,
  getOrderRatings,
  submitPetProfile,
  
  // 工具函数（引用单独导出的函数）
  calculateTimeInterval,
  formatDateTime,
  formatOrderStatus,
  formatPetType,
  formatServiceType,
  generateOrderNumber,
  validateRequirementData,
  calculateReputationScore,
  filterOrders
}

// 默认导出 orderAPI
export default orderAPI