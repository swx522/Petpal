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
          <select class="filter-select">
            <option value="">全部类型</option>
            <option value="walk">遛狗服务</option>
            <option value="feed">喂食照顾</option>
            <option value="medical">就医陪伴</option>
            <option value="groom">美容护理</option>
            <option value="else">其它</option>
          </select>
          
          <select class="filter-select">
            <option value="">距离优先</option>
          </select>
        </div>
      </div>
    </div>

    <!-- 需求列表 -->
    <div class="requirements-container">
      <!-- 需求卡片列表 -->
      <div class="requirements-list">
        <div class="requirements-grid">
          <!-- 需求卡片 1 -->
          <div class="requirement-card" :class="{ urgent: requirement.urgent }" v-for="requirement in requirements" :key="requirement.id">
            
            <!-- 需求头部 -->
            <div class="card-header">
              <div class="pet-info">
                <div class="pet-avatar">{{ getPetEmoji(requirement.petType) }}</div>
                <div class="pet-details">
                  <p class="pet-type">{{ getPetTypeName(requirement.petType) }}</p>
                </div>
              </div>
            </div>
            
            <!-- 需求类型 -->
            <div class="requirement-type">
              <span class="type-badge" :style="{ backgroundColor: getTypeColor(requirement.type) }">
                {{ getTypeName(requirement.type) }}
              </span>
              <span class="distance">📍 {{ requirement.distance }}km</span>
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
              <button class="accept-btn" @click="showAcceptDialog(requirement)">
                接受需求
              </button>
            </div>
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
          <button class="close-btn" @click="closeDialog">×</button>
        </div>
        
        <div class="dialog-body">
          <div class="confirm-info">
            <div class="info-row">
              <span class="info-label">需求类型：</span>
              <span class="info-value">{{ selectedRequirement.typeName }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">服务时间：</span>
              <span class="info-value">{{ formatTime(selectedRequirement.startTime) }}{{ formatTime(selectedRequirement.endTime) }}</span>
            </div>
            <div class="info-row">
              <span class="info-label">服务地点：</span>
              <span class="info-value">{{ selectedRequirement.location }}</span>
            </div>
          </div>
          
          <div class="dialog-actions">
            <button class="dialog-btn cancel-btn" @click="closeDialog">取消</button>
            <button class="dialog-btn confirm-btn" @click="confirmAccept">确认接受</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 已完成订单反馈模块（新增） -->
    <div class="completed-feedback-section">
      <div class="section-header">
        <h2>已完成订单反馈</h2>
        <p>查看您已完成服务的订单反馈</p>
      </div>

      <div class="feedback-container">
        <!-- 反馈列表 -->
        <div v-if="completedFeedbacks.length > 0" class="feedbacks-list">
          <div class="feedbacks-grid">
            <div 
              v-for="feedback in completedFeedbacks" 
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
                      :class="{ filled: star <= feedback.rating }"
                    >
                      ★
                    </span>
                  </span>
                  <span class="rating-value">{{ feedback.rating }}分</span>
                </div>
              </div>
              
              <div class="feedback-card-body">
                <div class="pet-user-info">
                  <div class="user-avatar">
                    {{ feedback.userName.charAt(0) }}
                  </div>
                  <div class="user-details">
                    <h5>{{ feedback.userName }}</h5>
                    <p class="user-reputation">信誉：{{ feedback.userRating }}/5.0</p>
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
                      <span class="pet-icon">{{ getPetEmoji(feedback.petType) }}</span>
                      <span class="pet-type-label">{{ getPetTypeName(feedback.petType) }}</span>
                    </div>
                  </div>
                </div>
              </div>
              
              <div class="feedback-card-footer">
                <div class="service-info">
                  <span class="info-item">📍 {{ feedback.location }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- 无反馈数据 -->
        <div v-else class="no-feedbacks">
          <div class="empty-state">
            <div class="empty-icon">📊</div>
            <h3>暂无反馈记录</h3>
            <p>完成服务后，用户评价会显示在这里</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// 需求数据
const requirements = ref([
  {
    id: 1,
    petType: "dog",
    type: "walk",
    typeName: "遛狗服务",
    petTypeName: "金毛犬",
    description: "需要帮忙遛狗1小时，多多很温顺，但力气比较大",
    rewardPoints: 80,
    distance: 1.2,
    location: "朝阳区三里屯",
    publisher: "张先生",
    startTime: "2024-01-15T14:00",
    endTime: "2024-01-15T16:00",
    postTime: "2024-01-15T09:30",
    urgent: true,
    matchRate: 92
  },
  {
    id: 2,
    petType: "cat",
    type: "feed",
    typeName: "喂食照顾",
    petTypeName: "英短猫",
    description: "出差2天，需要帮忙喂猫和清理猫砂",
    rewardPoints: 120,
    distance: 2.5,
    location: "海淀区中关村",
    publisher: "李女士",
    startTime: "2024-01-16T09:00",
    endTime: "2024-01-16T11:00",
    postTime: "2024-01-15T10:15",
    urgent: false,
    matchRate: 85
  }
])

// 已完成订单反馈数据（新增）
const completedFeedbacks = ref([
  {
    id: 1,
    orderId: "OD20231215001",
    serviceType: "遛狗服务",
    petType: "dog",
    userName: "张先生",
    userRating: 4.8,
    rating: 5,
    comment: "非常专业的遛狗服务，狗狗回来很开心！",
    location: "朝阳区三里屯",
    completedTime: "2023-12-15T16:30:00",
  },
  {
    id: 2,
    orderId: "OD20231214002",
    serviceType: "喂食照顾",
    petType: "cat",
    userName: "李女士",
    userRating: 4.5,
    rating: 4,
    comment: "按时喂食，还帮忙清理了猫砂，很细心",
    location: "海淀区中关村",
    completedTime: "2023-12-16T11:00:00",
  },
  {
    id: 3,
    orderId: "OD20231213003",
    serviceType: "美容护理",
    petType: "dog",
    userName: "王五",
    userRating: 4.9,
    rating: 5,
    comment: "洗澡很专业，狗狗看起来很舒服，服务态度很好",
    location: "西城区金融街",
    completedTime: "2023-12-13T16:00:00",
  }
])

// 状态
const loading = ref(false)
const showDialog = ref(false)
const selectedRequirement = ref({})

// 宠物表情映射
const getPetEmoji = (petType) => {
  const emojiMap = {
    dog: "🐶",
    cat: "🐱",
    rabbit: "🐰",
    bird: "🐦",
    other: "🐾"
  }
  return emojiMap[petType] || "🐾"
}

// 宠物类型名称
const getPetTypeName = (petType) => {
  const typeMap = {
    dog: "狗狗",
    cat: "猫咪",
    rabbit: "兔兔",
    bird: "鸟鸟",
    other: "其他宠物"
  }
  return typeMap[petType] || "宠物"
}

// 需求类型颜色
const getTypeColor = (type) => {
  const colorMap = {
    walk: "#3b82f6",    // 蓝色
    feed: "#10b981",    // 绿色
    medical: "#ef4444", // 红色
    groom: "#8b5cf6"    // 紫色
  }
  return colorMap[type] || "#6b7280"
}

// 需求类型名称
const getTypeName = (type) => {
  const typeMap = {
    walk: "遛狗服务",
    feed: "喂食照顾",
    medical: "就医陪伴",
    groom: "美容护理"
  }
  return typeMap[type] || "其他服务"
}

// 格式化时间
const formatTime = (timeString) => {
  const date = new Date(timeString)
  return date.toLocaleString('zh-CN', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 格式化日期（新增）
const formatDate = (dateString) => {
  if (!dateString) return ''
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', { 
    month: 'short', 
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// 显示接受对话框
const showAcceptDialog = (requirement) => {
  selectedRequirement.value = {
    ...requirement,
    typeName: getTypeName(requirement.type),
    petTypeName: getPetTypeName(requirement.petType)
  }
  showDialog.value = true
}

// 关闭对话框
const closeDialog = () => {
  showDialog.value = false
  selectedRequirement.value = {}
}

// 确认接受需求
const confirmAccept = () => {
  console.log('接受需求:', selectedRequirement.value)
  closeDialog()
}
</script>

<style scoped>
.accept-requirement {
  width: 100%;
  box-sizing: border-box;
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
}

.accept-btn:hover {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

.load-more-btn:hover {
  background: #f0fdf4;
  border-color: #22c55e;
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

.close-btn:hover {
  background: #f1f5f9;
  color: #64748b;
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

.cancel-btn {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cancel-btn:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.confirm-btn {
  background: #166534;
  color: white;
  border: none;
}

.confirm-btn:hover {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

/* ===== 已完成订单反馈模块样式（新增）===== */
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

/* 无反馈数据 */
.no-feedbacks {
  text-align: center;
  padding: 60px 40px;
}

.empty-state {
  max-width: 400px;
  margin: 0 auto;
}

.empty-icon {
  font-size: 64px;
  margin-bottom: 20px;
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
}
</style>