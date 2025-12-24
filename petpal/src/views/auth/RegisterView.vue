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
              <span class="benefit-text">积分奖励系统</span>
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
              <label for="email">邮箱（可选）</label>
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
              <div class="password-hints">
                <div class="hint" :class="{ satisfied: hasUppercase }">至少一个大写字母</div>
                <div class="hint" :class="{ satisfied: hasLowercase }">至少一个小写字母</div>
                <div class="hint" :class="{ satisfied: hasNumber }">至少一个数字</div>
                <div class="hint" :class="{ satisfied: hasSpecialChar }">至少一个特殊字符</div>
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

            <!-- 用户协议 -->
            <div class="agreement">
              <label class="checkbox-label">
                <input type="checkbox" v-model="registerForm.agreeTerms" :class="{ 'error': termsError }">
                <span class="checkmark"></span>
                我已阅读并同意
                <a href="#" class="terms-link">《用户协议》</a>
                和
                <a href="#" class="terms-link">《隐私政策》</a>
              </label>
              <div v-if="termsError" class="error-message">{{ termsError }}</div>
            </div>

            <!-- 注册按钮 -->
            <button type="submit" class="submit-btn" :disabled="loading">
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

<!-- src/views/auth/RegisterView.vue -->
<script setup>
import { ref, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { userAPI } from '@/utils/user.js'

const router = useRouter()

// 注册表单数据
const registerForm = reactive({
  username: '',
  phone: '',
  email: '',
  captcha: '',
  password: '',
  confirmPassword: '',
  hasPet: '',
  agreeTerms: false
})

const loading = ref(false)
const captchaCooldown = ref(0)

// 注册处理
const handleRegister = async () => {
  // 验证表单
  loading.value = true

  try {
    // 调用注册API
    const response = await userAPI.register({
      username: registerForm.username,
      password: registerForm.password,
      phone: registerForm.phone,
      email: registerForm.email || undefined,
      captcha: registerForm.captcha
    })

    if (response.success) {
      // 保存token到localStorage
      localStorage.setItem('auth_token', response.data.token)
      localStorage.setItem('user_id', response.data.userId)
      
      // 保存基本的用户信息
      userAPI.saveUserInfo({
        name: registerForm.username,
        level: 1,
        role: 'member'
      })
      
      ElMessage.success('注册成功！欢迎加入宠物互助平台')
      
      // 跳转到首页
      router.push('/init')
    } else {
      // 处理错误信息
      ElMessage.error(response.message || '注册失败')
    }
  } catch (error) {
    console.error('注册错误:', error)
    
    // 处理不同类型的错误
    if (error.status === 400) {
      ElMessage.error('注册信息有误，请检查输入')
    } else if (error.status === 409) {
      ElMessage.error('用户已存在，请直接登录')
    } else if (error.message.includes('网络连接失败')) {
      ElMessage.error('网络连接失败，请检查网络设置')
    } else {
      ElMessage.error(error.data?.message || error.message || '注册失败')
    }
  } finally {
    loading.value = false
  }
}

// 发送验证码
const sendCaptcha = async () => {
  if (!registerForm.phone.trim()) {
    ElMessage.warning('请输入手机号')
    return
  }

  if (!/^1[3-9]\d{9}$/.test(registerForm.phone)) {
    ElMessage.warning('请输入正确的手机号')
    return
  }

  captchaCooldown.value = 60
  
  try {
    const response = await userAPI.sendCaptcha(registerForm.phone)
    
    if (response.success) {
      ElMessage.success('验证码已发送')
      
      // 启动倒计时
      const timer = setInterval(() => {
        captchaCooldown.value--
        if (captchaCooldown.value <= 0) {
          clearInterval(timer)
        }
      }, 1000)
      
      // 组件卸载时清除定时器
      onUnmounted(() => clearInterval(timer))
    } else {
      ElMessage.error(response.message || '发送验证码失败')
      captchaCooldown.value = 0
    }
  } catch (error) {
    console.error('发送验证码错误:', error)
    ElMessage.error('网络错误，请稍后重试')
    captchaCooldown.value = 0
  }
}
</script>

<style scoped>
/* 复用登录页面的基础样式 */
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
}

.benefit-item {
  display: flex;
  align-items: center;
  gap: 15px;
}

.benefit-icon {
  font-size: 24px;
  width: 40px;
  text-align: center;
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
  overflow-y: auto;
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

/* 注册表单样式 */
.register-form {
  display: flex;
  flex-direction: column;
  gap: 25px;
}

/* 验证码输入样式 */
.captcha-input {
  display: flex;
  gap: 10px;
}

.captcha-input .input-with-icon {
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

/* 密码强度指示器 */
.password-strength {
  margin-top: 10px;
}

.strength-meter {
  height: 6px;
  background: #f1f5f9;
  border-radius: 3px;
  overflow: hidden;
  margin-bottom: 6px;
}

.strength-fill {
  height: 100%;
  transition: width 0.3s;
}

.strength-fill.weak {
  background: #ef4444;
}

.strength-fill.fair {
  background: #f59e0b;
}

.strength-fill.good {
  background: #3b82f6;
}

.strength-fill.strong {
  background: #22c55e;
}

.strength-text {
  font-size: 12px;
  color: #64748b;
  text-align: right;
}

/* 密码提示 */
.password-hints {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 8px;
  margin-top: 10px;
}

.hint {
  font-size: 12px;
  color: #94a3b8;
  display: flex;
  align-items: center;
  gap: 6px;
}

.hint:before {
  content: "○";
  font-size: 8px;
}

.hint.satisfied {
  color: #22c55e;
}

.hint.satisfied:before {
  content: "✓";
  color: #22c55e;
  font-weight: bold;
}

/* 宠物选项 */
.pet-options {
  display: flex;
  gap: 15px;
}

.option-label {
  flex: 1;
  position: relative;
}

.option-label input[type="radio"] {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.option-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  padding: 20px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s;
  text-align: center;
}

.option-label:hover .option-content {
  border-color: #cbd5e1;
  background: #f8fafc;
}

.option-label.selected .option-content {
  border-color: #22c55e;
  background: #f0fdf4;
  color: #166534;
}

.option-icon {
  font-size: 28px;
}

.option-text {
  font-weight: 500;
  font-size: 14px;
}

/* 用户协议 */
.agreement {
  margin: 10px 0;
}

.checkbox-label {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  cursor: pointer;
  font-size: 14px;
  color: #475569;
  line-height: 1.5;
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
  flex-shrink: 0;
  margin-top: 2px;
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

.checkbox-label input[type="checkbox"].error + .checkmark {
  border-color: #ef4444;
}

.terms-link {
  color: #166534;
  text-decoration: none;
  font-weight: 500;
}

.terms-link:hover {
  text-decoration: underline;
}

/* 注册按钮 */
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
  
  .password-hints {
    grid-template-columns: 1fr;
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
  
  .pet-options {
    flex-direction: column;
  }
  
  .captcha-input {
    flex-direction: column;
  }
  
  .captcha-btn {
    width: 100%;
    padding: 12px;
  }
}
</style>