using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using petpal.API.Services;
using System.Security.Claims;
using System.Linq;

namespace petpal.API.Controllers
{
    [ApiController]
    [Route("api/v1/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IGeolocationService _geolocationService;
        private readonly IReputationService _reputationService;

        public OrdersController(
            ApplicationDbContext context,
            IUserService userService,
            IGeolocationService geolocationService,
            IReputationService reputationService)
        {
            _context = context;
            _userService = userService;
            _geolocationService = geolocationService;
            _reputationService = reputationService;
        }

        /// <summary>
        /// 发布互助需求接口
        /// 用户发布宠物互助服务需求
        /// </summary>
        /// <param name="request">订单请求体</param>
        /// <returns>发布结果</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
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

                // 验证用户认证状态
                var isCertified = await _userService.ValidateCertificationAsync(userId);
                if (!isCertified)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "发布需求需要完成实名认证和宠物认证"
                    });
                }

                // 验证输入数据
                if (string.IsNullOrWhiteSpace(request.Title) ||
                    string.IsNullOrWhiteSpace(request.PetType) ||
                    string.IsNullOrWhiteSpace(request.ServiceType))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "标题、宠物类型和服务类型为必填项"
                    });
                }

                // 验证时间
                if (request.StartTime >= request.EndTime)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "开始时间必须早于结束时间"
                    });
                }

                // 获取用户位置信息，如果没有则要求用户先设置位置
                var user = await _userService.GetUserByIdAsync(userId);
                if (!user.Longitude.HasValue || !user.Latitude.HasValue)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "请先设置您的位置信息"
                    });
                }

                // 根据用户位置查找社区
                var community = await _geolocationService.FindCommunityByLocationAsync(user.Longitude.Value, user.Latitude.Value);

                // 创建订单
                var order = new MutualOrder
                {
                    OwnerId = userId,
                    Title = request.Title,
                    PetType = request.PetType,
                    ServiceType = request.ServiceType,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Description = request.Description,
                    CommunityId = community?.Id
                };

                _context.MutualOrders.Add(order);
                await _context.SaveChangesAsync();

                // 构建响应数据
                var responseData = new
                {
                    id = order.Id,
                    owner_id = order.OwnerId,
                    title = order.Title,
                    pet_type = order.PetType,
                    service_type = order.ServiceType,
                    start_time = order.StartTime,
                    end_time = order.EndTime,
                    description = order.Description,
                    status = order.Status.ToString(),
                    community_id = order.CommunityId,
                    created_at = order.CreatedAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "互助需求发布成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"发布互助需求失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取订单列表接口
        /// 获取用户的订单列表，支持分页和状态过滤
        /// </summary>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="status">订单状态过滤（可选）</param>
        /// <returns>订单列表</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
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

                // 构建查询
                var query = _context.MutualOrders
                    .Include(o => o.Owner)
                    .Include(o => o.Community)
                    .Include(o => o.Evaluations)
                    .Where(o => o.OwnerId == userId);

                // 状态过滤
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                {
                    query = query.Where(o => o.Status == orderStatus);
                }

                // 分页
                var totalCount = await query.CountAsync();
                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 构建响应数据
                var orderList = orders.Select(o => new
                {
                    id = o.Id,
                    owner_id = o.OwnerId,
                    title = o.Title,
                    pet_type = o.PetType,
                    service_type = o.ServiceType,
                    start_time = o.StartTime,
                    end_time = o.EndTime,
                    description = o.Description,
                    status = o.Status.ToString(),
                    community_id = o.CommunityId,
                    community_name = o.Community?.Name,
                    created_at = o.CreatedAt,
                    evaluations_count = o.Evaluations.Count
                });

                var responseData = new
                {
                    orders = orderList,
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
                    Message = "获取订单列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取订单列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取订单详情接口
        /// 获取指定订单的详细信息
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单详情</returns>
        [HttpGet("{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(string orderId)
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

                // 获取订单详情
                var order = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .Include(o => o.Community)
                    .Include(o => o.Evaluations)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.OwnerId == userId);

                if (order == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "订单不存在或无权限查看"
                    });
                }

                // 构建响应数据
                var responseData = new
                {
                    id = order.Id,
                    owner_id = order.OwnerId,
                    title = order.Title,
                    pet_type = order.PetType,
                    service_type = order.ServiceType,
                    start_time = order.StartTime,
                    end_time = order.EndTime,
                    description = order.Description,
                    status = order.Status.ToString(),
                    community_id = order.CommunityId,
                    community_name = order.Community?.Name,
                    created_at = order.CreatedAt,
                    evaluations = order.Evaluations.Select(e => new
                    {
                        evaluation_id = e.Id,
                        evaluator_id = e.EvaluatorId,
                        evaluator_username = e.Evaluator.Username,
                        evaluated_user_id = e.EvaluatedUserId,
                        evaluated_username = e.EvaluatedUser.Username,
                        evaluation_type = e.EvaluationType,
                        score = e.Score,
                        content = e.Content,
                        created_at = e.CreatedAt
                    })
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取订单详情成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取订单详情失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 接受互助订单接口
        /// Sitter用户接受委托人的互助需求
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>接受结果</returns>
        [HttpPut("{orderId}/accept")]
        [Authorize]
        public async Task<IActionResult> AcceptOrder(string orderId)
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

                // 获取用户角色
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有Sitter用户才能接受订单"
                    });
                }

                // 获取订单
                var order = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "订单不存在"
                    });
                }

                // 验证订单状态
                if (order.Status != OrderStatus.Pending)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "订单已被接受或已完成"
                    });
                }

                // 验证不能接受自己的订单
                if (order.OwnerId == userId)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "不能接受自己的订单"
                    });
                }

                // 更新订单状态
                order.Status = OrderStatus.Accepted;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "成功接受订单，请按约定时间提供服务"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"接受订单失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 确认订单完成接口
        /// 订单完成后更新状态，准备进行评价
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>完成结果</returns>
        [HttpPut("{orderId}/complete")]
        [Authorize]
        public async Task<IActionResult> CompleteOrder(string orderId)
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

                // 获取订单
                var order = await _context.MutualOrders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.OwnerId == userId);

                if (order == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "订单不存在或无权限操作"
                    });
                }

                // 验证订单状态
                if (order.Status != OrderStatus.Accepted && order.Status != OrderStatus.InProgress)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "订单状态不允许完成操作"
                    });
                }

                // 更新订单状态
                order.Status = OrderStatus.Completed;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "订单已完成，请对对方进行评价"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"完成订单失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 评价订单接口
        /// 对完成的订单进行评分和评价
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <param name="request">评价请求体</param>
        /// <returns>评价结果</returns>
        [HttpPost("{orderId}/evaluate")]
        [Authorize]
        public async Task<IActionResult> EvaluateOrder(string orderId, [FromBody] EvaluateOrderRequest request)
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

                // 获取订单
                var order = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.OwnerId == userId);

                if (order == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "订单不存在或无权限评价"
                    });
                }

                // 验证订单状态
                if (order.Status != OrderStatus.Completed)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有已完成的订单才能进行评价"
                    });
                }

                // 目前只支持订单所有者进行评价
                string evaluationType = "owner_evaluation";
                string evaluatedUserId = order.OwnerId; // 这里可能需要修改逻辑，因为现在只有订单所有者能看到自己的订单

                // 检查是否已评价
                var existingEvaluation = await _context.OrderEvaluations
                    .FirstOrDefaultAsync(e => e.OrderId == orderId && e.EvaluatorId == userId);

                if (existingEvaluation != null)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "您已经对该订单进行过评价"
                    });
                }

                // 创建评价
                var evaluation = new OrderEvaluation
                {
                    OrderId = orderId,
                    EvaluatorId = userId,
                    EvaluatedUserId = evaluatedUserId,
                    EvaluationType = evaluationType,
                    Score = request.Score,
                    Content = request.Content
                };

                _context.OrderEvaluations.Add(evaluation);

                // 更新被评价者的信誉分数
                var evaluatedUser = await _context.Users.FindAsync(evaluatedUserId);
                if (evaluatedUser != null)
                {
                    // 创建临时评价对象来计算信誉变化
                    var tempEvaluation = new OrderEvaluation
                    {
                        Score = request.Score,
                        Content = request.Content
                    };
                    var reputationChange = _reputationService.CalculateReputationChange(tempEvaluation);
                    evaluatedUser.ReputationScore += reputationChange;
                    evaluatedUser.ReputationLevel = _reputationService.GetReputationLevel(evaluatedUser.ReputationScore);
                }

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "评价提交成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"评价订单失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取附近需求接口
        /// 根据地理位置获取附近的互助需求
        /// </summary>
        /// <param name="longitude">用户所在经度</param>
        /// <param name="latitude">用户所在纬度</param>
        /// <param name="radius">搜索半径（米），默认为3000米</param>
        /// <returns>附近订单列表</returns>
        [HttpGet("nearby")]
        [Authorize]
        public async Task<IActionResult> GetNearbyOrders([FromQuery] double longitude, [FromQuery] double latitude, [FromQuery] double radius = 3000)
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

                // 验证坐标
                if (!_geolocationService.IsValidCoordinate(latitude, longitude))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "无效的坐标值"
                    });
                }

                // 使用社区+距离的筛选逻辑
                var nearbyOrders = await _geolocationService.GetNearbyServicesAcrossCommunitiesAsync(
                    (double)latitude, (double)longitude, radius / 1000.0); // 转换为公里

                // 过滤掉自己的订单
                nearbyOrders = nearbyOrders.Where(o => o.OwnerId != userId).Take(20).ToList();

                // 构建响应数据
                var orderList = nearbyOrders.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    pet_type = o.PetType,
                    service_type = o.ServiceType,
                    start_time = o.StartTime,
                    end_time = o.EndTime,
                    description = o.Description,
                    distance = Math.Round(o.Distance ?? 0, 2),
                    created_at = o.CreatedAt
                });

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = orderList,
                    Message = $"获取附近订单成功，共找到 {orderList.Count()} 个订单"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取附近订单失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 手机号脱敏处理
        /// </summary>
        private string MaskPhoneNumber(string phone)
        {
            if (string.IsNullOrEmpty(phone) || phone.Length < 7)
                return phone;

            return phone.Substring(0, 3) + "****" + phone.Substring(7);
        }

        /// <summary>
        /// 创建订单请求模型
        /// </summary>
        public class CreateOrderRequest
        {
            public string Title { get; set; } = string.Empty;
            public string PetType { get; set; } = string.Empty;
            public string ServiceType { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string? Description { get; set; }
        }

        /// <summary>
        /// 评价订单请求模型
        /// </summary>
        public class EvaluateOrderRequest
        {
            public int Score { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}