using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using petpal.API.Models.DTOs;
using petpal.API.Services;
using System.Security.Claims;

namespace petpal.API.Controllers
{
    /// <summary>
    /// 服务者控制器
    /// 处理服务者（Sitter）相关的所有操作
    /// </summary>
    [ApiController]
    [Route("api/sitter")]
    [Authorize] // 所有接口都需要认证
    public class SitterController : ControllerBase
    {
        private readonly IRequestService _requestService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public SitterController(
            IRequestService requestService,
            IOrderService orderService,
            IUserService userService,
            ApplicationDbContext context)
        {
            _requestService = requestService;
            _orderService = orderService;
            _userService = userService;
            _context = context;
        }

        // ===============================
        // 服务者资质管理接口
        // ===============================

        /// <summary>
        /// 获取服务者审核状态
        /// </summary>
        [HttpGet("audit/status")]
        public async Task<IActionResult> GetAuditStatus()
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

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                var userDto = user.ToUserDto();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        sitterId = user.Id,
                        user = userDto,
                        auditStatus = user.SitterAuditStatus.ToString(),
                        stageDescription = GetAuditStageDescription(user.SitterAuditStatus),
                        estimatedCompletion = GetEstimatedCompletion(user.SitterAuditStatus),
                        progress = GetAuditProgress(user.SitterAuditStatus),
                        appliedAt = user.CreatedAt,
                        lastAuditAt = user.AuditAt
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 提交服务者资格申请
        /// </summary>
        [HttpPost("application")]
        public async Task<IActionResult> SubmitSitterApplication([FromBody] SubmitSitterApplicationRequest request)
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

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "用户不存在"
                    });
                }

                // 检查用户是否已经是服务者
                if (user.Role == UserRole.Sitter && user.SitterAuditStatus == SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "您已经是审核通过的服务者"
                    });
                }

                // 检查是否已有待审核的申请
                var existingApplication = await _context.SitterApplications
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == SitterAuditStatus.Pending);

                if (existingApplication != null)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "您已有待审核的申请，请耐心等待"
                    });
                }

                // 验证输入数据
                if (string.IsNullOrWhiteSpace(request.RealName))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "真实姓名不能为空"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.IdCardNumber))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "身份证号不能为空"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.JoinReason))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "加入原因不能为空"
                    });
                }

                // 创建申请记录
                var application = new SitterApplication
                {
                    UserId = userId,
                    RealName = request.RealName.Trim(),
                    IdCardNumber = request.IdCardNumber.Trim(),
                    JoinReason = request.JoinReason.Trim(),
                    Status = SitterAuditStatus.Pending
                };

                _context.SitterApplications.Add(application);

                // 更新用户审核状态
                user.SitterAuditStatus = SitterAuditStatus.Pending;
                _context.Users.Update(user);

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "服务者资格申请提交成功，请等待管理员审核"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"申请提交失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取我的申请记录
        /// </summary>
        [HttpGet("application")]
        public async Task<IActionResult> GetMyApplication()
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

                var application = await _context.SitterApplications
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.AppliedAt)
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "未找到申请记录"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        id = application.Id,
                        realName = application.RealName,
                        idCardNumber = application.IdCardNumber,
                        joinReason = application.JoinReason,
                        status = application.Status.ToString(),
                        appliedAt = application.AppliedAt,
                        reviewedAt = application.ReviewedAt,
                        reviewComment = application.ReviewComment
                    },
                    Message = "获取申请记录成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 提交审核资料
        /// </summary>
        [HttpPost("audit/materials")]
        public async Task<IActionResult> SubmitAuditMaterial([FromBody] SubmitAuditMaterialRequest request)
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

                // 这里需要扩展服务来处理审核资料提交
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "审核资料提交成功，等待管理员审核"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 获取已提交的审核资料
        /// </summary>
        [HttpGet("audit/materials")]
        public async Task<IActionResult> GetAuditMaterials()
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

                // 这里需要扩展服务来获取审核资料
                // 暂时返回空列表
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        materials = new List<object>(),
                        totalCount = 0
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ===============================
        // 服务者接单接口
        // ===============================

        /// <summary>
        /// 获取可接单的需求列表
        /// </summary>
        [HttpGet("requests/available")]
        public async Task<IActionResult> GetAvailableRequests([FromQuery] RequestFilters filters)
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

                var result = await _requestService.GetAvailableRequestsAsync(userId, filters);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        requests = result.Items,
                        pagination = new
                        {
                            result.Page,
                            result.PageSize,
                            result.TotalCount,
                            totalPages = result.TotalPages
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 查看单个可接需求的详情
        /// </summary>
        [HttpGet("requests/detail/{requestId}")]
        public async Task<IActionResult> GetRequestDetail(string requestId)
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

                var detail = await _requestService.GetRequestDetailAsync(requestId, userId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = detail
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 接受需求（接单）
        /// </summary>
        [HttpPost("requests/accept/{requestId}")]
        public async Task<IActionResult> AcceptRequest(string requestId)
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

                var result = await _requestService.AcceptRequestAsync(userId, requestId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result,
                    Message = "成功接受并完成需求，可以进行评价了"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        [HttpGet("location/distance")]
        public async Task<IActionResult> CalculateDistance([FromQuery] string targetUserId)
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

                var distance = await _requestService.CalculateDistanceAsync(userId, targetUserId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        distance,
                        unit = "米"
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // ===============================
        // 服务者订单管理接口
        // ===============================

        /// <summary>
        /// 获取服务者已完成的订单列表
        /// </summary>
        [HttpGet("orders/finished")]
        public async Task<IActionResult> GetFinishedOrders()
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

                var orders = await _orderService.GetCompletedOrdersAsync(userId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = orders
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// 查看用户对单个已完成订单的评价
        /// </summary>
        [HttpGet("orders/feedback/{orderId}")]
        public async Task<IActionResult> GetOrderFeedback(string orderId)
        {
            try
            {
                var evaluation = await _orderService.GetOrderEvaluationAsync(orderId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = evaluation
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // 已移除基于“信誉分”的接口；如需查看某用户的评价统计，请使用订单评价查询接口（/api/orders/{orderId}/ratings 或 /api/user/reviews）。

        // ===============================
        // 服务者个人信息管理接口
        // ===============================

        // 服务者个人 profile 已统一到 `api/profile`，此处相关路由已移除以避免重复实现。

        // 密码修改接口已合并到 `api/user/password`，此处已移除以避免重复实现。

        // 辅助方法
        private string GetAuditStageDescription(SitterAuditStatus status)
        {
            return status switch
            {
                SitterAuditStatus.NotApplied => "未申请",
                SitterAuditStatus.Pending => "资料审核中",
                SitterAuditStatus.Approved => "审核通过",
                SitterAuditStatus.Rejected => "审核拒绝",
                SitterAuditStatus.Resubmitted => "已重新提交",
                _ => "未知状态"
            };
        }

        private string GetEstimatedCompletion(SitterAuditStatus status)
        {
            return status switch
            {
                SitterAuditStatus.Pending => "1-3个工作日",
                SitterAuditStatus.Resubmitted => "1-3个工作日",
                _ => ""
            };
        }

        private int GetAuditProgress(SitterAuditStatus status)
        {
            return status switch
            {
                SitterAuditStatus.NotApplied => 0,
                SitterAuditStatus.Pending => 25,
                SitterAuditStatus.Approved => 100,
                SitterAuditStatus.Rejected => 0,
                SitterAuditStatus.Resubmitted => 25,
                _ => 0
            };
        }
    }

}
