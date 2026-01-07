import './assets/main.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

// 导入定位服务
import { locationService } from './utils/location.js'

const app = createApp(App)

app.use(createPinia())
app.use(router)

// 在应用启动时初始化定位服务
locationService.initialize().catch(error => {
  console.warn('高德地图服务初始化失败，将使用备用定位方式:', error)
})

app.mount('#app')
