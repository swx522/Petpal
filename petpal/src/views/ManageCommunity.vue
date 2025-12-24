<!-- src/views/ManageCommunity.vue -->
<template>
  <div class="manage-community">
    <!-- 页面标题和统计 -->
    <div class="page-header">
      <div class="header-left">
        <h1>管理社区</h1>
        <p>管理您的社区成员、活动和内容</p>
      </div>
      <div class="header-stats">
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.members }}</div>
          <div class="stat-label">社区成员</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.active }}</div>
          <div class="stat-label">活跃用户</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.posts }}</div>
          <div class="stat-label">今日动态</div>
        </div>
      </div>
    </div>

    <!-- 管理导航 -->
    <div class="management-tabs">
      <div class="tabs">
        <button 
          class="tab-btn" 
          :class="{ active: activeTab === 'members' }"
          @click="activeTab = 'members'"
        >
          👥 成员管理
        </button>
        <button 
          class="tab-btn" 
          :class="{ active: activeTab === 'content' }"
          @click="activeTab = 'content'"
        >
          📝 内容审核
        </button>
        <button 
          class="tab-btn" 
          :class="{ active: activeTab === 'settings' }"
          @click="activeTab = 'settings'"
        >
          ⚙️ 社区设置
        </button>
      </div>
    </div>

    <!-- 成员管理页面 -->
    <div class="tab-content" v-if="activeTab === 'members'">
      <!-- 搜索和筛选 -->
      <div class="content-header">
        <div class="search-box">
          <span class="search-icon">🔍</span>
          <input 
            type="text" 
            v-model="searchQuery" 
            placeholder="搜索成员姓名或宠物..."
            class="search-input"
          >
        </div>
        <div class="filter-options">
          <select v-model="memberFilter" class="filter-select">
            <option value="all">所有成员</option>
            <option value="active">活跃成员</option>
            <option value="new">新加入</option>
            <option value="verified">已验证</option>
          </select>
        </div>
      </div>

      <!-- 成员列表 -->
      <div class="members-grid">
        <div class="member-card" v-for="member in filteredMembers" :key="member.id">
          <!-- 管理员徽章 -->
          <div class="admin-badge" v-if="member.role === 'admin'">
            👑 管理员
          </div>
          
          <!-- 会员等级 -->
          <div class="level-badge" :class="getLevelClass(member.level)">
            Lv.{{ member.level }}
          </div>

          <div class="member-avatar">
            <div class="avatar-img">{{ member.avatar }}</div>
            <div class="online-status" :class="{ online: member.online }"></div>
          </div>
          
          <div class="member-info">
            <h3>{{ member.name }}</h3>
            <p class="member-pet">🐶 {{ member.pet }}</p>
            <p class="member-location">📍 {{ member.location }}</p>
            
            <div class="member-stats">
              <div class="stat">
                <span class="stat-number">{{ member.helped }}</span>
                <span class="stat-label">帮助</span>
              </div>
              <div class="stat">
                <span class="stat-number">{{ member.received }}</span>
                <span class="stat-label">接受</span>
              </div>
              <div class="stat">
                <span class="stat-number">{{ member.points }}</span>
                <span class="stat-label">积分</span>
              </div>
            </div>
          </div>

          <div class="member-actions">
            <button class="action-btn chat-btn" @click="messageMember(member)">
              💬 电话联系
            </button>
            <button class="action-btn remove-btn" @click="showRemoveDialog(member)">
              移除
            </button>
          </div>
        </div>
      </div>

      <!-- 成员统计 -->
      <div class="members-stats">
        <div class="stat-card">
          <h4>成员分布</h4>
          <div class="distribution-chart">
            <div class="chart-item" v-for="item in memberDistribution" :key="item.type">
              <div class="chart-label">{{ item.type }}</div>
              <div class="chart-bar">
                <div class="bar-fill" :style="{ width: item.percentage + '%' }"></div>
              </div>
              <div class="chart-value">{{ item.count }}人</div>
            </div>
          </div>
        </div>
        
        <div class="stat-card">
          <h4>活跃度趋势</h4>
          <div class="activity-chart">
            <div class="chart-points">
              <div 
                class="point" 
                v-for="(point, index) in activityData" 
                :key="index"
                :style="{ height: point + '%', left: (index * 20) + 'px' }"
                :title="'活跃度: ' + point"
              ></div>
            </div>
          </div>
          <div class="chart-legend">
            <span>近7天活跃度</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 内容审核页面 -->
    <div class="tab-content" v-if="activeTab === 'content'">
      <div class="content-review">
        <!-- 审核筛选 -->
        <div class="review-filters">
          <div class="filter-group">
            <button 
              class="filter-btn" 
              :class="{ active: reviewFilter === 'pending' }"
              @click="reviewFilter = 'pending'"
            >
              待审核 ({{ pendingCount }})
            </button>
            <button 
              class="filter-btn" 
              :class="{ active: reviewFilter === 'approved' }"
              @click="reviewFilter = 'approved'"
            >
              已通过
            </button>
            <button 
              class="filter-btn" 
              :class="{ active: reviewFilter === 'rejected' }"
              @click="reviewFilter = 'rejected'"
            >
              已拒绝
            </button>
          </div>
        </div>

        <!-- 审核列表 -->
        <div class="review-list">
          <div class="review-item" v-for="item in filteredReviews" :key="item.id">
            <div class="review-content">
              <div class="content-type">{{ item.type }}</div>
              <div class="content-title">{{ item.title }}</div>
              <div class="content-author">
                <span class="author-avatar">{{ item.authorAvatar }}</span>
                <span class="author-name">{{ item.author }}</span>
                <span class="post-time">{{ item.time }}</span>
              </div>
              <p class="content-text">{{ item.content }}</p>
              
              <div class="content-attachments" v-if="item.attachments">
                <div class="attachment" v-for="attachment in item.attachments" :key="attachment">
                  📎 {{ attachment }}
                </div>
              </div>
            </div>
            
            <div class="review-actions">
              <div class="review-buttons">
                <button class="approve-btn" @click="approveContent(item)">
                  ✅ 通过
                </button>
                <button class="reject-btn" @click="rejectContent(item)">
                  ❌ 拒绝
                </button>
                <button class="edit-btn" @click="editContent(item)">
                  ✏️ 编辑
                </button>
              </div>
              <div class="review-reason" v-if="item.reviewer">
                <span class="reviewer">审核人：{{ item.reviewer }}</span>
                <span class="reason" v-if="item.reason">原因：{{ item.reason }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 社区设置页面 -->
    <div class="tab-content" v-if="activeTab === 'settings'">
      <div class="settings-container">
        <div class="settings-form">
          <h3>社区设置</h3>
          
          <div class="setting-section">
            <h4>基本信息</h4>
            <div class="form-group">
              <label>社区名称</label>
              <input type="text" v-model="communitySettings.name" class="form-input">
            </div>
            <div class="form-group">
              <label>社区描述</label>
              <textarea v-model="communitySettings.description" rows="3" class="form-textarea"></textarea>
            </div>
          </div>
          
          <div class="setting-actions">
            <button class="btn-secondary">恢复默认</button>
            <button class="btn-primary" @click="saveSettings">保存设置</button>
          </div>
        </div>
      </div>
    </div>

    <!-- 创建活动对话框 -->
    <div class="modal-overlay" v-if="showModal" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ modalTitle }}</h2>
          <button class="close-btn" @click="closeModal">×</button>
        </div>
        <div class="modal-body">
          <!-- 根据不同的modalType显示不同的内容 -->
          <p v-if="modalType === 'create'">创建活动表单将在这里显示...</p>
          <p v-if="modalType === 'remove'">确定要移除成员吗？</p>
          <p v-if="modalType === 'delete'">确定要解散社区吗？此操作不可撤销！</p>
        </div>
        <div class="modal-actions">
          <button class="btn-secondary" @click="closeModal">取消</button>
          <button class="btn-primary" @click="confirmModal">
            {{ modalConfirmText }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

// 激活的标签页
const activeTab = ref('members')

// 社区统计
const communityStats = ref({
  members: 156,
  active: 48,
  posts: 23
})

// 搜索和筛选
const searchQuery = ref('')
const memberFilter = ref('all')

// 成员数据
const members = ref([
  { id: 1, name: '张三', avatar: '😊', pet: '多多', location: '北京朝阳', helped: 12, received: 8, points: 1560, level: 3, role: 'member', online: true },
  { id: 2, name: '李四', avatar: '🐶', pet: '旺财', location: '上海浦东', helped: 8, received: 5, points: 980, level: 2, role: 'moderator', online: true },
  { id: 3, name: '王五', avatar: '🐱', pet: '花花', location: '广州天河', helped: 15, received: 10, points: 2100, level: 4, role: 'admin', online: false },
  { id: 4, name: '赵六', avatar: '🐰', pet: '小白', location: '深圳南山', helped: 5, received: 3, points: 650, level: 1, role: 'member', online: true },
  { id: 5, name: '钱七', avatar: '🦊', pet: '豆豆', location: '杭州西湖', helped: 20, received: 15, points: 2800, level: 5, role: 'member', online: true },
  { id: 6, name: '孙八', avatar: '🐻', pet: '胖胖', location: '成都锦江', helped: 7, received: 4, points: 890, level: 2, role: 'member', online: false }
])

// 成员分布
const memberDistribution = ref([
  { type: '活跃成员', count: 48, percentage: 30 },
  { type: '普通成员', count: 85, percentage: 54 },
  { type: '新成员', count: 23, percentage: 15 }
])

// 活跃度数据
const activityData = ref([80, 65, 75, 90, 85, 70, 95])

// 活动数据
const activeActivities = ref([
  { 
    id: 1, 
    title: '周末遛狗聚会', 
    type: 'walk', 
    description: '周末在公园组织的大型遛狗社交活动', 
    date: '2024-01-20 14:00', 
    location: '中央公园', 
    participants: 15, 
    maxParticipants: 30, 
    status: 'active' 
  },
  { 
    id: 2, 
    title: '宠物训练基础课', 
    type: 'training', 
    description: '专业训犬师指导的基础训练课程', 
    date: '2024-01-22 10:00', 
    location: '社区活动中心', 
    participants: 8, 
    maxParticipants: 12, 
    status: 'active' 
  },
  { 
    id: 3, 
    title: '流浪猫救助活动', 
    type: 'adoption', 
    description: '帮助寻找流浪猫的领养家庭', 
    date: '2024-01-25 13:00', 
    location: '动物救助站', 
    participants: 25, 
    maxParticipants: 40, 
    status: 'upcoming' 
  }
])

// 活动统计
const activityStats = ref({
  total: 24,
  upcoming: 8,
  completed: 16
})

// 内容审核
const reviewFilter = ref('pending')
const pendingCount = ref(5)
const reviews = ref([
  { 
    id: 1, 
    type: '活动申请', 
    title: '夜跑遛狗活动', 
    author: '张三', 
    authorAvatar: '😊', 
    time: '2小时前', 
    content: '想组织一个晚上的遛狗活动...', 
    attachments: ['活动计划书.pdf'], 
    status: 'pending' 
  },
  { 
    id: 2, 
    type: '帖子', 
    title: '狗狗训练心得分享', 
    author: '李四', 
    authorAvatar: '🐶', 
    time: '5小时前', 
    content: '分享一些训练狗狗的小技巧...', 
    attachments: null, 
    status: 'pending' 
  }
])

// 社区设置
const communitySettings = ref({
  name: 'PetPal 北京社区',
  description: '北京地区的宠物爱好者和宠物主聚集地',
  allowPosting: true,
  requireApproval: false,
  contentReview: true,
  activityNotifications: 'all',
  memberNotifications: 'all'
})

// 邀请链接
const inviteLink = ref('https://petpal.com/invite/abc123')

// 模态框
const showModal = ref(false)
const modalType = ref('')
const modalTitle = ref('')
const modalConfirmText = ref('')
const selectedMember = ref(null)

// 计算属性
const filteredMembers = computed(() => {
  let result = members.value
  
  // 搜索过滤
  if (searchQuery.value) {
    result = result.filter(member => 
      member.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      member.pet.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }
  
  // 筛选过滤
  if (memberFilter.value === 'active') {
    result = result.filter(member => member.online)
  } else if (memberFilter.value === 'new') {
    result = result.filter(member => member.level <= 2)
  } else if (memberFilter.value === 'verified') {
    result = result.filter(member => member.level >= 3)
  }
  
  return result
})

const filteredReviews = computed(() => {
  if (reviewFilter.value === 'pending') {
    return reviews.value.filter(item => item.status === 'pending')
  }
  return reviews.value
})

// 方法
const getLevelClass = (level) => {
  if (level >= 4) return 'level-high'
  if (level >= 3) return 'level-medium'
  return 'level-low'
}

const getActivityColor = (type) => {
  const colors = {
    walk: '#3b82f6',
    training: '#10b981',
    adoption: '#8b5cf6',
    other: '#6b7280'
  }
  return colors[type] || '#6b7280'
}

const getActivityType = (type) => {
  const types = {
    walk: '遛狗聚会',
    training: '训练课程',
    adoption: '领养活动',
    other: '其他活动'
  }
  return types[type] || '活动'
}

const getStatusText = (status) => {
  const texts = {
    active: '进行中',
    upcoming: '即将开始',
    completed: '已完成'
  }
  return texts[status] || status
}

// 成员相关方法
const messageMember = (member) => {
  console.log('私信成员:', member.name)
}

const updateMemberRole = (member) => {
  console.log('更新成员角色:', member.name, member.role)
}

// 活动相关方法
const viewActivity = (activity) => {
  console.log('查看活动:', activity.title)
}

const editActivity = (activity) => {
  console.log('编辑活动:', activity.title)
}

const cancelActivity = (activity) => {
  if (confirm(`确定要取消活动 "${activity.title}" 吗？`)) {
    console.log('取消活动:', activity.title)
  }
}

const createActivity = (type) => {
  showCreateDialog(type)
}

// 内容审核方法
const approveContent = (item) => {
  item.status = 'approved'
  pendingCount.value--
  console.log('通过内容:', item.title)
}

const rejectContent = (item) => {
  item.status = 'rejected'
  pendingCount.value--
  console.log('拒绝内容:', item.title)
}

const editContent = (item) => {
  console.log('编辑内容:', item.title)
}

// 设置相关方法
const saveSettings = () => {
  console.log('保存设置:', communitySettings.value)
  alert('设置已保存！')
}

const exportCommunityData = () => {
  console.log('导出社区数据')
}

const showTransferDialog = () => {
  console.log('显示转让对话框')
}

const copyInviteLink = () => {
  navigator.clipboard.writeText(inviteLink.value)
  alert('邀请链接已复制到剪贴板！')
}

const generateNewLink = () => {
  inviteLink.value = `https://petpal.com/invite/${Math.random().toString(36).substr(2, 8)}`
  alert('已生成新的邀请链接！')
}

// 模态框方法
const showCreateDialog = (type = '') => {
  modalType.value = 'create'
  modalTitle.value = '创建新活动'
  modalConfirmText.value = '创建'
  showModal.value = true
}

const showRemoveDialog = (member) => {
  modalType.value = 'remove'
  modalTitle.value = '移除成员'
  modalConfirmText.value = '移除'
  selectedMember.value = member
  showModal.value = true
}

const showDeleteDialog = () => {
  modalType.value = 'delete'
  modalTitle.value = '解散社区'
  modalConfirmText.value = '解散'
  showModal.value = true
}

const closeModal = () => {
  showModal.value = false
  selectedMember.value = null
}

const confirmModal = () => {
  switch (modalType.value) {
    case 'create':
      console.log('创建活动')
      break
    case 'remove':
      console.log('移除成员:', selectedMember.value?.name)
      break
    case 'delete':
      console.log('解散社区')
      break
  }
  closeModal()
}
</script>

<style scoped>
.manage-community {
  width: 100%;
  box-sizing: border-box;
}

/* 页面头部 */
.page-header {
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

.header-stats {
  display: flex;
  gap: 30px;
  margin-top: 20px;
}

.stat-item {
  text-align: center;
  padding: 20px;
  background: #f8fafc;
  border-radius: 16px;
  min-width: 120px;
}

.stat-value {
  font-size: 28px;
  font-weight: 800;
  color: #166534;
  margin-bottom: 4px;
}

.stat-label {
  font-size: 14px;
  color: #64748b;
}

/* 管理导航 */
.management-tabs {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
  border-bottom: 2px solid #f1f5f9;
  padding-bottom: 10px;
}

.tabs {
  display: flex;
  gap: 5px;
  flex: 1;
}

.tab-btn {
  padding: 12px 24px;
  background: none;
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s;
}

.tab-btn:hover {
  background: #f8fafc;
  color: #475569;
}

.tab-btn.active {
  background: #166534;
  color: white;
}

.create-btn {
  background: #166534;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s;
}

.create-btn:hover {
  background: #14532d;
  transform: translateY(-1px);
}

/* 标签页内容 */
.tab-content {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* 成员管理页面 */
.content-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.search-box {
  display: flex;
  align-items: center;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 10px 16px;
  width: 300px;
}

.search-icon {
  color: #94a3b8;
  margin-right: 10px;
}

.search-input {
  border: none;
  outline: none;
  flex: 1;
  font-size: 14px;
  color: #475569;
}

.search-input::placeholder {
  color: #94a3b8;
}

.filter-select {
  padding: 10px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  font-size: 14px;
  color: #475569;
  min-width: 160px;
}

/* 成员卡片网格 */
.members-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 25px;
  margin-bottom: 40px;
}

.member-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
  position: relative;
  transition: all 0.3s;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.member-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #d1fae5;
}

.admin-badge {
  position: absolute;
  top: 16px;
  right: 16px;
  background: linear-gradient(135deg, #f59e0b, #d97706);
  color: white;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.level-badge {
  position: absolute;
  top: 16px;
  left: 16px;
  color: white;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.level-low {
  background: #94a3b8;
}

.level-medium {
  background: #3b82f6;
}

.level-high {
  background: #8b5cf6;
}

/* 成员头像 */
.member-avatar {
  text-align: center;
  margin: 20px 0 25px;
  position: relative;
}

.avatar-img {
  width: 80px;
  height: 80px;
  background: #f0fdf4;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 36px;
  margin: 0 auto 15px;
  border: 4px solid white;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.online-status {
  position: absolute;
  bottom: 20px;
  right: calc(50% - 45px);
  width: 16px;
  height: 16px;
  background: #94a3b8;
  border: 3px solid white;
  border-radius: 50%;
}

.online-status.online {
  background: #22c55e;
}

/* 成员信息 */
.member-info {
  text-align: center;
  margin-bottom: 25px;
}

.member-info h3 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 8px;
  font-weight: 700;
}

.member-pet {
  color: #64748b;
  font-size: 14px;
  margin-bottom: 4px;
}

.member-location {
  color: #94a3b8;
  font-size: 13px;
  margin-bottom: 20px;
}

.member-stats {
  display: flex;
  justify-content: space-around;
}

.stat {
  text-align: center;
}

.stat-number {
  display: block;
  font-size: 18px;
  font-weight: 700;
  color: #166534;
  margin-bottom: 2px;
}

.stat-label {
  font-size: 12px;
  color: #94a3b8;
}

/* 成员操作 */
.member-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.action-btn {
  padding: 10px;
  border-radius: 8px;
  border: none;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s;
}

.chat-btn {
  background: #3b82f6;
  color: white;
}

.chat-btn:hover {
  background: #2563eb;
}

.remove-btn {
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.remove-btn:hover {
  background: #fee2e2;
  color: #dc2626;
  border-color: #fca5a5;
}

.role-dropdown {
  width: 100%;
  padding: 10px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  font-size: 14px;
  color: #475569;
}

/* 成员统计 */
.members-stats {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 30px;
  margin-top: 40px;
}

.stat-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
}

.stat-card h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
}

.distribution-chart {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.chart-item {
  display: flex;
  align-items: center;
  gap: 15px;
}

.chart-label {
  width: 100px;
  font-size: 14px;
  color: #64748b;
}

.chart-bar {
  flex: 1;
  height: 12px;
  background: #f1f5f9;
  border-radius: 6px;
  overflow: hidden;
}

.bar-fill {
  height: 100%;
  background: linear-gradient(90deg, #22c55e, #10b981);
  border-radius: 6px;
}

.chart-value {
  width: 60px;
  text-align: right;
  font-size: 14px;
  color: #166534;
  font-weight: 600;
}

.activity-chart {
  height: 150px;
  position: relative;
  border-bottom: 1px solid #f1f5f9;
  margin-bottom: 15px;
}

.chart-points {
  position: absolute;
  bottom: 0;
  left: 20px;
  right: 20px;
  height: 100%;
}

.point {
  position: absolute;
  bottom: 0;
  width: 12px;
  background: #22c55e;
  border-radius: 6px 6px 0 0;
  transition: height 0.3s;
  cursor: help;
}

.chart-legend {
  text-align: center;
  color: #64748b;
  font-size: 13px;
}

/* 活动管理页面 */
.activities-container {
  display: flex;
  gap: 30px;
}

.upcoming-activities {
  flex: 2;
}

.upcoming-activities h3 {
  font-size: 20px;
  color: #1e293b;
  margin-bottom: 25px;
  font-weight: 700;
}

.activities-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.activity-item {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
  transition: all 0.3s;
}

.activity-item:hover {
  border-color: #d1fae5;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.activity-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.activity-type {
  color: white;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
}

.activity-status {
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
}

.activity-status.active {
  background: #f0fdf4;
  color: #22c55e;
}

.activity-status.upcoming {
  background: #eff6ff;
  color: #3b82f6;
}

.activity-status.completed {
  background: #f8fafc;
  color: #64748b;
}

.activity-content h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 12px;
  font-weight: 600;
}

.activity-desc {
  color: #64748b;
  font-size: 14px;
  line-height: 1.5;
  margin-bottom: 20px;
}

.activity-details {
  display: flex;
  flex-direction: column;
  gap: 10px;
  margin-bottom: 20px;
}

.detail {
  display: flex;
  align-items: center;
  gap: 10px;
}

.detail-icon {
  color: #94a3b8;
  width: 20px;
}

.detail-text {
  color: #64748b;
  font-size: 14px;
}

.activity-actions {
  display: flex;
  gap: 12px;
}

.view-btn, .edit-btn, .cancel-btn {
  padding: 10px 20px;
  border-radius: 8px;
  border: none;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s;
}

.view-btn {
  background: #3b82f6;
  color: white;
}

.view-btn:hover {
  background: #2563eb;
}

.edit-btn {
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #d1fae5;
}

.edit-btn:hover {
  background: #d1fae5;
}

.cancel-btn {
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.cancel-btn:hover {
  background: #fee2e2;
  color: #dc2626;
  border-color: #fca5a5;
}

/* 活动侧边栏 */
.activity-sidebar {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.sidebar-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
}

.sidebar-card h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
}

.activity-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 15px;
}

.quick-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.quick-btn {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 15px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.quick-btn:hover {
  background: #f0fdf4;
  border-color: #d1fae5;
  transform: translateX(5px);
}

.quick-btn span {
  font-size: 20px;
}

/* 内容审核页面 */
.review-filters {
  margin-bottom: 30px;
}

.filter-group {
  display: flex;
  gap: 10px;
  background: #f8fafc;
  padding: 6px;
  border-radius: 12px;
  width: fit-content;
}

.filter-btn {
  padding: 10px 24px;
  background: none;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s;
}

.filter-btn:hover {
  background: #e2e8f0;
}

.filter-btn.active {
  background: #166534;
  color: white;
}

.review-list {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.review-item {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
  display: flex;
  gap: 30px;
}

.review-content {
  flex: 2;
}

.content-type {
  display: inline-block;
  background: #eff6ff;
  color: #3b82f6;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 12px;
}

.content-title {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 12px;
  font-weight: 600;
}

.content-author {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 16px;
}

.author-avatar {
  width: 28px;
  height: 28px;
  background: #f0fdf4;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
}

.author-name {
  color: #475569;
  font-size: 14px;
  font-weight: 500;
}

.post-time {
  color: #94a3b8;
  font-size: 13px;
}

.content-text {
  color: #64748b;
  font-size: 14px;
  line-height: 1.6;
  margin-bottom: 16px;
}

.content-attachments {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.attachment {
  color: #64748b;
  font-size: 13px;
  display: flex;
  align-items: center;
  gap: 8px;
}

.review-actions {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.review-buttons {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.approve-btn, .reject-btn, .edit-btn {
  padding: 12px;
  border-radius: 8px;
  border: none;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s;
}

.approve-btn {
  background: #f0fdf4;
  color: #22c55e;
  border: 1px solid #d1fae5;
}

.approve-btn:hover {
  background: #d1fae5;
}

.reject-btn {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.reject-btn:hover {
  background: #fecaca;
}

.edit-btn {
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.edit-btn:hover {
  background: #e2e8f0;
}

.review-reason {
  font-size: 13px;
  color: #94a3b8;
  padding-top: 15px;
  border-top: 1px solid #f1f5f9;
}

.reviewer {
  display: block;
  margin-bottom: 4px;
}

/* 社区设置页面 */
.settings-container {
  display: flex;
  gap: 30px;
}

.settings-form {
  flex: 2;
}

.settings-form h3 {
  font-size: 24px;
  color: #1e293b;
  margin-bottom: 30px;
  font-weight: 700;
}

.setting-section {
  margin-bottom: 40px;
  padding-bottom: 40px;
  border-bottom: 1px solid #f1f5f9;
}

.setting-section h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
}

.form-group {
  margin-bottom: 25px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #475569;
  font-weight: 500;
  font-size: 14px;
}

.form-input, .form-textarea {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  color: #475569;
  transition: all 0.3s;
}

.form-input:focus, .form-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
}

.logo-upload {
  display: flex;
  align-items: center;
  gap: 20px;
}

.logo-preview {
  width: 80px;
  height: 80px;
  background: #f0fdf4;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px dashed #d1fae5;
}

.logo-text {
  font-size: 24px;
  font-weight: 800;
  color: #166534;
}

.upload-btn {
  padding: 10px 20px;
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #d1fae5;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.upload-btn:hover {
  background: #d1fae5;
}

.permission-settings {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.permission-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: #f8fafc;
  border-radius: 12px;
  transition: all 0.3s;
}

.permission-item:hover {
  background: #f1f5f9;
}

.permission-info h5 {
  font-size: 16px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 600;
}

.permission-info p {
  font-size: 14px;
  color: #64748b;
}

/* 开关 */
.switch {
  position: relative;
  display: inline-block;
  width: 60px;
  height: 34px;
}

.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: #e2e8f0;
  transition: .4s;
  border-radius: 34px;
}

.slider:before {
  position: absolute;
  content: "";
  height: 26px;
  width: 26px;
  left: 4px;
  bottom: 4px;
  background-color: white;
  transition: .4s;
  border-radius: 50%;
}

input:checked + .slider {
  background-color: #22c55e;
}

input:checked + .slider:before {
  transform: translateX(26px);
}

/* 通知设置 */
.notification-settings {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.notification-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: #f8fafc;
  border-radius: 12px;
}

.notification-item span {
  color: #475569;
  font-weight: 500;
}

.notification-select {
  padding: 8px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: white;
  font-size: 14px;
  color: #475569;
  min-width: 180px;
}

/* 设置操作按钮 */
.setting-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding-top: 30px;
  border-top: 1px solid #f1f5f9;
}

.btn-secondary, .btn-primary {
  padding: 12px 32px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-secondary {
  background: white;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.btn-secondary:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.btn-primary {
  background: #166534;
  color: white;
  border: none;
}

.btn-primary:hover {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

/* 设置侧边栏 */
.settings-sidebar {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.danger-zone {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.danger-btn {
  padding: 15px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: center;
}

.danger-btn:hover {
  background: #f1f5f9;
  transform: translateX(-5px);
}

.danger-btn.delete-btn:hover {
  background: #fee2e2;
  color: #dc2626;
  border-color: #fca5a5;
}

/* 邀请链接 */
.invite-section {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.invite-link {
  display: flex;
  gap: 10px;
}

.link-input {
  flex: 1;
  padding: 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  background: #f8fafc;
  color: #64748b;
  font-size: 14px;
}

.copy-btn {
  padding: 12px 20px;
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #d1fae5;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s;
}

.copy-btn:hover {
  background: #d1fae5;
}

.invite-btn {
  width: 100%;
  padding: 15px;
  background: #166534;
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.invite-btn:hover {
  background: #14532d;
}

/* 模态框 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(2px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
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

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 30px 30px 20px;
  border-bottom: 1px solid #f1f5f9;
}

.modal-header h2 {
  font-size: 24px;
  color: #1e293b;
  font-weight: 700;
}

.modal-header .close-btn {
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

.modal-header .close-btn:hover {
  background: #f1f5f9;
  color: #64748b;
}

.modal-body {
  padding: 30px;
  color: #64748b;
  line-height: 1.6;
}

.modal-actions {
  display: flex;
  gap: 15px;
  justify-content: flex-end;
  padding: 20px 30px 30px;
  border-top: 1px solid #f1f5f9;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .members-stats {
    grid-template-columns: 1fr;
  }
  
  .activities-container {
    flex-direction: column;
  }
  
  .settings-container {
    flex-direction: column;
  }
}

@media (max-width: 900px) {
  .management-tabs {
    flex-direction: column;
    gap: 20px;
    align-items: flex-start;
  }
  
  .tabs {
    width: 100%;
    overflow-x: auto;
    padding-bottom: 10px;
  }
  
  .review-item {
    flex-direction: column;
  }
}

@media (max-width: 768px) {
  .header-stats {
    flex-wrap: wrap;
  }
  
  .stat-item {
    flex: 1;
    min-width: auto;
  }
  
  .members-grid {
    grid-template-columns: 1fr;
  }
  
  .content-header {
    flex-direction: column;
    gap: 20px;
    align-items: stretch;
  }
  
  .search-box {
    width: 100%;
  }
  
  .activity-stats {
    grid-template-columns: repeat(3, 1fr);
  }
  
  .notification-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 10px;
  }
  
  .modal-content {
    width: 95%;
    margin: 20px;
  }
}

@media (max-width: 480px) {
  .activity-actions {
    flex-direction: column;
  }
  
  .view-btn, .edit-btn, .cancel-btn {
    width: 100%;
  }
  
  .member-stats {
    flex-direction: column;
    gap: 10px;
  }
  
  .setting-actions {
    flex-direction: column;
  }
  
  .btn-secondary, .btn-primary {
    width: 100%;
  }
}
</style>