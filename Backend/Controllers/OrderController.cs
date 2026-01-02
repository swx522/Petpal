using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Services;
using petpal.API.Data;
using petpal.API.Models;
using petpal.API.Models.DTOs;
using System.Security.Claims;
using System.Linq;

namespace petpal.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class OrderController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public OrderController(
            ApplicationDbContext context,
            IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // ===============================
        // 宠物主人 - 订单管理接口
        // ===============================

        /// <summary>
        /// 查询宠物主人发布的所有需求订单
        /// 宠物主人专用
        /// </summary>
        [HttpGet("order/my")]
        [Authorize]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] string? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 验证用户角色
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.User)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有宠物主人才能查看自己的订单"
                    });
                }

                var query = _context.MutualOrders
                    .Include(o => o.Community)
                    .Where(o => o.OwnerId == userId);

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                    {
                        query = query.Where(o => o.Status == orderStatus);
                    }
                }

                var totalCount = await query.CountAsync();
                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var orderList = orders.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    petType = o.PetType,
                    serviceType = o.ServiceType,
                    startTime = o.StartTime,
                    endTime = o.EndTime,
                    status = o.Status.ToString(),
                    community = o.Community?.ToCommunitySimpleDto(),
                    createdAt = o.CreatedAt,
                    acceptedAt = o.AcceptedAt,
                    completedAt = o.CompletedAt
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
                    Message = "获取我的订单列表成功"
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
        /// 获取宠物主人待评价的订单列表
        /// 宠物主人专用
        /// </summary>
        [HttpGet("order/to-evaluate")]
        [Authorize]
        public async Task<IActionResult> GetOrdersToEvaluate()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 验证用户角色
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.User)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有宠物主人才能查看待评价订单"
                    });
                }

                // 获取已完成的订单中还未评价的
                var completedOrders = await _context.MutualOrders
                    .Include(o => o.Evaluations)
                    .Where(o => o.OwnerId == userId && o.Status == OrderStatus.Completed)
                    .ToListAsync();

                // 过滤出还未被当前用户评价的订单
                var unevaluatedOrders = completedOrders
                    .Where(o => !o.Evaluations.Any(e => e.EvaluatorId == userId))
                    .Take(10) // 限制数量
                    .ToList();

                var orderList = unevaluatedOrders.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    serviceType = o.ServiceType,
                    completedAt = o.CompletedAt,
                    orderNumber = $"OD{o.CreatedAt:yyyyMMdd}{o.Id.Substring(0, 4).ToUpper()}"
                });

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = orderList,
                    Message = $"获取待评价订单成功，共{orderList.Count()}个订单待评价"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取待评价订单失败: {ex.Message}"
                });
            }
        }

        // ===============================
        // 服务者 - 订单管理接口
        // ===============================

        /// <summary>
        /// 获取服务者已完成的订单列表
        /// 服务者专用
        /// </summary>
        [HttpGet("orders/finished")]
        [Authorize]
        public async Task<IActionResult> GetFinishedOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 验证用户是否为服务者
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有服务者才能查看已完成订单"
                    });
                }

                // 获取服务者接受的已完成订单
                var query = _context.MutualOrders
                    .Include(o => o.Owner)
                    .Include(o => o.Evaluations)
                    .Where(o => o.Status == OrderStatus.Completed);

                // 这里需要通过某种方式关联服务者，暂时简化处理
                // 实际应该通过订单状态变化历史或专门的关联表来确定服务者

                var totalCount = await query.CountAsync();
                var orders = await query
                    .OrderByDescending(o => o.CompletedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var orderList = orders.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    serviceType = o.ServiceType,
                    completedAt = o.CompletedAt,
                    owner = o.Owner?.ToUserDto(),
                    evaluationCount = o.Evaluations.Count
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
                    Message = "获取已完成订单列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取已完成订单失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 查看用户对单个已完成订单的评价
        /// 服务者专用
        /// </summary>
        [HttpGet("orders/feedback/{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderFeedback(string id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 验证用户是否为服务者
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有服务者才能查看订单评价"
                    });
                }

                // 获取订单及其评价
                var order = await _context.MutualOrders
                    .Include(o => o.Evaluations)
                        .ThenInclude(e => e.Evaluator)
                    .FirstOrDefaultAsync(o => o.Id == id && o.Status == OrderStatus.Completed);

                if (order == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "订单不存在或未完成"
                    });
                }

                // 获取对当前服务者的评价（这里需要改进逻辑）
                var evaluations = order.Evaluations
                    .Where(e => e.EvaluationType.Contains("to_helper"))
                    .Select(e => new
                    {
                        evaluationId = e.Id,
                        evaluator = e.Evaluator?.ToUserDto(),
                        score = e.Score,
                        content = e.Content,
                        createdAt = e.CreatedAt
                    })
                    .ToList();

                var responseData = new
                {
                    orderId = order.Id,
                    orderTitle = order.Title,
                    completedAt = order.CompletedAt,
                    evaluations = evaluations,
                    averageScore = evaluations.Any() ? evaluations.Average(e => e.score) : 0
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取订单评价成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取订单评价失败: {ex.Message}"
                });
            }
        }

        // ===============================
        // 评价相关接口（全角色可用）
        // ===============================

        /// <summary>
        /// 对已完成的服务提交评价
        /// 全角色通用（宠物主人评价服务者，服务者评价宠物主人）
        /// </summary>
        [HttpPost("evaluate/submit")]
        [Authorize]
        public async Task<IActionResult> SubmitEvaluation([FromBody] SubmitEvaluationRequest request)
        {
            try
            {
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
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.Status == OrderStatus.Completed);

                if (order == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "订单不存在或未完成"
                    });
                }

                // 验证用户是否有权评价这个订单
                if (order.OwnerId != userId)
                {
                    // 这里应该还有服务者的验证逻辑
                    return Forbid("您没有权限评价此订单");
                }

                // 检查是否已评价
                var existingEvaluation = await _context.OrderEvaluations
                    .FirstOrDefaultAsync(e => e.OrderId == request.OrderId && e.EvaluatorId == userId);

                if (existingEvaluation != null)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "您已经对该订单进行过评价"
                    });
                }

                // 确定评价类型
                string evaluationType = order.OwnerId == userId ? "owner_to_helper" : "helper_to_owner";
                string evaluatedUserId = order.OwnerId == userId ? "helper_id" : order.OwnerId; // 需要改进

                // 创建评价
                var evaluation = new OrderEvaluation
                {
                    OrderId = request.OrderId,
                    EvaluatorId = userId,
                    EvaluatedUserId = evaluatedUserId,
                    EvaluationType = evaluationType,
                    Score = request.Score,
                    Content = request.Content
                };

                _context.OrderEvaluations.Add(evaluation);

                // 评价已记录到 OrderEvaluations，历史信誉分由评价统计代替，不在此处直接修改用户信誉字段。

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
                    Message = $"评价提交失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 修改已提交的评价
        /// 全角色通用
        /// </summary>
        [HttpPut("evaluate/update")]
        [Authorize]
        public async Task<IActionResult> UpdateEvaluation([FromBody] UpdateEvaluationRequest request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                // 获取评价
                var evaluation = await _context.OrderEvaluations
                    .FirstOrDefaultAsync(e => e.Id == request.EvaluationId && e.EvaluatorId == userId);

                if (evaluation == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "评价不存在或无权限修改"
                    });
                }

                // 检查是否在允许修改的时间范围内（例如24小时内）
                var timeDiff = DateTime.Now - evaluation.CreatedAt;
                if (timeDiff.TotalHours > 24)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "评价提交超过24小时，无法修改"
                    });
                }

                // 获取被评价者
                var evaluatedUser = await _context.Users.FindAsync(evaluation.EvaluatedUserId);
                if (evaluatedUser != null)
                {
                    // 撤销之前的信誉变化
                    var oldTempEvaluation = new OrderEvaluation
                    {
                        Score = evaluation.Score,
                        Content = evaluation.Content
                    };
                    // 不再在此处维护用户信誉字段，评价修改仅影响 OrderEvaluations 表
                }

                // 更新评价
                evaluation.Score = request.Score;
                evaluation.Content = request.Content;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "评价修改成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"评价修改失败: {ex.Message}"
                });
            }
        }


        // ===============================
        // 请求模型
        // ===============================

        public class SubmitEvaluationRequest
        {
            public string OrderId { get; set; } = string.Empty;
            public int Score { get; set; }
            public string Content { get; set; } = string.Empty;
        }

        public class UpdateEvaluationRequest
        {
            public string EvaluationId { get; set; } = string.Empty;
            public int Score { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}
