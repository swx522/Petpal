using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Services;
using petpal.API.Data;
using petpal.API.Models;
using System.Security.Claims;
using System.Linq;

namespace petpal.API.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IReputationService _reputationService;
        private readonly IGeolocationService _geolocationService;
        private readonly ApplicationDbContext _context;

        public UsersController(IUserService userService, IJwtService jwtService, IReputationService reputationService, IGeolocationService geolocationService, ApplicationDbContext context)
        {
            _userService = userService;
            _jwtService = jwtService;
            _reputationService = reputationService;
            _geolocationService = geolocationService;
            _context = context;
        }

        /// <summary>
        /// 用户注册接口
        /// 创建新用户账户，支持手机号注册
        /// </summary>
        /// <param name="request">注册请求体</param>
        /// <returns>注册结果及JWT令牌</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // 验证输入数据
                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.Phone))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "用户名、密码和手机号为必填项"
                    });
                }

                // 调用用户服务注册
                var user = await _userService.RegisterAsync(
                    request.Username,
                    request.Password,
                    request.Phone,
                    request.Email ?? "");

                // 生成JWT令牌
                var token = _jwtService.GenerateToken(user);

                // 构建响应数据
                var responseData = new
                {
                    userId = user.Id,
                    username = user.Username,
                    phone = MaskPhoneNumber(user.Phone), // 手机号脱敏
                    email = user.Email,
                    role = user.Role.ToString(),
                    reputationScore = user.ReputationScore,
                    reputationLevel = user.ReputationLevel,
                    isRealNameCertified = user.IsRealNameCertified,
                    isPetCertified = user.IsPetCertified,
                    createdAt = user.CreatedAt,
                    token = token
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "注册成功，欢迎加入宠物互助平台"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"注册失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 用户登录接口
        /// 支持手机号或邮箱登录
        /// </summary>
        /// <param name="request">登录请求体</param>
        /// <returns>登录结果及JWT令牌</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // 验证输入数据
                if (string.IsNullOrWhiteSpace(request.Account) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "账号和密码为必填项"
                    });
                }

                // 调用用户服务登录
                var user = await _userService.LoginAsync(request.Account, request.Password);

                // 生成JWT令牌
                var token = _jwtService.GenerateToken(user);

                // 构建响应数据
                var responseData = new
                {
                    userId = user.Id,
                    username = user.Username,
                    phone = MaskPhoneNumber(user.Phone),
                    email = user.Email,
                    role = user.Role.ToString(),
                    reputationScore = user.ReputationScore,
                    reputationLevel = user.ReputationLevel,
                    isRealNameCertified = user.IsRealNameCertified,
                    isPetCertified = user.IsPetCertified,
                    lastLoginAt = user.LastLoginAt,
                    token = token
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "登录成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"登录失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取用户信息接口
        /// 返回当前登录用户的基本信息和认证状态
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 构建响应数据
                var responseData = new
                {
                    userId = user.Id,
                    username = user.Username,
                    phone = MaskPhoneNumber(user.Phone),
                    email = user.Email,
                    role = user.Role.ToString(),
                    status = user.Status.ToString(),
                    reputationScore = user.ReputationScore,
                    reputationLevel = user.ReputationLevel,
                    isRealNameCertified = user.IsRealNameCertified,
                    isPetCertified = user.IsPetCertified,
                    sitterAuditStatus = user.SitterAuditStatus.ToString(),
                    careIntroduction = user.CareIntroduction,
                    serviceTypes = user.ServiceTypes,
                    qualificationDocuments = user.QualificationDocuments,
                    auditAt = user.AuditAt,
                    auditComment = user.AuditComment,
                    rejectionReason = user.RejectionReason,
                    createdAt = user.CreatedAt,
                    lastLoginAt = user.LastLoginAt,
                    petsCount = user.Pets.Count,
                    ordersAsRequesterCount = user.OrdersAsRequester.Count,
                    ordersAsHelperCount = user.OrdersAsHelper.Count
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取用户信息成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取用户信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 提交认证信息接口
        /// 支持实名认证和宠物认证
        /// </summary>
        /// <param name="request">认证请求体</param>
        /// <returns>认证提交结果</returns>
        [HttpPost("certification")]
        [Authorize]
        public async Task<IActionResult> SubmitCertification([FromBody] CertificationRequest request)
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 验证输入数据
                if (string.IsNullOrWhiteSpace(request.CertType) ||
                    request.CertImages == null || !request.CertImages.Any())
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "认证类型和认证图片为必填项"
                    });
                }

                // 验证认证类型
                if (request.CertType != "user" && request.CertType != "pet")
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "认证类型只能是 'user'（实名认证）或 'pet'（宠物认证）"
                    });
                }

                // 调用用户服务更新认证信息
                await _userService.UpdateUserCertificationAsync(userId, request.CertType, request.CertImages);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"{(request.CertType == "user" ? "实名" : "宠物")}认证信息提交成功，等待审核"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"提交认证信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 测试数据库连接接口
        /// 用于验证数据库连接状态和基本查询功能
        /// </summary>
        /// <returns>数据库连接测试结果</returns>
        [HttpGet("test-db-connection")]
        public async Task<IActionResult> TestDatabaseConnection()
        {
            try
            {
                // 测试数据库连接和基本查询
                var userCount = await _context.Users.CountAsync();

                // 尝试执行一个简单的查询来验证连接
                var connectionTest = await _context.Users.FirstOrDefaultAsync();

                var responseData = new
                {
                    connectionStatus = "Success",
                    queryResult = userCount,
                    serverInfo = "121.40.86.149:3306",
                    databaseName = "petpal",
                    testTimestamp = DateTime.Now
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "数据库连接测试成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"数据库连接测试失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 注册为Sitter接口
        /// 将普通用户角色升级为Sitter角色
        /// </summary>
        /// <returns>注册结果</returns>
        [HttpPost("become-sitter")]
        [Authorize]
        public async Task<IActionResult> BecomeSitter()
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 检查用户是否已经是Sitter
                if (user.Role == UserRole.Sitter)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "您已经是Sitter用户"
                    });
                }

                // 更新用户角色为Sitter
                user.Role = UserRole.Sitter;
                user.SitterAuditStatus = SitterAuditStatus.Pending; // 设置为待审核状态

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "成功注册为Sitter，请提交资质资料等待审核"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"注册Sitter失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 提交Sitter资质资料接口
        /// Sitter用户提交看护经验和服务资质信息
        /// </summary>
        /// <param name="request">Sitter资料请求体</param>
        /// <returns>提交结果</returns>
        [HttpPost("sitter/profile")]
        [Authorize]
        public async Task<IActionResult> SubmitSitterProfile([FromBody] SitterProfileRequest request)
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 验证用户是否为Sitter
                if (user.Role != UserRole.Sitter)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有Sitter用户才能提交资质资料"
                    });
                }

                // 更新Sitter资质信息
                user.CareIntroduction = request.CareIntroduction;
                user.ServiceTypes = request.ServiceTypes;
                user.QualificationDocuments = System.Text.Json.JsonSerializer.Serialize(request.QualificationDocuments);

                // 如果是从审核拒绝状态重新提交，更新状态
                if (user.SitterAuditStatus == SitterAuditStatus.Rejected)
                {
                    user.SitterAuditStatus = SitterAuditStatus.Resubmitted;
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Sitter资质资料提交成功，等待管理员审核"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"提交Sitter资质资料失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取Sitter资料接口
        /// 获取当前用户的Sitter资质信息和审核状态
        /// </summary>
        /// <returns>Sitter资料信息</returns>
        [HttpGet("sitter/profile")]
        [Authorize]
        public async Task<IActionResult> GetSitterProfile()
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 验证用户是否为Sitter
                if (user.Role != UserRole.Sitter)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有Sitter用户才能查看资质资料"
                    });
                }

                // 解析资质文件列表
                List<string> qualificationDocuments = new List<string>();
                if (!string.IsNullOrEmpty(user.QualificationDocuments))
                {
                    try
                    {
                        qualificationDocuments = System.Text.Json.JsonSerializer.Deserialize<List<string>>(user.QualificationDocuments) ?? new List<string>();
                    }
                    catch
                    {
                        qualificationDocuments = new List<string>();
                    }
                }

                // 构建响应数据
                var responseData = new
                {
                    userId = user.Id,
                    username = user.Username,
                    role = user.Role.ToString(),
                    sitterAuditStatus = user.SitterAuditStatus.ToString(),
                    careIntroduction = user.CareIntroduction,
                    serviceTypes = user.ServiceTypes,
                    qualificationDocuments = qualificationDocuments,
                    auditAt = user.AuditAt,
                    auditBy = user.AuditBy,
                    auditComment = user.AuditComment,
                    rejectionReason = user.RejectionReason
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取Sitter资料成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取Sitter资料失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取用户信誉档案接口
        /// 返回用户的信誉总分、信誉等级、完成订单数/好评率等数据
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信誉档案</returns>
        [HttpGet("{userId}/reputation")]
        [Authorize]
        public async Task<IActionResult> GetUserReputation(string userId)
        {
            try
            {
                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 获取用户的订单统计
                var completedOrdersAsRequester = await _context.MutualOrders
                    .Where(o => o.RequesterId == userId && o.Status == OrderStatus.Completed)
                    .CountAsync();

                var completedOrdersAsHelper = await _context.MutualOrders
                    .Where(o => o.HelperId == userId && o.Status == OrderStatus.Completed)
                    .CountAsync();

                // 获取用户收到的评价
                var receivedEvaluations = await _context.OrderEvaluations
                    .Where(e => e.EvaluatedUserId == userId)
                    .ToListAsync();

                var totalEvaluations = receivedEvaluations.Count;
                var positiveEvaluations = receivedEvaluations.Count(e => e.Score >= 4);
                var averageScore = totalEvaluations > 0 ? receivedEvaluations.Average(e => e.Score) : 0;

                // 计算好评率
                var positiveRate = totalEvaluations > 0 ? (double)positiveEvaluations / totalEvaluations * 100 : 0;

                // 构建响应数据
                var responseData = new
                {
                    userId = user.Id,
                    username = user.Username,
                    reputationScore = user.ReputationScore,
                    reputationLevel = user.ReputationLevel,
                    totalCompletedOrders = completedOrdersAsRequester + completedOrdersAsHelper,
                    ordersAsRequester = completedOrdersAsRequester,
                    ordersAsHelper = completedOrdersAsHelper,
                    totalEvaluations = totalEvaluations,
                    positiveEvaluations = positiveEvaluations,
                    positiveRate = Math.Round(positiveRate, 1),
                    averageScore = Math.Round(averageScore, 1),
                    recentEvaluations = receivedEvaluations
                        .OrderByDescending(e => e.CreatedAt)
                        .Take(5)
                        .Select(e => new
                        {
                            evaluationId = e.Id,
                            evaluatorUsername = e.Evaluator.Username,
                            score = e.Score,
                            content = e.Content,
                            createdAt = e.CreatedAt
                        })
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取用户信誉档案成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取用户信誉档案失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取信誉变化趋势接口
        /// 返回近段时间的信誉分变化数据
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="days">查询天数（默认30天）</param>
        /// <returns>信誉变化趋势数据</returns>
        [HttpGet("{userId}/reputation/trend")]
        [Authorize]
        public async Task<IActionResult> GetReputationTrend(string userId, [FromQuery] int days = 30)
        {
            try
            {
                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 获取指定时间范围内的评价记录
                var startDate = DateTime.Now.AddDays(-days);
                var evaluations = await _context.OrderEvaluations
                    .Where(e => e.EvaluatedUserId == userId && e.CreatedAt >= startDate)
                    .OrderBy(e => e.CreatedAt)
                    .ToListAsync();

                // 生成趋势数据
                var trendData = new List<object>();
                var currentScore = user.ReputationScore;

                // 如果没有评价记录，添加当前分数点
                if (!evaluations.Any())
                {
                    trendData.Add(new
                    {
                        date = DateTime.Now.ToString("yyyy-MM-dd"),
                        score = currentScore,
                        change = 0
                    });
                }
                else
                {
                    // 反向计算历史分数
                    foreach (var evaluation in evaluations.OrderByDescending(e => e.CreatedAt))
                    {
                        // 这里简化处理，实际应该从ReputationService中获取准确的历史分数变化
                        var tempEvaluation = new OrderEvaluation
                        {
                            Score = evaluation.Score,
                            Content = evaluation.Content
                        };
                        var change = _reputationService.CalculateReputationChange(tempEvaluation);

                        trendData.Add(new
                        {
                            date = evaluation.CreatedAt.ToString("yyyy-MM-dd"),
                            score = currentScore,
                            change = change,
                            evaluationType = evaluation.EvaluationType,
                            scoreValue = evaluation.Score
                        });

                        currentScore -= change;
                    }

                    // 反转列表以按时间正序显示
                    trendData.Reverse();
                }

                var responseData = new
                {
                    userId = user.Id,
                    currentScore = user.ReputationScore,
                    currentLevel = user.ReputationLevel,
                    trendData = trendData,
                    periodDays = days
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取信誉变化趋势成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取信誉变化趋势失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取最新评价列表接口
        /// 返回用户的历史评价记录
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>评价列表</returns>
        [HttpGet("{userId}/reviews")]
        [Authorize]
        public async Task<IActionResult> GetUserReviews(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 获取用户收到的评价
                var query = _context.OrderEvaluations
                    .Include(e => e.Evaluator)
                    .Include(e => e.Order)
                    .Where(e => e.EvaluatedUserId == userId);

                var totalCount = await query.CountAsync();
                var reviews = await query
                    .OrderByDescending(e => e.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 构建响应数据
                var reviewList = reviews.Select(e => new
                {
                    evaluationId = e.Id,
                    orderId = e.OrderId,
                    evaluator = new
                    {
                        userId = e.Evaluator.Id,
                        username = e.Evaluator.Username
                    },
                    evaluationType = e.EvaluationType,
                    score = e.Score,
                    content = e.Content,
                    createdAt = e.CreatedAt,
                    orderInfo = new
                    {
                        helpType = e.Order.HelpType.ToString(),
                        completedAt = e.Order.CompletedAt
                    }
                });

                var responseData = new
                {
                    userId = user.Id,
                    reviews = reviewList,
                    pagination = new
                    {
                        page = page,
                        pageSize = pageSize,
                        totalCount = totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取评价列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取评价列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 用户提交定位信息接口
        /// 根据用户的经纬度自动匹配社区，并更新用户定位信息
        /// </summary>
        /// <param name="request">定位请求体</param>
        /// <returns>定位更新结果</returns>
        [HttpPost("location")]
        [Authorize]
        public async Task<IActionResult> UpdateUserLocation([FromBody] UpdateLocationRequest request)
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 验证输入数据
                if (request.Latitude < -90 || request.Latitude > 90 ||
                    request.Longitude < -180 || request.Longitude > 180)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "无效的经纬度坐标"
                    });
                }

                // 获取用户信息
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 根据经纬度查找社区
                var community = await _geolocationService.FindCommunityByLocationAsync((decimal)request.Longitude, (decimal)request.Latitude);

                // 更新用户定位信息
                user.Longitude = (decimal)request.Longitude;
                user.Latitude = (decimal)request.Latitude;
                user.CommunityId = community?.Id;
                user.LocationUpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var responseData = new
                {
                    longitude = user.Longitude,
                    latitude = user.Latitude,
                    communityId = user.CommunityId,
                    communityName = community?.Name,
                    locationUpdatedAt = user.LocationUpdatedAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = community != null ? $"定位更新成功，已关联到社区：{community.Name}" : "定位更新成功，但未找到对应社区"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"更新定位信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取用户当前位置信息接口
        /// </summary>
        /// <returns>用户当前位置信息</returns>
        [HttpGet("location")]
        [Authorize]
        public async Task<IActionResult> GetUserLocation()
        {
            try
            {
                // 从JWT令牌中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 获取用户信息
                var user = await _context.Users
                    .Include(u => u.Community)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                var responseData = new
                {
                    longitude = user.Longitude,
                    latitude = user.Latitude,
                    community = user.Community != null ? new
                    {
                        communityId = user.Community.Id,
                        name = user.Community.Name,
                        centerLng = user.Community.GetCenter().CenterLng,
                        centerLat = user.Community.GetCenter().CenterLat
                    } : null,
                    locationUpdatedAt = user.LocationUpdatedAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取位置信息成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取位置信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 手机号脱敏处理
        /// 将手机号中间4位数字替换为星号
        /// </summary>
        /// <param name="phone">原始手机号</param>
        /// <returns>脱敏后的手机号</returns>
        private string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7)
                return phone;

            return phone.Substring(0, 3) + "****" + phone.Substring(7);
        }

        /// <summary>
        /// 用户注册请求模型
        /// </summary>
        public class RegisterRequest
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string? Email { get; set; }
        }

        /// <summary>
        /// 用户登录请求模型
        /// </summary>
        public class LoginRequest
        {
            public string Account { get; set; } = string.Empty; // 手机号或邮箱
            public string Password { get; set; } = string.Empty;
        }

        /// <summary>
        /// 认证信息提交请求模型
        /// </summary>
        public class CertificationRequest
        {
            public string CertType { get; set; } = string.Empty; // "user" 或 "pet"
            public List<string> CertImages { get; set; } = new List<string>();
        }

        /// <summary>
        /// Sitter资料提交请求模型
        /// </summary>
        public class SitterProfileRequest
        {
            public string CareIntroduction { get; set; } = string.Empty;
            public string ServiceTypes { get; set; } = string.Empty;
            public List<string> QualificationDocuments { get; set; } = new List<string>();
        }

        /// <summary>
        /// 用户定位更新请求模型
        /// </summary>
        public class UpdateLocationRequest
        {
            public double Longitude { get; set; }
            public double Latitude { get; set; }
        }
    }
}
