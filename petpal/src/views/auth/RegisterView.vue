<!-- src/views/auth/RegisterView.vue -->
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
          <h1 class="welcome-title">加入PetPal</h1>
          <p class="welcome-subtitle">加入我们的宠物互助社区，开始帮助更多可爱的宠物</p>
          <div class="benefits">
            <div class="benefit-item">
              <span class="benefit-icon">✨</span>
              <span class="benefit-text">专业的宠物社区</span>
            </div>
            <div class="benefit-item">
              <span class="benefit-icon">🤝</span>
              <span class="benefit-text">互帮互助的平台</span>
            </div>
            <div class="benefit-item">
              <span class="benefit-icon">🏆</span>
              <span class="benefit-text">信誉评价系统</span>
            </div>
            <div class="benefit-item">
              <span class="benefit-icon">🛡️</span>
              <span class="benefit-text">安全可靠的环境</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧注册表单 -->
      <div class="form-section">
        <div class="form-container">
          <div class="form-header">
            <h2>创建账户</h2>
            <p>填写基本信息，开启宠物互助之旅</p>
          </div>

          <form @submit.prevent="handleRegister" class="register-form">
            <!-- 用户名 -->
            <div class="form-group">
              <label for="username">用户名 *</label>
              <div class="input-with-icon">
                <span class="input-icon">👤</span>
                <input
                  id="username"
                  v-model="registerForm.username"
                  type="text"
                  placeholder="请输入用户名（2-20位字符）"
                  required
                  :class="{ 'error': usernameError }"
                  @input="clearError('username')"
                >
              </div>
              <div v-if="usernameError" class="error-message">{{ usernameError }}</div>
            </div>

            <!-- 手机号 -->
            <div class="form-group">
              <label for="phone">手机号 *</label>
              <div class="input-with-icon">
                <span class="input-icon">📱</span>
                <input
                  id="phone"
                  v-model="registerForm.phone"
                  type="tel"
                  placeholder="请输入手机号"
                  required
                  :class="{ 'error': phoneError }"
                  @input="clearError('phone')"
                >
              </div>
              <div v-if="phoneError" class="error-message">{{ phoneError }}</div>
            </div>

            <!-- 邮箱 -->
            <div class="form-group">
              <label for="email">邮箱</label>
              <div class="input-with-icon">
                <span class="input-icon">📧</span>
                <input
                  id="email"
                  v-model="registerForm.email"
                  type="email"
                  placeholder="请输入邮箱地址（选填）"
                  :class="{ 'error': emailError }"
                  @input="clearError('email')"
                >
              </div>
              <div v-if="emailError" class="error-message">{{ emailError }}</div>
            </div>

            <!-- 密码 -->
            <div class="form-group">
              <label for="password">密码 *</label>
              <div class="input-with-icon">
                <span class="input-icon">🔒</span>
                <input
                  id="password"
                  v-model="registerForm.password"
                  :type="showPassword ? 'text' : 'password'"
                  placeholder="请输入密码（6-20位字符）"
                  required
                  :class="{ 'error': passwordError }"
                  @input="clearError('password')"
                >
                <button
                  type="button"
                  class="password-toggle"
                  @click="showPassword = !showPassword"
                >
                  {{ showPassword ? '👁️' : '👁️‍🗨️' }}
                </button>
              </div>
              <div v-if="passwordError" class="error-message">{{ passwordError }}</div>
              <div class="password-strength" v-if="registerForm.password">
                <div class="strength-meter">
                  <div class="strength-fill" :style="{ width: passwordStrength + '%' }" :class="strengthClass"></div>
                </div>
                <div class="strength-text">{{ strengthText }}</div>
              </div>
            </div>

            <!-- 确认密码 -->
            <div class="form-group">
              <label for="confirmPassword">确认密码 *</label>
              <div class="input-with-icon">
                <span class="input-icon">🔒</span>
                <input
                  id="confirmPassword"
                  v-model="registerForm.confirmPassword"
                  :type="showConfirmPassword ? 'text' : 'password'"
                  placeholder="请再次输入密码"
                  required
                  :class="{ 'error': confirmPasswordError }"
                  @input="clearError('confirmPassword')"
                >
                <button
                  type="button"
                  class="password-toggle"
                  @click="showConfirmPassword = !showConfirmPassword"
                >
                  {{ showConfirmPassword ? '👁️' : '👁️‍🗨️' }}
                </button>
              </div>
              <div v-if="confirmPasswordError" class="error-message">{{ confirmPasswordError }}</div>
            </div>

            <!-- 角色选择（下拉框） -->
            <div class="form-group">
              <label for="role">请选择您的角色 *</label>
              <p class="role-hint">选择您在社区中的主要身份</p>
              <div class="input-with-icon">
                <span class="input-icon">👥</span>
                <select
                  id="role"
                  v-model="registerForm.role"
                  :class="{ 'error': roleError }"
                  @change="clearError('role')"
                  required
                >
                  <option value="" disabled selected>请选择角色</option>
                  <option value="User">宠物主人</option>
                  <option value="Sitter">宠物服务者</option>
                </select>
              </div>
              <div v-if="roleError" class="error-message">{{ roleError }}</div>
            </div>

            <!-- 注册按钮 -->
            <button type="submit" class="submit-btn" :disabled="loading || !isFormValid">
              <span v-if="!loading">注册账户</span>
              <span v-else class="loading-text">
                <span class="loading-spinner"></span> 注册中...
              </span>
            </button>

            <!-- 登录链接 -->
            <div class="login-link">
              已有账户？
              <router-link to="/login" class="link">立即登录</router-link>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { userAPI } from '@/utils/user.js'

const router = useRouter()

// 注册表单数据
const registerForm = reactive({
  username: '',
  phone: '',
  email: '',
  password: '',
  confirmPassword: '',
  role: '', // 用户角色
})

// 错误信息
const usernameError = ref('')
const phoneError = ref('')
const emailError = ref('')
const passwordError = ref('')
const confirmPasswordError = ref('')
const roleError = ref('')

const loading = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)

// 表单验证
const isFormValid = computed(() => {
  return (
    registerForm.username.trim() &&
    registerForm.phone.trim() &&
    registerForm.password.trim() &&
    registerForm.confirmPassword.trim() &&
    registerForm.role &&
    registerForm.password === registerForm.confirmPassword
  )
})

// 密码强度计算
const passwordStrength = computed(() => {
  const password = registerForm.password
  if (!password) return 0
  
  let score = 0
  if (password.length >= 6) score += 20
  if (password.length >= 8) score += 20
  if (/[A-Z]/.test(password)) score += 20
  if (/[0-9]/.test(password)) score += 20
  if (/[^A-Za-z0-9]/.test(password)) score += 20
  
  return Math.min(100, score)
})

const strengthClass = computed(() => {
  const strength = passwordStrength.value
  if (strength <= 40) return 'weak'
  if (strength <= 60) return 'fair'
  if (strength <= 80) return 'good'
  return 'strong'
})

const strengthText = computed(() => {
  const strength = passwordStrength.value
  if (strength <= 40) return '密码强度：弱'
  if (strength <= 60) return '密码强度：一般'
  if (strength <= 80) return '密码强度：良好'
  return '密码强度：强'
})

// 清除错误信息
const clearError = (field) => {
  switch(field) {
    case 'username':
      usernameError.value = ''
      break
    case 'phone':
      phoneError.value = ''
      break
    case 'email':
      emailError.value = ''
      break
    case 'password':
      passwordError.value = ''
      break
    case 'confirmPassword':
      confirmPasswordError.value = ''
      break
    case 'role':
      roleError.value = ''
      break
  }
}

// 注册处理
const handleRegister = async () => {
  // 清除之前的错误
  usernameError.value = ''
  phoneError.value = ''
  emailError.value = ''
  passwordError.value = ''
  confirmPasswordError.value = ''
  roleError.value = ''
  
  // 表单验证
  let isValid = true
  
  if (!registerForm.username.trim()) {
    usernameError.value = '请输入用户名'
    isValid = false
  } else if (registerForm.username.length < 2 || registerForm.username.length > 20) {
    usernameError.value = '用户名长度应为2-20位字符'
    isValid = false
  }
  
  if (!registerForm.phone.trim()) {
    phoneError.value = '请输入手机号'
    isValid = false
  } else if (!/^1[3-9]\d{9}$/.test(registerForm.phone)) {
    phoneError.value = '请输入正确的手机号'
    isValid = false
  }
  
  if (!registerForm.password) {
    passwordError.value = '请输入密码'
    isValid = false
  } else if (registerForm.password.length < 6) {
    passwordError.value = '密码长度至少6位'
    isValid = false
  }
  
  if (registerForm.password !== registerForm.confirmPassword) {
    confirmPasswordError.value = '两次输入的密码不一致'
    isValid = false
  }
  
  if (!registerForm.role) {
    roleError.value = '请选择您的角色'
    isValid = false
  }
  
  if (!isValid) {
    ElMessage.warning('请完善注册信息')
    return
  }

  loading.value = true

  try {
    // 调用注册API
    const response = await userAPI.register({
      username: registerForm.username,
      password: registerForm.password,
      phone: registerForm.phone,
      email: registerForm.email || undefined,
      role: registerForm.role === 'User' ? 0 : 1  // 数字类型：0-user, 1-sitter
    })

    if (response.success) {
      // 保存token到localStorage
      userAPI.saveLoginState(
        response.data.token,
        response.data.userId,
        {
          username: registerForm.username,
          role: registerForm.role,
          phone: registerForm.phone,
          email: registerForm.email
        }
      )
      
      const roleText = registerForm.role === 'User' ? '宠物主人' : '宠物服务者'
      ElMessage.success(`注册成功！欢迎加入宠物互助平台，您已注册为${roleText}`)
      
      // 跳转到首页
      router.push('/')
    } else {
      // 处理API返回的错误信息
      if (response.message.includes('用户名') || response.message.includes('Username')) {
        usernameError.value = response.message
      } else if (response.message.includes('手机号') || response.message.includes('Phone')) {
        phoneError.value = response.message
      } else if (response.message.includes('邮箱') || response.message.includes('Email')) {
        emailError.value = response.message
      } else {
        ElMessage.error(response.message || '注册失败')
      }
    }
  } catch (error) {
    console.error('注册错误:', error)
    
    // 处理HTTP错误
    if (error.status === 400) {
      ElMessage.error('注册信息有误，请检查输入')
    } else if (error.status === 409) {
      ElMessage.error('用户已存在，请直接登录')
    } else if (error.message?.includes('网络连接失败')) {
      ElMessage.error('网络连接失败，请检查网络设置')
    } else if (error.message?.includes('请求超时')) {
      ElMessage.error('请求超时，请稍后重试')
    } else {
      ElMessage.error(error.data?.message || error.message || '注册失败')
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

.benefits {
  display: flex;
  flex-direction: column;
  gap: 20px;
  margin-top: 30px;
}

.benefit-item {
  display: flex;
  align-items: center;
  gap: 15px;
}

.benefit-icon {
  font-size: 24px;
}

.benefit-text {
  font-size: 16px;
  color: rgba(255, 255, 255, 0.95);
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
.register-form {
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

.role-hint {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 4px;
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
  z-index: 1;
}

.input-with-icon input,
.input-with-icon select {
  width: 100%;
  padding: 14px 16px 14px 50px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  font-size: 15px;
  color: #1e293b;
  background: white;
  transition: all 0.3s;
  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;
}

.input-with-icon select {
  cursor: pointer;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='16' height='16' viewBox='0 0 24 24' fill='none' stroke='%2364748b' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpolyline points='6 9 12 15 18 9'%3E%3C/polyline%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 16px center;
  background-size: 16px;
  padding-right: 40px;
}

.input-with-icon input:focus,
.input-with-icon select:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

.input-with-icon input.error,
.input-with-icon select.error {
  border-color: #ef4444;
}

.input-with-icon input.error:focus,
.input-with-icon select.error:focus {
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
  z-index: 1;
}

.password-toggle:hover {
  color: #64748b;
}

.error-message {
  color: #ef4444;
  font-size: 13px;
  margin-top: 4px;
}

/* 密码强度指示器 */
.password-strength {
  margin-top: 8px;
}

.strength-meter {
  height: 6px;
  background: #e2e8f0;
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 4px;
}

.strength-fill {
  height: 100%;
  transition: width 0.3s, background-color 0.3s;
}

.strength-fill.weak {
  background-color: #ef4444;
}

.strength-fill.fair {
  background-color: #f59e0b;
}

.strength-fill.good {
  background-color: #3b82f6;
}

.strength-fill.strong {
  background-color: #22c55e;
}

.strength-text {
  font-size: 12px;
  color: #64748b;
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

/* 登录链接 */
.login-link {
  text-align: center;
  margin-top: 30px;
  color: #64748b;
  font-size: 15px;
}

.login-link .link {
  color: #166534;
  font-weight: 600;
  text-decoration: none;
  margin-left: 8px;
  transition: color 0.3s;
}

.login-link .link:hover {
  color: #14532d;
  text-decoration: underline;
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
  
  .benefits {
    gap: 15px;
  }
  
  .benefit-item {
    gap: 12px;
  }
  
  .benefit-icon {
    font-size: 20px;
  }
  
  .benefit-text {
    font-size: 14px;
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
  
  .input-with-icon input,
  .input-with-icon select {
    padding: 12px 16px 12px 50px;
    font-size: 14px;
  }
  
  .submit-btn {
    padding: 14px;
    font-size: 15px;
  }
}
</style>