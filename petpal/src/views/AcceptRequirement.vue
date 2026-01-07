<!-- src/views/AcceptRequirement.vue -->
<template>
  <div class="accept-requirement">
    <!-- 如果用户不是认证服务者，显示资质申请界面 -->
    <div v-if="!isApprovedSitter" class="audit-required-view">
      <!-- 审核状态展示 -->
      <div class="audit-status-container">
        <div class="status-header">
          <h1>成为服务者</h1>
          <p>通过审核后即可开始接单</p>
        </div>
        
        <div class="status-card" :class="auditStatusClass">
          <div class="status-content">
            <div class="status-icon">{{ statusIcon }}</div>
            <div class="status-info">
              <h3>{{ statusTitle }}</h3>
              <p class="status-description">{{ statusDescription }}</p>
              
              <!-- 进度条 -->
              <div v-if="showProgress" class="progress-section">
                <div class="progress-header">
                  <span>审核进度</span>
                  <span>{{ auditProgress }}%</span>
                </div>
                <div class="progress-bar">
                  <div class="progress-fill" :style="{ width: auditProgress + '%' }"></div>
                </div>
              </div>
              
              <!-- 操作按钮 -->
              <div class="action-buttons">
                <button 
                  v-if="canApply"
                  class="btn-apply"
                  @click="showApplicationForm = true"
                >
                  立即申请
                </button>
                
                <button 
                  v-if="canViewApplication"
                  class="btn-secondary"
                  @click="loadMyApplication"
                >
                  查看申请记录
                </button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- 申请表单 -->
        <div v-if="showApplicationForm" class="application-form-section">
          <div class="form-header">
            <h3>服务者资质申请</h3>
            <button class="btn-close" @click="showApplicationForm = false">×</button>
          </div>
          
          <form @submit.prevent="submitApplication" class="simple-form">
            <div class="form-group">
              <label>真实姓名 *</label>
              <input 
                v-model="applicationData.realName" 
                type="text" 
                placeholder="请输入真实姓名"
                required
              />
            </div>
            
            <div class="form-group">
              <label>身份证号 *</label>
              <input 
                v-model="applicationData.idCardNumber" 
                type="text" 
                placeholder="请输入18位身份证号码"
                maxlength="18"
                required
              />
            </div>
            
            <div class="form-group">
              <label>申请原因 *</label>
              <textarea 
                v-model="applicationData.joinReason" 
                placeholder="请说明您想成为服务者的原因..."
                rows="3"
                required
              ></textarea>
            </div>
            
            <div class="form-actions">
              <button 
                type="button" 
                class="btn-cancel"
                @click="showApplicationForm = false"
              >
                取消
              </button>
              <button 
                type="submit" 
                class="btn-submit"
                :disabled="!canSubmitApplication"
              >
                {{ submittingApplication ? '提交中...' : '提交申请' }}
              </button>
            </div>
          </form>
        </div>
        
        <!-- 申请记录详情 -->
        <div v-if="showApplicationDetail" class="application-detail">
          <div class="detail-header">
            <h3>申请记录详情</h3>
            <button class="btn-close" @click="showApplicationDetail = false">×</button>
          </div>
          
          <div v-if="myApplication" class="detail-content">
            <div class="detail-item">
              <span class="label">真实姓名：</span>
              <span class="value">{{ myApplication.realName }}</span>
            </div>
            <div class="detail-item">
              <span class="label">身份证号：</span>
              <span class="value">{{ myApplication.idCardNumber }}</span>
            </div>
            <div class="detail-item">
              <span class="label">申请状态：</span>
              <span class="value status" :class="myApplication.status">
                {{ getStatusText(myApplication.status) }}
              </span>
            </div>
            <div class="detail-item">
              <span class="label">申请时间：</span>
              <span class="value">{{ formatDate(myApplication.appliedAt) }}</span>
            </div>
            <div v-if="myApplication.reviewedAt" class="detail-item">
              <span class="label">审核时间：</span>
              <span class="value">{{ formatDate(myApplication.reviewedAt) }}</span>
            </div>
            <div v-if="myApplication.reviewComment" class="detail-item">
              <span class="label">审核意见：</span>
              <span class="value">{{ myApplication.reviewComment }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
    
    <!-- 如果用户是认证服务者，显示原来的接单界面 -->
    <div v-else>
      <!-- 原来的页面标题和筛选 -->
      <div class="page-header">
        <div class="header-left">
          <h1>接单需求</h1>
          <p>选择您能帮助的需求</p>
        </div>
        <div class="header-actions">
          <div class="filter-group">
            <select class="filter-select" v-model="selectedServiceType" @change="filterRequests">
              <option value="">全部类型</option>
              <option value="walk">遛狗服务</option>
              <option value="feed">喂食照顾</option>
              <option value="medical">就医陪伴</option>
              <option value="groom">美容护理</option>
              <option value="other">其他服务</option>
            </select>

            <select class="filter-select">
              <option value="">距离优先</option>
            </select>

             <!-- 新增：切换已接受的订单 -->
          <div class="view-toggle">
            <button 
              class="toggle-btn" 
              :class="{ active: !showAcceptedOrders }"
              @click="showAcceptedOrders = true"
            >
              可接需求
            </button>
            <button 
              class="toggle-btn" 
              :class="{ active: showAcceptedOrders }"
              @click="showAcceptedOrders = false"
            >
              已接受订单
            </button>
          </div>
        </div>
      </div>
</div>
      <!-- 加载状态 -->
      <div v-if="loading && !showDialog" class="loading-container">
        <div class="loading-spinner"></div>
        <p>加载中...</p>
      </div>

      <!-- 错误提示 -->
      <div v-if="errorMessage && !showDialog" class="error-container">
        <div class="error-message">
          <span class="error-icon">⚠️</span>
          <p>{{ errorMessage }}</p>
          <button class="retry-btn" @click="loadData">重试</button>
        </div>
      </div>

      <!-- 需求列表 -->
      <div class="requirements-container" v-if="!loading && !errorMessage">
        <!-- 需求卡片列表 -->
        <div class="requirements-list">
          <div class="requirements-grid">
            <!-- 需求卡片 -->
            <div 
              class="requirement-card" 
              :class="{ urgent: requirement.urgent }" 
              v-for="requirement in requirements" 
              :key="requirement.id"
            >
              
              <!-- 需求头部 -->
              <div class="card-header">
                <div class="pet-info">
                  <div class="pet-avatar">{{ requirement.petEmoji }}</div>
                  <div class="pet-details">
                    <h3>{{ requirement.title }}</h3>
                    <p class="pet-type">{{ requirement.petTypeName }}</p>
                  </div>
                </div>
              </div>
              
              <!-- 需求类型 -->
              <div class="requirement-type">
                <span class="type-badge" :style="{ backgroundColor: requirement.typeColor }">
                  {{ requirement.typeName }}
                </span>
                <span class="distance">📍 {{ requirement.distance.toFixed(1) }}km</span>
              </div>
              
              <!-- 需求详情 -->
              <div class="requirement-details">
                <p class="description">{{ requirement.description }}</p>
                
                <div class="detail-item">
                  <span class="detail-icon">⏰</span>
                  <span class="detail-text">{{ formatTime(requirement.startTime) }} - {{ formatTime(requirement.endTime) }}</span>
                </div>
                
                <div class="detail-item">
                  <span class="detail-icon">📍</span>
                  <span class="detail-text">{{ requirement.location }}</span>
                </div>
                
                <div class="detail-item">
                  <span class="detail-icon">👤</span>
                  <span class="detail-text">发布者：{{ requirement.publisher }}</span>
                </div>
              </div>
              
              <!-- 卡片底部 -->
              <div class="card-footer">
                <button class="accept-btn" @click="showAcceptDialog(requirement)" :disabled="accepting">
                  {{ accepting && selectedRequirement?.id === requirement.id ? '接单中...' : '接受需求' }}
                </button>
              </div>
            </div>
          </div>

          <!-- 分页控制 -->
          <div class="pagination-controls" v-if="pagination.totalPages > 1">
            <button 
              class="pagination-btn" 
              @click="changePage(pagination.page - 1)"
              :disabled="pagination.page <= 1"
            >
              上一页
            </button>
            
            <span class="pagination-info">
              第 {{ pagination.page }} 页 / 共 {{ pagination.totalPages }} 页
            </span>
            
            <button 
              class="pagination-btn" 
              @click="changePage(pagination.page + 1)"
              :disabled="pagination.page >= pagination.totalPages"
            >
              下一页
            </button>
          </div>

          <!-- 无数据提示 -->
          <div v-if="requirements.length === 0" class="no-data">
            <div class="empty-state">
              <div class="empty-icon">📋</div>
              <h3>暂无可用需求</h3>
              <p>当前没有可接单的服务需求</p>
            </div>
          </div>
        </div>
      </div>

<!-- 已接受订单列表 -->
      <div class="accepted-orders-container" v-if="!loading && !errorMessage && showAcceptedOrders">
        <div class="orders-list">
          <div class="orders-grid">
            <!-- 已接受订单卡片 -->
            <div 
              class="order-card" 
              v-for="order in acceptedOrders" 
              :key="order.id"
              :class="order.status"
            >
              <!-- 订单头部 -->
              <div class="order-header">
                <div class="order-info">
                  <div class="pet-avatar">{{ order.petEmoji }}</div>
                  <div class="order-details">
                    <h3>{{ order.title }}</h3>
                    <p class="order-id">订单号：{{ order.orderId }}</p>
                  </div>
                </div>
                
                <!-- 状态标签 -->
                <div class="status-badge" :class="order.status">
                  {{ getStatusText(order.status) }}
                </div>
              </div>
              
              <!-- 订单详情 -->
              <div class="order-details-section">
                <div class="detail-row">
                  <span class="detail-label">服务类型：</span>
                  <span class="detail-value">{{ order.serviceType }}</span>
                </div>
                
                <div class="detail-row">
                  <span class="detail-label">宠物：</span>
                  <span class="detail-value">{{ order.petName }}（{{ order.petType }}）</span>
                </div>
                
                <div class="detail-row">
                  <span class="detail-label">服务时间：</span>
                  <span class="detail-value">{{ formatDateTime(order.serviceTime) }}</span>
                </div>
                
                <div class="detail-row">
                  <span class="detail-label">服务地点：</span>
                  <span class="detail-value">{{ order.location }}</span>
                </div>
                
                <div class="detail-row">
                  <span class="detail-label">用户：</span>
                  <span class="detail-value">{{ order.userName }}</span>
                </div>
                
                <div class="detail-row">
                  <span class="detail-label">报酬：</span>
                  <span class="detail-value price">¥{{ order.price }}</span>
                </div>
              </div>
              
              <!-- 订单操作 -->
              <div class="order-actions">
                <button 
                  v-if="order.status === 'pending' || order.status === 'in_progress'"
                  class="action-btn complete-btn"
                  @click="markAsCompleted(order)"
                  :disabled="completingOrder === order.id"
                >
                  {{ completingOrder === order.id ? '处理中...' : '标记完成' }}
                </button>
                
                <button 
                  v-if="order.status === 'pending'"
                  class="action-btn cancel-btn"
                  @click="showCancelDialog(order)"
                >
                  取消订单
                </button>
                
                <button 
                  v-if="order.status === 'completed' && !order.hasFeedback"
                  class="action-btn feedback-btn"
                  @click="viewFeedback(order)"
                >
                  查看评价
                </button>
                
                <button 
                  v-if="order.status === 'completed' && order.hasFeedback"
                  class="action-btn feedback-btn"
                  @click="viewFeedback(order)"
                >
                  已评价
                </button>
                
                <button 
                  class="action-btn details-btn"
                  @click="viewOrderDetails(order)"
                >
                  详情
                </button>
              </div>
            </div>
          </div>
          
          <!-- 无已接受订单提示 -->
          <div v-if="acceptedOrders.length === 0" class="no-orders">
            <div class="empty-state">
              <div class="empty-icon">📦</div>
              <h3>暂无已接受的订单</h3>
              <p>您还没有接受任何服务订单</p>
              <button class="back-to-requirements" @click="showAcceptedOrders = false">
                查看可接需求
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- 接受需求对话框 -->
      <div class="accept-dialog" v-if="showDialog">
        <div class="dialog-overlay" @click="closeDialog"></div>
        <div class="dialog-content">
          <div class="dialog-header">
            <h2>确认接受需求</h2>
            <button class="close-btn" @click="closeDialog" :disabled="accepting">×</button>
          </div>
          
          <div class="dialog-body">
            <div class="confirm-info">
              <div class="info-row">
                <span class="info-label">需求标题：</span>
                <span class="info-value">{{ selectedRequirement.title }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">需求类型：</span>
                <span class="info-value">{{ selectedRequirement.typeName }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">宠物类型：</span>
                <span class="info-value">{{ selectedRequirement.petTypeName }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">服务时间：</span>
                <span class="info-value">{{ formatTime(selectedRequirement.startTime) }} - {{ formatTime(selectedRequirement.endTime) }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">服务地点：</span>
                <span class="info-value">{{ selectedRequirement.location }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">发布者：</span>
                <span class="info-value">{{ selectedRequirement.publisher }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">距离：</span>
                <span class="info-value">{{ selectedRequirement.distance.toFixed(1) }}km</span>
              </div>
            </div>
            
            <div class="dialog-actions">
              <button 
                class="dialog-btn cancel-btn" 
                @click="closeDialog"
                :disabled="accepting"
              >
                取消
              </button>
              <button 
                class="dialog-btn confirm-btn" 
                @click="confirmAccept"
                :disabled="accepting"
              >
                {{ accepting ? '接单中...' : '确认接受' }}
              </button>
            </div>
          </div>
        </div>
      </div>

<!-- 标记完成确认对话框 -->
      <div class="complete-dialog" v-if="showCompleteDialog">
        <div class="dialog-overlay" @click="closeCompleteDialog"></div>
        <div class="dialog-content">
          <div class="dialog-header">
            <h2>确认完成订单</h2>
            <button class="close-btn" @click="closeCompleteDialog" :disabled="completingOrder">×</button>
          </div>
          
          <div class="dialog-body">
            <div class="confirm-info">
              <div class="info-row">
                <span class="info-label">订单标题：</span>
                <span class="info-value">{{ selectedOrder?.title }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">订单编号：</span>
                <span class="info-value">{{ selectedOrder?.orderId }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">服务类型：</span>
                <span class="info-value">{{ selectedOrder?.serviceType }}</span>
              </div>
              <div class="info-row">
                <span class="info-label">服务时间：</span>
                <span class="info-value">{{ formatDateTime(selectedOrder?.serviceTime) }}</span>
              </div>
            </div>
            
            <div class="completion-notes">
              <label for="completionNotes">完成备注（可选）：</label>
              <textarea 
                id="completionNotes"
                v-model="completionNotes"
                placeholder="请输入服务完成的相关备注..."
                rows="3"
              ></textarea>
            </div>
            
            <div class="dialog-actions">
              <button 
                class="dialog-btn cancel-btn" 
                @click="closeCompleteDialog"
                :disabled="completingOrder"
              >
                取消
              </button>
              <button 
                class="dialog-btn confirm-complete-btn" 
                @click="confirmCompleteOrder"
                :disabled="completingOrder"
              >
                {{ completingOrder ? '处理中...' : '确认完成' }}
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

      <!-- 已完成订单反馈模块 -->
      <div class="completed-feedback-section" v-if="feedbacks.length > 0">
        <div class="section-header">
          <h2>已完成订单反馈</h2>
          <p>查看您已完成服务的订单反馈</p>
        </div>

        <div class="feedback-container">
          <!-- 反馈列表 -->
          <div class="feedbacks-list">
            <div class="feedbacks-grid">
              <div 
                v-for="feedback in feedbacks" 
                :key="feedback.id"
                class="feedback-card"
              >
                <div class="feedback-card-header">
                  <div class="order-info">
                    <h4>{{ feedback.serviceType }}</h4>
                    <p class="order-time">完成时间：{{ formatDate(feedback.completedTime) }}</p>
                  </div>
                  <div class="rating-display">
                    <span class="rating-stars">
                      <span 
                        v-for="star in 5" 
                        :key="star"
                        class="star"
                        :class="{ filled: star <= Math.round(feedback.rating) }"
                      >
                        ★
                      </span>
                    </span>
                    <span class="rating-value">{{ feedback.rating.toFixed(1) }}分</span>
                  </div>
                </div>
                
                <div class="feedback-card-body">
                  <div class="pet-user-info">
                    <div class="user-avatar">
                      {{ feedback.userName.charAt(0) }}
                    </div>
                    <div class="user-details">
                      <h5>{{ feedback.userName }}</h5>
                      <p class="user-reputation">信誉：{{ feedback.userRating.toFixed(1) }}/5.0</p>
                    </div>
                  </div>
                  
                  <div class="feedback-content">
                    <div class="comment-box">
                      <h6>用户评价：</h6>
                      <p class="comment-text">{{ feedback.comment }}</p>
                    </div>
                    
                    <div class="pet-details-box">
                      <h6>服务宠物：</h6>
                      <div class="pet-info-row">
                        <span class="pet-icon">{{ feedback.petEmoji }}</span>
                        <span class="pet-type-label">{{ feedback.petTypeName }}</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div class="feedback-card-footer">
                  <div class="service-info">
                    <span class="info-item">📍 {{ feedback.location }}</span>
                    <span class="info-item">订单号：{{ feedback.orderId }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import sitterService from '@/utils/sitter.js'
import { http } from '@/utils/http.js'

// 原来的状态管理
const loading = ref(true)
const loadingFeedbacks = ref(true)
const accepting = ref(false)
const showDialog = ref(false)
const showAcceptedOrders = ref(true)
const showCompleteDialog = ref(false)
const errorMessage = ref('')
const operationResult = ref(null)
const requirements = ref([])
const acceptedOrders = ref([])
const feedbacks = ref([])
const selectedRequirement = ref({})
const selectedServiceType = ref('')
const pagination = ref({
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 1
})

// 新增的审核相关状态
const isApprovedSitter = ref(false)
const auditStatus = ref(null)
const showApplicationForm = ref(false)
const showApplicationDetail = ref(false)
const submittingApplication = ref(false)
const myApplication = ref(null)

// 申请表单数据
const applicationData = ref({
  realName: '',
  idCardNumber: '',
  joinReason: ''
})

// 计算属性
const auditStatusClass = computed(() => {
  const status = auditStatus.value?.auditStatus
  return {
    'not-applied': status === 'NotApplied' || !status,
    'pending': status === 'Pending' || status === 'Resubmitted',
    'approved': status === 'Approved',
    'rejected': status === 'Rejected'
  }
})

const statusIcon = computed(() => {
  const status = auditStatus.value?.auditStatus
  const icons = {
    'NotApplied': '📝',
    'Pending': '⏳',
    'Resubmitted': '🔄',
    'Approved': '✅',
    'Rejected': '❌'
  }
  return icons[status] || '❓'
})

const statusTitle = computed(() => {
  const status = auditStatus.value?.auditStatus
  const titles = {
    'NotApplied': '未申请服务者资质',
    'Pending': '资质审核中',
    'Resubmitted': '重新审核中',
    'Approved': '已通过审核',
    'Rejected': '审核未通过'
  }
  return titles[status] || '审核状态未知'
})

const statusDescription = computed(() => {
  const status = auditStatus.value?.auditStatus
  const descriptions = {
    'NotApplied': '申请成为服务者，开启您的宠物照顾之旅',
    'Pending': '请耐心等待管理员审核，通常需要1-3个工作日',
    'Resubmitted': '您的补充资料正在审核中',
    'Approved': '恭喜！您现在可以开始接单了',
    'Rejected': '请根据审核意见修改后重新提交申请'
  }
  return auditStatus.value?.stageDescription || descriptions[status] || ''
})

const auditProgress = computed(() => {
  return auditStatus.value?.progress || 0
})

const showProgress = computed(() => {
  const status = auditStatus.value?.auditStatus
  return status === 'Pending' || status === 'Resubmitted'
})

const canApply = computed(() => {
  const status = auditStatus.value?.auditStatus
  return !status || status === 'NotApplied'
})

const canViewApplication = computed(() => {
  const status = auditStatus.value?.auditStatus
  return status && status !== 'NotApplied'
})

const canResubmit = computed(() => {
  return auditStatus.value?.auditStatus === 'Rejected'
})

const canSubmitApplication = computed(() => {
  return (
    applicationData.value.realName.trim() &&
    applicationData.value.idCardNumber.trim() &&
    applicationData.value.joinReason.trim() &&
    !submittingApplication.value
  )
})

// 生命周期
onMounted(async () => {
  // 首先检查审核状态
  await checkSitterStatus()
  // 如果是认证服务者，加载接单数据
  if (isApprovedSitter.value) {
    await loadData()
    await loadFeedbacks()
  }
})

// 检查服务者状态
const checkSitterStatus = async () => {
  try {
    const response = await sitterService.getAuditStatus()
    if (response.success) {
      auditStatus.value = response.data
      isApprovedSitter.value = response.data.auditStatus === 'Approved'
      
      // 如果被拒绝了，自动显示重新申请表单
      if (response.data.auditStatus === 'Rejected') {
        showApplicationForm.value = true
      }
    }
  } catch (error) {
    console.error('检查服务者状态失败:', error)
    // 如果API调用失败，假设用户需要申请
    isApprovedSitter.value = false
  }
}

// 提交申请
const submitApplication = async () => {
  if (!canSubmitApplication.value) return
  
  submittingApplication.value = true
  
  try {
    const response = await http.post('/sitter/application', applicationData.value)
    
    if (response.success) {
      // 重新检查状态
      await checkSitterStatus()
      showApplicationForm.value = false
      
      // 显示成功提示
      showOperationResult('success', '申请提交成功！请等待审核。')
      
      // 清空表单
      applicationData.value = {
        realName: '',
        idCardNumber: '',
        joinReason: ''
      }
    } else {
      showOperationResult('error', response.message || '提交失败')
    }
  } catch (error) {
    console.error('提交申请失败:', error)
    showOperationResult('error', sitterService.handleApiError(error))
  } finally {
    submittingApplication.value = false
  }
}

// 加载申请记录
const loadMyApplication = async () => {
  try {
    const response = await sitterService.getMyApplication()
    
    if (response.success) {
      myApplication.value = response.data
      showApplicationDetail.value = true
    } else {
      showOperationResult('error', response.message || '获取申请记录失败')
    }
  } catch (error) {
    console.error('获取申请记录失败:', error)
    showOperationResult('error', sitterService.handleApiError(error))
  }
}

const getStatusText = (status) => {
  const mapping = {
    'Pending': '审核中',
    'Approved': '已通过',
    'Rejected': '已拒绝',
    'Resubmitted': '重新提交'
  }
  return mapping[status] || status
}

// 原来的方法保持不变
const loadData = async () => {
  try {
    loading.value = true
    errorMessage.value = ''

    // 加载可用的需求列表
    const filters = {
      type: selectedServiceType.value,
      page: pagination.value.page,
      pageSize: pagination.value.pageSize
    }

    const response = await sitterService.getAvailableRequests(filters)
    if (response.success) {
      requirements.value = response.data.requests.map(req =>
        sitterService.formatRequestData(req)
      )
      pagination.value = {
        page: response.data.pagination.page,
        pageSize: response.data.pagination.pageSize,
        totalCount: response.data.pagination.totalCount,
        totalPages: response.data.pagination.totalPages
      }
    } else {
      errorMessage.value = response.message || '加载需求列表失败'
    }

    // 加载已接受的订单
    try {
      const acceptedResponse = await sitterService.getMyOrders({
        page: 1,
        pageSize: 20, // 显示更多已接受的订单
        executionStatus: 'Accepted' // 只获取已接受但未完成的订单
      })

      if (acceptedResponse.success) {
        acceptedOrders.value = acceptedResponse.data.orders || []
      } else {
        console.warn('加载已接受订单失败:', acceptedResponse.message)
        acceptedOrders.value = []
      }
    } catch (acceptedError) {
      console.error('加载已接受订单异常:', acceptedError)
      acceptedOrders.value = []
    }

  } catch (error) {
    console.error('加载数据失败:', error)
    errorMessage.value = sitterService.handleApiError(error)
    requirements.value = []
    acceptedOrders.value = []
  } finally {
    loading.value = false
  }
}

const loadFeedbacks = async () => {
  try {
    loadingFeedbacks.value = true
    const response = await sitterService.getMyOrders({
      page: 1,
      pageSize: 5,
      executionStatus: 'Completed'
    })
    
    if (response.success && response.data.orders.length > 0) {
      const feedbackPromises = response.data.orders.slice(0, 3).map(async order => {
        try {
          const feedbackRes = await sitterService.getOrderFeedback(order.id)
          if (feedbackRes.success) {
            return sitterService.formatFeedbackData(feedbackRes.data)
          }
        } catch (error) {
          console.error('加载订单反馈失败:', error)
        }
        return null
      })
      
      const feedbackResults = await Promise.all(feedbackPromises)
      feedbacks.value = feedbackResults.filter(fb => fb !== null)
    }
  } catch (error) {
    console.error('加载反馈失败:', error)
  } finally {
    loadingFeedbacks.value = false
  }
}

const filterRequests = () => {
  pagination.value.page = 1
  loadData()
}

const changePage = (page) => {
  if (page < 1 || page > pagination.value.totalPages) return
  pagination.value.page = page
  loadData()
}

const showAcceptDialog = async (requirement) => {
  try {
    selectedRequirement.value = requirement
    showDialog.value = true
  } catch (error) {
    showOperationResult('error', '计算距离失败: ' + sitterService.handleApiError(error))
  }
}

const closeDialog = () => {
  if (!accepting.value) {
    showDialog.value = false
    selectedRequirement.value = {}
  }
}

const confirmAccept = async () => {
  try {
    accepting.value = true
    
    const response = await sitterService.acceptRequest(selectedRequirement.value.id)
    
    if (response.success) {
      showOperationResult('success', '接单成功！' + (response.message || '请按约定时间提供服务'))
      
      requirements.value = requirements.value.filter(
        req => req.id !== selectedRequirement.value.id
      )
      
      pagination.value.totalCount--
      
      setTimeout(() => {
        showDialog.value = false
        selectedRequirement.value = {}
      }, 1500)
    } else {
      showOperationResult('error', response.message || '接单失败')
    }
  } catch (error) {
    console.error('接受需求失败:', error)
    showOperationResult('error', '接单失败: ' + sitterService.handleApiError(error))
  } finally {
    accepting.value = false
  }
}

const showOperationResult = (type, message) => {
  const icons = {
    success: '✅',
    error: '❌',
    warning: '⚠️'
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

const formatTime = (timeString) => {
  return sitterService.formatTime(timeString)
}

const formatDate = (dateString) => {
  return sitterService.formatDate(dateString)
}
</script>

<style scoped>
/* ===== 资质审核相关样式 ===== */
.audit-required-view {
  max-width: 800px;
  margin: 0 auto;
  padding: 40px 20px;
}

.status-header {
  text-align: center;
  margin-bottom: 40px;
}

.status-header h1 {
  font-size: 36px;
  color: #1e293b;
  margin-bottom: 12px;
  font-weight: 700;
}

.status-header p {
  font-size: 18px;
  color: #64748b;
}

.status-card {
  background: white;
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.1);
  border: 3px solid transparent;
  transition: all 0.3s;
}

.status-card.not-applied {
  border-color: #e2e8f0;
}

.status-card.pending {
  border-color: #fbbf24;
}

.status-card.approved {
  border-color: #22c55e;
}

.status-card.rejected {
  border-color: #ef4444;
}

.status-content {
  display: flex;
  align-items: center;
  gap: 30px;
}

.status-icon {
  font-size: 64px;
  flex-shrink: 0;
}

.status-info {
  flex: 1;
}

.status-info h3 {
  font-size: 24px;
  color: #1e293b;
  margin-bottom: 12px;
  font-weight: 600;
}

.status-description {
  color: #64748b;
  font-size: 16px;
  line-height: 1.6;
  margin-bottom: 24px;
}

/* 进度条 */
.progress-section {
  margin: 24px 0;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.progress-header span:first-child {
  color: #475569;
  font-size: 14px;
  font-weight: 500;
}

.progress-header span:last-child {
  color: #22c55e;
  font-size: 18px;
  font-weight: 600;
}

.progress-bar {
  height: 10px;
  background: #f1f5f9;
  border-radius: 5px;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: linear-gradient(90deg, #22c55e, #16a34a);
  border-radius: 5px;
  transition: width 0.5s ease;
}

/* 操作按钮 */
.action-buttons {
  display: flex;
  gap: 16px;
  margin-top: 30px;
}

.action-buttons button {
  padding: 12px 32px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.3s;
  border: none;
}

.btn-apply {
  background: #22c55e;
  color: white;
}

.btn-apply:hover {
  background: #16a34a;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.2);
}

.btn-secondary {
  background: white;
  border: 2px solid #e2e8f0;
  color: #475569;
}

.btn-secondary:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
  transform: translateY(-2px);
}

.btn-resubmit {
  background: #ef4444;
  color: white;
}

.btn-resubmit:hover {
  background: #dc2626;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.2);
}

/* 申请表单 */
.application-form-section {
  margin-top: 40px;
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.form-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.form-header h3 {
  font-size: 20px;
  color: #1e293b;
  font-weight: 600;
}

.btn-close {
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

.btn-close:hover {
  background: #f1f5f9;
  color: #64748b;
}

.simple-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

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

.form-group input,
.form-group textarea {
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 16px;
  color: #1e293b;
  transition: all 0.3s;
}

.form-group input:focus,
.form-group textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.form-actions {
  display: flex;
  gap: 16px;
  margin-top: 16px;
}

.btn-cancel,
.btn-submit {
  padding: 12px 32px;
  border-radius: 8px;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.3s;
  flex: 1;
}

.btn-cancel {
  background: white;
  border: 2px solid #e2e8f0;
  color: #475569;
}

.btn-cancel:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.btn-submit {
  background: #22c55e;
  border: 2px solid #22c55e;
  color: white;
}

.btn-submit:hover:not(:disabled) {
  background: #16a34a;
  border-color: #16a34a;
  transform: translateY(-2px);
}

.btn-submit:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

/* 申请记录详情 */
.application-detail {
  margin-top: 40px;
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
}

.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.detail-header h3 {
  font-size: 20px;
  color: #1e293b;
  font-weight: 600;
}

.detail-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.detail-item {
  display: flex;
  align-items: flex-start;
  padding: 12px 0;
  border-bottom: 1px solid #f1f5f9;
}

.detail-item:last-child {
  border-bottom: none;
}

.detail-item .label {
  color: #64748b;
  font-size: 14px;
  width: 100px;
  flex-shrink: 0;
}

.detail-item .value {
  color: #1e293b;
  font-size: 16px;
  flex: 1;
}

.detail-item .value.status {
  font-weight: 600;
  padding: 4px 12px;
  border-radius: 20px;
  display: inline-block;
}

.detail-item .value.status.Pending {
  background: #fef3c7;
  color: #92400e;
}

.detail-item .value.status.Approved {
  background: #d1fae5;
  color: #166534;
}

.detail-item .value.status.Rejected {
  background: #fee2e2;
  color: #dc2626;
}

/* ===== 原来的样式保持不变 ===== */
.accept-requirement {
  width: 100%;
  box-sizing: border-box;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
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

.error-container {
  padding: 20px;
  margin: 20px 0;
  background: #fee2e2;
  border: 1px solid #fca5a5;
  border-radius: 10px;
}

.error-message {
  display: flex;
  align-items: center;
  gap: 10px;
  color: #dc2626;
}

.error-icon {
  font-size: 20px;
}

.retry-btn {
  margin-left: auto;
  padding: 6px 12px;
  background: white;
  border: 1px solid #dc2626;
  color: #dc2626;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s;
}

.retry-btn:hover {
  background: #dc2626;
  color: white;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 40px;
}

.header-left h1 {
  font-size: 32px;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 8px;
}

.header-left p {
  color: #64748b;
  font-size: 16px;
}

.filter-group {
  display: flex;
  gap: 12px;
}

.filter-select {
  padding: 10px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  font-size: 14px;
  color: #475569;
  min-width: 140px;
  cursor: pointer;
}

.filter-select:focus {
  outline: none;
  border-color: #22c55e;
}

.requirements-container {
  display: flex;
  gap: 30px;
  margin-bottom: 60px;
}

.requirements-list {
  flex: 2.5;
}

.requirements-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 25px;
  margin-bottom: 30px;
}

.no-data {
  text-align: center;
  padding: 60px 20px;
}

.empty-state {
  max-width: 400px;
  margin: 0 auto;
}

.empty-icon {
  font-size: 64px;
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
}

.pagination-controls {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-top: 30px;
  padding-top: 20px;
  border-top: 1px solid #e2e8f0;
}

.pagination-btn {
  padding: 8px 16px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  color: #475569;
  cursor: pointer;
  transition: all 0.3s;
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
}

.requirement-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 24px;
  position: relative;
  transition: all 0.3s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.requirement-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #d1fae5;
}

.requirement-card.urgent {
  border-left: 4px solid #ef4444;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.pet-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.pet-avatar {
  width: 48px;
  height: 48px;
  background: #f0fdf4;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
}

.pet-details h3 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 700;
  line-height: 1.3;
}

.pet-type {
  font-size: 13px;
  color: #64748b;
}

.requirement-type {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.type-badge {
  color: white;
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
}

.distance {
  font-size: 13px;
  color: #64748b;
}

.requirement-details {
  margin-bottom: 20px;
}

.description {
  color: #475569;
  font-size: 14px;
  line-height: 1.5;
  margin-bottom: 16px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
}

.detail-icon {
  width: 20px;
  text-align: center;
  color: #94a3b8;
}

.detail-text {
  font-size: 13px;
  color: #64748b;
  flex: 1;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #f1f5f9;
}

.accept-btn {
  background: #166534;
  color: white;
  border: none;
  padding: 10px 24px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  width: 100%;
}

.accept-btn:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

.accept-btn:disabled {
  background: #9ca3af;
  cursor: not-allowed;
  transform: none;
}

.accept-dialog {
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
  max-width: 500px;
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

.confirm-info {
  margin-bottom: 30px;
}

.info-row {
  display: flex;
  margin-bottom: 15px;
  font-size: 15px;
}

.info-label {
  color: #64748b;
  width: 100px;
  flex-shrink: 0;
}

.info-value {
  color: #1e293b;
  font-weight: 500;
  flex: 1;
}

.dialog-actions {
  display: flex;
  gap: 15px;
  justify-content: flex-end;
}

.dialog-btn {
  padding: 12px 32px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  transition: all 0.3s;
}

.dialog-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.cancel-btn {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cancel-btn:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.confirm-btn {
  background: #166534;
  color: white;
  border: none;
}

.confirm-btn:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

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

.completed-feedback-section {
  margin-top: 60px;
  padding: 40px;
  background: white;
  border-radius: 20px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
  border: 1px solid #f1f5f9;
}

.section-header {
  margin-bottom: 40px;
}

.section-header h2 {
  font-size: 28px;
  color: #1e293b;
  margin-bottom: 8px;
  font-weight: 700;
}

.section-header p {
  color: #64748b;
  font-size: 16px;
}

.feedback-container {
  margin-top: 30px;
}

.feedbacks-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 25px;
}

.feedback-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  overflow: hidden;
  transition: all 0.3s;
}

.feedback-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  border-color: #d1fae5;
}

.feedback-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.order-info h4 {
  font-size: 16px;
  color: #1e293b;
  font-weight: 600;
  margin-bottom: 4px;
}

.order-time {
  font-size: 13px;
  color: #94a3b8;
}

.rating-display {
  display: flex;
  align-items: center;
  gap: 10px;
}

.rating-stars {
  display: flex;
  gap: 2px;
}

.star {
  font-size: 20px;
  color: #e2e8f0;
}

.star.filled {
  color: #fbbf24;
}

.rating-value {
  font-size: 18px;
  color: #166534;
  font-weight: 600;
}

.feedback-card-body {
  padding: 20px;
}

.pet-user-info {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
  padding-bottom: 15px;
  border-bottom: 1px solid #f1f5f9;
}

.user-avatar {
  width: 45px;
  height: 45px;
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 18px;
}

.user-details h5 {
  font-size: 16px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 600;
}

.user-reputation {
  font-size: 13px;
  color: #64748b;
}

.feedback-content {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.comment-box h6,
.pet-details-box h6 {
  font-size: 14px;
  color: #475569;
  margin-bottom: 8px;
  font-weight: 500;
}

.comment-text {
  color: #475569;
  font-size: 14px;
  line-height: 1.6;
  padding: 12px;
  background: #f8fafc;
  border-radius: 8px;
  border-left: 3px solid #d1fae5;
  min-height: 60px;
}

.pet-info-row {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  background: #f0fdf4;
  border-radius: 8px;
}

.pet-icon {
  font-size: 20px;
}

.pet-type-label {
  font-size: 12px;
  color: #64748b;
  background: white;
  padding: 2px 8px;
  border-radius: 12px;
  margin-left: auto;
}

.feedback-card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px;
  border-top: 1px solid #f1f5f9;
  background: #f8fafc;
}

.service-info {
  display: flex;
  gap: 20px;
}

.info-item {
  font-size: 13px;
  color: #64748b;
  display: flex;
  align-items: center;
  gap: 4px;
}

@media (max-width: 1200px) {
  .feedbacks-grid {
    grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  }
}

@media (max-width: 900px) {
  .page-header {
    flex-direction: column;
    gap: 20px;
  }
  
  .filter-group {
    width: 100%;
  }
  
  .filter-select {
    flex: 1;
  }
  
  .status-content {
    flex-direction: column;
    text-align: center;
  }
  
  .status-icon {
    font-size: 48px;
  }
}

@media (max-width: 768px) {
  .requirements-grid,
  .feedbacks-grid {
    grid-template-columns: 1fr;
  }
  
  .feedback-card-header,
  .feedback-card-footer {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }
  
  .service-info {
    flex-direction: column;
    gap: 8px;
  }
  
  .completed-feedback-section {
    padding: 25px;
  }
  
  .info-row {
    flex-direction: column;
    gap: 4px;
  }
  
  .info-label {
    width: auto;
    font-weight: 600;
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  .action-buttons button {
    width: 100%;
  }
}

@media (max-width: 480px) {
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
  
  .pet-info-row {
    flex-wrap: wrap;
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
  
  .status-card {
    padding: 20px;
  }
  
  .form-actions {
    flex-direction: column;
  }
}

/* ===== 新增样式 ===== */

/* 视图切换按钮 */
.view-toggle {
  display: flex;
  background: #f1f5f9;
  border-radius: 10px;
  padding: 4px;
  margin-left: 20px;
}

.toggle-btn {
  padding: 8px 20px;
  border: none;
  background: transparent;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s;
}

.toggle-btn:hover {
  color: #1e293b;
  background: rgba(255, 255, 255, 0.7);
}

.toggle-btn.active {
  background: white;
  color: #166534;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  font-weight: 600;
}

/* 已接受订单容器 */
.accepted-orders-container {
  margin-bottom: 60px;
}

.orders-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
  gap: 25px;
  margin-bottom: 30px;
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

.order-card.in_progress {
  border-left: 4px solid #3b82f6;
}

.order-card.completed {
  border-left: 4px solid #22c55e;
}

.order-card.cancelled {
  border-left: 4px solid #9ca3af;
  opacity: 0.8;
}

.order-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 20px;
}

.order-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.order-details h3 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 700;
  line-height: 1.3;
}

.order-id {
  font-size: 13px;
  color: #64748b;
}

/* 状态标签 */
.status-badge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.status-badge.pending {
  background: #fef3c7;
  color: #92400e;
}

.status-badge.in_progress {
  background: #dbeafe;
  color: #1e40af;
}

.status-badge.completed {
  background: #d1fae5;
  color: #166534;
}

.status-badge.cancelled {
  background: #f3f4f6;
  color: #6b7280;
}

/* 订单详情 */
.order-details-section {
  margin-bottom: 20px;
}

.detail-row {
  display: flex;
  margin-bottom: 10px;
  font-size: 14px;
}

.detail-label {
  color: #64748b;
  width: 80px;
  flex-shrink: 0;
}

.detail-value {
  color: #1e293b;
  flex: 1;
}

.detail-value.price {
  color: #166534;
  font-weight: 600;
  font-size: 16px;
}

/* 订单操作按钮 */
.order-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  padding-top: 16px;
  border-top: 1px solid #f1f5f9;
}

.action-btn {
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.complete-btn {
  background: #22c55e;
  color: white;
}

.complete-btn:hover:not(:disabled) {
  background: #16a34a;
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

.feedback-btn {
  background: #3b82f6;
  color: white;
}

.feedback-btn:hover:not(:disabled) {
  background: #2563eb;
}

.details-btn {
  background: transparent;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.details-btn:hover {
  background: #f8fafc;
  color: #475569;
}

.action-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

/* 无订单提示 */
.no-orders {
  text-align: center;
  padding: 80px 20px;
}

.no-orders .empty-icon {
  font-size: 64px;
  margin-bottom: 20px;
  opacity: 0.5;
}

.no-orders h3 {
  font-size: 20px;
  color: #334155;
  margin-bottom: 8px;
  font-weight: 600;
}

.no-orders p {
  color: #64748b;
  font-size: 15px;
  margin-bottom: 20px;
}

.back-to-requirements {
  padding: 10px 24px;
  background: #22c55e;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.back-to-requirements:hover {
  background: #16a34a;
  transform: translateY(-1px);
}

/* 完成订单对话框 */
.completion-notes {
  margin: 20px 0;
}

.completion-notes label {
  display: block;
  margin-bottom: 8px;
  color: #475569;
  font-size: 14px;
  font-weight: 500;
}

.completion-notes textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 14px;
  color: #1e293b;
  resize: vertical;
  transition: all 0.3s;
}

.completion-notes textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
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

/* ===== 原有样式保持不变 ===== */
/* ... 原有样式代码 ... */

@media (max-width: 900px) {
  /* 响应式调整 */
  .header-actions {
    display: flex;
    flex-direction: column;
    gap: 15px;
    width: 100%;
  }
  
  .view-toggle {
    margin-left: 0;
    justify-content: center;
  }
  
  .toggle-btn {
    flex: 1;
    text-align: center;
  }
}

@media (max-width: 768px) {
  .orders-grid {
    grid-template-columns: 1fr;
  }
  
  .order-header {
    flex-direction: column;
    gap: 12px;
  }
  
  .status-badge {
    align-self: flex-start;
  }
  
  .order-actions {
    justify-content: space-between;
  }
  
  .action-btn {
    flex: 1;
    min-width: calc(50% - 5px);
  }
}

@media (max-width: 480px) {
  .order-actions {
    flex-direction: column;
  }
  
  .action-btn {
    width: 100%;
  }
  
  .detail-row {
    flex-direction: column;
    gap: 4px;
  }
  
  .detail-label {
    width: auto;
    font-weight: 600;
  }
}
</style>