<!-- src/views/AcceptRequirement.vue -->
<template>
  <div class="accept-requirement">
    <!-- 页面标题和筛选 -->
    <div class="page-header">
      <div class="header-left">
        <h1>接单需求</h1>
        <p>选择您能帮助的需求，获取积分奖励</p>
      </div>
      <div class="header-actions">
        <div class="filter-group">
          <select class="filter-select">
            <option value="">全部类型</option>
            <option value="walk">遛狗服务</option>
            <option value="feed">喂食照顾</option>
            <option value="medical">就医陪伴</option>
            <option value="groom">美容护理</option>
          </select>
          
          <select class="filter-select">
            <option value="">距离优先</option>
            <option value="nearest">最近优先</option>
            <option value="highest">积分最高</option>
            <option value="urgent">紧急需求</option>
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
            <!-- 紧急标签 -->
            <div class="urgent-badge" v-if="requirement.urgent">
              🔥 紧急
            </div>
            
            <!-- 需求头部 -->
            <div class="card-header">
              <div class="pet-info">
                <div class="pet-avatar">{{ getPetEmoji(requirement.petType) }}</div>
                <div class="pet-details">
                  <h3>{{ requirement.petName }}</h3>
                  <p class="pet-type">{{ getPetTypeName(requirement.petType) }}</p>
                </div>
              </div>
              <div class="reward">
                <span class="reward-points">{{ requirement.rewardPoints }}</span>
                <span class="reward-label">积分</span>
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
                <span class="detail-text">{{ formatTime(requirement.startTime) }} • {{ requirement.duration }}小时</span>
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
            
            <!-- 技能匹配度 -->
            <div class="skill-match" v-if="requirement.matchRate">
              <div class="match-header">
                <span>技能匹配度</span>
                <span class="match-rate">{{ requirement.matchRate }}%</span>
              </div>
              <div class="match-bar">
                <div class="match-fill" :style="{ width: requirement.matchRate + '%' }"></div>
              </div>
            </div>
            
            <!-- 卡片底部 -->
            <div class="card-footer">
              <div class="time-info">
                <span class="time-icon">🕐</span>
                <span class="time-text">{{ formatTimeAgo(requirement.postTime) }}</span>
              </div>
              <button class="accept-btn" @click="showAcceptDialog(requirement)">
                接受需求
              </button>
            </div>
          </div>
        </div>
        
        <!-- 加载更多 -->
        <div class="load-more">
          <button class="load-more-btn" @click="loadMoreRequirements">
            <span v-if="!loading">加载更多需求</span>
            <span v-else>加载中...</span>
          </button>
        </div>
      </div>
      
      <!-- 右侧信息栏 -->
      <div class="right-sidebar">
        <!-- 我的技能 -->
        <div class="skills-card">
          <h3>我的技能</h3>
          <div class="skills-list">
            <div class="skill-item" v-for="skill in skills" :key="skill.id">
              <span class="skill-name">{{ skill.name }}</span>
              <span class="skill-level">{{ skill.level }}</span>
            </div>
          </div>
          <button class="edit-skills-btn">编辑技能</button>
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
              <span class="info-label">宠物信息：</span>
              <span class="info-value">{{ selectedRequirement.petName }}（{{ selectedRequirement.petTypeName }}）</span>
            </div>
            <div class="info-row">
              <span class="info-label">悬赏积分：</span>
              <span class="info-value reward-value">{{ selectedRequirement.rewardPoints }} 积分</span>
            </div>
            <div class="info-row">
              <span class="info-label">服务时间：</span>
              <span class="info-value">{{ formatTime(selectedRequirement.startTime) }}</span>
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
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// 需求数据
const requirements = ref([
  {
    id: 1,
    petName: "多多",
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
    duration: 1,
    postTime: "2024-01-15T09:30",
    urgent: true,
    matchRate: 92
  },
  {
    id: 2,
    petName: "花花",
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
    duration: 2,
    postTime: "2024-01-15T10:15",
    urgent: false,
    matchRate: 85
  },
  {
    id: 3,
    petName: "球球",
    petType: "rabbit",
    type: "feed",
    typeName: "喂食照顾",
    petTypeName: "垂耳兔",
    description: "需要帮忙照顾兔子3天，提供详细指导",
    rewardPoints: 150,
    distance: 3.1,
    location: "东城区王府井",
    publisher: "王小姐",
    startTime: "2024-01-17T08:00",
    duration: 3,
    postTime: "2024-01-15T11:20",
    urgent: false,
    matchRate: 78
  },
  {
    id: 4,
    petName: "旺财",
    petType: "dog",
    type: "groom",
    typeName: "美容护理",
    petTypeName: "泰迪犬",
    description: "需要洗澡和修剪毛发，宠物店太忙了约不上",
    rewardPoints: 200,
    distance: 1.8,
    location: "西城区金融街",
    publisher: "陈先生",
    startTime: "2024-01-15T15:00",
    duration: 2,
    postTime: "2024-01-15T12:45",
    urgent: true,
    matchRate: 65
  },
  {
    id: 5,
    petName: "咪咪",
    petType: "cat",
    type: "medical",
    typeName: "就医陪伴",
    petTypeName: "波斯猫",
    description: "需要陪同去宠物医院做年度体检",
    rewardPoints: 180,
    distance: 2.2,
    location: "丰台区方庄",
    publisher: "刘女士",
    startTime: "2024-01-16T10:00",
    duration: 3,
    postTime: "2024-01-15T13:30",
    urgent: false,
    matchRate: 90
  },
  {
    id: 6,
    petName: "豆豆",
    petType: "dog",
    type: "walk",
    typeName: "遛狗服务",
    petTypeName: "柯基犬",
    description: "每天傍晚遛狗半小时，连续一周",
    rewardPoints: 250,
    distance: 0.8,
    location: "朝阳区国贸",
    publisher: "赵先生",
    startTime: "2024-01-15T18:00",
    duration: 0.5,
    postTime: "2024-01-15T14:15",
    urgent: false,
    matchRate: 95
  }
])

// 我的技能
const skills = ref([
  { id: 1, name: "宠物喂养", level: "高级" },
  { id: 2, name: "遛狗服务", level: "中级" },
  { id: 3, name: "基础医疗", level: "初级" },
  { id: 4, name: "美容护理", level: "中级" },
  { id: 5, name: "宠物训练", level: "初级" }
])

// 今日数据
const todayStats = ref({
  accepted: 3,
  earned: 420,
  hours: 5.5
})

// 状态
const loading = ref(false)
const showDialog = ref(false)
const activeFilter = ref('nearby')
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

// 格式化时间差
const formatTimeAgo = (timeString) => {
  const now = new Date()
  const postTime = new Date(timeString)
  const diffMs = now - postTime
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMins / 60)
  
  if (diffMins < 60) {
    return `${diffMins}分钟前`
  } else if (diffHours < 24) {
    return `${diffHours}小时前`
  } else {
    return Math.floor(diffHours / 24) + '天前'
  }
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
  // 这里可以添加API调用
  alert(`已成功接受 "${selectedRequirement.value.petName}" 的需求！`)
  closeDialog()
  
  // 更新今日数据
  todayStats.value.accepted++
  todayStats.value.earned += selectedRequirement.value.rewardPoints
  todayStats.value.hours += selectedRequirement.value.duration
}

// 设置筛选
const setFilter = (filter) => {
  activeFilter.value = filter
  // 这里可以添加筛选逻辑
  console.log('设置筛选:', filter)
}

// 加载更多需求
const loadMoreRequirements = () => {
  loading.value = true
  // 模拟加载延迟
  setTimeout(() => {
    requirements.value.push(
      ...requirements.value.slice(0, 2).map((item, index) => ({
        ...item,
        id: requirements.value.length + index + 1,
        rewardPoints: item.rewardPoints + 50,
        distance: item.distance + 0.5
      }))
    )
    loading.value = false
  }, 1000)
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
}

.requirements-list {
  flex: 2.5;
}

.right-sidebar {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 20px;
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

.urgent-badge {
  position: absolute;
  top: -10px;
  right: 16px;
  background: #ef4444;
  color: white;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.3);
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

/* 悬赏积分 */
.reward {
  text-align: right;
}

.reward-points {
  font-size: 24px;
  font-weight: 800;
  color: #f59e0b;
  display: block;
  line-height: 1;
}

.reward-label {
  font-size: 12px;
  color: #94a3b8;
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

/* 技能匹配度 */
.skill-match {
  background: #f8fafc;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 20px;
}

.match-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 13px;
  color: #475569;
}

.match-rate {
  font-weight: 700;
  color: #22c55e;
}

.match-bar {
  height: 6px;
  background: #e2e8f0;
  border-radius: 3px;
  overflow: hidden;
}

.match-fill {
  height: 100%;
  background: linear-gradient(90deg, #22c55e, #10b981);
  border-radius: 3px;
  transition: width 0.3s;
}

/* 卡片底部 */
.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 16px;
  border-top: 1px solid #f1f5f9;
}

.time-info {
  display: flex;
  align-items: center;
  gap: 6px;
}

.time-icon {
  color: #94a3b8;
}

.time-text {
  font-size: 12px;
  color: #94a3b8;
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

/* 加载更多 */
.load-more {
  text-align: center;
}

.load-more-btn {
  background: white;
  color: #166534;
  border: 2px solid #d1fae5;
  padding: 12px 40px;
  border-radius: 25px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.load-more-btn:hover {
  background: #f0fdf4;
  border-color: #22c55e;
}

/* 右侧边栏卡片 */
.skills-card,
.stats-card,
.quick-filter-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
}

.skills-card h3,
.stats-card h3,
.quick-filter-card h3 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
}

/* 技能列表 */
.skills-list {
  margin-bottom: 20px;
}

.skill-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f1f5f9;
}

.skill-item:last-child {
  border-bottom: none;
}

.skill-name {
  color: #475569;
  font-size: 14px;
}

.skill-level {
  color: #22c55e;
  font-size: 12px;
  font-weight: 600;
  padding: 2px 8px;
  background: #f0fdf4;
  border-radius: 12px;
}

.edit-skills-btn {
  width: 100%;
  padding: 10px;
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #d1fae5;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.edit-skills-btn:hover {
  background: #d1fae5;
}

/* 今日数据 */
.today-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 15px;
  text-align: center;
}

.stat-item {
  padding: 15px;
  background: #f8fafc;
  border-radius: 12px;
}

.stat-value {
  font-size: 24px;
  font-weight: 800;
  color: #166534;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 12px;
  color: #64748b;
}

/* 快速筛选标签 */
.filter-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.filter-tag {
  padding: 6px 12px;
  background: #f8fafc;
  color: #64748b;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.filter-tag:hover {
  background: #e2e8f0;
}

.filter-tag.active {
  background: #166534;
  color: white;
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

.reward-value {
  color: #f59e0b;
  font-weight: 700;
  font-size: 18px;
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

/* 响应式设计 */
@media (max-width: 1200px) {
  .requirements-container {
    flex-direction: column;
  }
  
  .right-sidebar {
    flex-direction: row;
  }
  
  .skills-card,
  .stats-card,
  .quick-filter-card {
    flex: 1;
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
  .requirements-grid {
    grid-template-columns: 1fr;
  }
  
  .right-sidebar {
    flex-direction: column;
  }
  
  .today-stats {
    grid-template-columns: repeat(3, 1fr);
  }
  
  .filter-tags {
    justify-content: center;
  }
  
  .dialog-content {
    width: 95%;
    margin: 20px;
  }
}

@media (max-width: 480px) {
  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }
  
  .reward {
    text-align: left;
  }
  
  .dialog-actions {
    flex-direction: column;
  }
  
  .dialog-btn {
    width: 100%;
  }
}
</style>