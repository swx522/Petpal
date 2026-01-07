<!-- src/views/SitterOrders.vue -->
<template>
  <div class="sitter-orders">
    <!-- 页面标题和切换 -->
    <div class="page-header">
      <div class="header-left">
        <h1>我的订单</h1>
        <p>管理您已接受的订单服务</p>
      </div>
      
      <div class="header-actions">
        <!-- 订单状态筛选 -->
        <div class="status-filter">
          <button 
            v-for="filter in statusFilters" 
            :key="filter.id"
            class="filter-btn"
            :class="{ active: activeStatus === filter.id }"
            @click="activeStatus = filter.id"
          >
            <span class="filter-icon">{{ filter.icon }}</span>
            {{ filter.label }}
            <span v-if="filter.count" class="filter-count">{{ filter.count }}</span>
          </button>
        </div>
      </div>
    </div>
    
    <!-- 统计卡片 -->
    <div class="stats-container">
      <div class="stats-card total">
        <div class="stats-icon">📋</div>
        <div class="stats-info">
          <div class="stats-number">{{ stats.total }}</div>
          <div class="stats-label">总订单数</div>
        </div>
      </div>
      
      <div class="stats-card pending">
        <div class="stats-icon">⏳</div>
        <div class="stats-info">
          <div class="stats-number">{{ stats.pending }}</div>
          <div class="stats-label">待完成</div>
        </div>
      </div>   
      
      <div class="stats-card completed">
        <div class="stats-icon">✅</div>
        <div class="stats-info">
          <div class="stats-number">{{ stats.completed }}</div>
          <div class="stats-label">已完成</div>
        </div>
      </div>
    </div>
    
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-container">
      <div class="loading-spinner"></div>
      <p>加载订单中...</p>
    </div>
    
    <!-- 订单列表 -->
    <div v-else class="orders-container">
      <!-- 订单卡片网格 -->
      <div class="orders-grid">
        <div 
          v-for="order in filteredOrders" 
          :key="order.id"
          class="order-card"
          :class="order.status"
        >
          <!-- 订单头部 -->
          <div class="order-header">
            <div class="order-info">
              <div class="pet-avatar">{{ order.petEmoji }}</div>
              <div class="order-details">
                <h3>{{ order.title }}</h3>
                <div class="order-meta">
                  <span class="order-id">订单号: {{ order.orderId }}</span>
                  <span class="order-date">接单时间: {{ formatDate(order.acceptedAt) }}</span>
                </div>
              </div>
            </div>
            
            <div class="order-status">
              <div class="status-badge" :class="order.status">
                {{ getStatusText(order.status) }}
              </div>
            </div>
          </div>
          
          <!-- 订单详情 -->
          <div class="order-details-section">
            <div class="details-grid">
              <div class="detail-item">
                <span class="detail-label">
                  <span class="detail-icon">🐾</span>
                  服务类型:
                </span>
                <span class="detail-value">{{ order.serviceType }}</span>
              </div>
              
              <div class="detail-item">
                <span class="detail-label">
                  <span class="detail-icon">🐕</span>
                  宠物信息:
                </span>
                <span class="detail-value">{{ order.petName }} ({{ order.petType }})</span>
              </div>
              
              <div class="detail-item">
                <span class="detail-label">
                  <span class="detail-icon">⏰</span>
                  服务时间:
                </span>
                <span class="detail-value">
                  {{ formatDateTime(order.serviceTime) }}
                  <span v-if="order.urgency" class="urgent-tag">紧急</span>
                </span>
              </div>
              
              <div class="detail-item">
                <span class="detail-label">
                  <span class="detail-icon">💬</span>
                  备注:
                </span>
                <span class="detail-value">{{ order.requirements }}</span>
              </div>

              <!-- 社区信息 -->
              <div v-if="order.community" class="detail-item">
                <span class="detail-label">
                  <span class="detail-icon">🏘️</span>
                  服务社区:
                </span>
                <span class="detail-value">
                  {{ order.community.name }}
                  <button
                    @click="toggleMapView(order.id)"
                    class="map-toggle-btn"
                    :class="{ active: expandedMapOrder === order.id }"
                  >
                    <span class="map-icon">🗺️</span>
                    {{ expandedMapOrder === order.id ? '收起地图' : '查看地图' }}
                  </button>
                </span>
              </div>
            </div>

            <!-- 地图展开区域 -->
            <div v-if="expandedMapOrder === order.id && order.community" class="map-container">
              <div class="map-header">
                <h4>{{ order.community.name }} 位置</h4>
                <p v-if="order.community.description" class="community-description">
                  {{ order.community.description }}
                </p>
              </div>
              <div class="map-content" :id="`map-${order.id}`" :ref="`map-${order.id}`">
                <!-- 地图将在组件挂载时初始化 -->
              </div>
            </div>
          </div>
          
          <!-- 订单操作 -->
          <div class="order-actions">
            <!-- 待处理订单操作 -->
            <div v-if="order.status === 'pending'" class="action-buttons">              
                <button 
                class="action-btn contact-btn"
                @click="showCompleteDialog(order)"
                >
                点击确认完成
                </button>
            </div>
            
            <!-- 已完成订单操作 -->
            <div v-else-if="order.status === 'completed'" class="action-buttons">
              <button 
                v-if="!order.hasFeedback"
                class="action-btn feedback-btn"
                @click="viewFeedback(order)"
                :disabled="processingOrderId === order.id"
              >
                {{ processingOrderId === order.id ? '加载中...' : '查看评价' }}
              </button>
              
              <button 
                v-else
                class="action-btn feedback-btn has-feedback"
                @click="viewFeedback(order)"
              >
                ⭐ {{ order.rating }} 分
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 无订单提示 -->
      <div v-if="filteredOrders.length === 0" class="no-orders">
        <div class="empty-state">
          <div class="empty-icon">
            <span v-if="activeStatus === 'pending'">⏳</span>
            <span v-else-if="activeStatus === 'in_progress'">🔄</span>
            <span v-else-if="activeStatus === 'completed'">✅</span>
            <span v-else>📋</span>
          </div>
          <h3>{{ getEmptyMessage(activeStatus) }}</h3>
          <p>{{ getEmptyDescription(activeStatus) }}</p>
          <div class="empty-actions">
            <button v-if="activeStatus === 'all'" class="go-to-accept" @click="goToAcceptOrders">
              🦴 前往接单
            </button>
            <button v-if="activeStatus !== 'all'" class="back-to-all" @click="activeStatus = 'all'">
              查看所有订单
            </button>
          </div>
        </div>
      </div>
      
      <!-- 分页控制 -->
      <div v-if="filteredOrders.length > 0" class="pagination-controls">
        <button 
          class="pagination-btn prev-btn"
          @click="prevPage"
          :disabled="pagination.page <= 1"
        >
          ← 上一页
        </button>
        
        <div class="pagination-info">
          第 {{ pagination.page }} 页 / 共 {{ pagination.totalPages }} 页
          <span class="pagination-count">(共 {{ pagination.totalItems }} 个订单)</span>
        </div>
        
        <button 
          class="pagination-btn next-btn"
          @click="nextPage"
          :disabled="pagination.page >= pagination.totalPages"
        >
          下一页 →
        </button>
      </div>
    </div>
    
    <!-- 完成订单对话框 -->
    <div v-if="showCompleteDialogFlag" class="complete-dialog">
      <div class="dialog-overlay" @click="closeCompleteDialog"></div>
      <div class="dialog-content">
        <div class="dialog-header">
          <h2>确认完成订单</h2>
          <button class="close-btn" @click="closeCompleteDialog">×</button>
        </div>
        
        <div class="dialog-body">
          <!-- 订单信息 -->
          <div class="order-summary">
            <div class="summary-details">
              <div class="summary-item">
                <span>服务类型:</span>
                <strong>{{ selectedOrder?.serviceType }}</strong>
              </div>
              <div class="summary-item">
                <span>服务时间:</span>
                <strong>{{ formatDateTime(selectedOrder?.serviceTime) }}</strong>
              </div>
              <div class="summary-item">
                <span>服务地点:</span>
                <strong>{{ selectedOrder?.location }}</strong>
              </div>
            </div>
          </div>
          
          <!-- 服务完成表单 -->
          <div class="completion-form">
            <div class="form-section">
              <label class="form-label">
                <span class="label-icon">✅</span>
                服务完成情况
              </label>
              
              <div class="completion-checklist">
                <div 
                  v-for="item in completionChecklist" 
                  :key="item.id"
                  class="checklist-item"
                  :class="{ checked: item.checked }"
                  @click="toggleChecklistItem(item.id)"
                >
                  <div class="checkmark">{{ item.checked ? '✓' : '' }}</div>
                  <div class="checklist-content">
                    <div class="checklist-title">{{ item.title }}</div>
                    <div v-if="item.description" class="checklist-desc">{{ item.description }}</div>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="form-section">
              <label class="form-label">
                <span class="label-icon">💬</span>
                服务备注
              </label>
              <textarea 
                v-model="completionNotes"
                placeholder="请简要描述服务完成情况，如宠物的状态、特殊注意事项等..."
                rows="4"
                class="notes-textarea"
              ></textarea>
              <div class="notes-count">
                {{ completionNotes.length }}/500
              </div>
            </div>
            
            <div class="form-section">
              <label class="form-label">
                <span class="label-icon">📸</span>
                服务照片（可选）
              </label>
              <div class="photo-upload">
                <div class="upload-placeholder" @click="triggerFileUpload">
                  <div class="upload-icon">+</div>
                  <div class="upload-text">添加照片</div>
                </div>
                <input 
                  type="file" 
                  ref="fileInput"
                  multiple
                  accept="image/*"
                  @change="handlePhotoUpload"
                  style="display: none"
                />
                
                <div v-if="uploadedPhotos.length > 0" class="photo-preview">
                  <div 
                    v-for="(photo, index) in uploadedPhotos" 
                    :key="index"
                    class="preview-item"
                  >
                    <div class="preview-image">{{ photo.emoji }}</div>
                    <button class="remove-photo" @click="removePhoto(index)">×</button>
                  </div>
                </div>
              </div>
              <div class="photo-tip">最多上传 5 张照片，每张不超过 5MB</div>
            </div>
          </div>
          
          <!-- 确认区域 -->
          <div class="confirmation-section">
            <div class="agreement">
              <label class="checkbox-label">
                <input 
                  type="checkbox" 
                  v-model="agreeTerms"
                  class="checkbox-input"
                />
                <span class="checkbox-custom"></span>
                我已确认服务完成，并愿意接受客户的评价
              </label>
            </div>
            
            <div class="dialog-actions">
              <button 
                class="dialog-btn cancel-btn"
                @click="closeCompleteDialog"
                :disabled="processingOrderId"
              >
                取消
              </button>
              <button 
                class="dialog-btn confirm-complete-btn"
                @click="confirmCompleteOrder"
                :disabled="!canCompleteOrder || processingOrderId"
              >
                {{ processingOrderId ? '提交中...' : '确认完成' }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- 取消订单对话框 -->
    <div v-if="showCancelDialogFlag" class="cancel-dialog">
      <div class="dialog-overlay" @click="closeCancelDialog"></div>
      <div class="dialog-content">
        <div class="dialog-header">
          <h2>取消订单</h2>
          <button class="close-btn" @click="closeCancelDialog">×</button>
        </div>
        
        <div class="dialog-body">
          <div class="warning-message">
            <div class="warning-icon">⚠️</div>
            <div class="warning-content">
              <h4>取消订单会影响您的信誉评分</h4>
              <p>频繁取消订单可能导致接单权限受限</p>
            </div>
          </div>
          
          <div class="cancel-form">
            <div class="form-section">
              <label class="form-label">取消原因 *</label>
              <div class="cancel-reasons">
                <div 
                  v-for="reason in cancelReasons" 
                  :key="reason.id"
                  class="reason-option"
                  :class="{ selected: selectedCancelReason === reason.id }"
                  @click="selectCancelReason(reason.id)"
                >
                  <div class="reason-icon">{{ reason.icon }}</div>
                  <div class="reason-info">
                    <div class="reason-title">{{ reason.title }}</div>
                    <div class="reason-desc">{{ reason.description }}</div>
                  </div>
                </div>
              </div>
            </div>
            
            <div class="form-section">
              <label class="form-label">详细说明（可选）</label>
              <textarea 
                v-model="cancelExplanation"
                placeholder="请详细说明取消订单的原因..."
                rows="3"
                class="cancel-textarea"
              ></textarea>
            </div>
          </div>
          
          <div class="cancel-warning">
            <p>⚠️ 取消后，订单将不可恢复。请确认是否继续？</p>
          </div>
          
          <div class="dialog-actions">
            <button 
              class="dialog-btn back-btn"
              @click="closeCancelDialog"
              :disabled="processingOrderId"
            >
              返回
            </button>
            <button 
              class="dialog-btn confirm-cancel-btn"
              @click="confirmCancelOrder"
              :disabled="!selectedCancelReason || processingOrderId"
            >
              {{ processingOrderId ? '处理中...' : '确认取消' }}
            </button>
          </div>
        </div>
      </div>
    </div>
    
    <!-- 操作结果提示 -->
    <div v-if="operationResult" class="operation-result" :class="operationResult.type">
      <div class="result-content">
        <span class="result-icon">{{ operationResult.icon }}</span>
        <p>{{ operationResult.message }}</p>
        <button class="result-close" @click="operationResult = null">×</button>
      </div>
    </div>
    
    <!-- 订单详情侧边栏 -->
    <div v-if="showOrderDetail" class="order-detail-sidebar">
      <div class="sidebar-overlay" @click="closeOrderDetail"></div>
      <div class="sidebar-content">
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onActivated, onUnmounted, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { userAPI } from '@/utils/user.js'

const router = useRouter()

// 订单数据和状态
const orders = ref([])
const loading = ref(false)
const error = ref(null)

// 调试信息
console.log('🔧 SitterOrders.vue 初始化开始')
const activeStatus = ref('all')
const sortBy = ref('time')
const showCompleteDialogFlag = ref(false)
const showCancelDialogFlag = ref(false)
const showOrderDetail = ref(false)
const selectedOrder = ref(null)
const processingOrderId = ref(null)
const operationResult = ref(null)

// 地图相关状态
const expandedMapOrder = ref(null)
const mapInstances = ref(new Map())

// 完成订单相关状态
const completionNotes = ref('')
const agreeTerms = ref(false)
const uploadedPhotos = ref([])
const completionChecklist = ref([
  { id: 'pet_health', title: '宠物健康状况良好', description: '确认宠物无异常状况', checked: true },
  { id: 'service_complete', title: '服务内容已完成', description: '按照要求完成所有服务项目', checked: true },
  { id: 'environment_tidy', title: '环境整洁干净', description: '服务后清理现场', checked: false },
  { id: 'customer_satisfied', title: '客户表示满意', description: '与服务客户确认完成', checked: false }
])

// 取消订单相关状态
const selectedCancelReason = ref('')
const cancelExplanation = ref('')

// 分页
const pagination = ref({
  page: 1,
  pageSize: 10,
  totalItems: 0,
  totalPages: 1
})

// 筛选器
const statusFilters = ref([
  { id: 'all', label: '全部订单', icon: '📋', count: 0 },
  { id: 'pending', label: '待完成', icon: '⏳', count: 0 },
  { id: 'completed', label: '已完成', icon: '✅', count: 0 },
])

// 取消原因选项
const cancelReasons = ref([
  { id: 'time_conflict', icon: '⏰', title: '时间冲突', description: '有其他安排，无法按时提供服务' },
  { id: 'distance_too_far', icon: '📍', title: '距离太远', description: '服务地点超出预期距离' },
  { id: 'pet_special_needs', icon: '🐕', title: '宠物特殊需求', description: '宠物有超出能力的特殊需求' },
  { id: 'emergency', icon: '🚨', title: '紧急情况', description: '个人突发紧急情况' },
  { id: 'other', icon: '📝', title: '其他原因', description: '其他原因请详细说明' }
])

// 统计信息
const stats = computed(() => {
  const allOrders = orders.value
  return {
    total: allOrders.length,
    pending: allOrders.filter(o => o.status === 'pending').length,
    inProgress: allOrders.filter(o => o.status === 'in_progress').length,
    completed: allOrders.filter(o => o.status === 'completed').length,
    cancelled: allOrders.filter(o => o.status === 'cancelled').length
  }
})

// 筛选后的订单
const filteredOrders = computed(() => {
  let filtered = orders.value
  
  // 状态筛选
  if (activeStatus.value !== 'all') {
    filtered = filtered.filter(order => order.status === activeStatus.value)
  }
  
  // 排序
  filtered = [...filtered].sort((a, b) => {
    switch (sortBy.value) {
      case 'price':
        return b.price - a.price
      case 'distance':
        return parseFloat(a.distance) - parseFloat(b.distance)
      case 'time':
      default:
        return new Date(b.acceptedAt) - new Date(a.acceptedAt)
    }
  })
  
  return filtered
})

// 是否可以完成订单
const canCompleteOrder = computed(() => {
  const allChecked = completionChecklist.value.every(item => item.checked)
  return allChecked && agreeTerms.value
})

// 初始化
// 获取服务者订单数据
const fetchOrders = async (statusFilter = null) => {
  loading.value = true
  error.value = null

  try {
    const filters = {}
    if (statusFilter && statusFilter !== 'all') {
      filters.status = statusFilter
    }

    console.log('🔄 正在获取订单数据...', { statusFilter, filters })
    const response = await userAPI.getUserOrders(filters)

    if (response.success) {
      // 转换后端数据格式为前端需要的格式
      const transformedOrders = response.data.orders.map(order => ({
        id: order.id,
        orderId: order.orderNumber,
        title: order.title,
        serviceType: order.serviceType,
        petType: order.petType,
        petName: '宠物', // 后端数据中可能没有宠物名字，先用默认值
        petEmoji: order.petType === 'Dog' ? '🐕' : order.petType === 'Cat' ? '🐈' : '🐾',
        serviceTime: order.startTime,
        startTime: order.startTime,
        endTime: order.endTime,
        status: order.status.toLowerCase(),
        executionStatus: order.executionStatus.toLowerCase(),
        createdAt: order.createdAt,
        acceptedAt: order.acceptedAt,
        completedAt: order.completedAt,
        customerName: order.owner?.name || '宠物主人',
        customerPhone: order.owner?.phone,
        timeline: generateTimeline(order),
        location: '位置信息待完善', // 后端暂时没有详细地址
        requirements: order.title, // 暂时用标题作为需求描述
        community: order.community ? {
          id: order.community.id,
          name: order.community.name,
          description: order.community.description,
          centerLng: order.community.centerLng,
          centerLat: order.community.centerLat
        } : null
      }))

      orders.value = transformedOrders
      console.log('✅ 订单数据获取成功，共', transformedOrders.length, '个订单')

      // 更新筛选器计数
      updateFilterCounts()
    } else {
      error.value = response.message || '获取订单失败'
      console.error('❌ 获取订单失败:', response.message)
    }
  } catch (err) {
    error.value = err.message || '网络错误'
    console.error('❌ 获取订单数据失败:', err)
  } finally {
    loading.value = false
  }
}

// 生成订单时间线
const generateTimeline = (order) => {
  const timeline = []

  // 订单创建
  if (order.createdAt) {
    timeline.push({
      icon: '📝',
      title: '订单创建',
      time: formatDateTime(order.createdAt),
      completed: true,
      active: false
    })
  }

  // 接单时间
  if (order.acceptedAt) {
    timeline.push({
      icon: '👍',
      title: '您已接单',
      time: formatDateTime(order.acceptedAt),
      completed: true,
      active: false
    })
  }

  // 服务开始（简化逻辑）
  if (order.startTime && new Date(order.startTime) <= new Date()) {
    timeline.push({
      icon: '🚀',
      title: '服务开始',
      time: formatDateTime(order.startTime),
      completed: true,
      active: order.status.toLowerCase() === 'in_progress'
    })
  } else if (order.startTime) {
    timeline.push({
      icon: '⏳',
      title: '等待开始',
      time: `预计 ${formatDateTime(order.startTime)}`,
      completed: false,
      active: order.status.toLowerCase() === 'pending'
    })
  }

  // 服务完成
  if (order.completedAt) {
    timeline.push({
      icon: '✅',
      title: '服务完成',
      time: formatDateTime(order.completedAt),
      completed: true,
      active: order.status.toLowerCase() === 'completed'
    })
  }

  return timeline
}

// 格式化日期时间
const formatDateTime = (dateTime) => {
  if (!dateTime) return ''
  const date = new Date(dateTime)
  const month = (date.getMonth() + 1).toString().padStart(2, '0')
  const day = date.getDate().toString().padStart(2, '0')
  const hours = date.getHours().toString().padStart(2, '0')
  const minutes = date.getMinutes().toString().padStart(2, '0')
  return `${month}-${day} ${hours}:${minutes}`
}

// 前往接单页面
const goToAcceptOrders = () => {
  router.push('/accept')
}

// 监听状态筛选变化
watch(activeStatus, async (newStatus) => {
  console.log('状态筛选变化:', newStatus)
  await fetchOrders(newStatus)
})

// 地图相关函数
const toggleMapView = async (orderId) => {
  if (expandedMapOrder.value === orderId) {
    // 收起地图
    expandedMapOrder.value = null
    // 清理地图实例
    const mapInstance = mapInstances.value.get(orderId)
    if (mapInstance) {
      mapInstance.destroy()
      mapInstances.value.delete(orderId)
    }
  } else {
    // 展开地图
    expandedMapOrder.value = orderId
    // 延迟初始化地图，确保DOM已渲染
    await nextTick()
    await initializeMap(orderId)
  }
}

const initializeMap = async (orderId) => {
  try {
    const order = orders.value.find(o => o.id === orderId)
    if (!order || !order.community) return

    // 获取地图容器
    const mapContainer = document.getElementById(`map-${orderId}`)
    if (!mapContainer) return

    // 初始化高德地图
    const map = new window.AMap.Map(`map-${orderId}`, {
      center: [order.community.centerLng, order.community.centerLat],
      zoom: 15,
      resizeEnable: true
    })

    // 添加标记
    const marker = new window.AMap.Marker({
      position: [order.community.centerLng, order.community.centerLat],
      title: order.community.name
    })

    map.add(marker)

    // 保存地图实例
    mapInstances.value.set(orderId, map)

    console.log(`🗺️ 地图初始化完成: ${order.community.name}`)
  } catch (error) {
    console.error('地图初始化失败:', error)
  }
}

// 页面卸载时清理地图实例
onUnmounted(() => {
  mapInstances.value.forEach(map => {
    map.destroy()
  })
  mapInstances.value.clear()
})

onMounted(async () => {
  console.log('🔄 onMounted 执行开始')

  try {
    // 检查服务者审核状态
    console.log('🔍 开始检查审核状态')
    const auditResponse = await userAPI.getSitterAuditStatus()

    if (auditResponse.success) {
      const auditStatus = auditResponse.data.auditStatus
      console.log('✅ 审核状态:', auditStatus)

      // 如果审核未通过，跳转到接单页面
      if (auditStatus !== 'Approved') {
        const statusMessages = {
          'NotApplied': '您还未申请成为服务者，请先提交服务者资质申请。',
          'Pending': '您的服务者资质正在审核中，请耐心等待审核结果。',
          'Resubmitted': '您的补充资料正在审核中，请耐心等待。',
          'Rejected': '您的服务者资质申请未通过，请查看审核意见并重新提交申请。'
        }

        const message = statusMessages[auditStatus] || '您的服务者资质审核状态不允许访问此页面。'
        alert(message)

        // 跳转到接单页面
        router.push('/accept')
        return
      }
    } else {
      alert('无法获取审核状态，请稍后重试。')
      router.push('/accept')
      return
    }

    // 审核通过，获取订单数据
    console.log('📦 开始获取订单数据')
    await fetchOrders()
    updateFilterCounts()
    console.log('✅ onMounted 执行完成')
  } catch (error) {
    console.error('❌ onMounted 执行失败:', error)
    alert('页面加载失败，请刷新重试。')
  }
})

// 页面重新激活时刷新数据（用户从其他页面返回时）
onActivated(async () => {
  console.log('🔄 检测到页面重新激活，正在刷新订单数据...')

  // 显示一个轻量的加载提示
  const wasLoading = loading.value
  if (!wasLoading) {
    loading.value = true
  }

  await fetchOrders(activeStatus.value)
  updateFilterCounts()

  console.log('✅ 订单数据已更新')
})

// 更新筛选器计数
const updateFilterCounts = () => {
  statusFilters.value = statusFilters.value.map(filter => ({
    ...filter,
    count: filter.id === 'all' 
      ? orders.value.length 
      : orders.value.filter(o => o.status === filter.id).length
  }))
}

// 状态文本转换
const getStatusText = (status) => {
  const mapping = {
    'pending': '待完成',
    'in_progress': '进行中',
    'completed': '已完成',
    'cancelled': '已取消'
  }
  return mapping[status] || status
}

// 无订单提示消息
const getEmptyMessage = (status) => {
  const messages = {
    'all': '您目前没有订单',
    'pending': '暂无待完成订单',
    'in_progress': '暂无进行中订单',
    'completed': '暂无已完成订单',
    'cancelled': '暂无已取消订单'
  }
  return messages[status] || '暂无订单'
}

const getEmptyDescription = (status) => {
  const descriptions = {
    'all': '您还没有接受任何订单，前往接单页面开始您的宠物服务吧！',
    'pending': '待完成的订单会显示在这里',
    'in_progress': '正在服务的订单会显示在这里',
    'completed': '已完成的订单会显示在这里',
    'cancelled': '已取消的订单会显示在这里'
  }
  return descriptions[status] || ''
}

// 日期格式化
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit'
  })
}


// 开始订单
const startOrder = (order) => {
  processingOrderId.value = order.id
  
  setTimeout(() => {
    // 模拟 API 调用
    orders.value = orders.value.map(o => 
      o.id === order.id 
        ? { 
            ...o, 
            status: 'in_progress',
            timeline: [
              ...(o.timeline || []).slice(0, -1),
              { icon: '🔄', title: '服务进行中', time: '正在服务', completed: false, active: true }
            ]
          }
        : o
    )
    
    processingOrderId.value = null
    showOperationResult('success', '订单已开始！祝您服务顺利！')
    updateFilterCounts()
  }, 1000)
}

// 显示完成订单对话框
const showCompleteDialog = (order) => {
  selectedOrder.value = order
  showCompleteDialogFlag.value = true
  
  // 重置表单
  completionNotes.value = ''
  agreeTerms.value = false
  uploadedPhotos.value = []
  completionChecklist.value = completionChecklist.value.map(item => ({
    ...item,
    checked: ['pet_health', 'service_complete'].includes(item.id)
  }))
}

// 关闭完成订单对话框
const closeCompleteDialog = () => {
  if (!processingOrderId.value) {
    showCompleteDialogFlag.value = false
    selectedOrder.value = null
  }
}

// 切换检查项
const toggleChecklistItem = (itemId) => {
  const item = completionChecklist.value.find(item => item.id === itemId)
  if (item) {
    item.checked = !item.checked
  }
}

// 触发文件上传
const triggerFileUpload = () => {
  document.querySelector('input[type="file"]')?.click()
}

// 处理照片上传
const handlePhotoUpload = (event) => {
  const files = event.target.files
  if (files.length + uploadedPhotos.value.length > 5) {
    showOperationResult('error', '最多只能上传5张照片')
    return
  }
  
  for (let i = 0; i < files.length; i++) {
    if (files[i].size > 5 * 1024 * 1024) {
      showOperationResult('error', `${files[i].name} 文件超过5MB限制`)
      continue
    }
    
    // 模拟上传 - 实际中应该上传到服务器
    uploadedPhotos.value.push({
      id: Date.now() + i,
      name: files[i].name,
      emoji: '🖼️',
      size: files[i].size
    })
  }
  
  event.target.value = ''
}

// 移除照片
const removePhoto = (index) => {
  uploadedPhotos.value.splice(index, 1)
}

// 确认完成订单
const confirmCompleteOrder = () => {
  if (!canCompleteOrder.value || !selectedOrder.value) return
  
  processingOrderId.value = selectedOrder.value.id
  
  setTimeout(() => {
    // 模拟 API 调用
    orders.value = orders.value.map(o => 
      o.id === selectedOrder.value.id 
        ? { 
            ...o, 
            status: 'completed',
            completionNotes: completionNotes.value,
            completedAt: new Date().toISOString(),
            timeline: [
              ...(o.timeline || []).slice(0, -1),
              { icon: '✅', title: '服务完成', time: '刚刚完成', completed: true, active: true }
            ]
          }
        : o
    )
    
    processingOrderId.value = null
    showCompleteDialogFlag.value = false
    selectedOrder.value = null
    showOperationResult('success', '订单已完成！等待客户确认和评价。')
    updateFilterCounts()
  }, 1500)
}

// 显示取消订单对话框
const showCancelDialog = (order) => {
  selectedOrder.value = order
  showCancelDialogFlag.value = true
  
  // 重置表单
  selectedCancelReason.value = ''
  cancelExplanation.value = ''
}

// 关闭取消订单对话框
const closeCancelDialog = () => {
  if (!processingOrderId.value) {
    showCancelDialogFlag.value = false
    selectedOrder.value = null
  }
}

// 选择取消原因
const selectCancelReason = (reasonId) => {
  selectedCancelReason.value = reasonId
}

// 确认取消订单
const confirmCancelOrder = () => {
  if (!selectedCancelReason.value || !selectedOrder.value) return
  
  processingOrderId.value = selectedOrder.value.id
  
  setTimeout(() => {
    // 模拟 API 调用
    orders.value = orders.value.map(o => 
      o.id === selectedOrder.value.id 
        ? { 
            ...o, 
            status: 'cancelled',
            cancelReason: selectedCancelReason.value,
            cancelExplanation: cancelExplanation.value,
            cancelledAt: new Date().toISOString(),
            timeline: [
              ...(o.timeline || []).slice(0, -1),
              { icon: '❌', title: '订单取消', time: '刚刚取消', completed: true, active: true }
            ]
          }
        : o
    )
    
    processingOrderId.value = null
    showCancelDialogFlag.value = false
    selectedOrder.value = null
    showOperationResult('warning', '订单已取消。请注意频繁取消会影响信誉。')
    updateFilterCounts()
  }, 1500)
}

// 联系客户
const contactCustomer = (order) => {
  showOperationResult('info', `即将联系客户 ${order.customerName}...`)
  // 实际中这里可以跳转到聊天页面或拨打语音电话
}

// 查看评价
const viewFeedback = (order) => {
  processingOrderId.value = order.id
  
  setTimeout(() => {
    processingOrderId.value = null
    showOperationResult('info', `查看 ${order.petName} 的评价详情`)
  }, 500)
}

// 查看订单详情
const viewOrderDetails = (order) => {
  selectedOrder.value = order
  showOrderDetail.value = true
}

// 关闭订单详情
const closeOrderDetail = () => {
  showOrderDetail.value = false
  selectedOrder.value = null
}

// 查看取消原因
const viewCancelReason = (order) => {
  const reason = cancelReasons.value.find(r => r.id === order.cancelReason)
  showOperationResult('info', `取消原因: ${reason?.title || '未知原因'}`)
}

// 归档订单
const archiveOrder = (order) => {
  processingOrderId.value = order.id
  
  setTimeout(() => {
    orders.value = orders.value.filter(o => o.id !== order.id)
    processingOrderId.value = null
    showOperationResult('success', '订单已归档')
    updateFilterCounts()
  }, 1000)
}

// 创建重复订单
const createRepeatOrder = (order) => {
  const newOrder = {
    ...order,
    id: Date.now().toString(),
    orderId: `ORD${Date.now().toString().slice(-8)}`,
    status: 'pending',
    acceptedAt: new Date().toISOString(),
    timeline: [
      { icon: '📝', title: '订单创建', time: '刚刚创建', completed: true, active: false },
      { icon: '👍', title: '您已接单', time: '刚刚接单', completed: true, active: false },
      { icon: '⏳', title: '等待开始', time: '等待安排', completed: false, active: true }
    ]
  }
  
  orders.value.unshift(newOrder)
  showOperationResult('success', '已创建重复订单')
  updateFilterCounts()
}

// 分页控制
const prevPage = () => {
  if (pagination.value.page > 1) {
    pagination.value.page--
  }
}

const nextPage = () => {
  if (pagination.value.page < pagination.value.totalPages) {
    pagination.value.page++
  }
}

// 显示操作结果
const showOperationResult = (type, message) => {
  const icons = {
    success: '✅',
    error: '❌',
    warning: '⚠️',
    info: 'ℹ️'
  }
  
  operationResult.value = {
    type,
    icon: icons[type] || 'ℹ️',
    message
  }
  
  setTimeout(() => {
    operationResult.value = null
  }, 3000)
}
</script>

<style scoped>
.sitter-orders {
  width: 100%;
  max-width: 1400px;
  margin: 0 auto;
  padding: 30px 20px;
  box-sizing: border-box;
}

/* 页面标题 */
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 30px;
  flex-wrap: wrap;
  gap: 20px;
}

.header-left h1 {
  font-size: 36px;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 8px;
}

.header-left p {
  color: #64748b;
  font-size: 16px;
}

.header-actions {
  display: flex;
  gap: 20px;
  align-items: center;
  flex-wrap: wrap;
}

/* 状态筛选器 */
.status-filter {
  display: flex;
  background: #f8fafc;
  border-radius: 12px;
  padding: 6px;
  gap: 4px;
}

.filter-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  background: transparent;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;
}

.filter-btn:hover {
  background: rgba(255, 255, 255, 0.7);
  color: #475569;
}

.filter-btn.active {
  background: white;
  color: #166534;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  font-weight: 600;
}

.filter-icon {
  font-size: 16px;
}

.filter-count {
  background: #e2e8f0;
  color: #475569;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

/* 统计卡片 */
.stats-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

.stats-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  transition: all 0.3s;
  border: 1px solid #f1f5f9;
}

.stats-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.stats-card.total {
  border-left: 4px solid #3b82f6;
}

.stats-card.pending {
  border-left: 4px solid #f59e0b;
}


.stats-card.completed {
  border-left: 4px solid #22c55e;
}

.stats-icon {
  font-size: 40px;
  width: 60px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f8fafc;
  border-radius: 12px;
}

.stats-info {
  flex: 1;
}

.stats-number {
  font-size: 32px;
  font-weight: 700;
  color: #1e293b;
  margin-bottom: 4px;
}

.stats-label {
  font-size: 14px;
  color: #64748b;
}

/* 加载状态 */
.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 100px 20px;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #22c55e;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 订单网格 */
.orders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(450px, 1fr));
  gap: 25px;
  margin-bottom: 40px;
}

/* 订单卡片 */
.order-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 24px;
  transition: all 0.3s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.order-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
}

.order-card.pending {
  border-left: 4px solid #f59e0b;
}

.order-card.completed {
  border-left: 4px solid #22c55e;
}

.order-card.cancelled {
  border-left: 4px solid #9ca3af;
  opacity: 0.9;
}

/* 订单头部 */
.order-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
  padding-bottom: 20px;
  border-bottom: 1px solid #f1f5f9;
}

.order-info {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  flex: 1;
}

.pet-avatar {
  width: 56px;
  height: 56px;
  background: #f0fdf4;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
  flex-shrink: 0;
}

.order-details h3 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 8px;
  font-weight: 700;
  line-height: 1.4;
}

.order-meta {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.order-id,
.order-date {
  font-size: 13px;
  color: #64748b;
}

.order-status {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 8px;
  text-align: right;
}

.status-badge {
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-badge.pending {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.completed {
  background: #d1fae5;
  color: #166534;
}

.status-badge.cancelled {
  background: #f3f4f6;
  color: #6b7280;
}

.order-price {
  font-size: 20px;
  font-weight: 700;
  color: #166534;
}

/* 订单详情 */
.order-details-section {
  margin-bottom: 20px;
}

.details-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.detail-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.detail-label {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: #64748b;
}

.detail-icon {
  width: 20px;
  text-align: center;
}

.detail-value {
  font-size: 14px;
  color: #1e293b;
  font-weight: 500;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 8px;
}

.urgent-tag {
  background: #fee2e2;
  color: #dc2626;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
}

.distance {
  background: #f1f5f9;
  color: #475569;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
}

.customer-avatar {
  width: 24px;
  height: 24px;
  background: #3b82f6;
  color: white;
  border-radius: 50%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 600;
  margin-right: 6px;
}

.customer-rating {
  background: #fef3c7;
  color: #92400e;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 600;
  margin-left: 6px;
}

/* 订单操作 */
.order-actions {
  padding-top: 20px;
  border-top: 1px solid #f1f5f9;
}

.action-buttons {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.action-btn {
  padding: 10px 20px;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 100px;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}


.start-btn:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-1px);
}

.cancel-btn {
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cancel-btn:hover {
  background: #fee2e2;
  color: #dc2626;
  border-color: #fca5a5;
}

.contact-btn {
  background: #22c55e;
  color: white;
}

.contact-btn:hover:not(:disabled) {
  background: #16a34a;
  transform: translateY(-1px);
}

.complete-btn {
  background: #10b981;
  color: white;
}

.complete-btn:hover:not(:disabled) {
  background: #059669;
  transform: translateY(-1px);
}

.extend-btn {
  background: #8b5cf6;
  color: white;
}

.extend-btn:hover:not(:disabled) {
  background: #7c3aed;
  transform: translateY(-1px);
}

.feedback-btn {
  background: #f59e0b;
  color: white;
}

.feedback-btn:hover:not(:disabled) {
  background: #d97706;
  transform: translateY(-1px);
}

.feedback-btn.has-feedback {
  background: linear-gradient(135deg, #f59e0b, #fbbf24);
}

.details-btn {
  background: transparent;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.details-btn:hover:not(:disabled) {
  background: #f8fafc;
  color: #475569;
}

.reason-btn {
  background: #f3f4f6;
  color: #6b7280;
  border: 1px solid #e5e7eb;
}

.reason-btn:hover:not(:disabled) {
  background: #e5e7eb;
}

.archive-btn {
  background: #9ca3af;
  color: white;
}

.archive-btn:hover:not(:disabled) {
  background: #6b7280;
}

.repeat-btn {
  background: #3b82f6;
  color: white;
}

.repeat-btn:hover:not(:disabled) {
  background: #2563eb;
  transform: translateY(-1px);
}

/* 订单时间线 */
.order-timeline {
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #f1f5f9;
}

.timeline-title {
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 12px;
}

.timeline-steps {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
}

.timeline-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  flex: 1;
  position: relative;
  text-align: center;
}

.timeline-step:not(:last-child)::after {
  content: '';
  position: absolute;
  top: 20px;
  left: 50%;
  width: 100%;
  height: 2px;
  background: #e2e8f0;
  z-index: 1;
}

.timeline-step.completed:not(:last-child)::after {
  background: #22c55e;
}

.step-icon {
  width: 40px;
  height: 40px;
  background: #f8fafc;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 18px;
  position: relative;
  z-index: 2;
  transition: all 0.3s;
}

.timeline-step.active .step-icon {
  background: #22c55e;
  color: white;
  transform: scale(1.1);
}

.timeline-step.completed .step-icon {
  background: #22c55e;
  color: white;
}

.step-info {
  max-width: 100px;
}

.step-title {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 2px;
}

.step-time {
  font-size: 11px;
  color: #94a3b8;
}

/* 无订单提示 */
.no-orders {
  text-align: center;
  padding: 80px 20px;
}

.empty-state {
  max-width: 400px;
  margin: 0 auto;
}

.empty-icon {
  font-size: 72px;
  margin-bottom: 20px;
  opacity: 0.5;
}

.empty-state h3 {
  font-size: 20px;
  color: #334155;
  margin-bottom: 8px;
  font-weight: 600;
}

.empty-state p {
  color: #64748b;
  font-size: 15px;
  margin-bottom: 20px;
}

.empty-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.go-to-accept {
  padding: 12px 24px;
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
}

.go-to-accept:hover {
  background: linear-gradient(135deg, #2563eb, #1e40af);
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
}

.back-to-all {
  padding: 10px 24px;
  background: #22c55e;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.back-to-all:hover {
  background: #16a34a;
  transform: translateY(-1px);
}

/* 分页控制 */
.pagination-controls {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 30px;
  margin-top: 40px;
  padding-top: 30px;
  border-top: 1px solid #e2e8f0;
}

.pagination-btn {
  padding: 10px 20px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  color: #475569;
  cursor: pointer;
  font-weight: 500;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.pagination-btn:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.pagination-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.pagination-info {
  color: #64748b;
  font-size: 14px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.pagination-count {
  color: #94a3b8;
  font-size: 13px;
}

/* 对话框 */
.complete-dialog,
.cancel-dialog {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
}

.dialog-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(2px);
}

.dialog-content {
  position: relative;
  background: white;
  border-radius: 20px;
  width: 90%;
  max-width: 700px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideIn 0.3s ease;
}

@keyframes slideIn {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.dialog-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 30px 30px 20px;
  border-bottom: 1px solid #f1f5f9;
}

.dialog-header h2 {
  font-size: 24px;
  color: #1e293b;
  font-weight: 700;
}

.close-btn {
  background: none;
  border: none;
  font-size: 28px;
  color: #94a3b8;
  cursor: pointer;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: all 0.3s;
}

.close-btn:hover:not(:disabled) {
  background: #f1f5f9;
  color: #64748b;
}

.close-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.dialog-body {
  padding: 30px;
}

/* 完成订单对话框 */
.order-summary {
  background: #f8fafc;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 30px;
}

.summary-header {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #e2e8f0;
}

.summary-pet {
  width: 60px;
  height: 60px;
  background: #f0fdf4;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 28px;
}

.summary-info h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 700;
}

.summary-info p {
  font-size: 13px;
  color: #64748b;
}

.summary-details {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 15px;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 14px;
}

.summary-item span {
  color: #64748b;
}

.summary-item strong {
  color: #1e293b;
  font-weight: 600;
}

.summary-item .price {
  color: #166534;
  font-size: 16px;
}

/* 完成表单 */
.completion-form {
  margin-bottom: 30px;
}

.form-section {
  margin-bottom: 25px;
}

.form-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 15px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 15px;
}

.label-icon {
  font-size: 18px;
}

/* 完成检查清单 */
.completion-checklist {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.checklist-item {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.3s;
}

.checklist-item:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.checklist-item.checked {
  background: #d1fae5;
  border-color: #22c55e;
}

.checkmark {
  width: 24px;
  height: 24px;
  border: 2px solid #cbd5e1;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: bold;
  color: #22c55e;
  flex-shrink: 0;
  transition: all 0.3s;
}

.checklist-item.checked .checkmark {
  background: #22c55e;
  border-color: #22c55e;
  color: white;
}

.checklist-content {
  flex: 1;
}

.checklist-title {
  font-size: 15px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 4px;
}

.checklist-desc {
  font-size: 13px;
  color: #64748b;
}

/* 备注文本框 */
.notes-textarea {
  width: 100%;
  padding: 15px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  color: #1e293b;
  resize: vertical;
  transition: all 0.3s;
}

.notes-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.notes-count {
  text-align: right;
  font-size: 13px;
  color: #94a3b8;
  margin-top: 8px;
}

/* 照片上传 */
.photo-upload {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  margin-bottom: 10px;
}

.upload-placeholder {
  width: 100px;
  height: 100px;
  background: #f8fafc;
  border: 2px dashed #cbd5e1;
  border-radius: 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s;
}

.upload-placeholder:hover {
  background: #f1f5f9;
  border-color: #94a3b8;
}

.upload-icon {
  font-size: 32px;
  color: #94a3b8;
  margin-bottom: 8px;
}

.upload-text {
  font-size: 13px;
  color: #64748b;
}

.photo-preview {
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
}

.preview-item {
  position: relative;
  width: 100px;
  height: 100px;
}

.preview-image {
  width: 100%;
  height: 100%;
  background: #f1f5f9;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
}

.remove-photo {
  position: absolute;
  top: -8px;
  right: -8px;
  width: 24px;
  height: 24px;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 50%;
  font-size: 14px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.remove-photo:hover {
  background: #dc2626;
}

.photo-tip {
  font-size: 13px;
  color: #94a3b8;
}

/* 确认区域 */
.confirmation-section {
  background: #f8fafc;
  border-radius: 12px;
  padding: 20px;
}

.agreement {
  margin-bottom: 20px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  font-size: 14px;
  color: #475569;
  user-select: none;
}

.checkbox-input {
  display: none;
}

.checkbox-custom {
  width: 20px;
  height: 20px;
  border: 2px solid #cbd5e1;
  border-radius: 5px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s;
  flex-shrink: 0;
}

.checkbox-input:checked + .checkbox-custom {
  background: #22c55e;
  border-color: #22c55e;
}

.checkbox-input:checked + .checkbox-custom::after {
  content: '✓';
  color: white;
  font-size: 12px;
  font-weight: bold;
}

.dialog-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
}

.dialog-btn {
  padding: 12px 32px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  transition: all 0.3s;
  min-width: 120px;
}

.dialog-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.cancel-btn,
.back-btn {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cancel-btn:hover:not(:disabled),
.back-btn:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.confirm-complete-btn {
  background: #22c55e;
  color: white;
  border: none;
}

.confirm-complete-btn:hover:not(:disabled) {
  background: #16a34a;
  transform: translateY(-1px);
}

.confirm-cancel-btn {
  background: #ef4444;
  color: white;
  border: none;
}

.confirm-cancel-btn:hover:not(:disabled) {
  background: #dc2626;
  transform: translateY(-1px);
}

/* 取消订单对话框 */
.warning-message {
  display: flex;
  align-items: center;
  gap: 15px;
  background: #fef3c7;
  border: 1px solid #fbbf24;
  border-radius: 10px;
  padding: 20px;
  margin-bottom: 25px;
}

.warning-icon {
  font-size: 32px;
  flex-shrink: 0;
}

.warning-content h4 {
  font-size: 16px;
  color: #92400e;
  margin-bottom: 4px;
  font-weight: 600;
}

.warning-content p {
  font-size: 14px;
  color: #92400e;
  opacity: 0.8;
}

/* 取消原因选项 */
.cancel-reasons {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.reason-option {
  display: flex;
  align-items: center;
  gap: 15px;
  padding: 15px;
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.3s;
}

.reason-option:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.reason-option.selected {
  background: #fee2e2;
  border-color: #ef4444;
}

.reason-icon {
  font-size: 24px;
  width: 50px;
  height: 50px;
  background: #f1f5f9;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.reason-option.selected .reason-icon {
  background: #ef4444;
  color: white;
}

.reason-info {
  flex: 1;
}

.reason-title {
  font-size: 15px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 4px;
}

.reason-desc {
  font-size: 13px;
  color: #64748b;
}

.cancel-textarea {
  width: 100%;
  padding: 15px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  color: #1e293b;
  resize: vertical;
  transition: all 0.3s;
}

.cancel-textarea:focus {
  outline: none;
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.cancel-warning {
  background: #fee2e2;
  border: 1px solid #fca5a5;
  border-radius: 10px;
  padding: 15px;
  margin: 20px 0;
  text-align: center;
}

.cancel-warning p {
  color: #dc2626;
  font-size: 14px;
  margin: 0;
}

/* 操作结果提示 */
.operation-result {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 2000;
  max-width: 400px;
  animation: slideInRight 0.3s ease;
}

.operation-result.success {
  background: #dcfce7;
  border: 1px solid #86efac;
  color: #166534;
}

.operation-result.error {
  background: #fee2e2;
  border: 1px solid #fca5a5;
  color: #dc2626;
}

.operation-result.warning {
  background: #fef3c7;
  border: 1px solid #fcd34d;
  color: #92400e;
}

.operation-result.info {
  background: #dbeafe;
  border: 1px solid #93c5fd;
  color: #1e40af;
}

@keyframes slideInRight {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.result-content {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 15px 20px;
  border-radius: 10px;
}

.result-icon {
  font-size: 20px;
  flex-shrink: 0;
}

.result-content p {
  margin: 0;
  flex: 1;
}

.result-close {
  background: none;
  border: none;
  font-size: 20px;
  color: inherit;
  cursor: pointer;
  opacity: 0.7;
  transition: opacity 0.3s;
}

.result-close:hover {
  opacity: 1;
}

/* 订单详情侧边栏 */
.order-detail-sidebar {
  position: fixed;
  top: 0;
  right: 0;
  bottom: 0;
  z-index: 1000;
  display: flex;
  align-items: stretch;
}

.sidebar-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(2px);
}

.sidebar-content {
  position: relative;
  background: white;
  width: 100%;
  max-width: 500px;
  display: flex;
  flex-direction: column;
  z-index: 1;
  animation: slideInRight 0.3s ease;
}

.sidebar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 25px 30px;
  border-bottom: 1px solid #f1f5f9;
}

.sidebar-header h3 {
  font-size: 20px;
  color: #1e293b;
  font-weight: 700;
}

.sidebar-body {
  flex: 1;
  overflow-y: auto;
  padding: 30px;
}

.detail-section {
  margin-bottom: 30px;
}

.detail-section h4 {
  font-size: 16px;
  color: #475569;
  margin-bottom: 15px;
  font-weight: 600;
}

/* 地图相关样式 */
.map-toggle-btn {
  margin-left: 8px;
  padding: 4px 8px;
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
  transition: all 0.3s;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.map-toggle-btn:hover {
  background: linear-gradient(135deg, #2563eb, #1e40af);
  transform: translateY(-1px);
}

.map-toggle-btn.active {
  background: linear-gradient(135deg, #dc2626, #b91c1c);
}

.map-toggle-btn.active:hover {
  background: linear-gradient(135deg, #b91c1c, #991b1b);
}

.map-container {
  margin-top: 16px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  overflow: hidden;
  background: white;
}

.map-header {
  padding: 12px 16px;
  background: #f9fafb;
  border-bottom: 1px solid #e5e7eb;
}

.map-header h4 {
  margin: 0 0 4px 0;
  font-size: 16px;
  font-weight: 600;
  color: #1f2937;
}

.community-description {
  margin: 0;
  font-size: 14px;
  color: #6b7280;
}

.map-content {
  height: 300px;
  width: 100%;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .orders-grid {
    grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  }
}

@media (max-width: 900px) {
  .page-header {
    flex-direction: column;
  }
  
  .header-actions {
    width: 100%;
  }
  
  .status-filter {
    overflow-x: auto;
    padding: 6px;
    flex-wrap: nowrap;
  }
  
  .filter-btn {
    flex-shrink: 0;
  }
  
  .stats-container {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .orders-grid {
    grid-template-columns: 1fr;
  }
  
  .details-grid {
    grid-template-columns: 1fr;
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  .action-btn {
    width: 100%;
  }
  
  .dialog-content {
    width: 95%;
  }
  
  .summary-details {
    grid-template-columns: 1fr;
  }
  
  .timeline-steps {
    flex-direction: column;
    gap: 20px;
  }
  
  .timeline-step:not(:last-child)::after {
    top: 50%;
    left: 20px;
    width: 2px;
    height: 100%;
  }
  
  .timeline-step {
    flex-direction: row;
    width: 100%;
    text-align: left;
  }
  
  .step-info {
    max-width: none;
    flex: 1;
  }
}

@media (max-width: 480px) {
  .page-header {
    gap: 15px;
  }
  
  .header-left h1 {
    font-size: 28px;
  }
  
  .stats-container {
    grid-template-columns: 1fr;
  }
  
  .order-header {
    flex-direction: column;
    gap: 15px;
  }
  
  .order-status {
    align-items: flex-start;
    text-align: left;
  }
  
  .dialog-actions {
    flex-direction: column;
  }
  
  .dialog-btn {
    width: 100%;
  }
  
  .operation-result {
    left: 20px;
    right: 20px;
    max-width: none;
  }
  
  .sidebar-content {
    max-width: 100%;
  }
}
</style>