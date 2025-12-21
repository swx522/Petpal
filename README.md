# 宠物互助平台 - petpal API

## 项目概述

宠物互助平台是一个基于ASP.NET Core开发的Web API项目，为宠物主人提供互助服务。平台允许用户发布宠物照顾需求，其他用户可以接受并提供帮助。通过信誉评价系统确保服务质量。

## 🏗️ 技术架构

### 核心技术栈
- **框架**: ASP.NET Core 10.0 (Web API)
- **数据库**: MySQL 8.0+
- **身份认证**: JWT (JSON Web Token)
- **API文档**: Swagger/OpenAPI

### 项目结构
```
petpal/
├── Controllers/          # API控制器层
├── Data/                # 数据访问层
├── Models/              # 领域模型
├── Services/            # 业务服务层
├── appsettings.json     # 配置文件
├── Program.cs           # 应用程序入口
└── petpal.csproj        # 项目文件
```

## 📊 开发进度

### ✅ 已完成阶段
- **第一阶段**: 用户管理系统 - 注册、登录、认证、资料管理
- **第二阶段**: 互助订单管理 - 发布需求、接受订单、状态管理、评价、地理位置服务
- **第三阶段**: Sitter资质审核管理系统 - Sitter注册、资料提交、管理员审核、状态管理

---

> 📖 **详细启动指南**: 请查看 [`STARTUP_GUIDE.md`](STARTUP_GUIDE.md) 获取完整的启动说明和故障排除指南。

> 📋 **部署检查清单**: 请查看 [`DEPLOYMENT_CHECKLIST.md`](DEPLOYMENT_CHECKLIST.md) 确保生产环境部署完整。

### 环境要求
- .NET 10.0.101 SDK
- MySQL 8.0+ (服务器地址: 121.40.86.149:3306)
- Redis (可选，用于缓存)

### 环境检查

在启动项目之前，建议运行环境检查工具：

```cmd
# 检查环境配置
.\check-env.bat

# 详细的MySQL连接诊断
.\test-mysql-connection.bat
```

这个工具会检查：
- .NET SDK是否已安装
- MySQL连接是否正常
- 项目文件是否完整

### 安装步骤

1. **克隆项目**
   ```cmd
   git clone <repository-url>
   cd petpal
   ```

2. **配置数据库连接**
   ```cmd
   # 复制配置文件模板
   copy appsettings.template.json appsettings.json

   # 编辑appsettings.json，设置MySQL密码：
   # 将 "Password=YOUR_PASSWORD_HERE" 替换为实际密码
   ```

3. **环境检查**
   ```cmd
   .\check-env.bat
   ```

3. **快速启动（推荐）**
   使用提供的启动脚本自动完成所有设置：

```cmd
# 方法1：双击运行 start.bat 文件

# 方法2：在命令行中执行（推荐）：
.\start.bat

# 环境检查：
.\check-env.bat
```

3. **手动安装步骤**
   如果启动脚本遇到问题，可以手动执行以下步骤：

   - **安装依赖**
     ```bash
     dotnet restore
     ```

   - **配置数据库**
     - 确保MySQL服务器正在运行 (121.40.86.149:3306)
     - 修改 `appsettings.json` 中的连接字符串
     ```json
     "ConnectionStrings": {
       "DefaultConnection": "Server=121.40.86.149;Port=3306;Database=petpal;User Id=petpal;Password=Lele050522;SslMode=None;AllowPublicKeyRetrieval=True;Connection Timeout=30"
     }
     ```

   - **连接诊断**
     ```cmd
     # 运行详细的MySQL连接诊断
     .\test-mysql-connection.bat
     ```

   - **初始化MySQL数据库（推荐）**
     ```cmd
     # 使用提供的SQL脚本初始化数据库
     mysql -h 121.40.86.149 -P 3306 -u root -p < database-init.sql
     ```

   - **运行数据库迁移**
     ```cmd
     dotnet ef database update
     ```

   - **启动应用程序**
     ```cmd
     dotnet run
     ```

4. **访问API文档**
   - 打开浏览器访问: `http://127.0.0.1:5002` (Swagger UI)
   - 或者访问: `https://127.0.0.1:5003` (如果HTTPS可用)

## 📋 API接口文档

### 用户管理模块

#### 1. 用户注册
```http
POST /api/v1/users/register
Content-Type: application/json

{
  "username": "张三",
  "password": "password123",
  "phone": "13800138000",
  "email": "zhangsan@example.com"
}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "userId": "550e8400-e29b-41d4-a716-446655440000",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  },
  "message": "注册成功，欢迎加入宠物互助平台"
}
```

#### 2. 用户登录
```http
POST /api/v1/users/login
Content-Type: application/json

{
  "account": "13800138000",
  "password": "password123"
}
```

#### 3. 获取用户信息
```http
GET /api/v1/users/profile
Authorization: Bearer {token}
```

#### 4. 提交认证信息
```http
POST /api/v1/users/certification
Authorization: Bearer {token}
Content-Type: application/json

{
  "certType": "user",
  "certImages": [
    "https://example.com/cert1.jpg",
    "https://example.com/cert2.jpg"
  ]
}
```

#### 5. 测试数据库连接
```http
GET /api/v1/users/test-db-connection
```
**响应示例:**
```json
{
  "success": true,
  "data": {
    "connectionStatus": "Success",
    "queryResult": 1,
    "serverInfo": "121.40.86.149:3306",
    "databaseName": "petpal"
  },
  "message": "数据库连接测试成功"
}
```

## 互助订单管理模块

### 6. 发布互助需求
```http
POST /api/v1/orders
Authorization: Bearer {token}
Content-Type: application/json

{
  "petId": "pet-uuid",
  "helpType": "Foster",
  "startTime": "2024-01-15T10:00:00Z",
  "endTime": "2024-01-16T10:00:00Z",
  "longitude": 116.4074,
  "latitude": 39.9042,
  "remark": "需要专业宠物照顾"
}
```

### 7. 获取订单列表
```http
GET /api/v1/orders?page=1&pageSize=10&status=Pending
Authorization: Bearer {token}
```

### 8. 获取订单详情
```http
GET /api/v1/orders/{orderId}
Authorization: Bearer {token}
```

### 9. 接受互助订单
```http
PUT /api/v1/orders/{orderId}/accept
Authorization: Bearer {token}
```

### 10. 确认订单完成
```http
PUT /api/v1/orders/{orderId}/complete
Authorization: Bearer {token}
```

### 11. 评价订单
```http
POST /api/v1/orders/{orderId}/evaluate
Authorization: Bearer {token}
Content-Type: application/json

{
  "score": 5,
  "content": "服务非常专业，宠物很喜欢！"
}
```

### 12. 获取附近需求
```http
GET /api/v1/orders/nearby?longitude=116.4074&latitude=39.9042&radius=3000
Authorization: Bearer {token}
```

## Sitter资质审核管理系统

### 13. 注册为Sitter
```http
POST /api/v1/users/become-sitter
Authorization: Bearer {token}
```

### 14. 提交Sitter资质资料
```http
POST /api/v1/users/sitter/profile
Authorization: Bearer {token}
Content-Type: application/json

{
  "careIntroduction": "我有5年宠物照顾经验，擅长猫狗寄养",
  "serviceTypes": "寄养、喂食、陪伴、遛狗",
  "qualificationDocuments": "[\"cert1.jpg\",\"cert2.jpg\"]"
}
```

### 15. 获取Sitter资料
```http
GET /api/v1/users/sitter/profile
Authorization: Bearer {token}
```

### 16. 获取待审核Sitter列表（管理员）
```http
GET /api/v1/admin/sitters/pending
Authorization: Bearer {admin-token}
```

### 17. 获取Sitter详情（管理员）
```http
GET /api/v1/admin/sitters/{userId}
Authorization: Bearer {admin-token}
```

### 18. 审核通过Sitter资质（管理员）
```http
PUT /api/v1/admin/sitters/{userId}/approve
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "comment": "资质审核通过，服务经验丰富"
}
```

### 19. 审核拒绝Sitter资质（管理员）
```http
PUT /api/v1/admin/sitters/{userId}/reject
Authorization: Bearer {admin-token}
Content-Type: application/json

{
  "comment": "资料审核完成",
  "rejectionReason": "资质资料不完整，请补充相关证书"
}
```

### 20. 获取所有Sitter列表（管理员）
```http
GET /api/v1/admin/sitters?page=1&pageSize=10&status=Approved
Authorization: Bearer {admin-token}
```

## 评价与信誉管理模块

### 21. 获取用户信誉档案接口
```http
GET /api/v1/users/{userId}/reputation
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "userId": "user-uuid",
    "username": "张三",
    "reputationScore": 150,
    "reputationLevel": "靠谱",
    "totalCompletedOrders": 25,
    "ordersAsRequester": 15,
    "ordersAsHelper": 10,
    "totalEvaluations": 20,
    "positiveEvaluations": 18,
    "positiveRate": 90.0,
    "averageScore": 4.5,
    "recentEvaluations": [...]
  },
  "message": "获取用户信誉档案成功"
}
```

### 22. 获取信誉变化趋势接口
```http
GET /api/v1/users/{userId}/reputation/trend?days=30
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "userId": "user-uuid",
    "currentScore": 150,
    "currentLevel": "靠谱",
    "trendData": [
      {
        "date": "2025-12-15",
        "score": 145,
        "change": 5,
        "evaluationType": "requester_to_helper",
        "scoreValue": 5
      }
    ],
    "periodDays": 30
  },
  "message": "获取信誉变化趋势成功"
}
```

### 23. 获取最新评价列表接口
```http
GET /api/v1/users/{userId}/reviews?page=1&pageSize=10
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "userId": "user-uuid",
    "reviews": [
      {
        "evaluationId": "eval-uuid",
        "orderId": "order-uuid",
        "evaluator": {
          "userId": "evaluator-uuid",
          "username": "评价人"
        },
        "evaluationType": "requester_to_helper",
        "score": 5,
        "content": "服务非常专业",
        "createdAt": "2025-12-20T10:00:00Z",
        "orderInfo": {
          "helpType": "Foster",
          "completedAt": "2025-12-19T15:00:00Z"
        }
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalCount": 25,
      "totalPages": 3
    }
  },
  "message": "获取评价列表成功"
}
```

## Sitter审核管理模块

### 24. 获取审核进度接口
```http
GET /api/v1/sitters/{sitterId}/audit/status
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "sitterId": "sitter-uuid",
    "username": "张三",
    "auditStatus": {
      "currentStage": "资料审核中",
      "stageDescription": "资料审核中",
      "estimatedCompletion": "1-3个工作日",
      "progress": 25
    },
    "submittedMaterialsCount": 3,
    "reputationLevel": "新手",
    "appliedAt": "2025-12-15T10:00:00Z",
    "lastAuditAt": "2025-12-18T14:00:00Z"
  },
  "message": "获取审核进度成功"
}
```

### 25. 提交审核资料接口
```http
POST /api/v1/sitters/{sitterId}/audit/materials
Authorization: Bearer {token}
Content-Type: application/json

{
  "materialType": "IdCard",
  "materialName": "身份证照片",
  "filePath": "/uploads/idcard.jpg",
  "fileSize": 2048576,
  "contentType": "image/jpeg"
}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "materialId": "material-uuid",
    "materialType": "IdCard",
    "materialName": "身份证照片",
    "uploadedAt": "2025-12-20T10:00:00Z",
    "status": "Pending"
  },
  "message": "审核资料提交成功，等待管理员审核"
}
```

### 26. 获取已提交资料接口
```http
GET /api/v1/sitters/{sitterId}/audit/materials
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "sitterId": "sitter-uuid",
    "materials": [
      {
        "materialId": "material-uuid",
        "materialType": "IdCard",
        "materialName": "身份证照片",
        "filePath": "/uploads/idcard.jpg",
        "fileSize": 2048576,
        "contentType": "image/jpeg",
        "status": "Approved",
        "reviewComment": "资料审核通过",
        "uploadedAt": "2025-12-15T10:00:00Z",
        "reviewedAt": "2025-12-16T14:00:00Z"
      }
    ],
    "totalCount": 3
  },
  "message": "获取审核资料成功"
}
```

## 服务状态管理模块

### 27. 获取服务列表接口
```http
GET /api/v1/services?type=Foster&status=Pending&page=1&pageSize=10
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "services": [
      {
        "serviceId": "service-uuid",
        "requester": {
          "userId": "requester-uuid",
          "username": "李四",
          "reputationScore": 120,
          "reputationLevel": "靠谱"
        },
        "pet": {
          "petId": "pet-uuid",
          "name": "旺财",
          "type": "Dog",
          "breed": "金毛",
          "age": 2,
          "isVaccinated": true,
          "description": "非常乖巧的金毛犬"
        },
        "helpType": "Foster",
        "startTime": "2025-12-25T10:00:00Z",
        "endTime": "2025-12-27T10:00:00Z",
        "duration": 48.0,
        "longitude": 116.4074,
        "latitude": 39.9042,
        "remark": "需要专业宠物照顾",
        "orderImages": [
          "/uploads/pet1.jpg",
          "/uploads/pet2.jpg"
        ],
        "createdAt": "2025-12-20T09:00:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "pageSize": 10,
      "totalCount": 25,
      "totalPages": 3
    }
  },
  "message": "获取服务列表成功"
}
```

### 28. 获取服务详情接口
```http
GET /api/v1/services/{serviceId}
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "serviceId": "service-uuid",
    "requester": {
      "userId": "requester-uuid",
      "username": "李四",
      "phone": "138****0000",
      "reputationScore": 120,
      "reputationLevel": "靠谱",
      "isRealNameCertified": true,
      "isPetCertified": true
    },
    "pet": {
      "petId": "pet-uuid",
      "name": "旺财",
      "type": "Dog",
      "breed": "金毛",
      "age": 2,
      "isVaccinated": true,
      "description": "非常乖巧的金毛犬"
    },
    "helpType": "Foster",
    "startTime": "2025-12-25T10:00:00Z",
    "endTime": "2025-12-27T10:00:00Z",
    "duration": 48.0,
    "longitude": 116.4074,
    "latitude": 39.9042,
    "remark": "需要专业宠物照顾",
    "orderImages": [
      "/uploads/pet1.jpg",
      "/uploads/pet2.jpg"
    ],
    "createdAt": "2025-12-20T09:00:00Z"
  },
  "message": "获取服务详情成功"
}
```

### 29. 接单操作接口
```http
POST /api/v1/services/{serviceId}/accept
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "message": "成功接受服务，请按约定时间提供服务"
}
```

## 社区本地化服务模块

### 30. 用户提交定位信息接口
```http
POST /api/v1/users/location
Authorization: Bearer {token}
Content-Type: application/json

{
  "longitude": 116.4074,
  "latitude": 39.9042
}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "longitude": 116.4074,
    "latitude": 39.9042,
    "communityId": 1,
    "communityName": "XX小区",
    "locationUpdatedAt": "2025-12-21T10:00:00Z"
  },
  "message": "定位更新成功，已关联到社区：XX小区"
}
```

### 31. 获取用户当前位置信息接口
```http
GET /api/v1/users/location
Authorization: Bearer {token}
```

**响应示例:**
```json
{
  "success": true,
  "data": {
    "longitude": 116.4074,
    "latitude": 39.9042,
    "community": {
      "communityId": 1,
      "name": "XX小区",
      "centerLng": 116.4074,
      "centerLat": 39.9042
    },
    "locationUpdatedAt": "2025-12-21T10:00:00Z"
  },
  "message": "获取位置信息成功"
}
```

## 🗄️ 数据库设计

### 核心数据表

#### Users（用户表）
| 字段名 | MySQL类型 | C#类型 | 说明 |
|--------|-----------|---------|------|
| Id | VARCHAR(255) | string | 主键，用户ID |
| Username | nvarchar(50) | 用户名 |
| PasswordHash | nvarchar(100) | 密码哈希 |
| Phone | nvarchar(20) | 手机号码 |
| Email | nvarchar(100) | 邮箱 |
| Role | int | 用户角色 |
| Status | int | 用户状态 |
| ReputationScore | int | 信誉分数 |
| ReputationLevel | nvarchar(max) | 信誉等级 |
| IsRealNameCertified | bit | 实名认证状态 |
| IsPetCertified | bit | 宠物认证状态 |
| CreatedAt | datetime2 | 创建时间 |
| LastLoginAt | datetime2 | 最后登录时间 |
| CommunityId | INT | 所在社区ID |
| Longitude | decimal(9,6) | 当前位置经度 |
| Latitude | decimal(8,6) | 当前位置纬度 |
| LocationUpdatedAt | datetime2 | 位置更新时间 |

#### MutualOrders（互助订单表）
| 字段名 | MySQL类型 | C#类型 | 说明 |
|--------|-----------|---------|------|
| Id | VARCHAR(255) | string | 主键，订单ID |
| RequesterId | nvarchar(450) | 委托人ID |
| HelperId | nvarchar(450) | 帮助者ID |
| PetId | nvarchar(450) | 宠物ID |
| HelpType | int | 互助类型 |
| Status | int | 订单状态 |
| StartTime | datetime2 | 开始时间 |
| EndTime | datetime2 | 结束时间 |
| Longitude | float | 服务地点经度 |
| Latitude | float | 服务地点纬度 |
| CommunityId | INT | 服务所在社区ID |
| OrderImages | nvarchar(max) | 订单附加图片（JSON格式） |
| Remark | nvarchar(500) | 备注 |
| CreatedAt | datetime2 | 创建时间 |
| AcceptedAt | datetime2 | 接单时间 |
| CompletedAt | datetime2 | 完成时间 |

#### Pets（宠物表）
| 字段名 | MySQL类型 | C#类型 | 说明 |
|--------|-----------|---------|------|
| Id | VARCHAR(255) | string | 主键，宠物ID |
| UserId | nvarchar(450) | 主人ID |
| Name | nvarchar(50) | 宠物姓名 |
| Type | nvarchar(max) | 宠物类型 |
| Breed | nvarchar(50) | 宠物品种 |
| Age | int | 宠物年龄 |
| IsVaccinated | bit | 是否接种疫苗 |
| Description | nvarchar(500) | 宠物描述 |
| CreatedAt | datetime2 | 创建时间 |

#### AuditMaterials（审核材料表）
| 字段名 | MySQL类型 | C#类型 | 说明 |
|--------|-----------|---------|------|
| Id | VARCHAR(255) | string | 主键，材料ID |
| SitterId | nvarchar(450) | Sitter用户ID |
| MaterialType | int | 材料类型（身份证/宠物证明/环境照片等） |
| MaterialName | nvarchar(100) | 材料名称 |
| FilePath | nvarchar(max) | 文件路径或URL |
| FileSize | bigint | 文件大小（字节） |
| ContentType | nvarchar(50) | 文件类型（如：image/jpeg） |
| Status | int | 审核状态（待审核/已通过/已拒绝） |
| ReviewComment | nvarchar(500) | 审核意见 |
| UploadedAt | datetime2 | 上传时间 |
| ReviewedAt | datetime2 | 审核时间 |

#### Communities（社区表）
| 字段名 | MySQL类型 | C#类型 | 说明 |
|--------|-----------|---------|------|
| Id | INT | int | 主键，社区ID |
| Name | nvarchar(100) | 社区名称 |
| MinLng | decimal(9,6) | 社区范围最小经度 |
| MaxLng | decimal(9,6) | 社区范围最大经度 |
| MinLat | decimal(8,6) | 社区范围最小纬度 |
| MaxLat | decimal(8,6) | 社区范围最大纬度 |
| Description | nvarchar(500) | 社区描述 |
| CreatedAt | datetime2 | 创建时间 |
| IsActive | bit | 是否激活 |

## 🔐 安全特性

### JWT身份认证
- 使用HMAC-SHA256算法签名
- Token有效期7天
- 支持用户角色和权限控制

### 数据保护
- 密码使用SHA256哈希存储
- 敏感信息（如手机号）在API响应中脱敏
- 请求频率限制（待实现）

### 业务安全
- 用户必须完成双重认证（实名+宠物）才能发布需求
- 信誉评价系统防止恶意行为
- 地理位置限制确保互助在合理范围内

## 📊 信誉评价系统

### 等级体系
- **新手**: 0-199分
- **靠谱**: 200-499分
- **爱心达人**: 500分以上

### 评分规则
- 5星评价: +10分
- 4星评价: +5分
- 3星评价: 0分
- 2星评价: -5分
- 1星评价: -10分
- 附加评价内容: +2分

## 🌐 第三方服务集成

### 高德地图API
- 计算两点地理距离
- 验证互助服务范围（1-3公里）

### 阿里云OSS
- 上传认证图片和宠物照片
- CDN加速图片访问

### 阿里云短信
- 发送验证码（预留功能）
- 发送订单提醒通知

## 🧪 测试

### 单元测试
```cmd
dotnet test
```

### API测试
使用Postman或Swagger UI进行接口测试

### 数据库迁移测试
```cmd
dotnet ef database update
```

## 🚀 部署

### 开发环境
```cmd
dotnet run --environment Development
```

### 生产环境
```cmd
dotnet publish -c Release -o ./publish
dotnet ./publish/petpal.dll --urls "http://0.0.0.0:5000;https://0.0.0.0:5001"
```

**MySQL生产环境配置：**
- 确保MySQL服务器安全配置（强密码、限制IP访问）
- 考虑使用连接池配置
- 定期备份数据库
- 监控数据库性能

### Windows IIS部署
推荐在Windows Server上使用IIS部署：
1. 安装IIS和.NET Core Hosting Bundle
2. 发布应用程序：`dotnet publish -c Release -o ./publish`
3. 在IIS中创建网站，指向publish目录
4. 配置应用程序池为"No Managed Code"

## 📝 开发规范

### 代码风格
- 使用C# 10.0语言特性
- 遵循RESTful API设计规范
- 统一响应格式：`{ success: bool, data: any, message: string }`

## 🤝 贡献指南

1. Fork项目
2. 创建功能分支：`git checkout -b feature/new-feature`
3. 提交更改：`git commit -m 'Add new feature'`
4. 推送分支：`git push origin feature/new-feature`
5. 创建Pull Request

## 🔄 后续开发计划

### 第二阶段（互助订单管理）
- [ ] 发布互助需求接口
- [ ] 接受互助订单接口
- [ ] 订单状态管理
- [ ] 订单评价功能

### 第三阶段（社区功能）
- [ ] 本地化服务查询
- [ ] 地理位置服务
- [ ] 第三方服务集成

### 第四阶段（优化和扩展）
- [ ] 缓存优化
- [ ] 性能监控
- [ ] 移动端适配

---

**最后更新**: 2025年12月21日 (新增社区本地化服务、经纬度管理、图片上传、信誉评价、审核管理、服务状态管理功能)
