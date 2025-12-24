export const API_CONFIG = {
  // 开发环境API地址 - 改为相对路径
  DEV_BASE_URL: '',  // 空字符串或 '/api' 前缀
  
  // 生产环境API地址
  PROD_BASE_URL: 'https://api.petpal.com',
  
  // API版本
  API_VERSION: 'v1',
  
  // 请求超时时间（毫秒）
  TIMEOUT: 30000,
  
  // 是否启用调试模式
  DEBUG: process.env.NODE_ENV !== 'production'
}

// 获取当前环境的API基础URL
export const BASE_URL = API_CONFIG.DEBUG ? 
  API_CONFIG.DEV_BASE_URL : 
  API_CONFIG.PROD_BASE_URL

// 完整API基础路径
export const API_BASE_URL = API_CONFIG.DEBUG ? 
  '/api/v1' :  // 开发环境用相对路径
  `${API_CONFIG.PROD_BASE_URL}/api/${API_CONFIG.API_VERSION}`  // 生产环境用完整URL