<!-- src/views/PublishRequirement.vue -->
<template>
  <div class="publish-requirement">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1>发布需求</h1>
      <p>填写您的宠物需求信息，社区成员会来帮助您</p>
    </div>

    <!-- 发布需求表单 -->
    <div class="publish-form">
      <div class="form-card">
        <h3>需求基本信息</h3>
        
        <!-- 宠物信息 -->
        <div class="form-section">
          <h4>宠物信息</h4>
          <div class="form-grid">
            <div class="form-group">
              <label>宠物类型 *</label>
              <select v-model="publishData.petType" class="form-select">
                <option value="">请选择宠物类型</option>
                <option value="dog">狗狗 🐶</option>
                <option value="cat">猫咪 🐱</option>
                <option value="rabbit">兔兔 🐰</option>
                <option value="bird">鸟鸟 🐦</option>
                <option value="other">其他 🐾</option>
              </select>
            </div>
          </div>
        </div>

        <!-- 需求类型 -->
        <div class="form-section">
          <h4>需求类型</h4>
          <div class="requirement-types">
            <div class="type-card" :class="{ active: selectedType === 'walk' }" @click="selectedType = 'walk'">
              <div class="type-icon">🚶</div>
              <div class="type-info">
                <h5>遛狗服务</h5>
                <p>需要帮忙遛宠物</p>
              </div>
            </div>
            
            <div class="type-card" :class="{ active: selectedType === 'feed' }" @click="selectedType = 'feed'">
              <div class="type-icon">🍽️</div>
              <div class="type-info">
                <h5>喂食照顾</h5>
                <p>临时喂食照顾</p>
              </div>
            </div>
            
            <div class="type-card" :class="{ active: selectedType === 'medical' }" @click="selectedType = 'medical'">
              <div class="type-icon">🏥</div>
              <div class="type-info">
                <h5>就医陪伴</h5>
                <p>需要陪宠物就医</p>
              </div>
            </div>
            
            <div class="type-card" :class="{ active: selectedType === 'groom' }" @click="selectedType = 'groom'">
              <div class="type-icon">✂️</div>
              <div class="type-info">
                <h5>美容护理</h5>
                <p>洗澡、修剪等</p>
              </div>
            </div>
          </div>
        </div>

        <!-- 时间安排 -->
        <div class="form-section">
          <h4>时间安排</h4>
          <div class="form-grid">
            <div class="form-group">
              <label>开始时间 *</label>
              <input v-model="publishData.startTime" type="datetime-local" class="form-input">
            </div>
            
            <div class="form-group">
              <label>结束时间 *</label>
              <input v-model="publishData.endTime" type="datetime-local" class="form-input">
            </div>
          </div>
        </div>

        <!-- 详细描述 -->
        <div class="form-section">
          <h4>需求描述</h4>
          <textarea 
            v-model="publishData.description"
            class="form-textarea" 
            placeholder="请详细描述您的需求，包括宠物的特殊习惯、注意事项等..."
            rows="4"
          ></textarea>
        </div>

        <!-- 联系方式 -->
        <div class="form-section">
          <h4>联系方式</h4>
          <input v-model="publishData.contact" type="text" placeholder="默认展示邮箱" class="form-input mt-2">
        </div>

        <!-- 提交按钮 -->
        <div class="form-actions">
          <button @click="submitRequirement" class="btn-primary">立即发布</button>
        </div>
      </div>

      <!-- 侧边提示 -->
      <div class="side-tips">
        <div class="tips-card">
          <h4>发布提示</h4>
          <ul class="tips-list">
            <li>尽量详细描述需求，提高匹配度</li>
            <li>设置合理的悬赏积分</li>
            <li>确认联系方式正确</li>
            <li>完成后请及时确认订单</li>
          </ul>
        </div>
      </div>
    </div>

    <!-- 评价已完成订单部分 -->
    <div v-if="showReviewSection" class="review-section">
      <div class="section-header">
        <h2>评价已完成订单</h2>
        <p>为已完成的服务进行评价，帮助其他用户选择</p>
      </div>

      <!-- 待评价订单 -->
      <div class="review-container">
        <div v-if="pendingReviews.length > 0" class="pending-reviews">
          <h3 class="review-title">待评价订单</h3>
          
          <div class="reviews-grid">
            <div 
              v-for="order in pendingReviews" 
              :key="order.id"
              class="review-card"
            >
              <div class="review-card-header">
                <div class="order-info">
                  <h4>{{ order.serviceType }}</h4>
                  <p class="order-time">{{ formatDate(order.completedTime) }}</p>
                </div>
                <div class="pet-info">
                  <span class="pet-icon">{{ getPetIcon(order.petType) }}</span>
                </div>
              </div>
              
              <div class="review-card-body">
                <div class="service-provider">
                  <div class="provider-avatar">
                    {{ order.providerName.charAt(0) }}
                  </div>
                  <div class="provider-info">
                    <h5>{{ order.providerName }}</h5>
                    <p class="rating">服务评分：{{ order.providerRating }}/5.0</p>
                  </div>
                </div>
                
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
                    <span class="rating-value">{{ order.userRating }}/5</span>
                  </div>
                  
                  <div class="comment-input">
                    <label>评价内容：</label>
                    <textarea 
                      v-model="order.userComment"
                      placeholder="请描述服务体验，分享您的感受..."
                      rows="3"
                      class="comment-textarea"
                    ></textarea>
                  </div>
                </div>
              </div>
              
              <div class="review-card-actions">
                <button 
                  @click="submitReview(order)"
                  :disabled="order.userRating === 0"
                  class="btn-review"
                >
                  提交评价
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- 已评价订单 -->
        <div v-if="completedReviews.length > 0" class="completed-reviews">
          <h3 class="review-title">已评价订单</h3>
          
          <div class="reviews-grid">
            <div 
              v-for="order in completedReviews" 
              :key="order.id"
              class="review-card completed"
            >
              <div class="review-card-header">
                <div class="order-info">
                  <h4>{{ order.serviceType }}</h4>
                  <p class="order-time">{{ formatDate(order.reviewedTime) }}</p>
                </div>
                <div class="rating-badge">
                  <span class="badge-icon">⭐</span>
                  <span class="badge-value">{{ order.userRating }}分</span>
                </div>
              </div>
              
              <div class="review-card-body">
                <div class="service-provider">
                  <div class="provider-avatar">
                    {{ order.providerName.charAt(0) }}
                  </div>
                  <div class="provider-info">
                    <h5>{{ order.providerName }}</h5>
                  </div>
                </div>
                
                <div class="review-content">
                  <p class="comment">{{ order.userComment }}</p>
                  <div class="review-meta">
                    <span class="meta-item">服务时间：{{ formatDate(order.completedTime) }}</span>
                  </div>
                </div>
              </div>
              
              <div class="review-card-actions">
                <button 
                  @click="editReview(order)"
                  class="btn-edit"
                >
                  修改评价
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- 无待评价订单 -->
        <div v-if="pendingReviews.length === 0 && completedReviews.length === 0" class="no-reviews">
          <div class="empty-state">
            <div class="empty-icon">📝</div>
            <h3>暂无待评价订单</h3>
            <p>完成服务后，您可以在此处为服务提供者评价</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'

// 发布需求相关
const selectedType = ref('walk')
const rewardPoints = ref(50)

const publishData = reactive({
  petType: '',
  startTime: '',
  endTime: '',
  description: '',
  contact: ''
})

// 评价相关
const showReviewSection = ref(true)
const pendingReviews = ref([])
const completedReviews = ref([])

// 模拟数据
const mockOrders = [
  {
    id: 1,
    orderNumber: 'OD20231215001',
    serviceType: '遛狗服务',
    petType: 'dog',
    petName: '旺财',
    providerName: '张三',
    providerRating: 4.8,
    completedTime: '2023-12-14T15:30:00',
    userRating: 0,
    userComment: '',
    reviewedTime: null
  },
  {
    id: 2,
    orderNumber: 'OD20231214002',
    serviceType: '喂食照顾',
    petType: 'cat',
    petName: '咪咪',
    providerName: '李四',
    providerRating: 4.5,
    completedTime: '2023-12-13T10:00:00',
    userRating: 5,
    userComment: '非常细心的照顾，猫咪很喜欢！',
    reviewedTime: '2023-12-13T16:20:00'
  },
  {
    id: 3,
    orderNumber: 'OD20231213003',
    serviceType: '美容护理',
    petType: 'dog',
    petName: '球球',
    providerName: '王五',
    providerRating: 4.9,
    completedTime: '2023-12-12T14:00:00',
    userRating: 4,
    userComment: '洗澡很干净，狗狗看起来很舒服',
    reviewedTime: '2023-12-12T18:45:00'
  }
]

onMounted(() => {
  loadReviews()
})

const loadReviews = () => {
  // 模拟加载数据
  pendingReviews.value = mockOrders.filter(order => order.userRating === 0)
  completedReviews.value = mockOrders.filter(order => order.userRating > 0)
}

const getPetIcon = (petType) => {
  const icons = {
    dog: '🐶',
    cat: '🐱',
    rabbit: '🐰',
    bird: '🐦',
    other: '🐾'
  }
  return icons[petType] || '🐾'
}

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

const rateOrder = (order, rating) => {
  order.userRating = rating
}

const submitReview = (order) => {
  if (!order.userRating) {
    alert('请先选择评分')
    return
  }
  
  // 模拟提交
  order.reviewedTime = new Date().toISOString()
  pendingReviews.value = pendingReviews.value.filter(o => o.id !== order.id)
  completedReviews.value.unshift(order)
  
  alert('评价提交成功！')
}

const editReview = (order) => {
  // 将已评价订单移回待评价
  completedReviews.value = completedReviews.value.filter(o => o.id !== order.id)
  order.userRating = 0
  order.userComment = ''
  order.reviewedTime = null
  pendingReviews.value.unshift(order)
}

const submitRequirement = () => {
  // 验证和提交发布需求的逻辑
  console.log('发布需求数据:', publishData)
  alert('需求发布成功！')
}
</script>

<style scoped>
/* 原有样式保持不变，以下是新增的评价部分样式 */

/* 评价部分头部 */
.review-section {
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

/* 评价标题 */
.review-title {
  font-size: 20px;
  color: #334155;
  margin-bottom: 25px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 12px;
}

/* 评价网格 */
.reviews-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 25px;
  margin-bottom: 40px;
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
}

.review-card.completed {
  border-color: #d1fae5;
}

.review-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.review-card.completed .review-card-header {
  background: #f0fdf4;
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

.pet-info {
  display: flex;
  align-items: center;
  gap: 8px;
}

.pet-icon {
  font-size: 24px;
}

.pet-name {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
}

.rating-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  background: #fef3c7;
  padding: 6px 12px;
  border-radius: 20px;
}

.badge-icon {
  font-size: 16px;
}

.badge-value {
  font-size: 14px;
  color: #92400e;
  font-weight: 600;
}

/* 评价卡片主体 */
.review-card-body {
  padding: 20px;
}

.service-provider {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-bottom: 20px;
}

.provider-avatar {
  width: 45px;
  height: 45px;
  background: linear-gradient(135deg, #22c55e, #166534);
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 18px;
}

.provider-info h5 {
  font-size: 16px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 600;
}

.provider-info .rating {
  font-size: 13px;
  color: #64748b;
}

/* 评价表单 */
.review-form {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.rating-input {
  display: flex;
  align-items: center;
  gap: 15px;
}

.rating-input label {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
}

.stars {
  display: flex;
  gap: 4px;
}

.star {
  font-size: 28px;
  color: #e2e8f0;
  cursor: pointer;
  transition: color 0.2s;
  line-height: 1;
}

.star:hover,
.star.active {
  color: #fbbf24;
}

.rating-value {
  font-size: 14px;
  color: #64748b;
  font-weight: 500;
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
}

.comment-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

/* 已评价内容 */
.review-content {
  margin-top: 15px;
}

.comment {
  color: #475569;
  font-size: 14px;
  line-height: 1.6;
  margin-bottom: 12px;
}

.review-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
}

.meta-item {
  font-size: 13px;
  color: #94a3b8;
}

/* 评价卡片操作 */
.review-card-actions {
  padding: 20px;
  border-top: 1px solid #f1f5f9;
  display: flex;
  justify-content: flex-end;
}

.btn-review,
.btn-edit {
  padding: 10px 24px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  border: none;
}

.btn-review {
  background: #166534;
  color: white;
}

.btn-review:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

.btn-review:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
}

.btn-edit {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.btn-edit:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

/* 空状态 */
.no-reviews {
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
  .reviews-grid {
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  }
}

@media (max-width: 768px) {
  .review-section {
    padding: 25px;
    margin-top: 40px;
  }
  
  .reviews-grid {
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
}

@media (max-width: 480px) {
  .review-section {
    padding: 20px;
  }
  
  .section-header h2 {
    font-size: 24px;
  }
}

.publish-requirement {
  width: 100%;
  box-sizing: border-box;
}

/* 页面标题 */
.page-header {
  margin-bottom: 40px;
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

/* 发布表单布局 */
.publish-form {
  display: flex;
  gap: 30px;
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
}

/* 表单卡片标题 */
.form-card h3 {
  font-size: 24px;
  color: #166534;
  margin-bottom: 30px;
  font-weight: 700;
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
  transition: border-color 0.2s;
}

.form-input:focus,
.form-select:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
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
}

.type-card:hover {
  border-color: #d1fae5;
  transform: translateY(-2px);
}

.type-card.active {
  border-color: #22c55e;
  background: #f0fdf4;
}

.type-icon {
  font-size: 32px;
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
}

/* 文本区域 */
.form-textarea {
  width: 100%;
  padding: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  resize: vertical;
  transition: border-color 0.2s;
}

.form-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.mt-2 {
  margin-top: 10px;
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
}

.btn-primary {
  background: #166534;
  color: white;
  border: none;
}

.btn-primary:hover {
  background: #14532d;
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(22, 101, 52, 0.2);
}

/* 侧边提示卡片 */
.tips-card,
.stats-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
}

.tips-card h4,
.stats-card h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
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
}

.tips-list li:last-child {
  border-bottom: none;
}

.tips-list li:before {
  content: "✓";
  color: #22c55e;
  font-weight: bold;
  margin-right: 10px;
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
  font-size: 20px;
  font-weight: 700;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .publish-form {
    flex-direction: column;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .form-card {
    padding: 25px;
  }
  
  .requirement-types {
    grid-template-columns: 1fr;
  }
  
  .contact-info {
    flex-direction: column;
    gap: 15px;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .btn-primary,
  .btn-secondary {
    width: 100%;
  }
}
</style>