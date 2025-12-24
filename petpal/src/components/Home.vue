<template>
  <div class="home-view">
    <header class="welcome-banner">
      <div class="banner-text">
        <h1>欢迎回来，PetPal 守护者！ 👋</h1>
      </div>
      <div class="banner-illustration">🐾</div>
    </header>
  </div>
</template>

<script setup>

</script>

<style scoped>
/* 整个页面撑满右侧区域 */
.home-view {
  width: 100%;
  box-sizing: border-box;
}

/* 欢迎 Banner：横向铺满，使用柔和背景 */
.welcome-banner {
  background: #f0fdf4;
  border-radius: 24px;
  padding: 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.banner-text h1 {
  font-size: 28px;
  color: #166534;
  margin-bottom: 8px;
  font-weight: 800;
}

.banner-text p {
  color: #64748b;
  font-size: 16px;
}

.banner-illustration {
  font-size: 60px;
  opacity: 0.2;
}
</style>