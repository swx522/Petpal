<!-- src/views/Profile.vue -->
<template>
  <div class="profile-container">
    <!-- 页面标题 -->
    <div class="profile-header">
      <h1 class="page-title">个人主页</h1>
      <p class="page-subtitle">管理您的个人信息和账户设置</p>
    </div>

    <div class="profile-content">
      <!-- 左侧：个人信息卡片 -->
      <div class="profile-sidebar">
        <div class="user-card">
          <div class="user-avatar-large">
            {{ userInitials }}
          </div>
          <div class="user-basic-info">
            <h3 class="user-name">{{ userInfo.name || '用户' }}</h3>
            <p class="user-role">{{ roleText }}</p>
            <p class="user-join-date" v-if="userInfo.joinDate">注册时间: {{ formatDate(userInfo.joinDate) }}</p>
            <p class="user-join-date" v-else>注册时间: 加载中...</p>
          </div>
        </div>

        <!-- 社区信息 -->
        <div class="community-card">
          <h4 class="card-title">
            <span class="card-icon">🏘️</span> 我的社区
          </h4>
          <div class="community-info">
            <div class="community-item">
              <span class="community-label">社区名称</span>
              <span class="community-value">{{ communityInfo.name || '未加入社区' }}</span>
            </div>
            <div class="community-item">
              <span class="community-label">社区地址</span>
              <span class="community-value">{{ communityInfo.address || '--' }}</span>
            </div>
            <div class="community-item">
              <span class="community-label">成员数量</span>
              <span class="community-value">{{ communityInfo.memberCount || '--' }}人</span>
            </div>
          </div>
          <button class="view-community-btn" @click="viewCommunity" :disabled="!communityInfo.name">
            查看社区详情
          </button>
        </div>
      </div>

      <!-- 右侧：信息编辑区域 -->
      <div class="profile-main">
        <!-- 个人信息编辑 -->
        <div class="info-card">
          <div class="card-header">
            <h3 class="card-title">
              <span class="card-icon">👤</span> 个人信息
            </h3>
            <button 
              class="edit-btn" 
              @click="toggleEditMode('personal')"
              :class="{ 'editing': editingPersonal }"
              :disabled="loading"
            >
              {{ editingPersonal ? '保存修改' : '编辑信息' }}
            </button>
          </div>
          
          <div class="info-form">
            <div class="form-group">
              <label class="form-label">用户名</label>
              <input 
                type="text" 
                class="form-input" 
                :class="{ 'editing': editingPersonal }"
                v-model="userInfo.name"
                :disabled="!editingPersonal || loading"
                placeholder="请输入用户名"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">邮箱地址</label>
              <input 
                type="email" 
                class="form-input" 
                :class="{ 'editing': editingPersonal }"
                v-model="userInfo.email"
                :disabled="!editingPersonal || loading"
                placeholder="请输入邮箱地址"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">手机号码</label>
              <input 
                type="tel" 
                class="form-input" 
                :class="{ 'editing': editingPersonal }"
                v-model="userInfo.phone"
                :disabled="!editingPersonal || loading"
                placeholder="请输入手机号码"
              />
            </div>
            
            <div class="form-group">
              <label class="form-label">角色</label>
              <div class="role-display">
                <span class="role-badge">{{ roleText }}</span>
                <span class="role-hint">（角色不可更改）</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 修改密码 -->
        <div class="info-card">
          <div class="card-header">
            <h3 class="card-title">
              <span class="card-icon">🔒</span> 密码安全
            </h3>
            <button 
              class="edit-btn" 
              @click="toggleEditMode('password')"
              :class="{ 'editing': editingPassword }"
              :disabled="loading"
            >
              {{ editingPassword ? '取消修改' : '修改密码' }}
            </button>
          </div>
          
          <div class="password-form" v-if="editingPassword">
            <div class="form-group">
              <label class="form-label">当前密码</label>
              <div class="password-input-group">
                <input 
                  :type="showOldPassword ? 'text' : 'password'"
                  class="form-input"
                  v-model="passwordInfo.oldPassword"
                  :disabled="loading"
                  placeholder="请输入当前密码"
                />
                <button 
                  class="toggle-password-btn"
                  @click="showOldPassword = !showOldPassword"
                  type="button"
                  :disabled="loading"
                >
                  {{ showOldPassword ? '🙈' : '👁️' }}
                </button>
              </div>
              <p v-if="passwordError" class="error-message">
                {{ passwordError }}
              </p>
            </div>
            
            <div class="form-group">
              <label class="form-label">新密码</label>
              <div class="password-input-group">
                <input 
                  :type="showNewPassword ? 'text' : 'password'"
                  class="form-input"
                  v-model="passwordInfo.newPassword"
                  :disabled="loading"
                  placeholder="请输入新密码（至少6位）"
                  @input="validatePassword"
                />
                <button 
                  class="toggle-password-btn"
                  @click="showNewPassword = !showNewPassword"
                  type="button"
                  :disabled="loading"
                >
                  {{ showNewPassword ? '🙈' : '👁️' }}
                </button>
              </div>
              <!-- 移除密码强度条，改为简单提示 -->
              <p v-if="passwordInfo.newPassword.length > 0 && passwordInfo.newPassword.length < 6" 
                class="error-message">
                ❌ 密码至少需要6位
              </p>
              <p v-else-if="passwordInfo.newPassword.length >= 6" 
                class="success-message">
                ✅ 密码长度符合要求
              </p>
            </div>
            
            <div class="form-group">
              <label class="form-label">确认新密码</label>
              <div class="password-input-group">
                <input 
                  :type="showConfirmPassword ? 'text' : 'password'"
                  class="form-input"
                  v-model="passwordInfo.confirmPassword"
                  :disabled="loading"
                  placeholder="请再次输入新密码"
                />
                <button 
                  class="toggle-password-btn"
                  @click="showConfirmPassword = !showConfirmPassword"
                  type="button"
                  :disabled="loading"
                >
                  {{ showConfirmPassword ? '🙈' : '👁️' }}
                </button>
              </div>
              <p v-if="passwordInfo.newPassword !== passwordInfo.confirmPassword && passwordInfo.confirmPassword" 
                class="error-message">
                ❌ 两次输入的密码不一致
              </p>
              <p v-else-if="passwordInfo.newPassword === passwordInfo.confirmPassword && passwordInfo.confirmPassword" 
                class="success-message">
                ✅ 密码匹配
              </p>
            </div>
            
            <div class="form-actions">
              <button class="btn-secondary" @click="cancelPasswordChange" :disabled="loading">
                取消
              </button>
              <button 
                class="btn-primary" 
                @click="changePassword"
                :disabled="!isPasswordFormValid || loading"
              >
                {{ loading ? '处理中...' : '确认修改' }}
              </button>
            </div>
          </div>
          <div v-else class="password-security-tips">
            <p class="security-tip">🔐 为了您的账户安全，建议定期更换密码</p>
            <p class="security-tip">💡 密码至少需要6位字符</p>
          </div>
        </div>

        <!-- 账户操作 -->
        <div class="info-card">
          <div class="card-header">
            <h3 class="card-title">
              <span class="card-icon">⚙️</span> 账户操作
            </h3>
          </div>
          
          <div class="account-actions">
            <button class="action-btn logout-btn" @click="handleLogout" :disabled="loading">
              <span class="btn-icon">🚪</span> 退出登录
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 删除账户确认对话框 -->
    <div class="modal-overlay" v-if="showDeleteModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>确认删除账户</h3>
          <button class="close-btn" @click="showDeleteModal = false" :disabled="deleting">×</button>
        </div>
        <div class="modal-body">
          <div class="warning-message">
            <span class="warning-icon-big">⚠️</span>
            <h4>此操作不可撤销！</h4>
            <p>删除账户将会：</p>
            <ul class="delete-consequences">
              <li>永久删除您的所有个人信息</li>
              <li>清除您的发布需求和接单记录</li>
              <li>移除您的社区成员身份</li>
              <li>不可恢复所有数据</li>
            </ul>
            <div class="confirm-input">
              <input 
                type="text" 
                v-model="deleteConfirmation"
                :disabled="deleting"
                placeholder="请输入'确认删除'以继续"
                class="confirm-input-field"
              />
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button class="btn-secondary" @click="showDeleteModal = false" :disabled="deleting">
            取消
          </button>
          <button 
            class="btn-danger" 
            @click="deleteAccount"
            :disabled="deleteConfirmation !== '确认删除' || deleting"
          >
            {{ deleting ? '删除中...' : '确认删除账户' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { userAPI } from '@/utils/user.js'

const router = useRouter()

// 用户信息
const userInfo = ref({
  name: '',
  email: '',
  phone: '',
  joinDate: ''
})

// 社区信息
const communityInfo = ref({
  name: '',
  address: '',
  memberCount: 0
})

// 编辑状态
const editingPersonal = ref(false)
const editingPassword = ref(false)

// 密码相关
const passwordInfo = ref({
  oldPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const showOldPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)
const passwordError = ref('')

// 删除账户相关
const showDeleteModal = ref(false)
const deleteConfirmation = ref('')

// 加载状态
const loading = ref(false)
const deleting = ref(false)

// 从本地存储获取角色信息
const userRole = ref(userAPI.getUserRole())

// 计算属性
const userInitials = computed(() => {
  const name = userInfo.value.name || '用户'
  return name.substring(0, 2)
})

const roleText = computed(() => {
  const roleMap = {
    'owner': '宠物主人',
    'sitter': '服务者',
    'moderator': '管理者'
  }
  return roleMap[userRole.value] || '用户'
})

const isPasswordFormValid = computed(() => {
  return (
    passwordInfo.value.oldPassword &&
    passwordInfo.value.newPassword &&
    passwordInfo.value.confirmPassword &&
    passwordInfo.value.newPassword === passwordInfo.value.confirmPassword &&
    passwordInfo.value.newPassword.length >= 6  // 改为至少6位
  )
})

// 方法
const toggleEditMode = async (type) => {
  if (type === 'personal') {
    editingPersonal.value = !editingPersonal.value
    if (editingPersonal.value) {
      // 进入编辑模式，备份原始数据
      backupUserInfo()
    } else {
      // 保存修改
      await savePersonalInfo()
    }
  } else if (type === 'password') {
    editingPassword.value = !editingPassword.value
    if (!editingPassword.value) {
      resetPasswordForm()
    }
  }
}

let originalUserInfo = null
const backupUserInfo = () => {
  originalUserInfo = { ...userInfo.value }
}

const savePersonalInfo = async () => {
  loading.value = true
  try {
    const updateData = {
      username: userInfo.value.name,
      email: userInfo.value.email,
      phone: userInfo.value.phone
    }
    
    const response = await userAPI.updateUserInfo(updateData)
    
    console.log('更新API响应:', response)
    
    // 注意：PUT请求返回的是 {success: true, data: null, message: '更新成功'}
    // 所以我们要检查 response.success，而不是 response.data
    if (response.success) {
      // 更新本地存储的用户信息
      userAPI.updateLocalUserInfo(updateData)
      alert('个人信息已更新')
      
      // 刷新页面数据
      await loadUserData()
      editingPersonal.value = false
    } else {
      alert(response.message || '更新失败')
      // 恢复原始数据
      if (originalUserInfo) {
        userInfo.value = { ...originalUserInfo }
      }
    }
  } catch (error) {
    console.error('保存个人信息失败:', error)
    alert('保存失败，请稍后重试')
    // 恢复原始数据
    if (originalUserInfo) {
      userInfo.value = { ...originalUserInfo }
    }
  } finally {
    loading.value = false
  }
}

const resetPasswordForm = () => {
  passwordInfo.value = {
    oldPassword: '',
    newPassword: '',
    confirmPassword: ''
  }
  passwordError.value = ''
  showOldPassword.value = false
  showNewPassword.value = false
  showConfirmPassword.value = false
}

const cancelPasswordChange = () => {
  editingPassword.value = false
  resetPasswordForm()
}

const changePassword = async () => {
  if (!isPasswordFormValid.value) return
  
  loading.value = true
  try {
    const response = await userAPI.changePassword({
      oldPassword: passwordInfo.value.oldPassword,
      newPassword: passwordInfo.value.newPassword
    })
    
    // 注意：检查 response.success
    if (response.success) {
      alert('密码修改成功！')
      editingPassword.value = false
      resetPasswordForm()
    } else {
      passwordError.value = response.message || '密码修改失败'
    }
  } catch (error) {
    console.error('修改密码失败:', error)
    passwordError.value = '当前密码不正确'
  } finally {
    loading.value = false
  }
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  
  try {
    return new Date(dateString).toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    })
  } catch (error) {
    return dateString
  }
}

const viewCommunity = () => {
  // TODO: 需要社区页面路由
  // router.push('/community')
  alert('社区功能开发中...')
}

const loadUserData = async () => {
  loading.value = true
  try {
    const response = await userAPI.getUserInfo()
    
    console.log('加载用户数据响应:', response)
    
    // GET请求：检查 response.success && response.data
    if (response.success && response.data) {
      const apiData = response.data
      
      // 更新用户信息
      userInfo.value = {
        name: apiData.username || '',
        email: apiData.email || '',
        phone: apiData.phone || '',
        joinDate: apiData.createdAt || ''
      }
      
      // 更新角色
      if (apiData.role !== undefined) {
        const roleMap = {
          0: 'owner',    // 宠物主人
          1: 'sitter',   // 服务者
          2: 'moderator' // 管理者
        }
        userRole.value = roleMap[apiData.role] || 'owner'
        localStorage.setItem('petpal_userRole', userRole.value)
      }
      
      // 更新本地存储的用户信息
      userAPI.updateLocalUserInfo({
        username: userInfo.value.name,
        email: userInfo.value.email,
        phone: userInfo.value.phone,
        role: userRole.value,
        createdAt: userInfo.value.joinDate
      })
    }
  } catch (error) {
    console.error('加载用户数据失败:', error)
    // 从本地存储获取
    const savedUser = userAPI.getCurrentUser()
    if (savedUser) {
      userInfo.value = {
        name: savedUser.username || '',
        email: savedUser.email || '',
        phone: savedUser.phone || '',
        joinDate: savedUser.createdAt || ''
      }
    }
  } finally {
    loading.value = false
  }
}

const handleLogout = async () => {
  if (confirm('确定要退出登录吗？')) {
    loading.value = true
    try {
      await userAPI.logout()
      router.push('/login')
    } catch (error) {
      console.error('退出登录失败:', error)
      // 即使API调用失败也清除本地存储
      userAPI.clearLocalStorage()
      router.push('/login')
    } finally {
      loading.value = false
    }
  }
}

const showDeleteConfirm = () => {
  showDeleteModal.value = true
  deleteConfirmation.value = ''
}

const deleteAccount = async () => {
  if (deleteConfirmation.value !== '确认删除') {
    alert('请输入正确的确认文字')
    return
  }
  
  deleting.value = true
  try {
    const response = await userAPI.deleteAccount(deleteConfirmation.value)
    
    // 注意：检查 response.success
    if (response.success) {
      alert('账户已成功删除')
      userAPI.clearLocalStorage()
      router.push('/')
    } else {
      alert(response.message || '删除失败')
    }
  } catch (error) {
    console.error('删除账户失败:', error)
    alert('删除失败，请稍后重试')
  } finally {
    deleting.value = false
    showDeleteModal.value = false
  }
}

// 页面加载时获取用户数据
// 页面加载时获取用户数据
onMounted(async () => {
  loading.value = true
  try {
    // 从API获取用户信息
    const response = await userAPI.getUserInfo()
    
    console.log('API返回的完整数据:', response)
    console.log('API返回的Data字段:', response.data)
    
    // 根据你的调试信息，response.data直接就是用户数据对象
    if (response.data) {
      const apiData = response.data  // 直接使用response.data
      
      console.log('API返回的用户数据:', apiData)
      console.log('email字段值:', apiData.email)
      console.log('phone字段值:', apiData.phone)
      
      // 更新用户信息
      userInfo.value = {
        name: apiData.username || '',
        email: apiData.email || '',
        phone: apiData.phone || '',
        joinDate: apiData.createdAt || ''
      }
      
      // 更新角色
      if (apiData.role !== undefined) {
        const roleMap = {
          0: 'owner',    // 宠物主人
          1: 'sitter',   // 服务者
          2: 'moderator' // 管理者
        }
        userRole.value = roleMap[apiData.role] || 'owner'
        localStorage.setItem('petpal_userRole', userRole.value)
      }
      
      // 更新本地存储的用户信息
      userAPI.updateLocalUserInfo({
        username: userInfo.value.name,
        email: userInfo.value.email,
        phone: userInfo.value.phone,
        role: userRole.value,
        createdAt: userInfo.value.joinDate
      })
      
      console.log('更新后的用户信息:', userInfo.value)
    } else {
      console.log('API返回数据为空')
      // 如果API获取失败，从本地存储获取
      const savedUser = userAPI.getCurrentUser()
      if (savedUser) {
        userInfo.value = {
          name: savedUser.username || '',
          email: savedUser.email || '',
          phone: savedUser.phone || '',
          joinDate: savedUser.createdAt || ''
        }
        
        if (savedUser.role) {
          userRole.value = savedUser.role
        }
      }
    }
  } catch (error) {
    console.error('加载用户信息失败:', error)
    // 从本地存储获取用户信息
    const savedUser = userAPI.getCurrentUser()
    if (savedUser) {
      userInfo.value = {
        name: savedUser.username || '',
        email: savedUser.email || '',
        phone: savedUser.phone || '',
        joinDate: savedUser.createdAt || ''
      }
      
      if (savedUser.role) {
        userRole.value = savedUser.role
      }
    }
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
/* 保持原来的所有样式完全不变 */
.profile-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.profile-header {
  margin-bottom: 40px;
}

/* 保持原来的所有样式完全不变 */
.profile-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.profile-header {
  margin-bottom: 40px;
}

/* ... 保持所有原有样式完全不变 ... */

/* 最后一部分的响应式设计 */
@media (max-width: 768px) {
  .profile-container {
    padding: 15px;
  }
  
  .page-title {
    font-size: 24px;
  }
  
  .info-card,
  .user-card,
  .community-card {
    padding: 20px;
  }
  
  .account-actions {
    flex-direction: column;
  }
  
  .modal-content {
    margin: 10px;
  }
}

.profile-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.profile-header {
  margin-bottom: 40px;
}

.page-title {
  font-size: 32px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 10px 0;
}

.page-subtitle {
  font-size: 16px;
  color: #64748b;
  margin: 0;
}

.profile-content {
  display: grid;
  grid-template-columns: 300px 1fr;
  gap: 30px;
}

/* 左侧样式 */
.profile-sidebar {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.user-card {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border: 1px solid #f1f5f9;
}

.user-avatar-large {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: 600;
  margin: 0 auto 20px;
}

.user-basic-info {
  text-align: center;
  margin-bottom: 25px;
}

.user-name {
  font-size: 24px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 8px 0;
}

.user-role {
  font-size: 14px;
  color: #64748b;
  margin: 0 0 8px 0;
}

.user-join-date {
  font-size: 13px;
  color: #94a3b8;
  margin: 0;
}

.user-stats {
  display: flex;
  justify-content: space-around;
  border-top: 1px solid #f1f5f9;
  padding-top: 20px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.stat-number {
  font-size: 20px;
  font-weight: 700;
  color: #166534;
}

.stat-label {
  font-size: 12px;
  color: #64748b;
  margin-top: 4px;
}

.community-card {
  background: white;
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border: 1px solid #f1f5f9;
}

.card-title {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 18px;
  font-weight: 600;
  color: #1e293b;
  margin: 0 0 20px 0;
}

.card-icon {
  font-size: 20px;
}

.community-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
}

.community-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.community-label {
  font-size: 14px;
  color: #64748b;
}

.community-value {
  font-size: 14px;
  font-weight: 500;
  color: #1e293b;
}

.view-community-btn {
  width: 100%;
  padding: 12px;
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.view-community-btn:hover {
  background: #f1f5f9;
  border-color: #cbd5e1;
  transform: translateY(-1px);
}

/* 右侧样式 */
.profile-main {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.info-card {
  background: white;
  border-radius: 16px;
  padding: 30px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border: 1px solid #f1f5f9;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.edit-btn {
  padding: 8px 20px;
  background: #f1f5f9;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.edit-btn:hover {
  background: #e2e8f0;
}

.edit-btn.editing {
  background: #22c55e;
  border-color: #22c55e;
  color: white;
}

.edit-btn.editing:hover {
  background: #16a34a;
}

.info-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-label {
  font-size: 14px;
  font-weight: 600;
  color: #475569;
}

.form-input {
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 15px;
  color: #1e293b;
  transition: all 0.3s;
  background: #f8fafc;
}

.form-input:focus {
  outline: none;
  border-color: #22c55e;
  background: white;
}

.form-input.editing {
  background: white;
  border-color: #cbd5e1;
}

.form-input:disabled {
  background: #f8fafc;
  color: #64748b;
  cursor: not-allowed;
}

.role-display {
  display: flex;
  align-items: center;
  gap: 10px;
}

.role-badge {
  background: #f0fdf4;
  color: #166534;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 14px;
  font-weight: 600;
}

.role-hint {
  font-size: 13px;
  color: #94a3b8;
}

.verification-hint {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #dc2626;
  margin: 8px 0 0 0;
}

.verification-hint.verified {
  color: #16a34a;
}

.warning-icon {
  font-size: 14px;
}

.success-icon {
  font-size: 14px;
}

.verify-btn {
  background: none;
  border: none;
  color: #4f46e5;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
  font-size: 13px;
}

.verify-btn:hover {
  text-decoration: underline;
}

/* 密码表单样式 */
.password-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.password-input-group {
  position: relative;
  display: flex;
}

.password-input-group .form-input {
  flex: 1;
  padding-right: 50px;
}

.toggle-password-btn {
  position: absolute;
  right: 10px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  font-size: 18px;
  cursor: pointer;
  color: #64748b;
  padding: 5px;
}

.error-message {
  color: #dc2626;
  font-size: 13px;
  margin: 5px 0 0 0;
}

.password-strength {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 8px;
}

.strength-bar {
  flex: 1;
  height: 4px;
  background: #e2e8f0;
  border-radius: 2px;
  overflow: hidden;
}

.strength-bar::after {
  content: '';
  display: block;
  height: 100%;
  transition: width 0.3s;
}

.strength-bar.strength-weak::after {
  width: 25%;
  background: #dc2626;
}

.strength-bar.strength-medium::after {
  width: 50%;
  background: #f59e0b;
}

.strength-bar.strength-strong::after {
  width: 100%;
  background: #16a34a;
}

.strength-text {
  font-size: 13px;
  color: #64748b;
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 10px;
}

.btn-secondary {
  padding: 12px 24px;
  background: #f1f5f9;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-secondary:hover {
  background: #e2e8f0;
}

.btn-primary {
  padding: 12px 24px;
  background: #22c55e;
  border: none;
  border-radius: 10px;
  color: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary:hover:not(:disabled) {
  background: #16a34a;
  transform: translateY(-1px);
}

.btn-primary:disabled {
  background: #94a3b8;
  cursor: not-allowed;
}

.password-security-tips {
  padding: 20px;
  background: #f8fafc;
  border-radius: 10px;
}

.security-tip {
  font-size: 14px;
  color: #64748b;
  margin: 0 0 8px 0;
}

.security-tip:last-child {
  margin-bottom: 0;
}

/* 账户操作样式 */
.account-actions {
  display: flex;
  gap: 15px;
  margin-bottom: 20px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  border: 2px solid transparent;
}

.logout-btn {
  background: #fef2f2;
  color: #dc2626;
  border-color: #fecaca;
}

.logout-btn:hover {
  background: #fee2e2;
  transform: translateY(-1px);
}

.delete-btn {
  background: #f8fafc;
  color: #475569;
  border-color: #e2e8f0;
}

.delete-btn:hover {
  background: #f1f5f9;
  transform: translateY(-1px);
}

.danger-zone {
  padding: 20px;
  background: #fef2f2;
  border-radius: 10px;
  border: 1px solid #fecaca;
}

.danger-title {
  color: #dc2626;
  font-size: 16px;
  margin: 0 0 10px 0;
}

.danger-hint {
  color: #991b1b;
  font-size: 14px;
  margin: 0;
}

/* 模态框样式 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 20px;
}

.modal-content {
  background: white;
  border-radius: 20px;
  width: 100%;
  max-width: 500px;
  max-height: 90vh;
  overflow-y: auto;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: modalSlideIn 0.3s ease;
}

@keyframes modalSlideIn {
  from {
    opacity: 0;
    transform: translateY(30px);
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

.modal-header h3 {
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

.modal-body {
  padding: 30px;
}

.warning-message {
  text-align: center;
}

.warning-icon-big {
  font-size: 48px;
  display: block;
  margin-bottom: 20px;
}

.warning-message h4 {
  color: #dc2626;
  font-size: 20px;
  margin: 0 0 15px 0;
}

.warning-message p {
  color: #475569;
  font-size: 16px;
  margin: 0 0 15px 0;
}

.delete-consequences {
  text-align: left;
  color: #64748b;
  font-size: 14px;
  margin: 0 0 20px 20px;
  padding: 0;
}

.delete-consequences li {
  margin-bottom: 8px;
}

.confirm-input {
  margin-top: 20px;
}

.confirm-input-field {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 15px;
  color: #1e293b;
  transition: all 0.3s;
}

.confirm-input-field:focus {
  outline: none;
  border-color: #dc2626;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px 30px 30px;
  border-top: 1px solid #f1f5f9;
}

.btn-danger {
  padding: 12px 24px;
  background: #dc2626;
  border: none;
  border-radius: 10px;
  color: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c;
  transform: translateY(-1px);
}

.btn-danger:disabled {
  background: #94a3b8;
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .profile-content {
    grid-template-columns: 1fr;
  }
  
  .profile-sidebar {
    order: 2;
  }
  
  .profile-main {
    order: 1;
  }
}

@media (max-width: 768px) {
  .profile-container {
    padding: 15px;
  }
  
  .page-title {
    font-size: 24px;
  }
  
  .info-card,
  .user-card,
  .community-card {
    padding: 20px;
  }
  
  .account-actions {
    flex-direction: column;
  }
  
  .modal-content {
    margin: 10px;
  }
}
</style>