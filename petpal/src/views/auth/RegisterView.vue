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
              <label for="email">邮箱 *</label>
              <div class="input-with-icon">
                <span class="input-icon">📧</span>
                <input
                  id="email"
                  v-model="registerForm.email"
                  type="email"
                  placeholder="请输入邮箱地址"
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

            <!-- 角色选择 -->
            <div class="form-group">
              <label class="form-label">请选择您的角色 *</label>
              <p class="role-hint">选择您在社区中的主要身份（注册后可申请其他角色）</p>
              
              <div class="role-options">
                <div 
                  class="role-option" 
                  :class="{ 'selected': registerForm.role === 'owner' }"
                  @click="selectRole('owner')"
                >
                  <div class="role-icon">🐶</div>
                  <div class="role-info">
                    <h4 class="role-title">宠物主人</h4>
                    <p class="role-description">我有宠物，需要帮助</p>
                    <ul class="role-features">
                      <li>发布宠物照看需求</li>
                      <li>寻找可靠的服务者</li>
                      <li>管理我的宠物信息</li>
                    </ul>
                  </div>
                  <div class="role-selector">
                    <div class="selector-circle" :class="{ 'selected': registerForm.role === 'owner' }"></div>
                  </div>
                </div>
                
                <div 
                  class="role-option" 
                  :class="{ 'selected': registerForm.role === 'sitter' }"
                  @click="selectRole('sitter')"
                >
                  <div class="role-icon">🦴</div>
                  <div class="role-info">
                    <h4 class="role-title">宠物服务者</h4>
                    <p class="role-description">我喜欢宠物，提供帮助</p>
                    <ul class="role-features">
                      <li>接单赚取额外收入</li>
                      <li>帮助照顾可爱宠物</li>
                      <li>建立服务信誉</li>
                    </ul>
                  </div>
                  <div class="role-selector">
                    <div class="selector-circle" :class="{ 'selected': registerForm.role === 'sitter' }"></div>
                  </div>
                </div>
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

// 注册表单数据 - 添加role字段
const registerForm = reactive({
  username: '',
  phone: '',
  email: '',
  captcha: '',
  password: '',
  confirmPassword: '',
  role: '', // 新增：用户角色
  agreeTerms: false
})

// 错误信息
const usernameError = ref('')
const phoneError = ref('')
const emailError = ref('')
const passwordError = ref('')
const confirmPasswordError = ref('')
const roleError = ref('') // 新增：角色错误信息

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
    registerForm.role && // 必须选择角色
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

// 选择角色
const selectRole = (role) => {
  registerForm.role = role
  roleError.value = ''
}

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
  }
}

// 注册处理 - 添加角色信息
const handleRegister = async () => {
  // 表单验证
  let isValid = true
  
  if (!registerForm.username.trim()) {
    usernameError.value = '请输入用户名'
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
    // 调用注册API - 添加角色信息
    const response = await userAPI.register({
      username: registerForm.username,
      password: registerForm.password,
      phone: registerForm.phone,
      email: registerForm.email || undefined,
      captcha: registerForm.captcha,
      role: registerForm.role // 新增：传递角色信息
    })

    if (response.success) {
      // 保存token到localStorage
      localStorage.setItem('auth_token', response.data.token)
      localStorage.setItem('user_id', response.data.userId)
      
      // 保存用户信息，包括角色
      userAPI.saveUserInfo({
        name: registerForm.username,
        level: 1,
        role: registerForm.role, // 保存角色
        phone: registerForm.phone,
        email: registerForm.email
      })
      
      // 保存角色到本地存储，供Layout.vue使用
      localStorage.setItem('petpal_userRole', registerForm.role)
      
      ElMessage.success(`注册成功！欢迎加入宠物互助平台，您已注册为${registerForm.role === 'owner' ? '宠物主人' : '宠物服务者'}`)
      
      // 跳转到首页
      router.push('/init')
    } else {
      ElMessage.error(response.message || '注册失败')
    }
  } catch (error) {
    console.error('注册错误:', error)
    
    if (error.status === 400) {
      ElMessage.error('注册信息有误，请检查输入')
    } else if (error.status === 409) {
      ElMessage.error('用户已存在，请直接登录')
    } else if (error.message?.includes('网络连接失败')) {
      ElMessage.error('网络连接失败，请检查网络设置')
    } else {
      ElMessage.error(error.data?.message || error.message || '注册失败')
    }
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
/* 复用已有的基础样式，只添加新组件的样式 */

/* 角色选择样式 */
.form-label {
  display: block;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
  font-size: 14px;
}

.role-hint {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 16px;
}

.role-options {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.role-option {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s;
  position: relative;
}

.role-option:hover {
  border-color: #cbd5e1;
  background: #f8fafc;
}

.role-option.selected {
  border-color: #22c55e;
  background: #f0fdf4;
}

.role-icon {
  font-size: 32px;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.role-info {
  flex: 1;
}

.role-title {
  font-size: 16px;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 4px 0;
}

.role-description {
  font-size: 13px;
  color: #64748b;
  margin: 0 0 8px 0;
}

.role-features {
  list-style: none;
  padding: 0;
  margin: 0;
}

.role-features li {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 4px;
  display: flex;
  align-items: center;
}

.role-features li:before {
  content: "•";
  color: #22c55e;
  margin-right: 6px;
  font-weight: bold;
}

.role-selector {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 24px;
  height: 24px;
  flex-shrink: 0;
}

.selector-circle {
  width: 20px;
  height: 20px;
  border: 2px solid #cbd5e1;
  border-radius: 50%;
  transition: all 0.3s;
  position: relative;
}

.selector-circle.selected {
  border-color: #22c55e;
  background: #22c55e;
}

.selector-circle.selected:after {
  content: "✓";
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-size: 12px;
  font-weight: bold;
}

/* 错误信息样式 */
.error-message {
  color: #ef4444;
  font-size: 12px;
  margin-top: 6px;
}

/* 输入框错误状态 */
.input-with-icon input.error {
  border-color: #ef4444 !important;
}

.input-with-icon input.error:focus {
  border-color: #ef4444 !important;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

/* 响应式调整 */
@media (max-width: 768px) {
  .role-option {
    flex-direction: column;
    text-align: center;
    gap: 12px;
  }
  
  .role-info {
    text-align: center;
  }
  
  .role-features li {
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .role-option {
    padding: 12px;
  }
  
  .role-icon {
    font-size: 28px;
    width: 40px;
    height: 40px;
  }
}
</style>