<!-- src/views/PublishRequirement.vue -->
<template>
  <div class="publish-requirement">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1>发布需求</h1>
      <p>填写您的宠物需求信息，社区成员会来帮助您</p>
    </div>

    <!-- 我的订单快速查看模块 -->
    <div class="quick-orders-section">
      <div class="section-header">
        <h2>
          <span class="section-title">我的已发布订单</span>
          <span class="order-count">({{ myOrders.length }})</span>
        </h2>
        <div class="section-actions">
          <button 
            @click="refreshMyOrders" 
            class="btn-refresh"
            :disabled="loading.myOrders"
          >
            <span v-if="loading.myOrders" class="btn-spinner small"></span>
            {{ loading.myOrders ? '刷新中...' : '🔄 刷新' }}
          </button>
          <button 
            @click="toggleMyOrders" 
            class="btn-toggle"
          >
            {{ showMyOrders ? '收起' : '展开' }}
          </button>
        </div>
      </div>
      
      <!-- 加载状态 -->
      <div v-if="loading.myOrders" class="loading-orders">
        <div class="mini-spinner"></div>
        <p>加载订单中...</p>
      </div>
      
      <!-- 空状态 -->
      <div v-else-if="showMyOrders && myOrders.length === 0" class="empty-orders">
        <div class="empty-icon">📝</div>
        <h3>暂无已发布订单</h3>
        <p>发布您的第一个需求吧！</p>
      </div>
      
      <!-- 订单列表 -->
      <div v-else-if="showMyOrders && myOrders.length > 0" class="orders-grid">
        <div 
          v-for="order in myOrders" 
          :key="order.id"
          class="order-card"
        >
          <div class="order-card-header">
            <div class="order-status" :class="getStatusClass(order.status)">
              {{ getStatusText(order.status) }}
            </div>
            <div class="order-time">
              {{ formatDate(order.createdAt) }}
            </div>
          </div>
          
          <div class="order-card-body">
            <h4 class="order-title">{{ order.title }}</h4>
            <div class="order-meta">
              <div class="meta-item">
                <span class="meta-label">宠物类型：</span>
                <span class="meta-value">{{ getPetTypeText(order.petType) }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">服务类型：</span>
                <span class="meta-value">{{ getServiceTypeText(order.serviceType) }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">服务时间：</span>
                <span class="meta-value">{{ formatOrderTime(order) }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 加载状态 -->
    <div v-if="loading.petTypes || loading.serviceCategories" class="loading-state">
      <div class="spinner"></div>
      <p>正在加载数据...</p>
    </div>

    <!-- 发布需求表单 -->
    <div v-else class="publish-form">
      <div class="form-card">
        <h3>需求基本信息</h3>
        
        <!-- 宠物信息 -->
        <div class="form-section">
          <h4>宠物信息</h4>
          <div class="form-grid">
            <div class="form-group">
              <label>宠物类型 *</label>
              <select v-model="publishData.petType" class="form-select" :disabled="loading.submit">
                <option value="">请选择宠物类型</option>
                <option v-for="petType in petTypes" :key="petType.value" :value="petType.value">
                  {{ petType.label }}
                </option>
              </select>
              <div v-if="validationErrors.petType" class="error-message">{{ validationErrors.petType }}</div>
            </div>
          </div>
        </div>

        <!-- 需求类型 -->
        <div class="form-section">
          <h4>需求类型 *</h4>
          <div class="requirement-types">
            <div 
              v-for="service in serviceCategories" 
              :key="service.value"
              class="type-card" 
              :class="{ active: publishData.serviceType === service.value }" 
              @click="publishData.serviceType = service.value"
            >
              <div class="type-icon">{{ getServiceIcon(service.value) }}</div>
              <div class="type-info">
                <h5>{{ service.label }}</h5>
                <p>{{ service.description }}</p>
              </div>
            </div>
          </div>
          <div v-if="validationErrors.serviceType" class="error-message">{{ validationErrors.serviceType }}</div>
        </div>

        <!-- 时间安排 -->
        <div class="form-section">
          <h4>时间安排 *</h4>
          <div class="time-inputs">
            <div class="form-grid">
              <div class="form-group">
                <label>开始时间</label>
                <input 
                  v-model="publishData.startTime" 
                  type="datetime-local" 
                  class="form-input"
                  :min="minStartTime"
                  :disabled="loading.submit"
                  @change="validateTime"
                >
                <div v-if="validationErrors.startTime" class="error-message">{{ validationErrors.startTime }}</div>
              </div>
              
              <div class="form-group">
                <label>结束时间</label>
                <input 
                  v-model="publishData.endTime" 
                  type="datetime-local" 
                  class="form-input"
                  :min="publishData.startTime || minStartTime"
                  :disabled="loading.submit"
                  @change="validateTime"
                >
                <div v-if="validationErrors.endTime" class="error-message">{{ validationErrors.endTime }}</div>
              </div>
            </div>
            <div v-if="timeInterval" class="time-interval">
              服务时长：{{ timeInterval }}
            </div>
          </div>
        </div>

        <!-- 标题 -->
        <div class="form-section">
          <h4>需求标题 *</h4>
          <input 
            v-model="publishData.title" 
            type="text" 
            class="form-input" 
            placeholder="例如：帮我遛一下可爱的柯基犬"
            :disabled="loading.submit"
            maxlength="100"
          >
          <div class="char-count" :class="{ 'limit-reached': publishData.title.length > 100 }">
            {{ publishData.title.length }}/100
          </div>
          <div v-if="validationErrors.title" class="error-message">{{ validationErrors.title }}</div>
        </div>

        <!-- 详细描述 -->
        <div class="form-section">
          <h4>需求描述 *</h4>
          <textarea 
            v-model="publishData.description"
            class="form-textarea" 
            placeholder="请详细描述您的需求，包括宠物的特殊习惯、注意事项等..."
            rows="4"
            :disabled="loading.submit"
            maxlength="500"
          ></textarea>
          <div class="char-count" :class="{ 'limit-reached': publishData.description.length > 500 }">
            {{ publishData.description.length }}/500
          </div>
          <div v-if="validationErrors.description" class="error-message">{{ validationErrors.description }}</div>
        </div>

        <!-- 提交按钮 -->
        <div class="form-actions">
          <button 
            @click="submitRequirement" 
            class="btn-primary"
            :disabled="loading.submit || !isFormValid"
          >
            <span v-if="loading.submit" class="btn-spinner"></span>
            {{ loading.submit ? '发布中...' : '立即发布' }}
          </button>
        </div>
      </div>

      <!-- 侧边提示 -->
      <div class="side-tips">
        <div class="tips-card">
          <h4>发布提示</h4>
          <ul class="tips-list">
            <li>尽量详细描述需求，提高匹配度</li>
            <li>确认时间安排合理</li>
            <li>完成后请及时确认订单</li>
            <li>准确的宠物类型有助于更好匹配</li>
          </ul>
        </div>

        <!-- 统计数据 -->
        <div class="stats-card">
          <h4>发布统计</h4>
          <div class="stats-item">
            <span class="stat-label">待评价订单</span>
            <span class="stat-value">{{ pendingReviews.length }}个</span>
          </div>
          <div class="stats-item">
            <span class="stat-label">已发布订单</span>
            <span class="stat-value">{{ myOrders.length }}个</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 发布成功弹窗 -->
    <div v-if="showSuccessModal" class="modal-overlay" @click.self="closeModal">
      <div class="modal-content">
        <div class="modal-icon">🎉</div>
        <h3>需求发布成功！</h3>
        <p>您的需求已发布到社区，服务者将会看到并接受您的订单。</p>
        <div class="modal-details">
          <div class="detail-item">
            <span class="detail-label">需求标题：</span>
            <span class="detail-value">{{ publishedOrder.title }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">服务类型：</span>
<<<<<<< HEAD
            <span class="detail-value">{{ getServiceTypeText(publishedOrder.serviceType) }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">宠物类型：</span>
            <span class="detail-value">{{ getPetTypeText(publishedOrder.petType) }}</span>
=======
            <span class="detail-value">{{ formatServiceType(publishedOrder.serviceType).label }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">宠物类型：</span>
            <span class="detail-value">{{ formatPetType(publishedOrder.petType).label }}</span>
>>>>>>> fac8cf5f256ccd13eeaf5661bdecaf41533b39c7
          </div>
          <div class="detail-item">
            <span class="detail-label">开始时间：</span>
            <span class="detail-value">{{ formatDateTime(publishedOrder.startTime) }}</span>
          </div>
          <div class="detail-item">
            <span class="detail-label">结束时间：</span>
            <span class="detail-value">{{ formatDateTime(publishedOrder.endTime) }}</span>
          </div>
          <div v-if="publishedOrder.communityName" class="detail-item">
            <span class="detail-label">发布到：</span>
            <span class="detail-value">{{ publishedOrder.communityName }}</span>
          </div>
        </div>
        <div class="modal-actions">
          <button @click="goToMyOrders" class="btn-primary">查看我的订单</button>
          <button @click="closeModal" class="btn-secondary">继续发布</button>
        </div>
      </div>
    </div>

    <!-- 错误提示 -->
    <div v-if="showError" class="error-alert">
      <div class="alert-icon">⚠️</div>
      <div class="alert-content">
        <h4>{{ errorTitle }}</h4>
        <p>{{ errorMessage }}</p>
      </div>
      <button @click="closeError" class="alert-close">×</button>
    </div>

    <!-- 评价已完成订单部分 -->
    <div v-if="showReviewSection && pendingReviews.length > 0" class="review-section">
      <div class="section-header">
        <h2>评价已完成订单</h2>
        <p>为已完成的服务进行评价，帮助其他用户选择</p>
      </div>

      <!-- 待评价订单 -->
      <div class="review-container">
        <div class="pending-reviews">
          <h3 class="review-title">
            待评价订单
            <span class="review-count">{{ pendingReviews.length }}个</span>
          </h3>
          
          <div class="reviews-grid">
            <div 
              v-for="order in pendingReviews" 
              :key="order.id"
              class="review-card"
            >
              <div class="review-card-header">
                <div class="order-info">
<<<<<<< HEAD
                  <h4>{{ getServiceTypeText(order.serviceType) }}</h4>
=======
                  <h4>{{ formatServiceType(order.serviceType).label }}</h4>
>>>>>>> fac8cf5f256ccd13eeaf5661bdecaf41533b39c7
                  <p class="order-time">
                    完成于 {{ formatDateTime(order.completedAt) }}
                  </p>
                  <div class="order-number">
                    {{ order.orderNumber || generateOrderNumber(order.id, order.createdAt) }}
                  </div>
                </div>
                <div class="pet-info">
<<<<<<< HEAD
                  <span class="pet-icon">{{ getPetTypeIcon(order.petType) }}</span>
                  <span class="pet-name">{{ getPetTypeText(order.petType) }}</span>
=======
                  <span class="pet-icon">{{ formatPetType(order.petType).icon }}</span>
                  <span class="pet-name">{{ formatPetType(order.petType).label }}</span>
>>>>>>> fac8cf5f256ccd13eeaf5661bdecaf41533b39c7
                </div>
              </div>
              
              <div class="review-card-body">
                <div class="review-form">
                  <div class="rating-input">
                    <label>服务评分：</label>
                    <div class="stars">
                      <span 
                        v-for="star in 5" 
                        :key="star"
                        class="star"
                        :class="{ active: order.userRating >= star }"
                        @click="rateOrder(order, star)"
                      >
                        ★
                      </span>
                    </div>
                    <span class="rating-value">{{ order.userRating || 0 }}/5</span>
                  </div>
                  
                  <div class="comment-input">
                    <label>评价内容：</label>
                    <textarea 
                      v-model="order.userComment"
                      placeholder="请描述服务体验，分享您的感受..."
                      rows="3"
                      class="comment-textarea"
                      maxlength="200"
                    ></textarea>
                    <div class="char-count">{{ order.userComment?.length || 0 }}/200</div>
                  </div>
                </div>
              </div>
              
              <div class="review-card-actions">
                <button 
                  @click="submitReview(order)"
                  :disabled="!order.userRating || order.userRating === 0 || loading.evaluation"
                  class="btn-review"
                >
                  <span v-if="loading.evaluation && currentReviewOrder === order.id" class="btn-spinner small"></span>
                  {{ loading.evaluation && currentReviewOrder === order.id ? '提交中...' : '提交评价' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { orderAPI } from '@/utils/order.js'

// 路由实例
const router = useRouter()

// 加载状态
const loading = reactive({
  petTypes: false,
  serviceCategories: false,
  location: false,
  submit: false,
  evaluation: false,
  myOrders: false
})

// 数据
const petTypes = ref([])
const serviceCategories = ref([])
const pendingReviews = ref([])
const myOrders = ref([])

// 发布数据
const publishData = reactive({
  petType: '',
  serviceType: '',
  startTime: '',
  endTime: '',
  description: '',
  title: ''
})

// 表单验证错误
const validationErrors = reactive({
  petType: '',
  serviceType: '',
  startTime: '',
  endTime: '',
  description: '',
  title: ''
})

// 模态框和状态
const showSuccessModal = ref(false)
const showError = ref(false)
const errorTitle = ref('')
const errorMessage = ref('')
const showReviewSection = ref(true)
const showMyOrders = ref(true) // 控制是否显示我的订单
const currentReviewOrder = ref(null)
const publishedOrder = ref({})

// 计算属性
const minStartTime = computed(() => {
  const now = new Date()
  now.setMinutes(now.getMinutes() - now.getTimezoneOffset())
  return now.toISOString().slice(0, 16)
})

const timeInterval = computed(() => {
  if (!publishData.startTime || !publishData.endTime) return null
  return calculateTimeInterval(publishData.startTime, publishData.endTime)
})

const isFormValid = computed(() => {
  return publishData.petType && 
         publishData.serviceType && 
         publishData.startTime && 
         publishData.endTime && 
         publishData.title.trim().length >= 3 && 
         publishData.description.trim().length >= 10
})

// 生命周期
onMounted(() => {
  loadInitialData()
})

// 监听表单变化
watch(() => publishData.petType, () => {
  if (validationErrors.petType) validationErrors.petType = ''
})

watch(() => publishData.serviceType, () => {
  if (validationErrors.serviceType) validationErrors.serviceType = ''
})

watch(() => publishData.startTime, () => {
  if (validationErrors.startTime) validationErrors.startTime = ''
  if (validationErrors.endTime && publishData.endTime < publishData.startTime) {
    publishData.endTime = ''
  }
})

watch(() => publishData.endTime, () => {
  if (validationErrors.endTime) validationErrors.endTime = ''
})

watch(() => publishData.title, () => {
  if (validationErrors.title && publishData.title.trim().length >= 3) {
    validationErrors.title = ''
  }
})

watch(() => publishData.description, () => {
  if (validationErrors.description && publishData.description.trim().length >= 10) {
    validationErrors.description = ''
  }
})

// API调用方法
const loadInitialData = async () => {
  try {
    await Promise.all([
      loadPetTypes(),
      loadServiceCategories(),
      loadPendingReviews(),
      loadMyOrders() // 新增：加载我的订单
    ])
  } catch (error) {
    console.error('初始化数据失败:', error)
    showErrorAlert('初始化失败', '无法加载初始数据，请刷新页面重试')
  }
}

const loadPetTypes = async () => {
  try {
    loading.petTypes = true
    const response = await orderAPI.getPetTypes()
    
    if (response.success && response.data) {
      petTypes.value = response.data
    } else {
      // 如果API失败，使用默认数据
      petTypes.value = [
        { value: 'dog', label: '狗狗 🐶', description: '犬类宠物' },
        { value: 'cat', label: '猫咪 🐱', description: '猫类宠物' },
        { value: 'rabbit', label: '兔兔 🐰', description: '兔子等小型宠物' },
        { value: 'bird', label: '鸟鸟 🐦', description: '鸟类宠物' },
        { value: 'other', label: '其他 🐾', description: '其他宠物类型' }
      ]
    }
  } catch (error) {
    console.error('加载宠物类型失败:', error)
    petTypes.value = []
  } finally {
    loading.petTypes = false
  }
}

const loadServiceCategories = async () => {
  try {
    loading.serviceCategories = true
    const response = await orderAPI.getServiceCategories()
    
    if (response.success && response.data) {
      serviceCategories.value = response.data
    } else {
      // 如果API失败，使用默认数据
      serviceCategories.value = [
        { value: 'walk', label: '遛狗服务 🚶', description: '帮您遛狗，保持宠物健康' },
        { value: 'feed', label: '喂食照顾 🍽️', description: '定时喂食，照顾宠物饮食' },
        { value: 'medical', label: '就医陪伴 🏥', description: '陪同宠物就医，提供照顾' },
        { value: 'groom', label: '美容护理 ✂️', description: '洗澡、修剪、美容服务' },
        { value: 'other', label: '其他服务 🐾', description: '其他宠物服务需求' }
      ]
    }
  } catch (error) {
    console.error('加载服务类型失败:', error)
    serviceCategories.value = []
  } finally {
    loading.serviceCategories = false
  }
}

const loadUserLocation = async () => {
  try {
    loading.location = true
    const response = await orderAPI.getLocation()
    
    if (response.success && response.data) {
      Object.assign(userLocation, response.data)
    }
  } catch (error) {
    console.error('加载位置信息失败:', error)
    // 不显示错误，允许用户继续发布
  } finally {
    loading.location = false
  }
}

const loadPendingReviews = async () => {
  try {
    const response = await orderAPI.getOrdersToEvaluate()
    
    if (response.success && response.data) {
      pendingReviews.value = Array.isArray(response.data) 
        ? response.data.map(order => ({
            ...order,
            userRating: 0,
            userComment: ''
          }))
        : []
    }
  } catch (error) {
    console.error('加载待评价订单失败:', error)
    pendingReviews.value = []
  }
}

// 新增：加载我的订单
const loadMyOrders = async () => {
  try {
    loading.myOrders = true
    const response = await orderAPI.getMyOrders({
      page: 1,
      pageSize: 10
    })
    
    if (response.success && response.data) {
      // 根据后端返回的数据结构调整
      if (response.data.orders && Array.isArray(response.data.orders)) {
        myOrders.value = response.data.orders
      } else if (Array.isArray(response.data)) {
        myOrders.value = response.data
      } else {
        myOrders.value = []
      }
    } else {
      myOrders.value = []
    }
  } catch (error) {
    console.error('加载我的订单失败:', error)
    myOrders.value = []
  } finally {
    loading.myOrders = false
  }
}

// 表单验证
const validateTime = () => {
  if (!publishData.startTime || !publishData.endTime) return
  
  const start = new Date(publishData.startTime)
  const end = new Date(publishData.endTime)
  const now = new Date()
  
  if (start >= end) {
    validationErrors.startTime = '开始时间必须早于结束时间'
    validationErrors.endTime = '结束时间必须晚于开始时间'
    return false
  }
  
  if (start <= now) {
    validationErrors.startTime = '开始时间必须晚于当前时间'
    return false
  }
  
  return true
}

// 提交需求
const submitRequirement = async () => {
  // 验证表单
  const validation = orderAPI.validateRequirementData(publishData)
  
  if (!validation.isValid) {
    // 显示错误信息
    Object.keys(validation.fieldErrors).forEach(key => {
      if (validation.fieldErrors[key]) {
        validationErrors[key] = validation.fieldErrors[key]
      }
    })
    
    // 滚动到第一个错误
    const firstErrorField = Object.keys(validation.fieldErrors).find(key => validation.fieldErrors[key])
    if (firstErrorField) {
      const element = document.querySelector(`[name="${firstErrorField}"]`) || 
                      document.querySelector(`[v-model="${firstErrorField}"]`)
      element?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
    
    return
  }
  
  try {
    loading.submit = true
    
    const requestData = {
      title: publishData.title.trim(),
      petType: publishData.petType,
      serviceType: publishData.serviceType,
      startTime: publishData.startTime,
      endTime: publishData.endTime,
      description: publishData.description.trim()
    }
    
    const response = await orderAPI.createRequest(requestData)
    
    if (response.success) {
      publishedOrder.value = response.data
      showSuccessModal.value = true
      resetForm()
      
      // 刷新我的订单列表
      await loadMyOrders()
      
      // 延迟关闭模态框
      setTimeout(() => {
        if (showSuccessModal.value) {
          showSuccessModal.value = false
        }
      }, 5000)
    } else {
      showErrorAlert('发布失败', response.message || '请稍后重试')
    }
  } catch (error) {
    console.error('发布需求失败:', error)
    
    // 根据错误类型显示不同消息
    if (error.status === 401) {
      showErrorAlert('认证失败', '请重新登录后发布需求')
      router.push('/login')
    } else if (error.status === 403) {
      showErrorAlert('权限不足', '您尚未完成宠物认证，请先完成认证')
      router.push('/certification')
    } else if (error.status === 400) {
      showErrorAlert('参数错误', error.details || '请检查填写的信息是否正确')
    } else {
      showErrorAlert('发布失败', error.message || error.details || '网络错误，请稍后重试')
    }
  } finally {
    loading.submit = false
  }
}

// 提交评价
const submitReview = async (order) => {
  if (!order.userRating || order.userRating === 0) {
    showErrorAlert('评价失败', '请先选择评分')
    return
  }
  
  try {
    loading.evaluation = true
    currentReviewOrder.value = order.id
    
    const evaluationData = {
      orderId: order.id,
      score: order.userRating,
      content: order.userComment || ''
    }
    
    const response = await orderAPI.submitEvaluation(evaluationData)
    
    if (response.success) {
      // 从待评价列表中移除
      pendingReviews.value = pendingReviews.value.filter(o => o.id !== order.id)
      
      // 显示成功消息
      showSuccessAlert('评价成功', '感谢您的评价！')
      
      // 如果所有评价都完成，隐藏评价区域
      if (pendingReviews.value.length === 0) {
        setTimeout(() => {
          showReviewSection.value = false
        }, 2000)
      }
    } else {
      showErrorAlert('评价失败', response.message || '请稍后重试')
    }
  } catch (error) {
    console.error('提交评价失败:', error)
    
    if (error.status === 401) {
      showErrorAlert('认证失败', '请重新登录后评价')
      router.push('/login')
    } else if (error.status === 403) {
      showErrorAlert('权限不足', '您没有权限评价此订单')
    } else if (error.status === 404) {
      showErrorAlert('订单不存在', '该订单可能已被删除')
      pendingReviews.value = pendingReviews.value.filter(o => o.id !== order.id)
    } else {
      showErrorAlert('评价失败', error.message || error.details || '网络错误，请稍后重试')
    }
  } finally {
    loading.evaluation = false
    currentReviewOrder.value = null
  }
}

// 工具函数
const getServiceIcon = (serviceType) => {
  const icons = {
    walk: '🚶',
    feed: '🍽️',
    medical: '🏥',
    groom: '✂️',
    other: '🐾'
  }
  return icons[serviceType] || '🐾'
}

const formatTimeAgo = (dateString) => {
  if (!dateString) return ''
  
  const date = new Date(dateString)
  const now = new Date()
  const diffMs = now - date
  const diffMins = Math.floor(diffMs / (1000 * 60))
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60))
  const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24))
  
  if (diffMins < 1) return '刚刚'
  if (diffMins < 60) return `${diffMins}分钟前`
  if (diffHours < 24) return `${diffHours}小时前`
  if (diffDays < 30) return `${diffDays}天前`
  
  return formatDateTime(date, 'dateOnly')
}

const rateOrder = (order, rating) => {
  order.userRating = rating
}

const refreshLocation = async () => {
  await loadUserLocation()
  showSuccessAlert('位置更新', '位置信息已更新')
}

const resetForm = () => {
  publishData.petType = ''
  publishData.serviceType = ''
  publishData.startTime = ''
  publishData.endTime = ''
  publishData.description = ''
  publishData.title = ''
  
  // 重置错误
  Object.keys(validationErrors).forEach(key => {
    validationErrors[key] = ''
  })
}

const closeModal = () => {
  showSuccessModal.value = false
}

const goToMyOrders = () => {
  router.push('/publish')
}

const showErrorAlert = (title, message) => {
  errorTitle.value = title
  errorMessage.value = message
  showError.value = true
  
  // 5秒后自动关闭
  setTimeout(() => {
    showError.value = false
  }, 5000)
}

const showSuccessAlert = (title, message) => {
  // 这里可以扩展实现成功提示
  console.log(`${title}: ${message}`)
}

const closeError = () => {
  showError.value = false
}

// 新增：我的订单相关函数
const toggleMyOrders = () => {
  showMyOrders.value = !showMyOrders.value
}

const refreshMyOrders = () => {
  loadMyOrders()
}

const viewOrderDetail = (orderId) => {
  router.push(`/orders/${orderId}`)
}

const getStatusClass = (status) => {
  const statusMap = {
    'Pending': 'status-pending',
    'Accepted': 'status-accepted',
    'InProgress': 'status-inprogress',
    'Completed': 'status-completed',
    'Cancelled': 'status-cancelled'
  }
  return statusMap[status] || 'status-pending'
}

const getStatusText = (status) => {
  const statusMap = {
    'Pending': '待接单',
    'Accepted': '已接单',
    'InProgress': '进行中',
    'Completed': '已完成',
    'Cancelled': '已取消'
  }
  return statusMap[status] || status
}

const getPetTypeText = (petType) => {
  const petTypeMap = {
    'dog': '狗狗',
    'cat': '猫咪',
    'rabbit': '兔兔',
    'bird': '鸟鸟',
    'other': '其他'
  }
  return petTypeMap[petType] || petType
}

const getPetTypeIcon = (petType) => {
  const iconMap = {
    'dog': '🐶',
    'cat': '🐱',
    'rabbit': '🐰',
    'bird': '🐦',
    'other': '🐾'
  }
  return iconMap[petType] || '🐾'
}

const getServiceTypeText = (serviceType) => {
  const serviceTypeMap = {
    'walk': '遛狗服务',
    'feed': '喂食照顾',
    'medical': '就医陪伴',
    'groom': '美容护理',
    'other': '其他服务'
  }
  return serviceTypeMap[serviceType] || serviceType
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN')
}

const formatDateTime = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleString('zh-CN', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatOrderTime = (order) => {
  if (order.startTime && order.endTime) {
    const start = new Date(order.startTime)
    const end = new Date(order.endTime)
    return `${start.getMonth() + 1}/${start.getDate()} ${start.getHours()}:${start.getMinutes().toString().padStart(2, '0')} - ${end.getMonth() + 1}/${end.getDate()} ${end.getHours()}:${end.getMinutes().toString().padStart(2, '0')}`
  }
  return ''
}

const calculateTimeInterval = (startTime, endTime) => {
  if (!startTime || !endTime) return ''
  const start = new Date(startTime)
  const end = new Date(endTime)
  const diffMs = end - start
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
}

const generateOrderNumber = (orderId, createdAt) => {
  try {
    const date = new Date(createdAt)
    const dateStr = date.toISOString().slice(0, 10).replace(/-/g, '')
    const idPrefix = orderId ? orderId.substring(0, 4).toUpperCase() : 'XXXX'
    return `OD${dateStr}${idPrefix}`
  } catch (error) {
    return `OD${Date.now().toString().slice(-8)}`
  }
}
</script>

<style scoped>
/* 基础样式 */
.publish-requirement {
  width: 100%;
  box-sizing: border-box;
  min-height: 100vh;
  background: linear-gradient(135deg, #f0fdf4 0%, #f8fafc 100%);
  padding: 40px 20px;
}

.page-header {
  margin-bottom: 40px;
  text-align: center;
}

.page-header h1 {
  font-size: 32px;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 8px;
}

.page-header p {
  color: #64748b;
  font-size: 16px;
}

/* 我的订单快速查看模块 */
.quick-orders-section {
  background: white;
  border-radius: 20px;
  padding: 30px;
  margin-bottom: 40px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid #f1f5f9;
  max-width: 1200px;
  margin-left: auto;
  margin-right: auto;
}

.quick-orders-section .section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.quick-orders-section .section-header h2 {
  font-size: 22px;
  color: #1e293b;
  display: flex;
  align-items: center;
  gap: 8px;
}

.section-title {
  font-weight: 700;
}

.order-count {
  font-size: 16px;
  color: #64748b;
  font-weight: 500;
}

.section-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.btn-refresh {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 6px;
}

.btn-refresh:hover:not(:disabled) {
  background: #f1f5f9;
  border-color: #cbd5e1;
}

.btn-refresh:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-toggle {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  background: #22c55e;
  color: white;
  border: none;
  transition: all 0.2s;
}

.btn-toggle:hover {
  background: #16a34a;
}

.loading-orders {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
}

.mini-spinner {
  width: 24px;
  height: 24px;
  border: 2px solid #f1f5f9;
  border-top-color: #22c55e;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 12px;
}

.empty-orders {
  text-align: center;
  padding: 40px 20px;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  opacity: 0.5;
}

.empty-orders h3 {
  color: #64748b;
  margin-bottom: 8px;
  font-size: 18px;
}

.empty-orders p {
  color: #94a3b8;
  font-size: 14px;
}

.orders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.order-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 12px;
  overflow: hidden;
  transition: all 0.3s;
  cursor: pointer;
}

.order-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  border-color: #d1fae5;
}

.order-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.order-status {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.status-pending {
  background: #fef3c7;
  color: #92400e;
}

.status-accepted {
  background: #dbeafe;
  color: #1e40af;
}

.status-inprogress {
  background: #ede9fe;
  color: #5b21b6;
}

.status-completed {
  background: #d1fae5;
  color: #065f46;
}

.status-cancelled {
  background: #fee2e2;
  color: #991b1b;
}

.order-time {
  font-size: 12px;
  color: #94a3b8;
}

.order-card-body {
  padding: 16px;
}

.order-title {
  font-size: 16px;
  color: #1e293b;
  margin-bottom: 12px;
  font-weight: 600;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.order-meta {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.meta-item {
  display: flex;
  align-items: center;
}

.meta-label {
  font-size: 12px;
  color: #64748b;
  min-width: 60px;
}

.meta-value {
  font-size: 13px;
  color: #475569;
  font-weight: 500;
}

.order-card-footer {
  padding: 16px;
  border-top: 1px solid #f1f5f9;
}

.order-actions {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.btn-view {
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 12px;
  cursor: pointer;
  background: #22c55e;
  color: white;
  border: none;
  transition: all 0.2s;
}

.btn-view:hover {
  background: #16a34a;
}

.order-id {
  font-size: 11px;
  color: #94a3b8;
  font-family: monospace;
}

.view-all {
  text-align: center;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #f1f5f9;
}

.btn-view-all {
  padding: 8px 16px;
  border-radius: 8px;
  font-size: 14px;
  cursor: pointer;
  background: transparent;
  color: #22c55e;
  border: 1px solid #22c55e;
  transition: all 0.2s;
}

.btn-view-all:hover {
  background: #f0fdf4;
}

/* 加载状态 */
.loading-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f1f5f9;
  border-top-color: #22c55e;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 发布表单布局 */
.publish-form {
  display: flex;
  gap: 30px;
  max-width: 1200px;
  margin: 0 auto;
}

.form-card {
  flex: 2;
  background: #ffffff;
  border: 1px solid #f1f5f9;
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.side-tips {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 20px;
  min-width: 300px;
}

/* 表单部分 */
.form-section {
  margin-bottom: 40px;
}

.form-section h4 {
  font-size: 18px;
  color: #334155;
  margin-bottom: 20px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
}

.form-section h4:after {
  content: '*';
  color: #ef4444;
  font-size: 14px;
}

.time-inputs {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.time-interval {
  font-size: 14px;
  color: #64748b;
  text-align: center;
  padding: 8px;
  background: #f8fafc;
  border-radius: 8px;
  border: 1px solid #e2e8f0;
}

/* 表单网格 */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

/* 表单元素 */
.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
}

.form-input,
.form-select {
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  background: white;
  transition: all 0.2s;
  font-family: inherit;
}

.form-input:focus,
.form-select:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.form-input:disabled,
.form-select:disabled {
  background: #f8fafc;
  cursor: not-allowed;
  opacity: 0.7;
}

/* 错误消息 */
.error-message {
  color: #ef4444;
  font-size: 12px;
  margin-top: 4px;
  min-height: 16px;
}

/* 字符计数 */
.char-count {
  text-align: right;
  font-size: 12px;
  color: #94a3b8;
  margin-top: 4px;
  min-height: 16px;
}

.char-count.limit-reached {
  color: #ef4444;
}

/* 需求类型卡片 */
.requirement-types {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 15px;
}

.type-card {
  border: 2px solid #f1f5f9;
  border-radius: 12px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 15px;
  background: white;
}

.type-card:hover {
  border-color: #d1fae5;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.type-card.active {
  border-color: #22c55e;
  background: #f0fdf4;
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.1);
}

.type-icon {
  font-size: 32px;
  flex-shrink: 0;
}

.type-info {
  flex: 1;
}

.type-info h5 {
  font-size: 16px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 600;
}

.type-info p {
  font-size: 13px;
  color: #64748b;
  line-height: 1.4;
}

/* 文本区域 */
.form-textarea {
  width: 100%;
  padding: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  resize: vertical;
  min-height: 100px;
  transition: all 0.2s;
  font-family: inherit;
  line-height: 1.5;
}

.form-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.form-textarea:disabled {
  background: #f8fafc;
  cursor: not-allowed;
}

/* 表单操作按钮 */
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding-top: 30px;
  border-top: 1px solid #f1f5f9;
}

.btn-primary {
  padding: 12px 32px;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  background: #166534;
  color: white;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-width: 120px;
}

.btn-primary:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(22, 101, 52, 0.2);
}

.btn-primary:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
  opacity: 0.7;
}

/* 按钮加载器 */
.btn-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

.btn-spinner.small {
  width: 14px;
  height: 14px;
}

/* 侧边提示卡片 */
.tips-card,
.stats-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.02);
}

.tips-card h4,
.stats-card h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
}

.tips-card h4:before,
.stats-card h4:before {
  content: '';
  display: block;
  width: 4px;
  height: 16px;
  background: #22c55e;
  border-radius: 2px;
}

.tips-list {
  list-style: none;
  padding: 0;
}

.tips-list li {
  padding: 10px 0;
  color: #64748b;
  font-size: 14px;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  align-items: flex-start;
  gap: 10px;
}

.tips-list li:last-child {
  border-bottom: none;
}

.tips-list li:before {
  content: "✓";
  color: #22c55e;
  font-weight: bold;
  flex-shrink: 0;
  margin-top: 2px;
}

/* 统计数据 */
.stats-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f1f5f9;
}

.stats-item:last-child {
  border-bottom: none;
}

.stat-label {
  color: #64748b;
  font-size: 14px;
}

.stat-value {
  color: #166534;
  font-size: 14px;
  font-weight: 600;
}

.stat-value.text-warning {
  color: #f59e0b;
}

.stat-value.text-success {
  color: #22c55e;
}

/* 模态框 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
  animation: fadeIn 0.2s ease;
}

.modal-content {
  background: white;
  border-radius: 20px;
  padding: 40px;
  max-width: 500px;
  width: 100%;
  text-align: center;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
  animation: slideUp 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { 
    opacity: 0;
    transform: translateY(20px);
  }
  to { 
    opacity: 1;
    transform: translateY(0);
  }
}

.modal-icon {
  font-size: 64px;
  margin-bottom: 20px;
  animation: bounce 1s ease infinite;
}

@keyframes bounce {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.modal-content h3 {
  font-size: 24px;
  color: #1e293b;
  margin-bottom: 12px;
  font-weight: 700;
}

.modal-content p {
  color: #64748b;
  margin-bottom: 30px;
  line-height: 1.6;
}

.modal-details {
  background: #f8fafc;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 30px;
  text-align: left;
}

.detail-item {
  display: flex;
  margin-bottom: 12px;
  line-height: 1.5;
}

.detail-item:last-child {
  margin-bottom: 0;
}

.detail-label {
  color: #64748b;
  font-size: 14px;
  min-width: 80px;
  flex-shrink: 0;
}

.detail-value {
  color: #1e293b;
  font-weight: 500;
  word-break: break-word;
}

.modal-actions {
  display: flex;
  gap: 15px;
}

.btn-secondary {
  padding: 12px 24px;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
  transition: all 0.3s;
  flex: 1;
}

.btn-secondary:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

/* 错误提示 */
.error-alert {
  position: fixed;
  top: 20px;
  right: 20px;
  background: #fee2e2;
  border: 1px solid #fecaca;
  border-radius: 12px;
  padding: 16px 20px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  max-width: 400px;
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.1);
  z-index: 1001;
  animation: slideInRight 0.3s ease;
}

@keyframes slideInRight {
  from {
    opacity: 0;
    transform: translateX(100%);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

.alert-icon {
  font-size: 20px;
  line-height: 1;
  flex-shrink: 0;
  margin-top: 2px;
}

.alert-content {
  flex: 1;
  min-width: 0;
}

.alert-content h4 {
  color: #991b1b;
  font-size: 14px;
  font-weight: 600;
  margin-bottom: 4px;
}

.alert-content p {
  color: #dc2626;
  font-size: 13px;
  line-height: 1.4;
  word-break: break-word;
}

.alert-close {
  background: none;
  border: none;
  color: #dc2626;
  font-size: 20px;
  cursor: pointer;
  line-height: 1;
  padding: 0;
  margin: -4px -8px -4px 0;
  flex-shrink: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  transition: background-color 0.2s;
}

.alert-close:hover {
  background: rgba(220, 38, 38, 0.1);
}

/* 评价部分 */
.review-section {
  margin-top: 60px;
  padding: 40px;
  background: white;
  border-radius: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid #f1f5f9;
  max-width: 1200px;
  margin-left: auto;
  margin-right: auto;
}

.review-section .section-header {
  margin-bottom: 40px;
}

.review-section .section-header h2 {
  font-size: 28px;
  color: #1e293b;
  margin-bottom: 8px;
  font-weight: 700;
}

.review-section .section-header p {
  color: #64748b;
  font-size: 16px;
}

/* 评价网格 */
.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 25px;
}

/* 评价卡片 */
.review-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  overflow: hidden;
  transition: all 0.3s;
}

.review-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  border-color: #d1fae5;
}

.review-title {
  font-size: 20px;
  color: #334155;
  margin-bottom: 25px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 12px;
}

.review-count {
  background: #22c55e;
  color: white;
  font-size: 14px;
  padding: 4px 12px;
  border-radius: 20px;
  font-weight: 500;
}

.review-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 20px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.order-info h4 {
  font-size: 16px;
  color: #1e293b;
  font-weight: 600;
  margin-bottom: 8px;
}

.order-time {
  font-size: 13px;
  color: #94a3b8;
  margin-bottom: 4px;
}

.order-number {
  font-size: 12px;
  color: #64748b;
  font-family: monospace;
  background: #f1f5f9;
  padding: 2px 8px;
  border-radius: 4px;
  display: inline-block;
}

.pet-info {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-shrink: 0;
}

.pet-icon {
  font-size: 24px;
}

.pet-name {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
}

.review-card-body {
  padding: 20px;
}

/* 评价表单 */
.review-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.rating-input {
  display: flex;
  align-items: center;
  gap: 15px;
  flex-wrap: wrap;
}

.rating-input label {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
  flex-shrink: 0;
}

.stars {
  display: flex;
  gap: 4px;
}

.star {
  font-size: 28px;
  color: #e2e8f0;
  cursor: pointer;
  transition: all 0.2s;
  line-height: 1;
}

.star:hover {
  transform: scale(1.2);
}

.star:hover,
.star.active {
  color: #fbbf24;
}

.rating-value {
  font-size: 14px;
  color: #64748b;
  font-weight: 500;
  min-width: 40px;
}

.comment-input {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.comment-input label {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
}

.comment-textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  resize: vertical;
  min-height: 80px;
  transition: border-color 0.2s;
  font-family: inherit;
  line-height: 1.5;
}

.comment-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.review-card-actions {
  padding: 20px;
  border-top: 1px solid #f1f5f9;
  display: flex;
  justify-content: flex-end;
}

.btn-review {
  padding: 10px 24px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  border: none;
  background: #166534;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  min-width: 100px;
}

.btn-review:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

.btn-review:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
  opacity: 0.7;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .publish-form {
    flex-direction: column;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
  
  .requirement-types {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .reviews-grid {
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  }
  
  .orders-grid {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }
  
  .side-tips {
    min-width: auto;
  }
}

@media (max-width: 768px) {
  .publish-requirement {
    padding: 20px 15px;
  }
  
  .page-header h1 {
    font-size: 28px;
  }
  
  .quick-orders-section {
    padding: 20px;
  }
  
  .quick-orders-section .section-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
  
  .section-actions {
    width: 100%;
    justify-content: space-between;
  }
  
  .form-card {
    padding: 25px;
  }
  
  .review-section {
    padding: 25px;
    margin-top: 40px;
  }
  
  .modal-content {
    padding: 30px 20px;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .reviews-grid {
    grid-template-columns: 1fr;
  }
  
  .orders-grid {
    grid-template-columns: 1fr;
  }
  
  .requirement-types {
    grid-template-columns: 1fr;
  }
  
  .review-card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .rating-input {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .error-alert {
    left: 20px;
    right: 20px;
    max-width: none;
  }
}

@media (max-width: 480px) {
  .page-header h1 {
    font-size: 24px;
  }
  
  .quick-orders-section .section-header h2 {
    font-size: 20px;
  }
  
  .form-card h3 {
    font-size: 20px;
  }
  
  .review-section {
    padding: 20px;
  }
  
  .review-section .section-header h2 {
    font-size: 24px;
  }
  
  .modal-actions button {
    width: 100%;
  }
}
</style>