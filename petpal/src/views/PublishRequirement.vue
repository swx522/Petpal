<!-- src/views/PublishRequirement.vue -->
<template>
  <div class="publish-requirement">
    <!-- 页面标题 -->
    <div class="page-header">
      <h1>发布需求</h1>
      <p>填写您的宠物需求信息，社区成员会来帮助您</p>
    </div>

    <!-- 发布需求表单 -->
    <div class="publish-form">
      <div class="form-card">
        <h3>需求基本信息</h3>
        
        <!-- 宠物信息 -->
        <div class="form-section">
          <h4>宠物信息</h4>
          <div class="form-grid">
            <div class="form-group">
              <label>宠物类型 *</label>
              <select class="form-select">
                <option value="">请选择宠物类型</option>
                <option value="dog">狗狗 🐶</option>
                <option value="cat">猫咪 🐱</option>
                <option value="rabbit">兔兔 🐰</option>
                <option value="bird">鸟鸟 🐦</option>
                <option value="other">其他 🐾</option>
              </select>
            </div>
            
            <div class="form-group">
              <label>宠物昵称 *</label>
              <input type="text" placeholder="如：多多、花花" class="form-input">
            </div>
          </div>
        </div>

        <!-- 需求类型 -->
        <div class="form-section">
          <h4>需求类型</h4>
          <div class="requirement-types">
            <div class="type-card" :class="{ active: selectedType === 'walk' }" @click="selectedType = 'walk'">
              <div class="type-icon">🚶</div>
              <div class="type-info">
                <h5>遛狗服务</h5>
                <p>需要帮忙遛宠物</p>
              </div>
            </div>
            
            <div class="type-card" :class="{ active: selectedType === 'feed' }" @click="selectedType = 'feed'">
              <div class="type-icon">🍽️</div>
              <div class="type-info">
                <h5>喂食照顾</h5>
                <p>临时喂食照顾</p>
              </div>
            </div>
            
            <div class="type-card" :class="{ active: selectedType === 'medical' }" @click="selectedType = 'medical'">
              <div class="type-icon">🏥</div>
              <div class="type-info">
                <h5>就医陪伴</h5>
                <p>需要陪宠物就医</p>
              </div>
            </div>
            
            <div class="type-card" :class="{ active: selectedType === 'groom' }" @click="selectedType = 'groom'">
              <div class="type-icon">✂️</div>
              <div class="type-info">
                <h5>美容护理</h5>
                <p>洗澡、修剪等</p>
              </div>
            </div>
          </div>
        </div>

        <!-- 时间安排 -->
        <div class="form-section">
          <h4>时间安排</h4>
          <div class="form-grid">
            <div class="form-group">
              <label>开始时间 *</label>
              <input type="datetime-local" class="form-input">
            </div>
            
            <div class="form-group">
              <label>结束时间 *</label>
              <input type="datetime-local" class="form-input">
            </div>
          </div>
        </div>

        <!-- 详细描述 -->
        <div class="form-section">
          <h4>需求描述</h4>
          <textarea 
            class="form-textarea" 
            placeholder="请详细描述您的需求，包括宠物的特殊习惯、注意事项等..."
            rows="4"
          ></textarea>
        </div>

        <!-- 联系方式 -->
        <div class="form-section">
          <h4>联系方式</h4>
          <div class="contact-info">
            <div class="contact-method">
              <input type="checkbox" id="contact-wechat" checked>
              <label for="contact-wechat">邮箱联系</label>
            </div>
            <div class="contact-method">
              <input type="checkbox" id="contact-phone">
              <label for="contact-phone">电话联系</label>
            </div>
          </div>
          <input type="text" placeholder="默认展示邮箱" class="form-input mt-2">
        </div>

        <!-- 提交按钮 -->
        <div class="form-actions">
          <button class="btn-primary">立即发布</button>
        </div>
      </div>

      <!-- 侧边提示 -->
      <div class="side-tips">
        <div class="tips-card">
          <h4>发布提示</h4>
          <ul class="tips-list">
            <li>尽量详细描述需求，提高匹配度</li>
            <li>设置合理的悬赏积分</li>
            <li>确认联系方式正确</li>
            <li>完成后请及时确认订单</li>
          </ul>
        </div>
        
        <div class="stats-card">
          <div class="stats-item">
            <div class="stat-label">信誉积分</div>
            <div class="stat-value">1560 分</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const selectedType = ref('walk')
const rewardPoints = ref(50)
</script>

<style scoped>
.publish-requirement {
  width: 100%;
  box-sizing: border-box;
}

/* 页面标题 */
.page-header {
  margin-bottom: 40px;
}

.page-header h1 {
  font-size: 32px;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 8px;
}

.page-header p {
  color: #64748b;
  font-size: 16px;
}

/* 发布表单布局 */
.publish-form {
  display: flex;
  gap: 30px;
}

.form-card {
  flex: 2;
  background: #ffffff;
  border: 1px solid #f1f5f9;
  border-radius: 20px;
  padding: 40px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.side-tips {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* 表单卡片标题 */
.form-card h3 {
  font-size: 24px;
  color: #166534;
  margin-bottom: 30px;
  font-weight: 700;
}

/* 表单部分 */
.form-section {
  margin-bottom: 40px;
}

.form-section h4 {
  font-size: 18px;
  color: #334155;
  margin-bottom: 20px;
  font-weight: 600;
}

/* 表单网格 */
.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

/* 表单元素 */
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

.form-input,
.form-select {
  padding: 12px 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  background: white;
  transition: border-color 0.2s;
}

.form-input:focus,
.form-select:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

/* 需求类型卡片 */
.requirement-types {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 15px;
}

.type-card {
  border: 2px solid #f1f5f9;
  border-radius: 12px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 15px;
}

.type-card:hover {
  border-color: #d1fae5;
  transform: translateY(-2px);
}

.type-card.active {
  border-color: #22c55e;
  background: #f0fdf4;
}

.type-icon {
  font-size: 32px;
}

.type-info h5 {
  font-size: 16px;
  color: #1e293b;
  margin-bottom: 4px;
  font-weight: 600;
}

.type-info p {
  font-size: 13px;
  color: #64748b;
}

/* 文本区域 */
.form-textarea {
  width: 100%;
  padding: 16px;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  font-size: 14px;
  resize: vertical;
  transition: border-color 0.2s;
}

.form-textarea:focus {
  outline: none;
  border-color: #22c55e;
  box-shadow: 0 0 0 3px rgba(34, 197, 94, 0.1);
}

/* 联系方式 */
.contact-info {
  display: flex;
  gap: 30px;
  margin-bottom: 15px;
}

.contact-method {
  display: flex;
  align-items: center;
  gap: 8px;
}

.contact-method input[type="checkbox"] {
  width: 18px;
  height: 18px;
}

.contact-method label {
  font-size: 14px;
  color: #475569;
  cursor: pointer;
}

.mt-2 {
  margin-top: 10px;
}

/* 悬赏积分 */
.reward-section {
  background: #f8fafc;
  padding: 25px;
  border-radius: 12px;
}

.reward-slider {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 10px;
}

.slider {
  flex: 1;
  height: 6px;
  border-radius: 3px;
  background: #e2e8f0;
  outline: none;
  -webkit-appearance: none;
}

.slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #22c55e;
  cursor: pointer;
  border: 3px solid white;
  box-shadow: 0 2px 6px rgba(0,0,0,0.15);
}

.reward-value {
  font-size: 20px;
  font-weight: 700;
  color: #166534;
  min-width: 100px;
  text-align: center;
}

.reward-note {
  font-size: 13px;
  color: #94a3b8;
  text-align: center;
}

/* 表单操作按钮 */
.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: 15px;
  padding-top: 30px;
  border-top: 1px solid #f1f5f9;
}

.btn-primary,
.btn-secondary {
  padding: 12px 32px;
  border-radius: 10px;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-primary {
  background: #166534;
  color: white;
  border: none;
}

.btn-primary:hover {
  background: #14532d;
  transform: translateY(-2px);
  box-shadow: 0 6px 12px rgba(22, 101, 52, 0.2);
}

.btn-secondary {
  background: white;
  color: #475569;
  border: 1px solid #e2e8f0;
}

.btn-secondary:hover {
  background: #f8fafc;
  border-color: #cbd5e1;
}

/* 侧边提示卡片 */
.tips-card,
.stats-card {
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 16px;
  padding: 25px;
}

.tips-card h4,
.stats-card h4 {
  font-size: 18px;
  color: #1e293b;
  margin-bottom: 20px;
  font-weight: 600;
}

.tips-list {
  list-style: none;
  padding: 0;
}

.tips-list li {
  padding: 10px 0;
  color: #64748b;
  font-size: 14px;
  border-bottom: 1px solid #f1f5f9;
}

.tips-list li:last-child {
  border-bottom: none;
}

.tips-list li:before {
  content: "✓";
  color: #22c55e;
  font-weight: bold;
  margin-right: 10px;
}

/* 统计数据 */
.stats-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f1f5f9;
}

.stats-item:last-child {
  border-bottom: none;
}

.stat-label {
  color: #64748b;
  font-size: 14px;
}

.stat-value {
  color: #166534;
  font-size: 20px;
  font-weight: 700;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .publish-form {
    flex-direction: column;
  }
  
  .form-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .form-card {
    padding: 25px;
  }
  
  .requirement-types {
    grid-template-columns: 1fr;
  }
  
  .contact-info {
    flex-direction: column;
    gap: 15px;
  }
  
  .form-actions {
    flex-direction: column;
  }
  
  .btn-primary,
  .btn-secondary {
    width: 100%;
  }
}
</style>