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
          <div class="stat-value">{{ communityStats.totalMembers || 0 }}</div>
          <div class="stat-label">社区成员</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.petusers || 0 }}</div>
          <div class="stat-label">宠物主人</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.serviceProviders || 0 }}</div>
          <div class="stat-label">服务提供者</div>
        </div>
        <div class="stat-item">
          <div class="stat-value">{{ communityStats.pendingRequests || 0 }}</div>
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
            @input="searchMembers"
          >
        </div>
        <div class="filter-options">
          <select v-model="memberFilter" class="filter-select" @change="filterMembers">
            <option value="all">所有成员</option>
            <option value="User">宠物主人</option>
            <option value="Sitter">服务提供者</option>
          </select>
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loadingMembers" class="loading-state">
        <div class="loading-spinner"></div>
        <p>正在加载成员数据...</p>
      </div>

      <!-- 成员列表 -->
      <div class="members-grid" v-if="!loadingMembers && members.length > 0">
        <div class="member-card" v-for="member in members" :key="member.id">
          <!-- 用户类型标签 -->
          <div class="user-type-badge" :class="getRoleClass(member.role)">
            {{ member.role === 0 ? '🐾 宠物主人' : '🛠️ 服务提供者' }}
          </div>

          <div class="member-avatar">
            <div class="avatar-img">{{ getAvatarEmoji(member.name) }}</div>
            <div class="member-points">{{ member.creditScore || 100 }}分</div>
          </div>
          
          <div class="member-info">
            <h3>{{ member.nickName || member.username || '未命名用户' }}</h3>
          </div>

          <!-- 统计信息 -->
          <div class="member-stats" v-if="member.role === 'Sitter'">
            <div class="stat">
              <div class="stat-number">{{ member.completedOrders || 0 }}</div>
              <div class="stat-label">完成订单</div>
            </div>
            <div class="stat">
              <div class="stat-number">{{ member.averageRating ? member.averageRating.toFixed(1) : '0.0' }}</div>
              <div class="stat-label">平均评分</div>
            </div>
          </div>

          <div class="member-actions">
            <!-- 移除成员按钮 -->
            <button class="action-btn remove-btn" @click="showRemoveDialog(member)">
              移除成员
            </button>
          </div>
        </div>
      </div>

      <!-- 空状态 -->
      <div v-if="!loadingMembers && members.length === 0" class="no-members">
        <div class="empty-state">
          <div class="empty-icon">👥</div>
          <h3>暂无成员数据</h3>
          <p>当前没有符合条件的社区成员</p>
        </div>
      </div>

      <!-- 分页 -->
      <div v-if="!loadingMembers && members.length > 0 && membersPagination.totalPages > 1" class="pagination">
        <button 
          class="pagination-btn" 
          :disabled="membersPagination.page === 1" 
          @click="changeMembersPage(membersPagination.page - 1)"
        >
          上一页
        </button>
        <span class="pagination-info">
          第 {{ membersPagination.page }} 页 / 共 {{ membersPagination.totalPages }} 页
        </span>
        <button 
          class="pagination-btn" 
          :disabled="membersPagination.page >= membersPagination.totalPages" 
          @click="changeMembersPage(membersPagination.page + 1)"
        >
          下一页
        </button>
      </div>

      <!-- 成员统计 -->
      <div class="members-stats" v-if="memberDistribution.length > 0">
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

    <!-- 需求审核页面 -->
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
                @click="setReviewFilter('pending')"
              >
                待审核 ({{ pendingRequirements.length }})
              </button>
              <button 
                class="filter-btn" 
                :class="{ active: reviewFilter === 'approved' }"
                @click="setReviewFilter('approved')"
              >
                已通过
              </button>
              <button 
                class="filter-btn" 
                :class="{ active: reviewFilter === 'rejected' }"
                @click="setReviewFilter('rejected')"
              >
                已拒绝
              </button>
            </div>
            
            <div class="filter-select-group">
              <select v-model="typeFilter" class="filter-select" @change="filterRequirements">
                <option value="all">所有类型</option>
                <option value="walk">遛狗服务</option>
                <option value="feed">喂食照顾</option>
                <option value="medical">就医陪伴</option>
                <option value="groom">美容护理</option>
              </select>
            </div>
          </div>
        </div>

        <!-- 加载状态 -->
        <div v-if="loadingRequirements" class="loading-state">
          <div class="loading-spinner"></div>
          <p>正在加载需求数据...</p>
        </div>

        <!-- 审核列表 -->
        <div class="review-list" v-if="!loadingRequirements">
          <!-- 待审核需求 -->
          <div v-if="reviewFilter === 'pending' && filteredRequirements.length > 0" class="pending-reviews">
            <div class="requirements-list">
              <div 
                v-for="requirement in filteredRequirements" 
                :key="requirement.id"
                class="requirement-review-item"
              >
                <div class="requirement-header">
                  <div class="requirement-type-badge" :style="{ backgroundColor: getTypeColor(requirement.serviceType) }">
                    {{ getTypeName(requirement.serviceType) }}
                    <span v-if="requirement.isUrgent" class="urgent-indicator">❗</span>
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
                      <h4>{{ requirement.title || '未命名需求' }}</h4>
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
                        <span class="detail-value">{{ requirement.user?.nickName || requirement.user?.name || '匿名用户' }}</span>
                        <span v-if="requirement.user?.level" class="member-level">Lv.{{ requirement.user.level }}</span>
                      </div>
                      
                      <div class="detail-item">
                        <span class="detail-icon">📅</span>
                        <span class="detail-label">发布时间：</span>
                        <span class="detail-value">{{ formatDate(requirement.createdAt) }}</span>
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
                      :disabled="processingRequirement === requirement.id"
                    >
                      <span v-if="processingRequirement === requirement.id">处理中...</span>
                      <span v-else>✅ 通过审核</span>
                    </button>
                    
                    <button 
                      @click="toggleRejectionInput(requirement)"
                      class="action-btn reject-btn"
                      :disabled="processingRequirement === requirement.id"
                    >
                      {{ showRejectionInput === requirement.id ? '取消拒绝' : '❌ 拒绝发布' }}
                    </button>
                    
                    <button 
                      v-if="showRejectionInput === requirement.id"
                      @click="rejectRequirement(requirement)"
                      class="action-btn confirm-reject-btn"
                      :disabled="!rejectionReason.trim() || processingRequirement === requirement.id"
                    >
                      <span v-if="processingRequirement === requirement.id">处理中...</span>
                      <span v-else>确认拒绝</span>
                    </button>
                    
                    <button 
                      @click="viewPublisherProfile(requirement)"
                      class="action-btn view-btn"
                      :disabled="processingRequirement === requirement.id"
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
                  <div class="requirement-type-badge" :style="{ backgroundColor: getTypeColor(requirement.serviceType) }">
                    {{ getTypeName(requirement.serviceType) }}
                  </div>
                  
                  <div class="requirement-status" :class="requirement.status">
                    {{ requirement.status === 'Approved' ? '✅ 已通过' : '❌ 已拒绝' }}
                  </div>
                </div>
                
                <div class="requirement-content">
                  <div class="pet-info-section">
                    <div class="pet-avatar-small">{{ getPetEmoji(requirement.petType) }}</div>
                    <div class="pet-details">
                      <h4>{{ requirement.title || '未命名需求' }}</h4>
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
                        <span>{{ requirement.user?.nickName || requirement.user?.name || '匿名用户' }}</span>
                      </div>
                    </div>
                    
                    <!-- 审核信息 -->
                    <div class="review-info" v-if="requirement.reviewer">
                      <div class="reviewer-info">
                        <span class="reviewer-label">审核人：</span>
                        <span class="reviewer-name">{{ requirement.reviewerName || '管理员' }}</span>
                        <span class="review-time">{{ formatDate(requirement.reviewedTime) }}</span>
                      </div>
                      
                      <div class="rejection-reason" v-if="requirement.rejectionReason && requirement.status === 'Rejected'">
                        <span class="reason-label">拒绝原因：</span>
                        <span class="reason-text">{{ requirement.rejectionReason }}</span>
                      </div>
                    </div>
                  </div>
                </div>
                
                <div class="review-actions-section">
                  <div class="action-buttons">
                    <button 
                      v-if="requirement.status === 'Rejected'"
                      @click="reApproveRequirement(requirement)"
                      class="action-btn approve-btn"
                      :disabled="processingRequirement === requirement.id"
                    >
                      <span v-if="processingRequirement === requirement.id">处理中...</span>
                      <span v-else>🔄 重新审核</span>
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
                      :disabled="processingRequirement === requirement.id"
                    >
                      <span v-if="processingRequirement === requirement.id">处理中...</span>
                      <span v-else>🗑️ 删除记录</span>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- 空状态 -->
          <div v-if="filteredRequirements.length === 0 && !loadingRequirements" class="no-reviews">
            <div class="empty-state">
              <div class="empty-icon" v-if="reviewFilter === 'pending'">🎉</div>
              <div class="empty-icon" v-if="reviewFilter === 'approved'">📄</div>
              <div class="empty-icon" v-if="reviewFilter === 'rejected'">📝</div>
              <h3>{{ getEmptyStateTitle() }}</h3>
              <p>{{ getEmptyStateMessage() }}</p>
            </div>
          </div>
        </div>

        <!-- 分页 -->
        <div v-if="!loadingRequirements && filteredRequirements.length > 0 && requirementsPagination.totalPages > 1" class="pagination">
          <button 
            class="pagination-btn" 
            :disabled="requirementsPagination.page === 1" 
            @click="changeRequirementsPage(requirementsPagination.page - 1)"
          >
            上一页
          </button>
          <span class="pagination-info">
            第 {{ requirementsPagination.page }} 页 / 共 {{ requirementsPagination.totalPages }} 页
          </span>
          <button 
            class="pagination-btn" 
            :disabled="requirementsPagination.page >= requirementsPagination.totalPages" 
            @click="changeRequirementsPage(requirementsPagination.page + 1)"
          >
            下一页
          </button>
        </div>
      </div>
    </div>

    <!-- 社区设置页面 -->
    <div class="tab-content" v-if="activeTab === 'settings'">
      <div class="settings-container">
        <div class="settings-form">
          <h3>社区设置</h3>
          
          <!-- 加载状态 -->
          <div v-if="loadingSettings" class="loading-state">
            <div class="loading-spinner"></div>
            <p>正在加载社区设置...</p>
          </div>

          <div v-if="!loadingSettings">
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

            <div class="setting-section">
              <h4>审核设置</h4>
              <div class="form-group">
                <label class="checkbox-label">
                  <input type="checkbox" v-model="communitySettings.requireApproval">
                  需求发布需要审核
                </label>
                <p class="setting-description">开启后，所有新发布的需求都需要管理员审核才能显示</p>
              </div>
              <div class="form-group">
                <label class="checkbox-label">
                  <input type="checkbox" v-model="communitySettings.autoFlagSensitive">
                  自动标记敏感内容
                </label>
                <p class="setting-description">开启后，系统会自动检测并标记可能敏感的内容</p>
              </div>
              <div class="form-group">
                <label>紧急需求审核时间（小时）</label>
                <input type="number" v-model="communitySettings.urgentReviewTime" class="form-input" min="1" max="24">
                <p class="setting-description">标记为紧急的需求需要在此时间内完成审核</p>
              </div>
            </div>

            <div class="setting-section">
              <h4>拒绝模板</h4>
              <div class="form-group">
                <label>预设拒绝原因（每行一个）</label>
                <textarea v-model="communitySettings.rejectTemplates" rows="6" class="form-textarea"></textarea>
                <p class="setting-description">审核拒绝时可选择的预设原因，每行一个</p>
              </div>
            </div>
            
            <div class="setting-actions">
              <button class="btn-secondary" @click="resetSettings" :disabled="savingSettings">
                恢复默认
              </button>
              <button class="btn-primary" @click="saveSettings" :disabled="savingSettings">
                <span v-if="savingSettings">保存中...</span>
                <span v-else>保存设置</span>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

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
                <input v-model="editingRequirement.isUrgent" type="checkbox">
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
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { adminAPI } from '@/utils/admin.js'

// 激活的标签页
const activeTab = ref('members')

// 加载状态
const loadingMembers = ref(false)
const loadingRequirements = ref(false)
const loadingStats = ref(false)
const loadingSettings = ref(false)
const savingSettings = ref(false)
const processingRequirement = ref(null)

// 社区统计
const communityStats = ref({
  totalMembers: 0,
  petusers: 0,
  serviceProviders: 0,
  pendingRequests: 0
})

// 搜索和筛选
const searchQuery = ref('')
const memberFilter = ref('all')

// 成员数据
const members = ref([])
const membersPagination = ref({
  page: 1,
  pageSize: 12,
  totalCount: 0,
  totalPages: 0
})

// 需求审核相关数据
const reviewFilter = ref('pending')
const typeFilter = ref('all')

// 需求数据
const pendingRequirements = ref([])
const approvedRequirements = ref([])
const rejectedRequirements = ref([])
const requirementsPagination = ref({
  page: 1,
  pageSize: 10,
  totalCount: 0,
  totalPages: 0
})

// 社区设置
const communitySettings = ref({
  name: '',
  description: '',
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

// 成员分布和活跃度数据
const memberDistribution = ref([])
const activityData = ref([])

// 计算属性
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
    requirements = requirements.filter(req => req.serviceType === typeFilter.value)
  }
  
  return requirements
})

// 监听标签页切换
watch(activeTab, (newTab) => {
  if (newTab === 'members') {
    loadCommunityStats()
    loadMemberDistribution()
    loadActivityTrend()
    loadMembers()
  } else if (newTab === 'content') {
    loadRequirements()
  } else if (newTab === 'settings') {
    loadCommunitySettings()
  }
})

onMounted(() => {
  console.log('组件挂载，检查权限状态...')
  
  // 调试：查看当前用户信息
  const userRole = localStorage.getItem('petpal_userRole')
  const userId = localStorage.getItem('petpal_userId')
  const token = localStorage.getItem('auth_token')
  
  console.log('用户调试信息:', {
    userId,
    userRole,
    token: token ? '存在' : '不存在'
  })
  
  // 验证管理员权限
  verifyAdminPermission()
  loadInitialData()
})

const verifyAdminPermission = async () => {
  const result = await adminAPI.verifyAdminPermission()
  if (!result.success) {
    adminAPI.showError(result.message)
    // 可以跳转到无权限页面或首页
  }
}

const loadInitialData = () => {
  loadCommunityStats()
  loadMemberDistribution()
  loadActivityTrend()
  loadMembers()
}

// API调用方法 - 使用adminAPI
const loadCommunityStats = async () => {
  try {
    loadingStats.value = true
    const response = await adminAPI.getCommunityStats()
    if (response.success && response.data) {
      communityStats.value = {
        totalMembers: response.data.totalMembers || 0,
        petusers: response.data.petusers || 0,
        serviceProviders: response.data.serviceProviders || 0,
        pendingRequests: response.data.pendingRequests || 0
      }
    } else {
      adminAPI.showError(response.message || '加载社区统计失败')
    }
  } catch (error) {
    adminAPI.handleError(error, '加载社区统计')
  } finally {
    loadingStats.value = false
  }
}

const loadMemberDistribution = async () => {
  try {
    const response = await adminAPI.getMemberDistribution()
    if (response.success && response.data) {
      memberDistribution.value = response.data
    }
  } catch (error) {
    console.error('加载成员分布失败:', error)
    // 使用默认数据
    memberDistribution.value = [
      { type: '宠物主人', count: 0, percentage: 50 },
      { type: '服务提供者', count: 0, percentage: 50 }
    ]
  }
}

const loadActivityTrend = async () => {
  try {
    const response = await adminAPI.getActivityTrend(7)
    if (response.success && response.data) {
      activityData.value = response.data
    }
  } catch (error) {
    console.error('加载活跃度趋势失败:', error)
    // 使用默认数据
    activityData.value = [80, 65, 75, 90, 85, 70, 95]
  }
}

const loadMembers = async () => {
  try {
    loadingMembers.value = true
    
    // 构建筛选条件
    const filters = {
      page: membersPagination.value.page,
      pageSize: membersPagination.value.pageSize
    }
    
    // 角色筛选
    if (memberFilter.value === 'User' || memberFilter.value === 'Sitter') {
      filters.role = memberFilter.value
    }
    
    // 审核状态筛选
    if (['Pending', 'Approved', 'Rejected'].includes(memberFilter.value)) {
      filters.auditStatus = memberFilter.value
    }
    
    let response
    
    if (searchQuery.value.trim()) {
      response = await adminAPI.searchMembers(searchQuery.value.trim(), filters)
    } else {
      response = await adminAPI.getCommunityMembers(filters)
    }
    
    if (response.success && response.data) {
      const data = response.data
      members.value = data.members || []
      membersPagination.value = {
        page: data.page || 1,
        pageSize: data.pageSize || 12,
        totalCount: data.totalCount || 0,
        totalPages: Math.ceil((data.totalCount || 0) / (data.pageSize || 12))
      }
    } else {
      adminAPI.showError(response.message || '加载成员列表失败')
    }
  } catch (error) {
    adminAPI.handleError(error, '加载成员列表')
    members.value = []
  } finally {
    loadingMembers.value = false
  }
}

const loadRequirements = async () => {
  try {
    loadingRequirements.value = true
    
    const filters = {
      page: requirementsPagination.value.page,
      pageSize: requirementsPagination.value.pageSize,
      status: reviewFilter.value,
      serviceType: typeFilter.value === 'all' ? null : typeFilter.value
    }
    
    const response = await adminAPI.getReviewList(filters)
    
    if (response.success && response.data) {
      const data = response.data
      const requests = data.requests || []
      
      // 根据筛选条件存储到不同的列表
      if (reviewFilter.value === 'pending') {
        pendingRequirements.value = requests
      } else if (reviewFilter.value === 'approved') {
        approvedRequirements.value = requests
      } else if (reviewFilter.value === 'rejected') {
        rejectedRequirements.value = requests
      }
      
      requirementsPagination.value = {
        page: data.page || 1,
        pageSize: data.pageSize || 10,
        totalCount: data.totalCount || 0,
        totalPages: Math.ceil((data.totalCount || 0) / (data.pageSize || 10))
      }
    } else {
      adminAPI.showError(response.message || '加载需求列表失败')
    }
  } catch (error) {
    adminAPI.handleError(error, '加载需求列表')
    pendingRequirements.value = []
    approvedRequirements.value = []
    rejectedRequirements.value = []
  } finally {
    loadingRequirements.value = false
  }
}

const loadCommunitySettings = async () => {
  try {
    loadingSettings.value = true
    const response = await adminAPI.getCommunitySettings()
    if (response.success && response.data) {
      communitySettings.value = response.data
    } else {
      adminAPI.showError(response.message || '加载社区设置失败')
    }
  } catch (error) {
    adminAPI.handleError(error, '加载社区设置')
  } finally {
    loadingSettings.value = false
  }
}

// 成员相关方法
const searchMembers = () => {
  membersPagination.value.page = 1
  loadMembers()
}

const filterMembers = () => {
  membersPagination.value.page = 1
  loadMembers()
}

const filterRequirements = () => {
  requirementsPagination.value.page = 1
  loadRequirements()
}

const setReviewFilter = (filter) => {
  reviewFilter.value = filter
  requirementsPagination.value.page = 1
  loadRequirements()
}

const changeMembersPage = (page) => {
  membersPagination.value.page = page
  loadMembers()
}

const changeRequirementsPage = (page) => {
  requirementsPagination.value.page = page
  loadRequirements()
}

// 辅助方法 - 使用adminAPI的工具方法
const getAvatarEmoji = (name) => {
  if (!name) return '👤'
  const emojis = ['😊', '😄', '😃', '😀', '😁', '😂', '🤣', '😅', '😆', '😉', '😋', '😎', '😍', '😘']
  const index = name.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0)
  return emojis[index % emojis.length]
}

const getRoleClass = (role) => {
  return role === 'user' ? 'petuser' : 'serviceProvider'
}

const getAuditStatusClass = (status) => {
  const statusMap = {
    'Pending': 'pending',
    'Approved': 'approved',
    'Rejected': 'rejected'
  }
  return statusMap[status] || 'pending'
}

const getAuditStatusText = (status) => {
  const textMap = {
    'Pending': '待审核',
    'Approved': '已认证',
    'Rejected': '未通过'
  }
  return textMap[status] || '待审核'
}

const getLevelClass = (level) => {
  if (level >= 4) return 'level-high'
  if (level >= 3) return 'level-medium'
  return 'level-low'
}

// 宠物相关方法 - 使用adminAPI的工具方法
const getPetEmoji = adminAPI.getPetEmoji
const getPetTypeName = adminAPI.getPetTypeName
const getTypeColor = adminAPI.getTypeColor
const getTypeName = adminAPI.getTypeName

// 时间格式化 - 使用adminAPI的工具方法
const formatTime = adminAPI.formatTime
const formatDate = adminAPI.formatDate

// ===== 成员管理方法 =====
const updateUserRole = async (member) => {
  try {
    const response = await adminAPI.changeMemberRole(member.id, member.role)
    
    if (response.success) {
      adminAPI.showSuccess('成员角色修改成功')
      loadMembers()
      loadCommunityStats()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '修改成员角色')
  }
}

const approveQualification = async (member) => {
  try {
    const response = await adminAPI.approveQualification(member.id)
    
    if (response.success) {
      adminAPI.showSuccess('资质审核通过')
      loadMembers()
      loadCommunityStats()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '审核通过')
  }
}

const viewQualification = (member) => {
  alert(`查看资质信息：${member.sitterCertifications || '暂无资质信息'}`)
}

const viewRejectReason = (member) => {
  alert(`审核未通过原因：\n${member.rejectReason || '未提供具体原因'}`)
}

const showRejectDialog = (member) => {
  selectedMember.value = member
  modalType.value = 'rejectReview'
  modalTitle.value = '拒绝资质审核'
  modalConfirmText.value = '确认拒绝'
  showModal.value = true
}

const showReReviewDialog = (member) => {
  selectedMember.value = member
  modalType.value = 'reReview'
  modalTitle.value = '重新审核资质'
  modalConfirmText.value = '开始重新审核'
  showModal.value = true
}

const allowResubmit = async (member) => {
  try {
    const response = await adminAPI.allowResubmitQualification(member.id)
    
    if (response.success) {
      adminAPI.showSuccess('允许重新提交审核')
      loadMembers()
      loadCommunityStats()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '允许重审')
  }
}

// ===== 需求审核方法 =====
const approveRequirement = async (requirement) => {
  try {
    processingRequirement.value = requirement.id
    
    const response = await adminAPI.approveRequest(requirement.id)
    
    if (response.success) {
      adminAPI.showSuccess('需求审核通过')
      loadRequirements()
      loadCommunityStats()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '审核通过')
  } finally {
    processingRequirement.value = null
  }
}

const toggleRejectionInput = (requirement) => {
  if (showRejectionInput.value === requirement.id) {
    showRejectionInput.value = null
    rejectionReason.value = ''
  } else {
    showRejectionInput.value = requirement.id
    rejectionReason.value = ''
  }
}

const rejectRequirement = async (requirement) => {
  try {
    if (!rejectionReason.value.trim()) {
      adminAPI.showError('请填写拒绝原因')
      return
    }
    
    processingRequirement.value = requirement.id
    
    const response = await adminAPI.rejectRequest(requirement.id, rejectionReason.value)
    
    if (response.success) {
      adminAPI.showSuccess('需求已拒绝')
      showRejectionInput.value = null
      rejectionReason.value = ''
      loadRequirements()
      loadCommunityStats()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '审核拒绝')
  } finally {
    processingRequirement.value = null
  }
}

const reApproveRequirement = async (requirement) => {
  try {
    processingRequirement.value = requirement.id
    
    const response = await adminAPI.recheckRequest(requirement.id)
    
    if (response.success) {
      adminAPI.showSuccess('需求已重新提交审核')
      loadRequirements()
      loadCommunityStats()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '重新审核')
  } finally {
    processingRequirement.value = null
  }
}

const viewPublisherProfile = (requirement) => {
  if (requirement.user) {
    alert(`用户信息：
名称：${requirement.user.nickName || requirement.user.name}
等级：Lv.${requirement.user.level || 1}
信誉分：${requirement.user.creditScore || 100}`)
  } else {
    alert('用户信息不可用')
  }
}

const viewRequirementDetails = async (requirement) => {
  try {
    const response = await adminAPI.getReviewDetail(requirement.id)
    if (response.success && response.data) {
      const detail = response.data
      let details = `需求详情：\n`
      details += `类型：${getTypeName(detail.serviceType)}\n`
      details += `宠物：${getPetTypeName(detail.petType)}\n`
      details += `标题：${detail.title}\n`
      details += `描述：${detail.description}\n`
      details += `时间：${formatTime(detail.startTime)} - ${formatTime(detail.endTime)}\n`
      details += `地点：${detail.location || '未提供地址'}\n`
      details += `发布者：${detail.user?.nickName || detail.user?.name || '匿名用户'}\n`
      details += `状态：${detail.status === 'Approved' ? '已通过' : '已拒绝'}\n`
      
      if (detail.reviewer) {
        details += `审核人：${detail.reviewerName || detail.reviewer}\n`
        details += `审核时间：${formatDate(detail.reviewedTime)}\n`
      }
      
      if (detail.rejectionReason) {
        details += `拒绝原因：${detail.rejectionReason}\n`
      }
      
      alert(details)
    } else {
      adminAPI.showError(response.message || '获取详情失败')
    }
  } catch (error) {
    adminAPI.handleError(error, '获取需求详情')
  }
}

// ===== 成员移除方法 =====
const showRemoveDialog = (member) => {
  selectedMember.value = member
  modalType.value = 'remove'
  modalTitle.value = '移除成员'
  modalConfirmText.value = '移除'
  showModal.value = true
}

// ===== 审核记录删除方法 =====
const deleteReviewRecord = (requirement) => {
  selectedRequirement.value = requirement
  modalType.value = 'deleteRequirement'
  modalTitle.value = '删除审核记录'
  modalConfirmText.value = '确认删除'
  showModal.value = true
}

// ===== 空状态文本 =====
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

// ===== 社区设置方法 =====
const resetSettings = async () => {
  try {
    const response = await adminAPI.resetCommunitySettings()
    if (response.success) {
      adminAPI.showSuccess('设置已恢复为默认值')
      loadCommunitySettings()
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '重置设置')
  }
}

const saveSettings = async () => {
  try {
    savingSettings.value = true
    const response = await adminAPI.updateCommunitySettings(communitySettings.value)
    if (response.success) {
      adminAPI.showSuccess('设置已保存！')
    } else {
      adminAPI.showError(response.message)
    }
  } catch (error) {
    adminAPI.handleError(error, '保存设置')
  } finally {
    savingSettings.value = false
  }
}

// ===== 模态框方法 =====
const closeModal = () => {
  showModal.value = false
  selectedMember.value = null
  selectedRequirement.value = null
  editingRequirement.value = null
}

const confirmModal = async () => {
  try {
    switch (modalType.value) {
      case 'remove':
        if (selectedMember.value) {
          const confirmed = await adminAPI.confirmDialog(`确定要移除成员 ${selectedMember.value.nickName || selectedMember.value.username} 吗？`)
          if (!confirmed) return
          
          const response = await adminAPI.removeMember(selectedMember.value.id)
          
          if (response.success) {
            adminAPI.showSuccess('成员移除成功')
            closeModal()
            loadMembers()
            loadCommunityStats()
          } else {
            adminAPI.showError(response.message)
          }
        }
        break
        
      case 'deleteRequirement':
        if (selectedRequirement.value) {
          const confirmed = await adminAPI.confirmDialog('确定要删除这条审核记录吗？此操作不可撤销！')
          if (!confirmed) return
          
          const response = await adminAPI.deleteReviewRecord(selectedRequirement.value.id)
          
          if (response.success) {
            adminAPI.showSuccess('审核记录删除成功')
            closeModal()
            loadRequirements()
          } else {
            adminAPI.showError(response.message)
          }
        }
        break
        
      case 'rejectReview':
        if (selectedMember.value) {
          const reason = prompt('请输入拒绝原因：', '资质不符合要求')
          if (reason && selectedMember.value) {
            const response = await adminAPI.rejectQualification(selectedMember.value.id, reason)
            
            if (response.success) {
              adminAPI.showSuccess('审核拒绝成功')
              closeModal()
              loadMembers()
              loadCommunityStats()
            } else {
              adminAPI.showError(response.message)
            }
          }
        }
        break
        
      case 'reReview':
        if (selectedMember.value) {
          const confirmed = await adminAPI.confirmDialog(`确定要对 ${selectedMember.value.nickName || selectedMember.value.name} 进行重新审核吗？`)
          if (!confirmed) return
          
          const response = await adminAPI.reReviewQualification(selectedMember.value.id)
          
          if (response.success) {
            adminAPI.showSuccess('重新审核开始')
            closeModal()
            loadMembers()
            loadCommunityStats()
          } else {
            adminAPI.showError(response.message)
          }
        }
        break
        
      case 'editRequirement':
        if (editingRequirement.value && selectedRequirement.value) {
          const response = await adminAPI.editRequest(selectedRequirement.value.id, editingRequirement.value)
          
          if (response.success) {
            adminAPI.showSuccess('需求已更新')
            closeModal()
            loadRequirements()
          } else {
            adminAPI.showError(response.message)
          }
        }
        break
    }
  } catch (error) {
    adminAPI.handleError(error, '确认操作')
  }
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

/* 加载状态 */
.loading-state {
  text-align: center;
  padding: 60px 20px;
  color: #64748b;
}

.loading-spinner {
  display: inline-block;
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #166534;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 20px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

/* 分页 */
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 20px;
  margin-top: 30px;
  padding: 20px;
}

.pagination-btn {
  padding: 8px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: white;
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

/* 空状态 */
.no-members {
  text-align: center;
  padding: 60px 40px;
  background: white;
  border-radius: 16px;
  border: 2px dashed #e2e8f0;
  margin: 20px 0;
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

.approve-btn:hover:not(:disabled) {
  background: #a7f3d0;
  transform: translateY(-1px);
}

.approve-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.reject-btn {
  background: #fee2e2;
  color: #991b1b;
}

.reject-btn:hover:not(:disabled) {
  background: #fecaca;
  transform: translateY(-1px);
}

.reject-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

.edit-btn:hover:not(:disabled) {
  background: #dbeafe;
  transform: translateY(-1px);
}

.view-btn {
  background: #f8fafc;
  color: #64748b;
  border: 1px solid #e2e8f0;
}

.view-btn:hover:not(:disabled) {
  background: #e2e8f0;
}

.view-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.delete-btn {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.delete-btn:hover:not(:disabled) {
  background: #fecaca;
}

.delete-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

/* ===== 成员管理页面样式 ===== */
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

.user-type-badge.petuser {
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

.member-points {
  font-size: 14px;
  color: #166534;
  font-weight: 600;
  background: #d1fae5;
  padding: 4px 12px;
  border-radius: 12px;
  display: inline-block;
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

.role-dropdown {
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  font-size: 14px;
  color: #475569;
  background: white;
}

.role-dropdown:focus {
  outline: none;
  border-color: #166534;
}

.remove-btn {
  background: #fee2e2;
  color: #991b1b;
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.remove-btn:hover {
  background: #fecaca;
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

.btn-secondary:hover:not(:disabled) {
  background: #f8fafc;
  border-color: #cbd5e1;
}

.btn-secondary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #166534;
  color: white;
  border: none;
}

.btn-primary:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

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