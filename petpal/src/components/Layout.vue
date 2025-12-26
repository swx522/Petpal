<!-- src/components/Layout.vue -->
<template>
  <div class="petpal-app">
    <aside class="pet-sidebar">
      <div class="pet-logo" @click="goToHome">
        <span class="logo-icon">🐾</span>
        <span class="logo-text">PetPal</span>
      </div>

      <nav class="pet-nav">
        <div class="nav-group-label">社交互助</div>
        
        <!-- 发布需求 - 仅宠物主人可见 -->
        <div 
          class="nav-item" 
          :class="{
            active: activeNav === '/publish',
            unavailable: !isLoggedIn || userRole !== 'owner'
          }"
          @click="handleNavClick('/publish', 'owner')"
        >
          <i class="icon">📈</i> 
          <span>发布需求</span>
          <span v-if="!isLoggedIn || userRole !== 'owner'" class="nav-lock">🔒</span>
        </div>
        
        <!-- 接单需求 - 仅服务者可见 -->
        <div 
          class="nav-item" 
          :class="{
            active: activeNav === '/accept',
            unavailable: !isLoggedIn || userRole !== 'sitter'
          }"
          @click="handleNavClick('/accept', 'sitter')"
        >
          <i class="icon">🦴</i> 
          <span>接单需求</span>
          <span v-if="!isLoggedIn || userRole !== 'sitter'" class="nav-lock">🔒</span>
        </div>
        
        <!-- 管理社区 - 仅管理者可见 -->
        <div 
          class="nav-item" 
          :class="{
            active: activeNav === '/manage',
            unavailable: !isLoggedIn || userRole !== 'moderator'
          }"
          @click="handleNavClick('/manage', 'moderator')"
        >
          <i class="icon">🍱</i> 
          <span>管理社区</span>
          <span v-if="!isLoggedIn || userRole !== 'moderator'" class="nav-lock">🔒</span>
        </div>
      </nav>

      <div class="sidebar-footer">
        <!-- 简化：未登录时点击登录，登录后加锁显示状态 -->
        <div 
          class="user-pill" 
          :class="{ 'logged-in': isLoggedIn, 'logged-out': !isLoggedIn }"
          @click="handleUserPillClick"
        >
          <div class="user-avatar">{{ userInitials }}</div>
          <div class="user-info">
            <span class="user-name">{{ isLoggedIn ? userName : '点击登录' }}</span>
            <span v-if="isLoggedIn" class="user-level">{{ roleText }} Lv.{{ userLevel }}</span>
            <span v-else class="user-level">快速登录</span>
          </div>
          <div class="user-action-icon">
            <span v-if="isLoggedIn" class="locked-icon">🔒</span>
            <span v-else class="unlocked-icon">🔑</span>
          </div>
        </div>
      </div>
    </aside>

    <main class="pet-main">
      <header class="pet-header">
        <div class="header-breadcrumb">
          Dashboard / <span class="current">{{ currentPageName }}</span>
        </div>
        <div class="header-actions">
          <!-- 个人主页按钮 -->
          <button 
            @click="handleProfileClick" 
            class="action-btn profile-btn"
            :class="{ 'unavailable': !isLoggedIn }"
          >
            <span class="btn-icon">👤</span>
            个人主页
            <span v-if="!isLoggedIn" class="btn-lock">🔒</span>
          </button>
          
          <!-- 广场按钮 -->
          <button @click="goToSquare" class="action-btn">广场</button>
          
          <!-- 联系我们按钮 -->
          <button @click="showContactDialog" class="action-btn primary">联系我们</button>
        </div>
      </header>
      
      <div class="pet-view">
        <!-- 路由视图，显示当前页面内容 -->
        <router-view></router-view>
      </div>
    </main>

    <!-- 联系我们对话框 -->
    <div class="modal-overlay" v-if="showContactModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>联系我们</h3>
          <button class="close-btn" @click="showContactModal = false">×</button>
        </div>
        <div class="modal-body">
          <div class="contact-info">
            <div class="contact-item">
              <span class="contact-icon">📱</span>
              <div class="contact-details">
                <div class="contact-label">客服热线</div>
                <div class="contact-value">400-123-4567</div>
              </div>
            </div>
            <div class="contact-item">
              <span class="contact-icon">🕐</span>
              <div class="contact-details">
                <div class="contact-label">服务时间</div>
                <div class="contact-value">周一至周五 9:00-18:00</div>
              </div>
            </div>
            <div class="contact-item">
              <span class="contact-icon">📧</span>
              <div class="contact-details">
                <div class="contact-label">邮箱</div>
                <div class="contact-value">support@petpal.com</div>
              </div>
            </div>
            <div class="contact-item">
              <span class="contact-icon">📍</span>
              <div class="contact-details">
                <div class="contact-label">地址</div>
                <div class="contact-value">上海市嘉定区同济大学</div>
              </div>
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button class="btn-primary" @click="showContactModal = false">确定</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

// 添加这些响应式数据
const showContactModal = ref(false)

// 模拟用户数据 - 实际项目中应该从 store 或 API 获取
const isLoggedIn = ref(true) // 登录状态，默认未登录
const userRole = ref('moderator') // 用户角色，未登录时为''
const userName = ref('张三')
const userInitials = computed(() => {
  if (!isLoggedIn.value) return '登'
  return userName.value.substring(0, 2)
})
const userLevel = ref(5)

// 计算当前激活的导航
const activeNav = computed(() => route.path)

// 计算页面名称
const currentPageName = computed(() => {
  const pageMap = {
    '/publish': '发布需求',
    '/accept': '接单需求',
    '/manage': '管理社区',
  }
  return pageMap[route.path] || 'Dashboard'
})

// 角色文本显示
const roleText = computed(() => {
  const roleMap = {
    'owner': '宠物主人',
    'sitter': '服务者',
    'moderator': '管理者'
  }
  return roleMap[userRole.value] || ''
})

// 导航点击处理
const handleNavClick = (path, requiredRole) => {
  // 未登录时点击导航项
  if (!isLoggedIn.value) {
    if (confirm('该功能需要登录后才能使用，是否前往登录页面？')) {
      goToLogin()
    }
    return
  }
  
  // 已登录但角色不匹配
  if (userRole.value !== requiredRole) {
    const roleNameMap = {
      'owner': '宠物主人',
      'sitter': '服务者',
      'moderator': '管理者'
    }
    const requiredRoleName = roleNameMap[requiredRole] || requiredRole
    const currentRoleName = roleText.value || '未分配角色'
    alert(`当前角色"${currentRoleName}"无法访问此功能，仅限"${requiredRoleName}"使用。`)
    return
  }
  
  // 角色匹配，跳转到对应页面
  router.push(path)
}

// 个人主页按钮点击处理
const handleProfileClick = () => {
  if (!isLoggedIn.value) {
    if (confirm('个人主页需要登录后才能查看，是否前往登录页面？')) {
      goToLogin()
    }
    return
  }
  
  // 已登录，跳转到个人主页
  goToProfile()
}

// 左下角用户按钮点击处理
const handleUserPillClick = () => {
  if (isLoggedIn.value) {
    // 已登录状态下，按钮已加锁，点击显示提示信息
    showLoggedInMessage()
  } else {
    // 未登录，跳转到登录页面
    goToLogin()
  }
}

// 显示已登录提示信息
const showLoggedInMessage = () => {
  // 可以显示一个简短的提示，或者什么都不做
  console.log('您已登录，无需再次登录')
  // 或者可以显示一个轻量级的提示
  alert(`您已登录为 ${userName.value} (${roleText.value})\n\n如需退出登录，请通过其他方式操作。`)
}

// 导航函数
const goToHome = () => router.push('/')
const goToPublish = () => handleNavClick('/publish', 'owner')
const goToAccept = () => handleNavClick('/accept', 'sitter')
const goToManageCommunity = () => handleNavClick('/manage', 'moderator')
const goToSquare = () => router.push('/init')
const goToProfile = () => {
  router.push('/profile') // 假设个人主页路由为/profile
}
const goToLogin = () => {
  router.push('/login')
}
const goToRegister = () => {
  router.push('/register') // 假设有注册页面
}

// 登出处理
const handleLogout = () => {
  isLoggedIn.value = false
  userRole.value = ''
  router.push('/')
}

// 显示联系我们对话框
const showContactDialog = () => {
  showContactModal.value = true
}

// 测试函数 - 可以在控制台调用切换登录状态和角色
window.login = (role = 'owner') => {
  if (['owner', 'sitter', 'moderator'].includes(role)) {
    isLoggedIn.value = true
    userRole.value = role
    console.log(`已登录，角色: ${roleText.value}`)
  } else {
    console.error('无效的角色，请使用: owner, sitter, moderator')
  }
}

window.logout = () => {
  isLoggedIn.value = false
  userRole.value = ''
  console.log('已退出登录')
}

window.changeRole = (role) => {
  if (!isLoggedIn.value) {
    console.error('请先登录')
    return
  }
  if (['owner', 'sitter', 'moderator'].includes(role)) {
    userRole.value = role
    console.log(`角色已切换为: ${roleText.value}`)
  } else {
    console.error('无效的角色，请使用: owner, sitter, moderator')
  }
}

// 页面加载时检查登录状态（模拟）
onMounted(() => {
  // 模拟从本地存储检查登录状态
  const savedLoginState = localStorage.getItem('petpal_isLoggedIn')
  const savedRole = localStorage.getItem('petpal_userRole')
  
  if (savedLoginState === 'true' && savedRole) {
    isLoggedIn.value = true
    userRole.value = savedRole
  }
  
  // 监听登录状态变化，模拟保存到本地存储
  watch([isLoggedIn, userRole], ([newIsLoggedIn, newUserRole]) => {
    if (newIsLoggedIn) {
      localStorage.setItem('petpal_isLoggedIn', 'true')
      localStorage.setItem('petpal_userRole', newUserRole)
    } else {
      localStorage.removeItem('petpal_isLoggedIn')
      localStorage.removeItem('petpal_userRole')
    }
  })
})
</script>

<style scoped>
/* 添加在最前面 */
:global(html), :global(body) {
  margin: 0 !important;
  padding: 0 !important;
  width: 100% !important;
  height: 100% !important;
}

.petpal-app {
  display: flex;
  width: 100vw;
  height: 100vh;
  background-color: #ffffff;
  overflow: hidden;
  margin: 0;
  padding: 0;
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
}

/* 侧边栏：靠死左边，窄长大气 */
.pet-sidebar {
  width: 260px;
  background-color: #f8fafc;
  border-right: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  position: relative;
}

.pet-logo {
  padding: 40px 30px;
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  transition: all 0.3s;
}

.pet-logo:hover {
  transform: translateX(5px);
}

.logo-icon { 
  font-size: 28px; 
}

.logo-text { 
  font-size: 24px; 
  font-weight: 900; 
  color: #166534; 
  letter-spacing: -1px; 
}

.pet-nav { 
  flex: 1; 
  padding: 0 15px; 
}

.nav-group-label { 
  font-size: 12px; 
  color: #94a3b8; 
  padding: 20px 15px 10px; 
  text-transform: uppercase; 
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 18px;
  border-radius: 12px;
  color: #475569;
  font-weight: 600;
  cursor: pointer;
  margin-bottom: 4px;
  transition: all 0.3s;
  position: relative;
}

.nav-item:hover:not(.unavailable) {
  background-color: #f1f5f9;
  transform: translateX(5px);
}

.nav-item.active:not(.unavailable) {
  background-color: #22c55e;
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.2);
}

/* 被锁定的导航项样式 */
.nav-item.unavailable {
  opacity: 0.6;
  cursor: not-allowed;
}

.nav-item.unavailable:hover {
  background-color: transparent;
  transform: none;
}

.nav-item.unavailable .icon {
  filter: grayscale(100%);
}

.nav-lock {
  margin-left: auto;
  font-size: 14px;
  color: #94a3b8;
}

/* 用户等级显示角色 */
.user-level {
  font-size: 11px;
  color: #64748b;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 10px;
  display: inline-block;
  width: fit-content;
}

/* 侧边栏底部用户信息 */
.sidebar-footer { 
  padding: 30px 15px; 
  border-top: 1px solid #f1f5f9; 
}

/* 简化的用户按钮样式 */
.user-pill {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #fff;
  padding: 12px;
  border-radius: 16px;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);
  transition: all 0.3s;
  border: 2px solid transparent;
}

/* 未登录状态样式 - 可点击 */
.user-pill.logged-out {
  cursor: pointer;
  border-color: #e2e8f0;
}

.user-pill.logged-out:hover {
  box-shadow: 0 6px 12px rgba(0,0,0,0.1);
  transform: translateY(-2px);
  border-color: #cbd5e1;
  background: #f8fafc;
}

.user-pill.logged-out:active {
  transform: translateY(0);
}

/* 已登录状态样式 - 加锁不可点击 */
.user-pill.logged-in {
  cursor: not-allowed;
  opacity: 0.8;
  border-color: #f1f5f9;
  background: #f8fafc;
}

.user-pill.logged-in:hover {
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);
  transform: none;
  cursor: not-allowed;
}

.user-avatar { 
  width: 40px; 
  height: 40px; 
  color: white; 
  border-radius: 10px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-size: 14px;
  font-weight: 600;
}

/* 未登录时的登录按钮样式 */
.user-pill.logged-out .user-avatar {
  background: linear-gradient(135deg, #4f46e5, #7c3aed);
}

/* 已登录时的锁定样式 */
.user-pill.logged-in .user-avatar {
  background: linear-gradient(135deg, #22c55e, #16a34a);
}

.user-info {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.user-name {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
}

/* 未登录时的用户名样式 */
.user-pill.logged-out .user-name {
  color: #4f46e5;
  font-weight: 700;
}

/* 已登录时的用户名样式 */
.user-pill.logged-in .user-name {
  color: #166534;
}

/* 用户等级样式调整 */
.user-pill.logged-out .user-level {
  background: #e0e7ff;
  color: #4f46e5;
}

.user-pill.logged-in .user-level {
  background: #f0fdf4;
  color: #166534;
}

/* 用户操作图标 */
.user-action-icon {
  font-size: 16px;
  transition: all 0.3s;
}

/* 未登录时的解锁图标 */
.user-pill.logged-out .unlocked-icon {
  color: #4f46e5;
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
    transform: scale(1);
  }
  50% {
    opacity: 0.7;
    transform: scale(1.1);
  }
}

/* 已登录时的锁定图标 */
.user-pill.logged-in .locked-icon {
  color: #22c55e;
}

/* 右侧主体：无限宽阔 */
.pet-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
  background-color: #fff;
}

.pet-header {
  height: 80px;
  padding: 0 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f1f5f9;
}

.header-breadcrumb {
  font-size: 14px;
  color: #94a3b8;
}

.header-breadcrumb .current {
  color: #166534;
  font-weight: 600;
}

.header-actions { 
  display: flex; 
  gap: 15px; 
  align-items: center;
}

.action-btn {
  padding: 10px 24px;
  border-radius: 10px;
  border: 2px solid #e2e8f0;
  background: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  font-size: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.action-btn:hover:not(.unavailable) {
  background: #f8fafc;
  border-color: #cbd5e1;
  transform: translateY(-1px);
}

.action-btn.primary { 
  background: #166534; 
  color: white; 
  border: none; 
}

.action-btn.primary:hover {
  background: #14532d;
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

/* 个人主页按钮样式 */
.action-btn.profile-btn {
  position: relative;
}

/* 被锁定的按钮样式 */
.action-btn.unavailable {
  opacity: 0.6;
  cursor: not-allowed;
  border-color: #e2e8f0;
  background: #f8fafc;
}

.action-btn.unavailable:hover {
  background: #f8fafc;
  border-color: #e2e8f0;
  transform: none;
}

.action-btn.unavailable .btn-icon {
  filter: grayscale(100%);
}

.btn-lock {
  font-size: 12px;
  color: #94a3b8;
}

.btn-icon {
  font-size: 16px;
}

/* 核心内容区：这里是决定"大气"的关键 */
.pet-view {
  flex: 1;
  padding: 40px;
  overflow-y: auto;
}

/* 联系我们对话框 */
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
}

.contact-info {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

.contact-item {
  display: flex;
  align-items: center;
  gap: 20px;
}

.contact-icon {
  font-size: 24px;
  width: 40px;
  height: 40px;
  background: #f0fdf4;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #166534;
}

.contact-details {
  flex: 1;
}

.contact-label {
  font-size: 14px;
  color: #64748b;
  margin-bottom: 4px;
}

.contact-value {
  font-size: 16px;
  color: #1e293b;
  font-weight: 600;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  padding: 20px 30px 30px;
  border-top: 1px solid #f1f5f9;
}

.btn-primary {
  background: #166534;
  color: white;
  border: none;
  padding: 12px 32px;
  border-radius: 10px;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary:hover {
  background: #14532d;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(22, 101, 52, 0.2);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .petpal-app {
    flex-direction: column;
  }
  
  .pet-sidebar {
    width: 100%;
    flex-direction: row;
    height: 70px;
    padding: 0 20px;
    border-right: none;
    border-bottom: 1px solid #f1f5f9;
  }
  
  .pet-logo {
    padding: 0;
    margin-right: 20px;
  }
  
  .pet-nav {
    display: none; /* 移动端隐藏导航，可用汉堡菜单替代 */
  }
  
  .sidebar-footer {
    padding: 0;
    border-top: none;
    margin-left: auto;
  }
  
  .user-pill {
    box-shadow: none;
    padding: 8px;
    min-width: auto;
  }
  
  .user-info {
    display: none;
  }
  
  .user-action-icon {
    display: none;
  }
  
  .pet-header {
    height: 60px;
    padding: 0 20px;
  }
  
  .pet-view {
    padding: 20px;
  }
  
  .header-actions {
    gap: 10px;
  }
  
  .action-btn {
    padding: 8px 12px;
    font-size: 13px;
  }
  
  .btn-icon, .btn-lock {
    font-size: 12px;
  }
}

@media (max-width: 480px) {
  .modal-content {
    margin: 10px;
  }
  
  .modal-header {
    padding: 20px 20px 15px;
  }
  
  .modal-body {
    padding: 20px;
  }
  
  .modal-actions {
    padding: 15px 20px 20px;
  }
  
  .contact-item {
    gap: 15px;
  }
  
  .contact-icon {
    width: 36px;
    height: 36px;
    font-size: 20px;
  }
  
  /* 小屏幕下简化按钮文字 */
  .action-btn {
    padding: 8px 10px;
  }
  
  .action-btn span:not(.btn-icon):not(.btn-lock) {
    display: none;
  }
}
</style>