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
          <div class="stat-value">{{ communityStats.petOwners }}</div>
          <div class="stat-label">宠物主人</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.serviceProviders }}</div>
          <div class="stat-label">服务提供者</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.pendingReview }}</div>
          <div class="stat-label">待审核需求</div>
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
          📝 需求审核
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
            placeholder="搜索成员姓名..."
            class="search-input"
          >
        </div>
        <div class="filter-options">
          <select v-model="memberFilter" class="filter-select">
            <option value="all">所有成员</option>
            <option value="petOwner">宠物主人</option>
            <option value="serviceProvider">服务提供者</option>
            <option value="pendingReview">待审核</option>
            <option value="approved">已认证</option>
            <option value="rejected">未通过</option>
          </select>
        </div>
      </div>

      <!-- 成员列表 -->
      <div class="members-grid">
        <div class="member-card" v-for="member in filteredMembers" :key="member.id">
          <!-- 用户类型标签 -->
          <div class="user-type-badge" :class="member.userType">
            {{ member.userType === 'petOwner' ? '🐾 宠物主人' : '🛠️ 服务提供者' }}
          </div>

          <div class="member-avatar">
            <div class="avatar-img">{{ member.avatar }}</div>
          </div>
          
          <div class="member-info">
            <h3>{{ member.name }}</h3>
            <p class="member-location">📍 {{ member.location }}</p>
            
            <!-- 宠物信息（宠物主人显示） -->
            <div v-if="member.userType === 'petOwner' && member.pets" class="pets-info">
              <div class="pets-label">宠物：</div>
              <div class="pets-list">
                <span class="pet-tag" v-for="(pet, index) in member.pets" :key="index">
                  {{ pet.icon }} {{ pet.name }}
                </span>
              </div>
            </div>
          </div>

          <div class="member-actions"> 
            <!-- 用户类型切换按钮（管理员可以调整） -->
            <div class="user-type-actions">
              <select v-model="member.userType" class="role-dropdown" @change="updateUserType(member)">
                <option value="petOwner">宠物主人</option>
                <option value="serviceProvider">服务提供者</option>
              </select>
            </div>
            
            <!-- 移除成员按钮 -->
            <button class="action-btn remove-btn" @click="showRemoveDialog(member)">
              移除成员
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

    <!-- ===== 需求审核页面（重写） ===== -->
    <div class="tab-content" v-if="activeTab === 'content'">
      <div class="content-review">
        <!-- 审核统计和筛选 -->
        <div class="review-header">
          <div class="review-stats-cards">
            <div class="review-stat-card total">
              <div class="stat-icon">📋</div>
              <div class="stat-info">
                <h3>{{ pendingRequirements.length }}</h3>
                <p>待审核需求</p>
              </div>
            </div>
            <div class="review-stat-card approved">
              <div class="stat-icon">✅</div>
              <div class="stat-info">
                <h3>{{ approvedRequirements.length }}</h3>
                <p>已通过</p>
              </div>
            </div>
            <div class="review-stat-card rejected">
              <div class="stat-icon">❌</div>
              <div class="stat-info">
                <h3>{{ rejectedRequirements.length }}</h3>
                <p>已拒绝</p>
              </div>
            </div>
          </div>
          
          <div class="review-filters">
            <div class="filter-group">
              <button 
                class="filter-btn" 
                :class="{ active: reviewFilter === 'pending' }"
                @click="reviewFilter = 'pending'"
              >
                待审核 ({{ pendingRequirements.length }})
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
            
            <div class="filter-select-group">
              <select v-model="typeFilter" class="filter-select">
                <option value="all">所有类型</option>
                <option value="walk">遛狗服务</option>
                <option value="feed">喂食照顾</option>
                <option value="medical">就医陪伴</option>
                <option value="groom">美容护理</option>
              </select>
            </div>
          </div>
        </div>

        <!-- 审核列表 -->
        <div class="review-list">
          <!-- 待审核需求 -->
          <div v-if="reviewFilter === 'pending' && filteredRequirements.length > 0" class="pending-reviews">
            <div class="requirements-list">
              <div 
                v-for="requirement in filteredRequirements" 
                :key="requirement.id"
                class="requirement-review-item"
              >
                <div class="requirement-header">
                  <div class="requirement-type-badge" :style="{ backgroundColor: getTypeColor(requirement.type) }">
                    {{ getTypeName(requirement.type) }}
                    <span v-if="requirement.urgent" class="urgent-indicator">❗</span>
                  </div>
                  
                  <div class="requirement-status pending">
                    ⏳ 待审核
                  </div>
                </div>
                
                <div class="requirement-content">
                  <!-- 宠物信息 -->
                  <div class="pet-info-section">
                    <div class="pet-avatar-large">{{ getPetEmoji(requirement.petType) }}</div>
                    <div class="pet-details">
                      <h4>{{ requirement.petName || '未命名宠物' }}</h4>
                      <p class="pet-type">{{ getPetTypeName(requirement.petType) }}</p>
                    </div>
                  </div>
                  
                  <!-- 需求详情 -->
                  <div class="requirement-details">
                    <p class="description">{{ requirement.description }}</p>
                    
                    <div class="detail-row">
                      <div class="detail-item">
                        <span class="detail-icon">⏰</span>
                        <span class="detail-label">服务时间：</span>
                        <span class="detail-value">{{ formatTime(requirement.startTime) }} - {{ formatTime(requirement.endTime) }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">📍</span>
                        <span class="detail-label">服务地点：</span>
                        <span class="detail-value">{{ requirement.location || '未提供详细地址' }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">👤</span>
                        <span class="detail-label">发布者：</span>
                        <span class="detail-value">{{ requirement.publisher }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">📞</span>
                        <span class="detail-label">联系方式：</span>
                        <span class="detail-value">{{ requirement.contact || '未提供联系方式' }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">📅</span>
                        <span class="detail-label">发布时间：</span>
                        <span class="detail-value">{{ formatDate(requirement.postTime) }}</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <!-- 审核操作 -->
                <div class="review-actions-section">
                  <div class="rejection-reason-input" v-if="showRejectionInput === requirement.id">
                    <textarea 
                      v-model="rejectionReason" 
                      placeholder="请输入拒绝原因（必填），如：内容违规、联系方式无效、地址不详细等..."
                      class="reason-textarea"
                      rows="3"
                    ></textarea>
                  </div>
                  
                  <div class="action-buttons">
                    <button 
                      @click="approveRequirement(requirement)"
                      class="action-btn approve-btn"
                    >
                      ✅ 通过审核
                    </button>
                    
                    <button 
                      @click="toggleRejectionInput(requirement)"
                      class="action-btn reject-btn"
                    >
                      {{ showRejectionInput === requirement.id ? '取消拒绝' : '❌ 拒绝发布' }}
                    </button>
                    
                    <button 
                      v-if="showRejectionInput === requirement.id"
                      @click="rejectRequirement(requirement)"
                      class="action-btn confirm-reject-btn"
                      :disabled="!rejectionReason.trim()"
                    >
                      确认拒绝
                    </button>
                    
                    <button 
                      @click="viewPublisherProfile(requirement)"
                      class="action-btn view-btn"
                    >
                      👤 查看发布者
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 已审核需求（已通过和已拒绝） -->
          <div v-if="reviewFilter !== 'pending' && filteredRequirements.length > 0" class="reviewed-requirements">
            <div class="requirements-list">
              <div 
                v-for="requirement in filteredRequirements" 
                :key="requirement.id"
                class="requirement-review-item reviewed"
                :class="requirement.status"
              >
                <div class="requirement-header">
                  <div class="requirement-type-badge" :style="{ backgroundColor: getTypeColor(requirement.type) }">
                    {{ getTypeName(requirement.type) }}
                  </div>
                  
                  <div class="requirement-status" :class="requirement.status">
                    {{ requirement.status === 'approved' ? '✅ 已通过' : '❌ 已拒绝' }}
                  </div>
                </div>
                
                <div class="requirement-content">
                  <div class="pet-info-section">
                    <div class="pet-avatar-small">{{ getPetEmoji(requirement.petType) }}</div>
                    <div class="pet-details">
                      <h4>{{ requirement.petName || '未命名宠物' }}</h4>
                      <p class="pet-type">{{ getPetTypeName(requirement.petType) }}</p>
                    </div>
                  </div>
                  
                  <div class="requirement-details">
                    <p class="description">{{ requirement.description }}</p>
                    
                    <div class="detail-row compact">
                      <div class="detail-item">
                        <span class="detail-icon">⏰</span>
                        <span>{{ formatTime(requirement.startTime) }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">📍</span>
                        <span>{{ requirement.location || '未提供地址' }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">👤</span>
                        <span>{{ requirement.publisher }}</span>
                      </div>
                    </div>
                    
                    <!-- 审核信息 -->
                    <div class="review-info" v-if="requirement.reviewer">
                      <div class="reviewer-info">
                        <span class="reviewer-label">审核人：</span>
                        <span class="reviewer-name">{{ requirement.reviewer }}</span>
                        <span class="review-time">{{ formatDate(requirement.reviewedTime) }}</span>
                      </div>
                      
                      <div class="rejection-reason" v-if="requirement.rejectionReason && requirement.status === 'rejected'">
                        <span class="reason-label">拒绝原因：</span>
                        <span class="reason-text">{{ requirement.rejectionReason }}</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div class="review-actions-section">
                  <div class="action-buttons">
                    <button 
                      v-if="requirement.status === 'rejected'"
                      @click="reApproveRequirement(requirement)"
                      class="action-btn approve-btn"
                    >
                      🔄 重新审核
                    </button>
                    
                    <button 
                      @click="viewRequirementDetails(requirement)"
                      class="action-btn view-btn"
                    >
                      👁️ 查看详情
                    </button>
                    
                    <button 
                      @click="deleteReviewRecord(requirement)"
                      class="action-btn delete-btn"
                    >
                      🗑️ 删除记录
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 空状态 -->
          <div v-if="filteredRequirements.length === 0" class="no-reviews">
            <div class="empty-state">
              <div class="empty-icon" v-if="reviewFilter === 'pending'">🎉</div>
              <div class="empty-icon" v-if="reviewFilter === 'approved'">📄</div>
              <div class="empty-icon" v-if="reviewFilter === 'rejected'">📝</div>
              <h3>{{ getEmptyStateTitle() }}</h3>
              <p>{{ getEmptyStateMessage() }}</p>
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
        </div>
      </div>
    </div>>

    <!-- 模态框 -->
    <div class="modal-overlay" v-if="showModal" @click="closeModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h2>{{ modalTitle }}</h2>
          <button class="close-btn" @click="closeModal">×</button>
        </div>
        <div class="modal-body">
          <p v-if="modalType === 'remove'">确定要移除成员吗？</p>
          <p v-if="modalType === 'deleteRequirement'">确定要删除这条审核记录吗？此操作不可撤销！</p>
          <p v-if="modalType === 'rejectReview'">请输入拒绝审核的原因。</p>
          <p v-if="modalType === 'reReview'">确定要进行重新审核吗？</p>
          
          <!-- 编辑需求表单 -->
          <div v-if="modalType === 'editRequirement' && selectedRequirement" class="edit-requirement-form">
            <div class="form-group">
              <label>需求描述</label>
              <textarea v-model="editingRequirement.description" rows="4" class="form-textarea"></textarea>
            </div>
            <div class="form-group">
              <label>服务地点</label>
              <input v-model="editingRequirement.location" type="text" class="form-input">
            </div>
            <div class="form-group">
              <label>联系方式（仅管理员可见）</label>
              <input v-model="editingRequirement.contact" type="text" class="form-input">
            </div>
            <div class="form-group">
              <label>
                <input v-model="editingRequirement.urgent" type="checkbox">
                标记为紧急需求
              </label>
            </div>
          </div>
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
import { ref, reactive, computed, onMounted } from 'vue'

// 激活的标签页
const activeTab = ref('members')

// 社区统计
const communityStats = ref({
  members: 156,
  petOwners: 85,
  serviceProviders: 71,
  pendingReview: 5
})

// 搜索和筛选
const searchQuery = ref('')
const memberFilter = ref('all')

// 成员数据（保持不变）
const members = ref([
  { 
    id: 1, 
    name: '张三', 
    avatar: '😊', 
    location: '北京朝阳', 
    helped: 12, 
    received: 8, 
    points: 1560, 
    level: 3, 
    userType: 'serviceProvider',
    reviewStatus: 'pending',
    qualifications: ['宠物护理证书', '宠物急救证书'],
    reviewReason: ''
  },
  // ... 其他成员数据保持不变
])

// ===== 需求审核相关数据 =====
const reviewFilter = ref('pending')
const typeFilter = ref('all')
const urgencyFilter = ref('all')

// 待审核需求
const pendingRequirements = ref([
  {
    id: 101,
    type: 'walk',
    petType: 'dog',
    petName: '多多',
    description: '需要帮忙遛狗2小时，金毛犬，性格温顺但力气较大，需要有一定力量的帮助者',
    startTime: '2024-01-15T14:00:00',
    endTime: '2024-01-15T16:00:00',
    location: '北京市朝阳区三里屯SOHO',
    publisher: '张先生',
    publisherLevel: 3,
    contact: '138****8000',
    postTime: '2024-01-14T10:30:00',
    urgent: true,
    status: 'pending',
    complianceChecks: [
      { id: 1, icon: '✅', text: '联系方式合规', status: 'passed' },
      { id: 2, icon: '⚠️', text: '地址信息一般', status: 'warning' },
      { id: 3, icon: '✅', text: '需求描述清晰', status: 'passed' },
      { id: 4, icon: '❌', text: '紧急程度较高', status: 'failed' }
    ]
  },
  {
    id: 102,
    type: 'feed',
    petType: 'cat',
    petName: '咪咪',
    description: '出差3天，需要帮忙喂猫和清理猫砂，英短猫比较怕生，需要温柔耐心的帮助者',
    startTime: '2024-01-16T09:00:00',
    endTime: '2024-01-18T20:00:00',
    location: '北京市海淀区中关村',
    publisher: '李女士',
    publisherLevel: 2,
    contact: 'lily@example.com',
    postTime: '2024-01-14T15:45:00',
    urgent: false,
    status: 'pending',
    complianceChecks: [
      { id: 1, icon: '✅', text: '联系方式合规', status: 'passed' },
      { id: 2, icon: '✅', text: '地址信息详细', status: 'passed' },
      { id: 3, icon: '✅', text: '需求描述清晰', status: 'passed' },
      { id: 4, icon: '✅', text: '服务时间合理', status: 'passed' }
    ]
  },
  {
    id: 103,
    type: 'groom',
    petType: 'dog',
    petName: '小白',
    description: '需要帮忙给泰迪犬洗澡和修剪毛发，需要专业的美容服务',
    startTime: '2024-01-17T13:00:00',
    endTime: '2024-01-17T15:00:00',
    location: '北京市东城区王府井',
    publisher: '王先生',
    publisherLevel: 1,
    contact: 'wang@example.com',
    postTime: '2024-01-15T09:20:00',
    urgent: false,
    status: 'pending',
    complianceChecks: [
      { id: 1, icon: '✅', text: '联系方式合规', status: 'passed' },
      { id: 2, icon: '⚠️', text: '地址信息一般', status: 'warning' },
      { id: 3, icon: '✅', text: '需求描述清晰', status: 'passed' }
    ]
  },
  {
    id: 104,
    type: 'medical',
    petType: 'dog',
    petName: '旺财',
    description: '需要陪狗狗去医院打疫苗，需要有人陪伴并提供交通帮助',
    startTime: '2024-01-18T10:00:00',
    endTime: '2024-01-18T12:00:00',
    location: '北京市朝阳区望京',
    publisher: '赵女士',
    publisherLevel: 4,
    contact: 'zhao@example.com',
    postTime: '2024-01-15T14:30:00',
    urgent: true,
    status: 'pending',
    complianceChecks: [
      { id: 1, icon: '❌', text: '联系方式敏感', status: 'failed' },
      { id: 2, icon: '✅', text: '地址信息详细', status: 'passed' },
      { id: 3, icon: '✅', text: '需求描述清晰', status: 'passed' }
    ]
  }
])

// 已通过的需求
const approvedRequirements = ref([
  {
    id: 201,
    type: 'walk',
    petType: 'dog',
    petName: '豆豆',
    description: '每天下午需要遛狗1小时，柯基犬',
    startTime: '2024-01-14T16:00:00',
    endTime: '2024-01-14T17:00:00',
    location: '北京市西城区金融街',
    publisher: '钱先生',
    publisherLevel: 3,
    contact: 'qian@example.com',
    postTime: '2024-01-13T14:20:00',
    reviewedTime: '2024-01-13T15:30:00',
    reviewer: '管理员A',
    status: 'approved',
    urgent: false
  }
])

// 已拒绝的需求
const rejectedRequirements = ref([
  {
    id: 301,
    type: 'other',
    petType: 'other',
    petName: '未知',
    description: '需要特殊宠物服务，联系我详谈',
    startTime: '2024-01-16T20:00:00',
    endTime: '2024-01-16T22:00:00',
    location: '未知地点',
    publisher: '匿名用户',
    publisherLevel: 0,
    contact: '123456',
    postTime: '2024-01-14T22:10:00',
    reviewedTime: '2024-01-14T23:15:00',
    reviewer: '管理员B',
    rejectionReason: '内容违规，联系方式无效',
    status: 'rejected',
    urgent: false
  }
])

// 社区设置
const communitySettings = ref({
  name: 'PetPal 北京社区',
  description: '北京地区的宠物爱好者和宠物主聚集地',
  requireApproval: true,
  autoFlagSensitive: true,
  urgentReviewTime: '4',
  rejectTemplates: `联系方式不清晰\n地址信息不完整\n需求描述不明确\n内容涉及违规\n服务时间不合理\n宠物信息不全`
})

// 模态框
const showModal = ref(false)
const modalType = ref('')
const modalTitle = ref('')
const modalConfirmText = ref('')
const selectedMember = ref(null)
const selectedRequirement = ref(null)
const editingRequirement = ref(null)

// 拒绝相关
const showRejectionInput = ref(null)
const rejectionReason = ref('')

// 计算属性
const filteredMembers = computed(() => {
  let result = members.value
  
  // 搜索过滤
  if (searchQuery.value) {
    result = result.filter(member => 
      member.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    )
  }
  
  // 筛选过滤
  if (memberFilter.value === 'petOwner') {
    result = result.filter(member => member.userType === 'petOwner')
  } else if (memberFilter.value === 'serviceProvider') {
    result = result.filter(member => member.userType === 'serviceProvider')
  } else if (memberFilter.value === 'pendingReview') {
    result = result.filter(member => member.reviewStatus === 'pending')
  } else if (memberFilter.value === 'approved') {
    result = result.filter(member => member.reviewStatus === 'approved')
  } else if (memberFilter.value === 'rejected') {
    result = result.filter(member => member.reviewStatus === 'rejected')
  }
  
  return result
})

// 过滤需求列表
const filteredRequirements = computed(() => {
  let requirements = []
  
  // 根据筛选条件选择不同的需求列表
  if (reviewFilter.value === 'pending') {
    requirements = pendingRequirements.value
  } else if (reviewFilter.value === 'approved') {
    requirements = approvedRequirements.value
  } else if (reviewFilter.value === 'rejected') {
    requirements = rejectedRequirements.value
  }
  
  // 类型过滤
  if (typeFilter.value !== 'all') {
    requirements = requirements.filter(req => req.type === typeFilter.value)
  }
  
  // 紧急程度过滤
  if (urgencyFilter.value !== 'all') {
    requirements = requirements.filter(req => {
      if (urgencyFilter.value === 'urgent') return req.urgent === true
      if (urgencyFilter.value === 'normal') return req.urgent === false
      return true
    })
  }
  
  return requirements
})

// 成员分布和活跃度数据（保持不变）
const memberDistribution = ref([
  { type: '宠物主人', count: 85, percentage: 54 },
  { type: '服务提供者', count: 71, percentage: 46 }
])

const activityData = ref([80, 65, 75, 90, 85, 70, 95])

// 方法
const getLevelClass = (level) => {
  if (level >= 4) return 'level-high'
  if (level >= 3) return 'level-medium'
  return 'level-low'
}

// 宠物相关方法
const getPetEmoji = (petType) => {
  const emojiMap = {
    dog: '🐶',
    cat: '🐱',
    rabbit: '🐰',
    bird: '🐦',
    other: '🐾'
  }
  return emojiMap[petType] || '🐾'
}

const getPetTypeName = (petType) => {
  const typeMap = {
    dog: '狗狗',
    cat: '猫咪',
    rabbit: '兔兔',
    bird: '鸟鸟',
    other: '其他宠物'
  }
  return typeMap[petType] || '宠物'
}

const getTypeColor = (type) => {
  const colorMap = {
    walk: '#3b82f6',    // 蓝色
    feed: '#10b981',    // 绿色
    medical: '#ef4444', // 红色
    groom: '#8b5cf6',   // 紫色
    other: '#6b7280'    // 灰色
  }
  return colorMap[type] || '#6b7280'
}

const getTypeName = (type) => {
  const typeMap = {
    walk: '遛狗服务',
    feed: '喂食照顾',
    medical: '就医陪伴',
    groom: '美容护理',
  }
  return typeMap[type] || '其他服务'
}

// 时间格式化
const formatTime = (timeString) => {
  const date = new Date(timeString)
  return date.toLocaleString('zh-CN', {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
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

// ===== 需求审核方法 =====

// 通过审核
const approveRequirement = (requirement) => {
  const index = pendingRequirements.value.findIndex(r => r.id === requirement.id)
  if (index !== -1) {
    const approvedRequirement = {
      ...pendingRequirements.value[index],
      status: 'approved',
      reviewedTime: new Date().toISOString(),
      reviewer: '当前管理员'
    }
    approvedRequirements.value.unshift(approvedRequirement)
    pendingRequirements.value.splice(index, 1)
    communityStats.value.pendingReview--
    
    // 通知用户
    console.log(`需求审核通过：${requirement.petName} - ${getTypeName(requirement.type)}`)
    alert(`已通过需求审核：${requirement.petName} - ${getTypeName(requirement.type)}`)
  }
}

// 切换拒绝理由输入框
const toggleRejectionInput = (requirement) => {
  if (showRejectionInput.value === requirement.id) {
    showRejectionInput.value = null
    rejectionReason.value = ''
  } else {
    showRejectionInput.value = requirement.id
    rejectionReason.value = ''
  }
}

// 拒绝需求
const rejectRequirement = (requirement) => {
  if (!rejectionReason.value.trim()) {
    alert('请填写拒绝原因')
    return
  }
  
  const index = pendingRequirements.value.findIndex(r => r.id === requirement.id)
  if (index !== -1) {
    const rejectedRequirement = {
      ...pendingRequirements.value[index],
      status: 'rejected',
      reviewedTime: new Date().toISOString(),
      reviewer: '当前管理员',
      rejectionReason: rejectionReason.value
    }
    rejectedRequirements.value.unshift(rejectedRequirement)
    pendingRequirements.value.splice(index, 1)
    communityStats.value.pendingReview--
    showRejectionInput.value = null
    rejectionReason.value = ''
    
    console.log(`需求审核拒绝：${requirement.petName} - ${getTypeName(requirement.type)}`)
    alert(`已拒绝需求发布：${requirement.petName} - ${getTypeName(requirement.type)}`)
  }
}

// 编辑需求
const editRequirement = (requirement) => {
  selectedRequirement.value = requirement
  editingRequirement.value = { ...requirement }
  modalType.value = 'editRequirement'
  modalTitle.value = '编辑需求内容'
  modalConfirmText.value = '保存修改'
  showModal.value = true
}

// 查看发布者资料
const viewPublisherProfile = (requirement) => {
  // 在实际应用中，这里应该跳转到用户资料页面
  console.log('查看发布者资料：', requirement.publisher)
  alert(`即将查看用户 ${requirement.publisher} 的资料`)
}

// 重新审核已拒绝的需求
const reApproveRequirement = (requirement) => {
  const index = rejectedRequirements.value.findIndex(r => r.id === requirement.id)
  if (index !== -1) {
    const rependingRequirement = {
      ...rejectedRequirements.value[index],
      status: 'pending',
      reviewedTime: null,
      reviewer: null,
      rejectionReason: null
    }
    pendingRequirements.value.unshift(rependingRequirement)
    rejectedRequirements.value.splice(index, 1)
    communityStats.value.pendingReview++
    
    alert('需求已重新提交审核')
  }
}

// 查看需求详情
const viewRequirementDetails = (requirement) => {
  let details = `需求详情：\n`
  details += `类型：${getTypeName(requirement.type)}\n`
  details += `宠物：${requirement.petName}（${getPetTypeName(requirement.petType)}）\n`
  details += `描述：${requirement.description}\n`
  details += `时间：${formatTime(requirement.startTime)} - ${formatTime(requirement.endTime)}\n`
  details += `地点：${requirement.location}\n`
  details += `发布者：${requirement.publisher}\n`
  details += `状态：${requirement.status === 'approved' ? '已通过' : '已拒绝'}\n`
  
  if (requirement.reviewer) {
    details += `审核人：${requirement.reviewer}\n`
    details += `审核时间：${formatDate(requirement.reviewedTime)}\n`
  }
  
  if (requirement.rejectionReason) {
    details += `拒绝原因：${requirement.rejectionReason}\n`
  }
  
  alert(details)
}

// 删除审核记录
const deleteReviewRecord = (requirement) => {
  selectedRequirement.value = requirement
  modalType.value = 'deleteRequirement'
  modalTitle.value = '删除审核记录'
  modalConfirmText.value = '确认删除'
  showModal.value = true
}

// 空状态文本
const getEmptyStateTitle = () => {
  switch (reviewFilter.value) {
    case 'pending': return '暂无待审核需求'
    case 'approved': return '暂无已通过需求'
    case 'rejected': return '暂无已拒绝需求'
    default: return '暂无数据'
  }
}

const getEmptyStateMessage = () => {
  switch (reviewFilter.value) {
    case 'pending': return '所有发布的需求都已审核完毕'
    case 'approved': return '还没有需求通过审核'
    case 'rejected': return '还没有需求被拒绝'
    default: return '暂无相关数据'
  }
}

// 设置相关方法
const resetSettings = () => {
  communitySettings.value = {
    name: 'PetPal 北京社区',
    description: '北京地区的宠物爱好者和宠物主聚集地',
    requireApproval: true,
    autoFlagSensitive: true,
    urgentReviewTime: '4',
    rejectTemplates: `联系方式不清晰\n地址信息不完整\n需求描述不明确\n内容涉及违规\n服务时间不合理\n宠物信息不全`
  }
  alert('设置已恢复为默认值')
}

const saveSettings = () => {
  console.log('保存设置:', communitySettings.value)
  alert('设置已保存！')
}

// 成员相关方法（保持不变）
const approveQualification = (member) => {
  member.reviewStatus = 'approved'
  communityStats.value.pendingReview--
  console.log('通过审核:', member.name)
}

const showRejectDialog = (member) => {
  selectedMember.value = member
  modalType.value = 'rejectReview'
  modalTitle.value = '拒绝资质审核'
  modalConfirmText.value = '确认拒绝'
  showModal.value = true
}

const rejectQualification = (reason) => {
  if (selectedMember.value) {
    selectedMember.value.reviewStatus = 'rejected'
    selectedMember.value.reviewReason = reason
    communityStats.value.pendingReview--
    console.log('拒绝审核:', selectedMember.value.name, '原因:', reason)
  }
}

const viewQualification = (member) => {
  console.log('查看资质:', member.name)
}

const showReReviewDialog = (member) => {
  selectedMember.value = member
  modalType.value = 'reReview'
  modalTitle.value = '重新审核资质'
  modalConfirmText.value = '开始重新审核'
  showModal.value = true
}

const viewRejectReason = (member) => {
  alert(`审核未通过原因：\n${member.reviewReason || '未提供具体原因'}`)
}

const allowResubmit = (member) => {
  member.reviewStatus = 'pending'
  communityStats.value.pendingReview++
  console.log('允许重新提交:', member.name)
}

const updateUserType = (member) => {
  console.log('更新用户类型:', member.name, '新类型:', member.userType)
}

const showRemoveDialog = (member) => {
  selectedMember.value = member
  modalType.value = 'remove'
  modalTitle.value = '移除成员'
  modalConfirmText.value = '移除'
  showModal.value = true
}

// 模态框方法
const closeModal = () => {
  showModal.value = false
  selectedMember.value = null
  selectedRequirement.value = null
  editingRequirement.value = null
}

const confirmModal = () => {
  switch (modalType.value) {
    case 'remove':
      console.log('移除成员:', selectedMember.value?.name)
      break
      
    case 'deleteRequirement':
      // 删除审核记录
      if (selectedRequirement.value) {
        if (selectedRequirement.value.status === 'approved') {
          const index = approvedRequirements.value.findIndex(r => r.id === selectedRequirement.value.id)
          if (index !== -1) {
            approvedRequirements.value.splice(index, 1)
          }
        } else if (selectedRequirement.value.status === 'rejected') {
          const index = rejectedRequirements.value.findIndex(r => r.id === selectedRequirement.value.id)
          if (index !== -1) {
            rejectedRequirements.value.splice(index, 1)
          }
        }
        console.log('删除审核记录:', selectedRequirement.value.petName)
      }
      break
      
    case 'rejectReview':
      const reason = prompt('请输入拒绝原因：', '资质不符合要求')
      if (reason) {
        rejectQualification(reason)
      }
      break
      
    case 'reReview':
      if (confirm(`确定要对 ${selectedMember.value?.name} 进行重新审核吗？`)) {
        selectedMember.value.reviewStatus = 'pending'
        communityStats.value.pendingReview++
      }
      break
      
    case 'editRequirement':
      if (editingRequirement.value && selectedRequirement.value) {
        // 更新需求内容
        Object.assign(selectedRequirement.value, editingRequirement.value)
        console.log('需求已更新:', selectedRequirement.value.petName)
      }
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
  flex-wrap: wrap;
}

.stat-item {
  text-align: center;
  padding: 20px;
  background: #f8fafc;
  border-radius: 16px;
  min-width: 120px;
  flex: 1;
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

/* 标签页内容 */
.tab-content {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* ===== 需求审核页面样式 ===== */
.review-header {
  margin-bottom: 30px;
}

.review-stats-cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
  margin-bottom: 30px;
}

.review-stat-card {
  padding: 25px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  gap: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.review-stat-card.total {
  background: linear-gradient(135deg, #3b82f6, #1d4ed8);
  color: white;
}

.review-stat-card.approved {
  background: linear-gradient(135deg, #10b981, #047857);
  color: white;
}

.review-stat-card.rejected {
  background: linear-gradient(135deg, #ef4444, #b91c1c);
  color: white;
}

.review-stat-card .stat-icon {
  font-size: 40px;
}

.review-stat-card .stat-info h3 {
  font-size: 32px;
  font-weight: 800;
  margin-bottom: 4px;
}

.review-stat-card .stat-info p {
  font-size: 14px;
  opacity: 0.9;
}

/* 审核筛选 */
.review-filters {
  display: flex;
  flex-direction: column;
  gap: 20px;
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

.filter-select-group {
  display: flex;
  gap: 15px;
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

/* 需求审核项 */
.requirements-list {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.requirement-review-item {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  overflow: hidden;
  transition: all 0.3s;
}

.requirement-review-item:hover {
  border-color: #d1fae5;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.requirement-review-item.reviewed.approved {
  border-left: 4px solid #10b981;
}

.requirement-review-item.reviewed.rejected {
  border-left: 4px solid #ef4444;
}

.requirement-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.requirement-type-badge {
  color: white;
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 8px;
}

.urgent-indicator {
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.5; }
}

.requirement-status {
  padding: 6px 16px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
}

.requirement-status.pending {
  background: #fef3c7;
  color: #92400e;
}

.requirement-status.approved {
  background: #d1fae5;
  color: #065f46;
}

.requirement-status.rejected {
  background: #fee2e2;
  color: #991b1b;
}

.requirement-content {
  padding: 25px;
}

/* 宠物信息 */
.pet-info-section {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 25px;
  padding-bottom: 20px;
  border-bottom: 1px solid #f1f5f9;
}

.pet-avatar-large {
  width: 60px;
  height: 60px;
  background: #f0fdf4;
  border-radius: 15px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
}

.pet-avatar-small {
  width: 40px;
  height: 40px;
  background: #f0fdf4;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
}

.pet-details h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 600;
}

.pet-type {
  font-size: 14px;
  color: #64748b;
}

/* 需求详情 */
.requirement-details .description {
  color: #475569;
  font-size: 15px;
  line-height: 1.6;
  margin-bottom: 25px;
  padding: 15px;
  background: #f8fafc;
  border-radius: 10px;
  border-left: 3px solid #d1fae5;
}

.detail-row {
  display: flex;
  flex-direction: column;
  gap: 15px;
  margin-bottom: 25px;
}

.detail-row.compact {
  flex-direction: row;
  flex-wrap: wrap;
  gap: 20px;
}

.detail-item {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  color: #64748b;
}

.detail-label {
  color: #475569;
  font-weight: 500;
  min-width: 70px;
}

.detail-value {
  color: #1e293b;
  font-weight: 500;
}

.member-level {
  background: #e2e8f0;
  color: #475569;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  margin-left: 8px;
}

/* 审核信息 */
.review-info {
  background: #f8fafc;
  border-radius: 10px;
  padding: 15px;
  margin-top: 20px;
}

.reviewer-info {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  font-size: 13px;
  color: #64748b;
  margin-bottom: 10px;
}

.reviewer-label {
  font-weight: 500;
}

.reviewer-name {
  color: #166534;
  font-weight: 600;
}

.review-time {
  color: #94a3b8;
}

.rejection-reason {
  background: #fee2e2;
  border-radius: 8px;
  padding: 12px;
  font-size: 13px;
}

.reason-label {
  font-weight: 600;
  color: #991b1b;
}

.reason-text {
  color: #475569;
  margin-left: 8px;
}

/* 审核操作区域 */
.review-actions-section {
  padding: 20px 25px;
  background: #f8fafc;
  border-top: 1px solid #f1f5f9;
}

.rejection-reason-input {
  margin-bottom: 20px;
}

.reason-textarea {
  width: 100%;
  padding: 12px;
  border: 1px solid #ef4444;
  border-radius: 8px;
  font-size: 14px;
  resize: vertical;
  min-height: 80px;
}

.reason-textarea:focus {
  outline: none;
  border-color: #dc2626;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.action-buttons {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.action-btn {
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
}

.approve-btn {
  background: #d1fae5;
  color: #065f46;
}

.approve-btn:hover {
  background: #a7f3d0;
  transform: translateY(-1px);
}

.reject-btn {
  background: #fee2e2;
  color: #991b1b;
}

.reject-btn:hover {
  background: #fecaca;
  transform: translateY(-1px);
}

.confirm-reject-btn {
  background: #ef4444;
  color: white;
}

.confirm-reject-btn:hover:not(:disabled) {
  background: #dc2626;
}

.confirm-reject-btn:disabled {
  background: #cbd5e1;
  cursor: not-allowed;
}

.edit-btn {
  background: #eff6ff;
  color: #3b82f6;
}

.edit-btn:hover {
  background: #dbeafe;
  transform: translateY(-1px);
}

.view-btn {
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.view-btn:hover {
  background: #e2e8f0;
}

.delete-btn {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.delete-btn:hover {
  background: #fecaca;
}

/* 空状态 */
.no-reviews {
  text-align: center;
  padding: 60px 40px;
  background: white;
  border-radius: 16px;
  border: 2px dashed #e2e8f0;
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

/* 设置页面 */
.setting-description {
  font-size: 13px;
  color: #94a3b8;
  margin-top: 4px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #475569;
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  width: 18px;
  height: 18px;
}

/* 模态框中的编辑表单 */
.edit-requirement-form .form-group {
  margin-bottom: 20px;
}

.edit-requirement-form label {
  display: block;
  margin-bottom: 8px;
  color: #475569;
  font-weight: 500;
  font-size: 14px;
}

/* ===== 以下为原有样式（成员管理部分）保持不变 ===== */
/* 成员管理页面样式保持不变... */

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

/* 用户类型徽章 */
.user-type-badge {
  position: absolute;
  top: 16px;
  right: 16px;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  color: white;
}

.user-type-badge.petOwner {
  background: linear-gradient(135deg, #8b5cf6, #a78bfa);
}

.user-type-badge.serviceProvider {
  background: linear-gradient(135deg, #f59e0b, #fbbf24);
}

/* 审核状态徽章 */
.review-badge {
  position: absolute;
  top: 16px;
  left: 16px;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  color: white;
}

.review-badge.pending {
  background: #f59e0b;
}

.review-badge.rejected {
  background: #ef4444;
}

/* 等级徽章 */
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

.level-badge.level-low {
  background: #94a3b8;
}

.level-badge.level-medium {
  background: #3b82f6;
}

.level-badge.level-high {
  background: #8b5cf6;
}

/* 成员头像 */
.member-avatar {
  text-align: center;
  margin: 40px 0 25px;
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

.member-location {
  color: #94a3b8;
  font-size: 13px;
  margin-bottom: 20px;
}

/* 资质信息样式 */
.qualifications, .pets-info {
  margin: 15px 0;
}

.qualification-label, .pets-label {
  font-size: 13px;
  color: #64748b;
  margin-bottom: 8px;
  font-weight: 500;
}

.qualification-list, .pets-list {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  justify-content: center;
}

.qualification-tag, .pet-tag {
  background: #f0fdf4;
  color: #166534;
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.pet-tag {
  background: #eff6ff;
  color: #3b82f6;
}

.member-stats {
  display: flex;
  justify-content: space-around;
  margin-top: 20px;
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

/* 审核按钮样式 */
.review-actions, .reviewed-actions, .rejected-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 12px;
}

.member-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
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
  .review-stats-cards {
    grid-template-columns: repeat(3, 1fr);
  }
  
  .members-stats {
    grid-template-columns: 1fr;
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
  
  .filter-select-group {
    flex-direction: column;
  }
}

@media (max-width: 768px) {
  .header-stats {
    flex-wrap: wrap;
  }
  
  .stat-item {
    flex: 1 1 calc(50% - 15px);
    min-width: auto;
  }
  
  .review-stats-cards {
    grid-template-columns: 1fr;
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
  
  .filter-group {
    width: 100%;
    flex-direction: column;
  }
  
  .filter-btn {
    width: 100%;
    text-align: center;
  }
  
  .action-buttons {
    flex-direction: column;
  }
  
  .action-btn {
    width: 100%;
    justify-content: center;
  }
  
  .modal-content {
    width: 95%;
    margin: 20px;
  }
}

@media (max-width: 480px) {
  .requirement-header {
    flex-direction: column;
    gap: 15px;
    align-items: flex-start;
  }
  
  .pet-info-section {
    flex-direction: column;
    text-align: center;
  }
  
  .review-actions, .reviewed-actions, .rejected-actions {
    flex-direction: column;
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
  
  .stat-item {
    flex: 1 1 100%;
  }
}
</style>