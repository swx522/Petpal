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

        <!-- 社区信息卡片 -->
        <div class="community-card">
          <h4 class="card-title">
            <span class="card-icon">🏘️</span> 我的社区
          </h4>
          
          <!-- 没有社区时的提示 -->
          <div v-if="!hasCommunity" class="no-community-message">
            <div class="no-community-icon">🏠</div>
            <p class="no-community-text">您尚未加入任何社区</p>
            <p class="no-community-hint">加入社区可以享受更多互助服务</p>
            <button class="btn-primary find-community-btn" @click="findNearbyCommunity" :disabled="loading">
              <span class="btn-icon">🔍</span> 查找附近社区
            </button>
          </div>
          
          <!-- 有社区时的显示 -->
          <div v-else>
            <!-- 社区下拉框 -->
            <div class="community-select-group">
              <label class="form-label">选择社区</label>
              <select 
                v-model="selectedCommunityId"
                @change="onCommunityChange"
                class="community-select"
                :disabled="loading || userCommunities.length <= 1"
              >
                <option value="" disabled>请选择社区</option>
                <option 
                  v-for="community in userCommunities" 
                  :key="community.id"
                  :value="community.id"
                >
                  {{ community.name }}
                </option>
              </select>
              <p v-if="userCommunities.length <= 1" class="community-select-hint">
                您目前只加入了一个社区
              </p>
            </div>
            
            <!-- 社区信息显示 -->
            <div class="community-info">
              <div class="community-item">
                <span class="community-label">社区名称</span>
                <span class="community-value">{{ currentCommunityName }}</span>
              </div>
              <div class="community-item" v-if="currentCommunityDescription">
                <span class="community-label">社区描述</span>
                <span class="community-value description">{{ currentCommunityDescription }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- 位置信息卡片 -->
        <div class="location-card">
          <h4 class="card-title">
            <span class="card-icon">📍</span> 当前位置
          </h4>

          <div class="location-info">
            <div v-if="currentLocation" class="location-details">
              <div class="location-item">
                <span class="location-label">经纬度：</span>
                <span class="location-value">{{ currentLocation.latitude.toFixed(6) }}, {{ currentLocation.longitude.toFixed(6) }}</span>
              </div>
              <div v-if="currentLocation.address" class="location-item">
                <span class="location-label">地址：</span>
                <span class="location-value">{{ currentLocation.address }}</span>
              </div>
              <div class="location-item">
                <span class="location-label">更新时间：</span>
                <span class="location-value">{{ formatLocationTime(currentLocation.timestamp) }}</span>
              </div>
            </div>

            <div v-else class="no-location">
              <div class="no-location-icon">📍</div>
              <p>未获取到位置信息</p>
            </div>
          </div>

          <div class="location-actions">
            <button
              @click="updateLocation"
              class="btn-update-location"
              :disabled="locationLoading"
            >
            <span v-if="locationLoading" class="btn-spinner small"></span>
            {{ locationLoading ? '定位中...' : '📍 更新位置' }}
            </button>

            <div class="location-status">
              <span v-if="locationStatus === 'granted'" class="status-granted">✓ 定位权限已开启</span>
              <span v-else-if="locationStatus === 'denied'" class="status-denied">✗ 定位权限被拒绝</span>
              <span v-else-if="locationStatus === 'prompt'" class="status-prompt">? 需要定位权限</span>
              <span v-else class="status-unknown">? 未知权限状态</span>
            </div>
          </div>
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
            <p class="security-tip">📱 确保密码与其他网站不同</p>
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
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { userAPI } from '@/utils/user.js'
import {
  locationService,
  getCurrentPosition,
  updateUserLocation,
  checkLocationPermission
} from '@/utils/location.js'

const router = useRouter()

// 用户信息
const userInfo = ref({
  name: '',
  email: '',
  phone: '',
  joinDate: ''
})

// 社区相关状态
const userCommunities = ref([]) // 用户的所有社区列表
const selectedCommunityId = ref('') // 当前选中的社区ID
const hasCommunity = ref(false) // 是否有社区

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

// 加载状态
const loading = ref(false)
const locationLoading = ref(false)

// 位置相关状态
const currentLocation = ref(null)
const locationStatus = ref('unknown')
const locationWatcher = ref(null)

// 从本地存储获取角色信息
const userRole = ref(userAPI.getUserRole())

// 计算属性
const userInitials = computed(() => {
  const name = userInfo.value.name || '用户'
  return name.substring(0, 2)
})

const roleText = computed(() => {
  const roleMap = {
    'User': '宠物主人',
    'Sitter': '服务者',
    'Admin': '管理者'
  }
  return roleMap[userRole.value] || '用户'
})

const isPasswordFormValid = computed(() => {
  return (
    passwordInfo.value.oldPassword &&
    passwordInfo.value.newPassword &&
    passwordInfo.value.confirmPassword &&
    passwordInfo.value.newPassword === passwordInfo.value.confirmPassword &&
    passwordInfo.value.newPassword.length >= 6
  )
})

// 社区相关计算属性
const currentCommunityName = computed(() => {
  if (!hasCommunity.value || userCommunities.value.length === 0) return '未加入社区'
  if (!selectedCommunityId.value) {
    return userCommunities.value[0]?.name || '未命名社区'
  }
  const community = userCommunities.value.find(c => c.id === selectedCommunityId.value)
  return community?.name || '未找到社区'
})

const currentMemberCount = computed(() => {
  if (!hasCommunity.value || userCommunities.value.length === 0) return '--'
  if (!selectedCommunityId.value) {
    return userCommunities.value[0]?.memberCount || '--'
  }
  const community = userCommunities.value.find(c => c.id === selectedCommunityId.value)
  return community?.memberCount || '--'
})

const currentCommunityDescription = computed(() => {
  if (!hasCommunity.value || userCommunities.value.length === 0) return ''
  if (!selectedCommunityId.value) {
    return userCommunities.value[0]?.description || ''
  }
  const community = userCommunities.value.find(c => c.id === selectedCommunityId.value)
  return community?.description || ''
})

// 个人信息编辑方法
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

// 社区相关方法
const loadUserCommunities = async () => {
  try {
    console.log('🚀 开始获取社区信息...')
    
    // 首先获取当前社区
    const myCommunityResponse = await userAPI.getMyCommunity()
    
    console.log('🔍 社区API原始响应:', myCommunityResponse)
    console.log('📊 响应数据:', myCommunityResponse.data)
    console.log('🎯 响应类型:', typeof myCommunityResponse.data)
    
    if (myCommunityResponse.success) {
      // 检查返回的数据结构
      const data = myCommunityResponse.data
      
      // 情况1：后端返回了新格式（包含hasCommunity字段）
      if (data && typeof data === 'object' && 'hasCommunity' in data) {
        console.log('📝 新格式：包含hasCommunity字段')
        hasCommunity.value = data.hasCommunity === true
        
        if (data.hasCommunity && data.community) {
          console.log('✅ 用户有社区，社区数据:', data.community)
          userCommunities.value = [data.community]
          selectedCommunityId.value = data.community.id
        } else {
          console.log('⚠️ 用户没有社区')
          userCommunities.value = []
          selectedCommunityId.value = ''
        }
      }
      // 情况2：后端直接返回了社区对象（旧格式）
      else if (data && typeof data === 'object' && data.id) {
        console.log('📝 旧格式：直接返回社区对象')
        hasCommunity.value = true
        userCommunities.value = [data]
        selectedCommunityId.value = data.id
      }
      // 情况3：返回的是空对象或null
      else if (!data || Object.keys(data).length === 0) {
        console.log('📝 空数据或null')
        hasCommunity.value = false
        userCommunities.value = []
        selectedCommunityId.value = ''
      }
      // 情况4：其他未知格式
      else {
        console.log('❓ 未知数据格式:', data)
        hasCommunity.value = false
        userCommunities.value = []
        selectedCommunityId.value = ''
      }
    } else {
      console.log('❌ API返回失败:', myCommunityResponse.message)
      hasCommunity.value = false
      userCommunities.value = []
      selectedCommunityId.value = ''
    }
    
  } catch (error) {
    console.error('❌ 加载社区列表失败:', error)
    console.error('错误详情:', error.response?.data || error.message || error)
    
    // 如果是404错误，说明用户没有社区
    if (error.response?.status === 404) {
      console.log('⚠️ 用户没有社区（404错误）')
      hasCommunity.value = false
      userCommunities.value = []
      selectedCommunityId.value = ''
    } else {
      // 其他错误，使用默认状态
      hasCommunity.value = false
      userCommunities.value = []
      selectedCommunityId.value = ''
    }
  }
}

const onCommunityChange = (event) => {
  const communityId = event.target.value
  if (!communityId) return
  
  // 更新选中的社区ID
  selectedCommunityId.value = communityId
  console.log(`已切换到社区: ${currentCommunityName.value}`)
}

// 查找附近社区
const findNearbyCommunity = async () => {
  try {
    loading.value = true
    console.log('📍 开始查找附近社区...')
    
    // 提示用户需要位置权限
    alert('查找附近社区需要获取您的位置信息')
    
    // 尝试获取用户当前位置
    let latitude, longitude
    
    if (navigator.geolocation) {
      try {
        const position = await new Promise((resolve, reject) => {
          navigator.geolocation.getCurrentPosition(resolve, reject, {
            enableHighAccuracy: true,
            timeout: 10000,
            maximumAge: 0
          })
        })
        
        latitude = position.coords.latitude
        longitude = position.coords.longitude
        console.log(`📍 获取到用户位置: 纬度 ${latitude}, 经度 ${longitude}`)
      } catch (geoError) {
        console.warn('无法获取用户位置，使用默认位置:', geoError)
        // 使用默认位置（上海）
        latitude = 31.2304
        longitude = 121.4737
      }
    } else {
      console.warn('浏览器不支持地理位置API，使用默认位置')
      latitude = 31.2304
      longitude = 121.4737
    }
    
    // 调用查找社区API
    console.log(`🔍 开始查找社区: 纬度 ${latitude}, 经度 ${longitude}`)
    const response = await userAPI.findCommunity(longitude, latitude)
    
    console.log('🔍 查找社区结果:', response)
    
    if (response.success && response.data) {
      console.log('✅ 找到附近社区:', response.data)
      
      // 显示找到的社区信息
      const community = response.data
      const confirmJoin = confirm(
        `找到附近社区：${community.name}\n` +
        `描述：${community.description || '暂无描述'}\n` +
        `是否加入该社区？`
      )
      
      if (confirmJoin) {
      // 调用加入社区API
        console.log(`🚀 用户确认加入社区 ${community.id}`)
        const joinResponse = await userAPI.joinCommunity({ communityId: community.id })
        
        if (joinResponse.success) {
          alert('成功加入社区！')
          // 刷新社区信息
          await loadUserCommunities()
          
          // 更新本地用户信息
          const currentUser = userAPI.getCurrentUser()
          if (currentUser) {
            currentUser.communityId = community.id
            userAPI.saveUserInfo(currentUser)
          }
          
          // 发送通知给其他组件
          window.dispatchEvent(new CustomEvent('community-joined', {
            detail: { communityId: community.id }
          }))
        } else {
          alert(`加入社区失败: ${joinResponse.message}`)
        }
      }
     else {
      alert('附近没有找到可用的社区\n请尝试在其他位置查找。')
    }
  }} catch (error) {
    console.error('❌ 查找社区失败:', error)
    console.error('错误详情:', error.response?.data || error.message || error)
    alert('查找社区失败，请稍后重试或联系管理员')
  } finally {
    loading.value = false
  }
}

// 查看社区成员
const viewCommunityMembers = () => {
  if (!hasCommunity.value) return
  alert('查看社区成员功能正在开发中...')
  // router.push(`/community/${selectedCommunityId.value}/members`)
}

// 查看社区服务
const viewCommunityServices = () => {
  if (!hasCommunity.value) return
  alert('查看社区服务功能正在开发中...')
  // router.push(`/community/${selectedCommunityId.value}/services`)
}

const loadUserData = async () => {
  loading.value = true
  try {
    console.log('👤 开始加载用户数据...')
    const response = await userAPI.getUserInfo()
    
    console.log('👤 用户数据响应:', response)
    
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
        console.log('🎭 原始角色值:', apiData.role)
        userRole.value = apiData.role
        console.log('🔄 映射后的角色:', userRole.value)
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
    console.error('❌ 加载用户数据失败:', error)
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
      setTimeout(() => {
        window.location.reload()
      }, 100)
    } catch (error) {
      console.error('❌ 退出登录失败:', error)
      userAPI.clearLocalStorage()
      router.push('/login')
    } finally {
      loading.value = false
    }
  }
}

// ============ 位置相关函数 ============

// 检查定位权限状态
const checkLocationStatus = async () => {
  locationStatus.value = await checkLocationPermission()
}

// 更新用户位置
const updateLocation = async () => {
  locationLoading.value = true

  try {
    // 检查权限
    const permission = await locationService.requestPermission()
    if (permission === 'denied') {
      alert('需要定位权限才能更新位置信息，请在浏览器设置中允许定位权限')
      locationStatus.value = 'denied'
      return
    }

    // 获取当前位置
    const position = await getCurrentPosition({
      timeout: 15000,
      enableHighAccuracy: true
    })

    // 更新到后端
    await updateUserLocation(position.latitude, position.longitude, position.address)

    // 更新本地状态
    currentLocation.value = {
      ...position,
      timestamp: Date.now()
    }

    locationStatus.value = 'granted'

    // 显示成功消息
    alert('位置更新成功！您的位置信息已保存到数据库。')

  } catch (error) {
    console.error('位置更新失败:', error)

    let errorMessage = '位置更新失败，请重试'
    if (error.message.includes('超时')) {
      errorMessage = '定位超时，请检查网络连接后重试'
    } else if (error.message.includes('权限')) {
      errorMessage = '需要定位权限，请允许浏览器访问您的位置'
      locationStatus.value = 'denied'
    } else if (error.message.includes('定位失败')) {
      errorMessage = '无法获取您的位置，请检查GPS设置'
    }

    alert(errorMessage)
  } finally {
    locationLoading.value = false
  }
}

// 启动自动定位
const startAutoLocation = async () => {
  try {
    // 首先检查权限
    await checkLocationStatus()

    // 如果已经有权限，尝试获取一次位置
    if (locationStatus.value === 'granted') {
      // 静默获取位置（不显示加载状态）
      try {
        const position = await getCurrentPosition({ timeout: 10000 })
        currentLocation.value = {
          ...position,
          timestamp: Date.now()
        }
      } catch (error) {
        console.warn('自动获取位置失败:', error)
      }
    }

    // 启动位置监听（每5分钟检查一次位置变化）
    locationWatcher.value = locationService.watchPosition(
      async (error, position) => {
        if (error) {
          console.warn('位置监听错误:', error)
          return
        }

        // 检查位置是否发生显著变化（超过100米）
        if (currentLocation.value) {
          const distance = calculateDistance(
            currentLocation.value.latitude,
            currentLocation.value.longitude,
            position.latitude,
            position.longitude
          )

          // 如果距离超过100米，自动更新位置
          if (distance > 100) {
            console.log(`📍 检测到位置变化 ${distance.toFixed(0)}m，自动更新位置`)

            try {
              await updateUserLocation(position.latitude, position.longitude, position.address)
              currentLocation.value = {
                ...position,
                timestamp: Date.now()
              }
            } catch (updateError) {
              console.warn('自动更新位置失败:', updateError)
            }
          }
        } else {
          // 首次获取位置
          currentLocation.value = {
            ...position,
            timestamp: Date.now()
          }
        }
      },
      {
        timeout: 15000,
        enableHighAccuracy: true
      }
    )

  } catch (error) {
    console.error('启动自动定位失败:', error)
  }
}

// 停止自动定位
const stopAutoLocation = () => {
  if (locationWatcher.value) {
    locationWatcher.value()
    locationWatcher.value = null
  }
}

// 计算两点间距离（米）
const calculateDistance = (lat1, lng1, lat2, lng2) => {
  const R = 6371000 // 地球半径（米）
  const dLat = (lat2 - lat1) * Math.PI / 180
  const dLng = (lng2 - lng1) * Math.PI / 180
  const a = Math.sin(dLat/2) * Math.sin(dLat/2) +
            Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) *
            Math.sin(dLng/2) * Math.sin(dLng/2)
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a))
  return R * c
}

// 格式化位置更新时间
const formatLocationTime = (timestamp) => {
  if (!timestamp) return '未知'

  const now = Date.now()
  const diff = now - timestamp

  if (diff < 60000) return '刚刚'
  if (diff < 3600000) return `${Math.floor(diff / 60000)}分钟前`
  if (diff < 86400000) return `${Math.floor(diff / 3600000)}小时前`
  return `${Math.floor(diff / 86400000)}天前`
}

// 页面加载时获取数据
onMounted(async () => {
  console.log('🔄 Profile.vue 组件已挂载')
  console.log('🔍 当前用户角色:', userRole.value)
  
  // 并行加载用户数据和社区数据
  await Promise.all([
    loadUserData(),
    loadUserCommunities()
  ])
  
  console.log('✅ 所有数据加载完成')
  console.log('🏘️ 社区状态:', {
    hasCommunity: hasCommunity.value,
    communities: userCommunities.value,
    selectedCommunity: selectedCommunityId.value
  })

  // 启动自动定位功能
  await startAutoLocation()
})

// 在组件卸载时停止定位监听
onUnmounted(() => {
  stopAutoLocation()
})
</script>

<style scoped>
/* 保持原来的基础样式 */
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

/* 社区卡片样式 */
.community-card {
  background: white;
  border-radius: 16px;
  padding: 25px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
  border: 1px solid #f1f5f9;
  min-height: 300px;
  display: flex;
  flex-direction: column;
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

/* 没有社区的提示样式 */
.no-community-message {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 20px 0;
  text-align: center;
}

.no-community-icon {
  font-size: 48px;
  margin-bottom: 20px;
  opacity: 0.8;
}

.no-community-text {
  font-size: 16px;
  font-weight: 600;
  color: #475569;
  margin: 0 0 8px 0;
}

.no-community-hint {
  font-size: 14px;
  color: #64748b;
  margin: 0 0 20px 0;
}

.find-community-btn {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  background: linear-gradient(135deg, #22c55e, #16a34a);
  border: none;
  border-radius: 10px;
  color: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.find-community-btn:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
}

.find-community-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* 社区下拉框样式 */
.community-select-group {
  margin-bottom: 20px;
}

.community-select-group .form-label {
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  margin-bottom: 8px;
  display: block;
}

.community-select {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 15px;
  color: #1e293b;
  background: #f8fafc;
  transition: all 0.3s;
  cursor: pointer;
}

.community-select:focus {
  outline: none;
  border-color: #22c55e;
  background: white;
}

.community-select:disabled {
  background: #f1f5f9;
  color: #64748b;
  cursor: not-allowed;
}

.community-select-hint {
  font-size: 12px;
  color: #94a3b8;
  margin: 5px 0 0 0;
}

/* 社区信息显示样式 */
.community-info {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 20px;
  flex: 1;
}

.community-item {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.community-label {
  font-size: 14px;
  color: #64748b;
  white-space: nowrap;
}

.community-value {
  font-size: 14px;
  font-weight: 500;
  color: #1e293b;
  text-align: right;
  max-width: 60%;
  word-break: break-word;
}

.community-value.description {
  font-size: 13px;
  color: #64748b;
  font-style: italic;
}

/* 社区操作按钮 */
.community-actions {
  display: flex;
  gap: 10px;
  margin-top: auto;
}

.community-action-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px 16px;
  border: none;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.view-members-btn {
  background: #f0f9ff;
  color: #0369a1;
  border: 1px solid #bae6fd;
}

.view-members-btn:hover {
  background: #e0f2fe;
}

.view-services-btn {
  background: #f0fdf4;
  color: #166534;
  border: 1px solid #bbf7d0;
}

.view-services-btn:hover {
  background: #dcfce7;
}

/* 右侧样式保持不变 */
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

.error-message {
  color: #dc2626;
  font-size: 13px;
  margin: 5px 0 0 0;
}

.success-message {
  color: #16a34a;
  font-size: 13px;
  margin: 5px 0 0 0;
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
  
  .community-actions {
    flex-direction: column;
  }
  
  .community-action-btn {
    width: 100%;
  }
}

/* 位置信息卡片样式 */
.location-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 24px;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);
  border: 1px solid #f1f5f9;
}

.location-info {
  margin-bottom: 20px;
}

.location-details {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.location-item {
  display: flex;
  align-items: flex-start;
  gap: 8px;
}

.location-label {
  font-size: 14px;
  color: #64748b;
  font-weight: 600;
  min-width: 60px;
  flex-shrink: 0;
}

.location-value {
  font-size: 14px;
  color: #1e293b;
  word-break: break-word;
}

.no-location {
  text-align: center;
  padding: 20px 0;
  color: #94a3b8;
}

.no-location-icon {
  font-size: 32px;
  margin-bottom: 8px;
}

.location-actions {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.btn-update-location {
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white;
  border: none;
  padding: 12px 24px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.btn-update-location:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
}

.btn-update-location:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.location-status {
  text-align: center;
  font-size: 12px;
}

.status-granted {
  color: #22c55e;
}

.status-denied {
  color: #ef4444;
}

.status-prompt {
  color: #f59e0b;
}

.status-unknown {
  color: #94a3b8;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .location-actions {
    flex-direction: column;
    gap: 8px;
  }

  .btn-update-location {
    padding: 10px 20px;
    font-size: 14px;
  }
}
</style>