// src/utils/http.js
import { API_BASE_URL, API_CONFIG } from '@/config/api.js'

class HttpRequest {
  constructor() {
    this.baseURL = API_BASE_URL
    this.timeout = API_CONFIG.TIMEOUT
  }

  // 获取完整URL
  getFullUrl(url) {
    return url.startsWith('http') ? url : `${this.baseURL}${url}`
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
      
      // 如果响应不成功，抛出错误
      if (!response.ok) {
        throw {
          status: response.status,
          statusText: response.statusText,
          data
        }
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
      signal: controller.signal
    }

    if (data) {
      options.body = JSON.stringify(data)
    }

    try {
      const fullUrl = this.getFullUrl(url)
      
      if (API_CONFIG.DEBUG) {
        console.log(`[API Request] ${method} ${fullUrl}`, data)
      }

      const response = await fetch(fullUrl, options)
      const result = await this.handleResponse(response)

      if (API_CONFIG.DEBUG) {
        console.log(`[API Response] ${method} ${fullUrl}`, result)
      }

      return result
    } catch (error) {
      if (error.name === 'AbortError') {
        throw new Error('请求超时，请检查网络连接')
      }
      
      // 处理网络错误
      if (!navigator.onLine) {
        throw new Error('网络连接失败，请检查网络设置')
      }
      
      // 处理HTTP错误
      if (error.status) {
        throw error
      }
      
      // 处理其他错误
      throw new Error(`请求失败: ${error.message}`)
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