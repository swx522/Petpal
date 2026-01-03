<!-- src/views/AcceptRequirement.vue -->
<template>
  <div class="accept-requirement">
    <!-- 页面标题和筛选 -->
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
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import sitterService from '@/utils/sitter.js'

// 状态管理
const loading = ref(true)
const loadingFeedbacks = ref(true)
const accepting = ref(false)
const showDialog = ref(false)
const errorMessage = ref('')
const operationResult = ref(null)

// 数据
const requirements = ref([])
const feedbacks = ref([])
const selectedRequirement = ref({})
const selectedServiceType = ref('')

// 分页信息
const pagination = ref({
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 1
})

// 计算属性
const hasRequirements = computed(() => requirements.value.length > 0)
const hasFeedbacks = computed(() => feedbacks.value.length > 0)

// 生命周期
onMounted(() => {
  loadData()
  loadFeedbacks()
})

// 加载数据
const loadData = async () => {
  try {
    loading.value = true
    errorMessage.value = ''
    
    // 检查服务者状态
    const isApproved = await sitterService.checkSitterStatus()
    if (!isApproved) {
      errorMessage.value = '请先完成服务者资质审核'
      loading.value = false
      return
    }
    
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
  } catch (error) {
    console.error('加载数据失败:', error)
    errorMessage.value = sitterService.handleApiError(error)
    requirements.value = []
  } finally {
    loading.value = false
  }
}

// 加载反馈数据
const loadFeedbacks = async () => {
  try {
    loadingFeedbacks.value = true
    const response = await sitterService.getFinishedOrders({
      page: 1,
      pageSize: 5 // 只加载最近5条反馈
    })
    
    if (response.success && response.data.orders.length > 0) {
      // 为每个订单加载详细反馈
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

// 筛选请求
const filterRequests = () => {
  pagination.value.page = 1 // 重置到第一页
  loadData()
}

// 切换页码
const changePage = (page) => {
  if (page < 1 || page > pagination.value.totalPages) return
  pagination.value.page = page
  loadData()
}

// 显示接受对话框
const showAcceptDialog = async (requirement) => {
  try {
    // 可以在这里计算距离
    // const distanceRes = await sitterService.calculateDistance(requirement.id)
    // requirement.distance = distanceRes.data?.distance || requirement.distance
    
    selectedRequirement.value = requirement
    showDialog.value = true
  } catch (error) {
    showOperationResult('error', '计算距离失败: ' + sitterService.handleApiError(error))
  }
}

// 关闭对话框
const closeDialog = () => {
  if (!accepting.value) {
    showDialog.value = false
    selectedRequirement.value = {}
  }
}

// 确认接受需求
const confirmAccept = async () => {
  try {
    accepting.value = true
    
    const response = await sitterService.acceptRequest(selectedRequirement.value.id)
    
    if (response.success) {
      showOperationResult('success', '接单成功！' + (response.message || '请按约定时间提供服务'))
      
      // 从列表中移除已接单的需求
      requirements.value = requirements.value.filter(
        req => req.id !== selectedRequirement.value.id
      )
      
      // 更新分页信息
      pagination.value.totalCount--
      
      // 关闭对话框
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

// 显示操作结果
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
  
  // 3秒后自动关闭
  setTimeout(() => {
    operationResult.value = null
  }, 3000)
}

// 格式化时间（使用服务中的方法）
const formatTime = (timeString) => {
  return sitterService.formatTime(timeString)
}

// 格式化日期（使用服务中的方法）
const formatDate = (dateString) => {
  return sitterService.formatDate(dateString)
}
</script>

<style scoped>
.accept-requirement {
  width: 100%;
  box-sizing: border-box;
}

/* 加载状态 */
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

/* 错误容器 */
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

/* 页面标题 */
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

/* 筛选组 */
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

/* 需求容器 */
.requirements-container {
  display: flex;
  gap: 30px;
  margin-bottom: 60px;
}

.requirements-list {
  flex: 2.5;
}

/* 需求网格 */
.requirements-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 25px;
  margin-bottom: 30px;
}

/* 无数据提示 */
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

/* 分页控制 */
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

/* 需求卡片 */
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

/* 卡片头部 */
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

/* 需求类型 */
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

/* 需求详情 */
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

/* 卡片底部 */
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

/* 对话框 */
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

/* ===== 已完成订单反馈模块样式 ===== */
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

/* 反馈容器 */
.feedback-container {
  margin-top: 30px;
}

/* 反馈网格 */
.feedbacks-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(400px, 1fr));
  gap: 25px;
}

/* 反馈卡片 */
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

/* 反馈卡片头部 */
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

/* 评分显示 */
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

/* 反馈卡片主体 */
.feedback-card-body {
  padding: 20px;
}

/* 宠物主人信息 */
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

/* 反馈内容 */
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

/* 宠物信息行 */
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

/* 反馈卡片底部 */
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

/* 响应式设计 */
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
}
</style>