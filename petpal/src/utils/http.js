// src/utils/http.js
import { API_BASE_URL, API_CONFIG } from '@/config/api.js'

class HttpRequest {
  constructor() {
    this.baseURL = API_BASE_URL
    this.timeout = API_CONFIG.TIMEOUT
  }

  // 获取完整URL - 处理代理情况
  getFullUrl(url) {
    // 如果url已经是完整URL，直接返回
    if (url.startsWith('http')) {
      return url
    }
    
    // 如果baseURL是相对路径（如/api），直接拼接
    if (this.baseURL.startsWith('/')) {
      return `${this.baseURL}${url}`
    }
    
    // 否则拼接完整URL
    return `${this.baseURL}${url}`
  }

  // 获取请求头
  getHeaders() {
    const headers = {
      'Content-Type': 'application/json',
      'Accept': 'application/json'
    }

    // 添加认证token
    const token = localStorage.getItem('auth_token')
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    return headers
  }

  // 处理响应
async handleResponse(response) {
  const contentType = response.headers.get('content-type')
  
  // 检查响应类型
  if (contentType && contentType.includes('application/json')) {
    const data = await response.json()
    
    // 修改：即使HTTP状态码不是200-299，如果data.success为true，也认为是成功的
    if (!response.ok && !data?.success) {
      const error = {
        status: response.status,
        statusText: response.statusText,
        data
      }
      throw error
    }
    
    return data
  } else {
    // 处理非JSON响应
    const text = await response.text()
    
    if (!response.ok) {
      throw {
        status: response.status,
        statusText: response.statusText,
        data: { message: text }
      }
    }
    
    return { data: text }
  }
}

  // 发送请求
  async request(method, url, data = null) {
    const controller = new AbortController()
    const timeoutId = setTimeout(() => controller.abort(), this.timeout)

    const options = {
      method,
      headers: this.getHeaders(),
      signal: controller.signal,
      mode: 'cors', // 明确指定cors模式
      credentials: 'same-origin' // 对于同域请求
    }

    if (data) {
      options.body = JSON.stringify(data)
    }

    try {
      const fullUrl = this.getFullUrl(url)
      
      // 开发环境调试信息
      if (API_CONFIG.DEBUG) {
        console.group(`[API ${method}] ${fullUrl}`)
        console.log('请求URL:', fullUrl)
        if (data) console.log('请求数据:', data)
        console.log('请求头:', options.headers)
        console.groupEnd()
      }

      const response = await fetch(fullUrl, options)
      
      // 检查网络错误
      if (!response) {
        throw new Error('网络请求失败，请检查网络连接')
      }
      
      // 检查CORS相关错误
      if (response.type === 'opaque' || response.type === 'opaqueredirect') {
        console.warn('CORS警告: 响应类型为', response.type)
      }
      
      const result = await this.handleResponse(response)

      if (API_CONFIG.DEBUG) {
        console.group(`[API Response] ${method} ${fullUrl}`)
        console.log('响应状态:', response.status, response.statusText)
        console.log('响应数据:', result)
        console.groupEnd()
      }

      return result
    } catch (error) {
      console.error('HTTP请求错误:', error)
      
      // 处理不同类型的错误
      if (error.name === 'AbortError') {
        throw new Error(`请求超时（${this.timeout}ms），请检查网络连接或稍后重试`)
      }
      
      // 处理网络连接错误
      if (!navigator.onLine) {
        throw new Error('网络连接失败，请检查网络设置')
      }
      
      // 处理CORS错误
      if (error.message?.includes('CORS') || error.message?.includes('cross-origin')) {
        const fullUrl = this.getFullUrl(url)
        throw new Error(`CORS错误: 无法访问 ${fullUrl}。请确保：
          1. 后端已正确配置CORS
          2. 前端代理配置正确
          3. API地址正确`)
      }
      
      // 处理Fetch API错误
      if (error.message?.includes('Failed to fetch')) {
        const fullUrl = this.getFullUrl(url)
        throw new Error(`网络请求失败: ${fullUrl}。可能原因：
          1. 后端服务器未运行
          2. 网络连接问题
          3. CORS配置错误`)
      }
      
      // 处理HTTP状态码错误
      if (error.status) {
        const statusMessages = {
          400: '请求参数错误',
          401: '未授权，请重新登录',
          403: '访问被拒绝',
          404: '请求的资源不存在',
          409: '资源冲突',
          500: '服务器内部错误',
          502: '网关错误',
          503: '服务不可用',
          504: '网关超时'
        }
        
        const message = statusMessages[error.status] || `HTTP错误 ${error.status}`
        throw {
          status: error.status,
          message: message,
          details: error.data?.message || error.statusText
        }
      }
      
      // 处理其他错误
      throw new Error(`请求失败: ${error.message || '未知错误'}`)
    } finally {
      clearTimeout(timeoutId)
    }
  }

  // GET请求
  get(url, params = null) {
    let queryString = ''
    if (params) {
      queryString = '?' + new URLSearchParams(params).toString()
    }
    return this.request('GET', url + queryString)
  }

  // POST请求
  post(url, data) {
    return this.request('POST', url, data)
  }

  // PUT请求
  put(url, data) {
    return this.request('PUT', url, data)
  }

  // DELETE请求
  delete(url) {
    return this.request('DELETE', url)
  }

  // PATCH请求
  patch(url, data) {
    return this.request('PATCH', url, data)
  }
}

// 创建单例实例
export const http = new HttpRequest()