<!-- src/views/auth/LoginView.vue -->
<template>
  <div class="auth-page">
    <div class="auth-container">
      <!-- 左侧欢迎区域 -->
      <div class="welcome-section">
        <div class="welcome-content">
          <div class="logo">
            <span class="logo-icon">🐾</span>
            <span class="logo-text">PetPal</span>
          </div>
          <h1 class="welcome-title">欢迎回来</h1>
          <p class="welcome-subtitle">重新加入宠物互助社区，继续帮助更多可爱的宠物</p>
          <div class="pet-illustration">
            <div class="pet-item">🐶</div>
            <div class="pet-item">🐱</div>
            <div class="pet-item">🐰</div>
            <div class="pet-item">🐦</div>
          </div>
        </div>
      </div>

      <!-- 右侧登录表单 -->
      <div class="form-section">
        <div class="form-container">
          <div class="form-header">
            <h2>登录账户</h2>
            <p>请输入您的账户信息</p>
          </div>

          <form @submit.prevent="handleLogin" class="login-form">
            <!-- 账号输入 -->
            <div class="form-group">
              <label for="account">手机号</label>
              <div class="input-with-icon">
                <span class="input-icon">📱</span>
                <input
                  id="account"
                  v-model="loginForm.account"
                  type="text"
                  placeholder="请输入手机号"
                  required
                  :class="{ 'error': accountError }"
                  @input="clearError('account')"
                >
              </div>
              <div v-if="accountError" class="error-message">{{ accountError }}</div>
            </div>

            <!-- 密码输入 -->
            <div class="form-group">
              <label for="password">密码</label>
              <div class="input-with-icon">
                <span class="input-icon">🔒</span>
                <input
                  id="password"
                  v-model="loginForm.password"
                  :type="showPassword ? 'text' : 'password'"
                  placeholder="请输入密码"
                  required
                  :class="{ 'error': passwordError }"
                  @input="clearError('password')"
                >
                <button
                  type="button"
                  class="password-toggle"
                  @click="togglePasswordVisibility"
                >
                  {{ showPassword ? '👁️' : '👁️‍🗨️' }}
                </button>
              </div>
              <div v-if="passwordError" class="error-message">{{ passwordError }}</div>
            </div>

            <!-- 登录按钮 -->
            <button type="submit" class="submit-btn" :disabled="loading">
              <span v-if="!loading">登录</span>
              <span v-else class="loading-text">
                <span class="loading-spinner"></span> 登录中...
              </span>
            </button>

            <!-- 注册链接 -->
            <div class="register-link">
              还没有账户？
              <router-link to="/register" class="link">立即注册</router-link>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<!-- src/views/auth/LoginView.vue -->
<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { userAPI } from '@/utils/user.js'

const router = useRouter()

// 登录表单数据
const loginForm = reactive({
  account: '',
  password: '',
  rememberMe: false
})

const loading = ref(false)
const accountError = ref('')
const passwordError = ref('')
const showPassword = ref(false) // 确保这个变量被定义

// 切换密码显示状态
const togglePasswordVisibility = () => {
  showPassword.value = !showPassword.value
  // 强制更新DOM，确保立即生效
  const passwordInput = document.getElementById('password')
  if (passwordInput) {
    passwordInput.type = showPassword.value ? 'text' : 'password'
    // 触发一个小延迟确保焦点和状态更新
    setTimeout(() => {
      passwordInput.focus()
    }, 10)
  }
}

// 登录处理
const handleLogin = async () => {
  // 清除之前的错误
  accountError.value = ''
  passwordError.value = ''
  
  // 表单验证
  if (!loginForm.account.trim()) {
    accountError.value = '请输入手机号或用户名'
    return
  }
  
  if (!loginForm.password.trim()) {
    passwordError.value = '请输入密码'
    return
  }

  loading.value = true

  try {
    // 调用登录API
    const response = await userAPI.login({
      account: loginForm.account,
      password: loginForm.password
    })

    if (response.success) {
      // 保存token到localStorage
      localStorage.setItem('auth_token', response.data.token)
      localStorage.setItem('user_id', response.data.userId)
      
      // 获取并保存用户信息
      const userInfo = await userAPI.getUserInfo(response.data.userId)
      if (userInfo.success) {
        userAPI.saveUserInfo(userInfo.data)
      }
      
      ElMessage.success('登录成功！')
      
      // 跳转到首页
      router.push('/init')
    } else {
      // 根据错误信息设置相应的错误提示
      if (response.message.includes('账号') || response.message.includes('用户不存在')) {
        accountError.value = response.message
      } else if (response.message.includes('密码') || response.message.includes('密码错误')) {
        passwordError.value = response.message
      } else {
        ElMessage.error(response.message || '登录失败')
      }
    }
  } catch (error) {
    console.error('登录错误:', error)
    
    // 处理不同类型的错误
    if (error.status === 401) {
      accountError.value = '账号或密码错误'
    } else if (error.status === 404) {
      ElMessage.error('服务器连接失败，请检查API地址')
    } else if (error.message.includes('网络连接失败')) {
      ElMessage.error('网络连接失败，请检查网络设置')
    } else if (error.message.includes('请求超时')) {
      ElMessage.error('请求超时，请稍后重试')
    } else {
      ElMessage.error(error.data?.message || error.message || '登录失败')
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.auth-page {
  min-height: 100vh;
  background: linear-gradient(135deg, #f0fdf4 0%, #d1fae5 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.auth-container {
  width: 100%;
  max-width: 1200px;
  min-height: 700px;
  background: white;
  border-radius: 24px;
  overflow: hidden;
  display: flex;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.1);
}

/* 左侧欢迎区域 */
.welcome-section {
  flex: 1;
  background: linear-gradient(135deg, #166534 0%, #22c55e 100%);
  color: white;
  padding: 60px 50px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.welcome-content {
  max-width: 500px;
  margin: 0 auto;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 40px;
}

.logo-icon {
  font-size: 36px;
}

.logo-text {
  font-size: 28px;
  font-weight: 900;
  letter-spacing: -1px;
}

.welcome-title {
  font-size: 40px;
  font-weight: 800;
  margin-bottom: 20px;
  line-height: 1.2;
}

.welcome-subtitle {
  font-size: 18px;
  color: rgba(255, 255, 255, 0.9);
  margin-bottom: 50px;
  line-height: 1.6;
}

.pet-illustration {
  display: flex;
  gap: 20px;
  margin-top: 30px;
}

.pet-item {
  font-size: 48px;
  animation: float 3s ease-in-out infinite;
}

.pet-item:nth-child(2) {
  animation-delay: 0.5s;
}

.pet-item:nth-child(3) {
  animation-delay: 1s;
}

.pet-item:nth-child(4) {
  animation-delay: 1.5s;
}

@keyframes float {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-20px);
  }
}

/* 右侧表单区域 */
.form-section {
  flex: 1;
  padding: 60px 50px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.form-container {
  width: 100%;
  max-width: 420px;
}

.form-header {
  margin-bottom: 40px;
  text-align: center;
}

.form-header h2 {
  font-size: 32px;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 8px;
}

.form-header p {
  color: #64748b;
  font-size: 16px;
}

/* 表单样式 */
.login-form {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

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

.input-with-icon {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 16px;
  color: #94a3b8;
  font-size: 18px;
}

.input-with-icon input {
  width: 100%;
  padding: 14px 16px 14px 50px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  font-size: 15px;
  color: #1e293b;
  background: white;
  transition: all 0.3s;
}

.input-with-icon input:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.input-with-icon input.error {
  border-color: #ef4444;
}

.input-with-icon input.error:focus {
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.password-toggle {
  position: absolute;
  right: 16px;
  background: none;
  border: none;
  color: #94a3b8;
  font-size: 18px;
  cursor: pointer;
  padding: 4px;
  transition: color 0.3s;
}

.password-toggle:hover {
  color: #64748b;
}

.error-message {
  color: #ef4444;
  font-size: 13px;
  margin-top: 4px;
}

/* 记住我和忘记密码 */
.remember-forgot {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  font-size: 14px;
  color: #475569;
}

.checkbox-label input[type="checkbox"] {
  display: none;
}

.checkmark {
  width: 18px;
  height: 18px;
  border: 2px solid #cbd5e1;
  border-radius: 4px;
  position: relative;
  transition: all 0.3s;
}

.checkbox-label input[type="checkbox"]:checked + .checkmark {
  background: #22c55e;
  border-color: #22c55e;
}

.checkbox-label input[type="checkbox"]:checked + .checkmark:after {
  content: "✓";
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 12px;
  font-weight: bold;
}

.forgot-link {
  color: #166534;
  font-size: 14px;
  text-decoration: none;
  font-weight: 500;
  transition: color 0.3s;
}

.forgot-link:hover {
  color: #14532d;
  text-decoration: underline;
}

/* 提交按钮 */
.submit-btn {
  background: #166534;
  color: white;
  border: none;
  padding: 16px;
  border-radius: 12px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  margin-top: 10px;
}

.submit-btn:hover:not(:disabled) {
  background: #14532d;
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(22, 101, 52, 0.3);
}

.submit-btn:disabled {
  background: #94a3b8;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.loading-text {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

.loading-spinner {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.3);
  border-radius: 50%;
  border-top-color: white;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* 第三方登录 */
.divider {
  color: #94a3b8;
  font-size: 14px;
  position: relative;
  margin: 25px 0;
}

.divider:before,
.divider:after {
  content: "";
  position: absolute;
  top: 50%;
  width: 45%;
  height: 1px;
  background: #e2e8f0;
}

.divider:before {
  left: 0;
}

.divider:after {
  right: 0;
}

.social-buttons {
  display: flex;
  gap: 15px;
  justify-content: center;
}

.social-btn {
  flex: 1;
  padding: 14px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  background: white;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
}

.social-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

.social-btn.wechat:hover {
  border-color: #07C160;
  background: #f0fdf0;
}

.social-btn.weibo:hover {
  border-color: #E6162D;
  background: #fff0f0;
}

/* 注册链接 */
.register-link {
  text-align: center;
  margin-top: 30px;
  color: #64748b;
  font-size: 15px;
}

.register-link .link {
  color: #166534;
  font-weight: 600;
  text-decoration: none;
  margin-left: 8px;
  transition: color 0.3s;
}

.register-link .link:hover {
  color: #14532d;
  text-decoration: underline;
}

/* 模态框 */
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
  z-index: 1000;
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

.forgot-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.forgot-form .form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.forgot-form label {
  font-size: 14px;
  color: #475569;
  font-weight: 500;
}

.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  color: #1e293b;
  background: white;
  transition: all 0.3s;
}

.form-input:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.captcha-input {
  display: flex;
  gap: 10px;
}

.captcha-input .form-input {
  flex: 1;
}

.captcha-btn {
  padding: 0 20px;
  background: #f0fdf4;
  color: #166534;
  border: 2px solid #d1fae5;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;
  min-width: 120px;
}

.captcha-btn:hover:not(:disabled) {
  background: #d1fae5;
}

.captcha-btn:disabled {
  background: #f1f5f9;
  color: #94a3b8;
  border-color: #e2e8f0;
  cursor: not-allowed;
}

.modal-actions {
  display: flex;
  gap: 15px;
  justify-content: flex-end;
  padding: 20px 30px 30px;
  border-top: 1px solid #f1f5f9;
}

.btn-secondary,
.btn-primary {
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
  border: 2px solid #e2e8f0;
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

.btn-primary:disabled {
  background: #94a3b8;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

/* 响应式设计 */
@media (max-width: 900px) {
  .auth-container {
    flex-direction: column;
    min-height: auto;
  }
  
  .welcome-section {
    padding: 40px 30px;
  }
  
  .form-section {
    padding: 40px 30px;
  }
  
  .welcome-title {
    font-size: 32px;
  }
  
  .welcome-subtitle {
    font-size: 16px;
  }
  
  .pet-illustration {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .auth-page {
    padding: 10px;
  }
  
  .auth-container {
    border-radius: 16px;
  }
  
  .welcome-section {
    padding: 30px 20px;
  }
  
  .form-section {
    padding: 30px 20px;
  }
  
  .form-header h2 {
    font-size: 28px;
  }
  
  .social-buttons {
    flex-direction: column;
  }
  
  .modal-content {
    margin: 10px;
  }
  
  .modal-actions {
    flex-direction: column;
  }
  
  .btn-secondary,
  .btn-primary {
    width: 100%;
  }
}
</style>