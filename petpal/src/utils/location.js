// src/utils/location.js
import AMapLoader from '@amap/amap-jsapi-loader'
import { userAPI } from './user.js'

// 高德地图配置
const AMAP_CONFIG = {
  key: '3a494517a046a80814d9faf52d939ddd', // 高德API Key
  version: '2.0',
  plugins: ['AMap.Geolocation', 'AMap.Geocoder']
}

// 定位服务类
class LocationService {
  constructor() {
    this.amap = null
    this.geolocation = null
    this.isInitialized = false
  }

  /**
   * 初始化高德地图
   */
  async initialize() {
    if (this.isInitialized) return

    try {
      this.amap = await AMapLoader.load({
        key: AMAP_CONFIG.key,
        version: AMAP_CONFIG.version,
        plugins: AMAP_CONFIG.plugins
      })
      this.isInitialized = true
      console.log('🎯 高德地图初始化成功')
    } catch (error) {
      console.error('🎯 高德地图初始化失败:', error)
      throw new Error('高德地图服务初始化失败，请检查网络连接')
    }
  }

  /**
   * 获取用户当前位置
   * @param {Object} options - 定位选项
   * @returns {Promise<{latitude: number, longitude: number, address?: string}>}
   */
  async getCurrentPosition(options = {}) {
    await this.initialize()

    return new Promise((resolve, reject) => {
      try {
        // 创建定位实例
        this.geolocation = new this.amap.Geolocation({
          enableHighAccuracy: true,     // 是否使用高精度定位
          timeout: options.timeout || 10000, // 定位超时时间
          maximumAge: options.maximumAge || 60000, // 定位结果缓存时间
          convert: true,                // 自动偏移坐标
          showButton: false,            // 不显示定位按钮
          showMarker: false,            // 不显示定位点
          showCircle: false,            // 不显示定位精度圈
          panToLocation: false,         // 定位成功后不自动移动到定位点
          zoomToAccuracy: false         // 定位成功后不自动调整地图视野
        })

        // 开始定位
        this.geolocation.getCurrentPosition((status, result) => {
          if (status === 'complete') {
            const position = {
              latitude: result.position.lat,
              longitude: result.position.lng,
              address: result.formattedAddress || result.addressComponent?.formattedAddress,
              accuracy: result.accuracy,
              locationType: result.location_type
            }

            console.log('📍 高德定位成功:', position)
            resolve(position)
          } else {
            console.error('📍 高德定位失败:', result)

            // 根据错误类型给出相应提示
            let errorMessage = '定位失败'
            switch (result) {
              case 'PERMISSION_DENIED':
                errorMessage = '用户拒绝了位置权限请求'
                break
              case 'POSITION_UNAVAILABLE':
                errorMessage = '位置信息不可用'
                break
              case 'TIMEOUT':
                errorMessage = '获取位置超时'
                break
              default:
                errorMessage = result.message || '未知定位错误'
            }

            reject(new Error(errorMessage))
          }
        })
      } catch (error) {
        console.error('📍 定位服务错误:', error)
        reject(new Error('定位服务初始化失败'))
      }
    })
  }

  /**
   * 监听位置变化（连续定位）
   * @param {Function} callback - 位置变化回调函数
   * @param {Object} options - 监听选项
   */
  watchPosition(callback, options = {}) {
    this.initialize().then(() => {
      try {
        // 创建定位实例
        const watchGeolocation = new this.amap.Geolocation({
          enableHighAccuracy: true,
          timeout: 10000,
          maximumAge: 60000,
          convert: true,
          showButton: false,
          showMarker: false,
          showCircle: false,
          panToLocation: false,
          zoomToAccuracy: false
        })

        // 开始监听位置变化
        const watchId = watchGeolocation.watchPosition((status, result) => {
          if (status === 'complete') {
            const position = {
              latitude: result.position.lat,
              longitude: result.position.lng,
              address: result.formattedAddress || result.addressComponent?.formattedAddress,
              accuracy: result.accuracy,
              timestamp: Date.now()
            }

            callback(null, position)
          } else {
            callback(new Error(result.message || '位置监听失败'), null)
          }
        })

        // 返回停止监听的函数
        return () => {
          if (watchId) {
            watchGeolocation.clearWatch(watchId)
          }
        }
      } catch (error) {
        callback(new Error('位置监听初始化失败'), null)
        return () => {}
      }
    })
  }

  /**
   * 逆地理编码（坐标转地址）
   * @param {number} longitude - 经度
   * @param {number} latitude - 纬度
   */
  async reverseGeocode(longitude, latitude) {
    await this.initialize()

    return new Promise((resolve, reject) => {
      try {
        const geocoder = new this.amap.Geocoder()

        geocoder.getAddress([longitude, latitude], (status, result) => {
          if (status === 'complete' && result.info === 'OK') {
            const addressInfo = result.regeocode
            resolve({
              address: addressInfo.formattedAddress,
              province: addressInfo.addressComponent.province,
              city: addressInfo.addressComponent.city,
              district: addressInfo.addressComponent.district,
              township: addressInfo.addressComponent.township,
              street: addressInfo.addressComponent.street,
              streetNumber: addressInfo.addressComponent.streetNumber
            })
          } else {
            reject(new Error('逆地理编码失败'))
          }
        })
      } catch (error) {
        reject(new Error('逆地理编码服务错误'))
      }
    })
  }

  /**
   * 更新用户位置到后端
   * @param {number} latitude - 纬度
   * @param {number} longitude - 经度
   * @param {string} address - 地址（可选）
   */
  async updateUserLocation(latitude, longitude, address = null) {
    try {
      const result = await userAPI.updateLocation({
        latitude,
        longitude,
        address
      })

      if (result.success) {
        console.log('✅ 用户位置更新成功')
        return result
      } else {
        throw new Error(result.message || '位置更新失败')
      }
    } catch (error) {
      console.error('❌ 用户位置更新失败:', error)
      throw error
    }
  }

  /**
   * 检查定位权限
   */
  async checkPermission() {
    if (!navigator.permissions) {
      return 'unknown'
    }

    try {
      const result = await navigator.permissions.query({ name: 'geolocation' })
      return result.state // 'granted', 'denied', 'prompt'
    } catch (error) {
      return 'unknown'
    }
  }

  /**
   * 请求定位权限
   */
  async requestPermission() {
    return new Promise((resolve) => {
      if (!navigator.geolocation) {
        resolve('unsupported')
        return
      }

      navigator.geolocation.getCurrentPosition(
        () => resolve('granted'),
        (error) => {
          switch (error.code) {
            case error.PERMISSION_DENIED:
              resolve('denied')
              break
            case error.POSITION_UNAVAILABLE:
              resolve('unavailable')
              break
            case error.TIMEOUT:
              resolve('timeout')
              break
            default:
              resolve('error')
          }
        },
        { timeout: 10000 }
      )
    })
  }
}

// 创建单例实例
export const locationService = new LocationService()

// 便捷方法
export const getCurrentPosition = (options) => locationService.getCurrentPosition(options)
export const watchPosition = (callback, options) => locationService.watchPosition(callback, options)
export const updateUserLocation = (latitude, longitude, address) => locationService.updateUserLocation(latitude, longitude, address)
export const reverseGeocode = (longitude, latitude) => locationService.reverseGeocode(longitude, latitude)
export const checkLocationPermission = () => locationService.checkPermission()
export const requestLocationPermission = () => locationService.requestPermission()
