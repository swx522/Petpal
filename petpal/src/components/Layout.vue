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
        <div 
          class="nav-item" 
          :class="{ active: activeNav === '/publish' }"
          @click="goToPublish"
        >
          <i class="icon">📈</i> <span>发布需求</span>
        </div>
        <div 
          class="nav-item" 
          :class="{ active: activeNav === '/accept' }"
          @click="goToAccept"
        >
          <i class="icon">🦴</i> <span>接单需求</span>
        </div>
        <div 
          class="nav-item" 
          :class="{ active: activeNav === '/manage' }"
          @click="goToManageCommunity"
        >
          <i class="icon">🍱</i> <span>管理社区</span>
        </div>
      </nav>

      <div class="sidebar-footer">
        <div class="user-pill" @click="toggleUserMenu">
          <div class="user-avatar">{{ userInitials }}</div>
          <div class="user-info">
            <span class="user-name">{{ userName }}</span>
            <span class="user-level">Lv.{{ userLevel }}</span>
          </div>
          <div class="user-menu-dropdown" v-if="showUserMenu">
            <div class="dropdown-item" @click="goToProfile">
              <i class="dropdown-icon">👤</i> 个人中心
            </div>
            <div class="dropdown-item" @click="handleLogout">
              <i class="dropdown-icon">🚪</i> 退出登录
            </div>
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
          <button @click="goToSquare" class="action-btn">广场</button>
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

<!-- src/components/Layout.vue -->
<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'

const router = useRouter()
const route = useRoute()

// 添加这些响应式数据
const showUserMenu = ref(false)
const showContactModal = ref(false)

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

// 导航函数
const goToHome = () => router.push('/')
const goToPublish = () => router.push('/publish')
const goToAccept = () => router.push('/accept')
const goToManageCommunity = () => router.push('/manage')
const goToReviewRequirements = () => router.push('/review-requirements')
const goToReviewOrders = () => router.push('/review-orders')
const goToReviewCommunity = () => router.push('/review-community')
const goToSquare = () => router.push('/init')
const goToProfile = () => router.push('/login')

// 用户菜单切换
const toggleUserMenu = () => {
  showUserMenu.value = !showUserMenu.value
}

// 显示联系我们对话框
const showContactDialog = () => {
  showContactModal.value = true
}

// 其他已有代码保持不变...
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
}

.nav-item:hover {
  background-color: #f1f5f9;
  transform: translateX(5px);
}

.nav-item.active {
  background-color: #22c55e;
  color: #ffffff;
  box-shadow: 0 4px 12px rgba(34, 197, 94, 0.2);
}

/* 侧边栏底部用户信息 */
.sidebar-footer { 
  padding: 30px 15px; 
  border-top: 1px solid #f1f5f9; 
}

.user-pill {
  display: flex;
  align-items: center;
  gap: 12px;
  background: #fff;
  padding: 12px;
  border-radius: 16px;
  box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);
  cursor: pointer;
  position: relative;
  transition: all 0.3s;
}

.user-pill:hover {
  box-shadow: 0 6px 12px rgba(0,0,0,0.1);
  transform: translateY(-2px);
}

.user-avatar { 
  width: 40px; 
  height: 40px; 
  background: linear-gradient(135deg, #22c55e, #16a34a);
  color: white; 
  border-radius: 10px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-size: 14px;
  font-weight: 600;
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

.user-level {
  font-size: 11px;
  color: #64748b;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 10px;
  display: inline-block;
  width: fit-content;
}

/* 用户菜单下拉 */
.user-menu-dropdown {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 12px;
  box-shadow: 0 10px 25px rgba(0,0,0,0.1);
  margin-top: 10px;
  z-index: 1000;
  overflow: hidden;
  animation: slideDown 0.2s ease;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.dropdown-item {
  padding: 12px 16px;
  display: flex;
  align-items: center;
  gap: 10px;
  color: #475569;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.3s;
}

.dropdown-item:hover {
  background: #f8fafc;
  color: #166534;
}

.dropdown-icon {
  font-size: 16px;
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
}

.action-btn:hover {
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
  }
  
  .user-info {
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
    padding: 8px 16px;
    font-size: 13px;
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
}
</style>