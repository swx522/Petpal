// src/config/api.js
export const API_CONFIG = {
  // 开发环境API地址 - 使用完整的后端地址
  DEV_BASE_URL: 'http://127.0.0.1:5002',  // 你的后端地址
  
  // 生产环境API地址
  PROD_BASE_URL: 'https://api.petpal.com',
  
  // 请求超时时间（毫秒）
  TIMEOUT: 30000,
  
  // 是否启用调试模式
  DEBUG: import.meta.env.DEV
}

// 获取当前环境的API基础URL
export const BASE_URL = API_CONFIG.DEBUG ? 
  API_CONFIG.DEV_BASE_URL : 
  API_CONFIG.PROD_BASE_URL

export const API_BASE_URL = import.meta.env.DEV
  ? '/api'  // Vite代理路径
  : 'https://api.petpal.com/api'