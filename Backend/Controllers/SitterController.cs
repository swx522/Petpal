using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Models;
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
        private readonly IReputationService _reputationService;

        public SitterController(
            IRequestService requestService,
            IOrderService orderService,
            IUserService userService,
            IReputationService reputationService)
        {
            _requestService = requestService;
            _orderService = orderService;
            _userService = userService;
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

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        sitterId = user.Id,
                        username = user.Username,
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

                var requests = await _requestService.GetAvailableRequestsAsync(userId, filters);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        requests,
                        filters.Page,
                        filters.PageSize
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
                    Message = "成功接受需求，请按约定时间提供服务"
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

        /// <summary>
        /// 获取需求发布者的信誉评分
        /// </summary>
        [HttpGet("user/reputation/{userId}")]
        public async Task<IActionResult> GetRequesterReputation(string userId)
        {
            try
            {
                var reputation = await _orderService.GetUserReputationAsync(userId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        reputation.UserId,
                        reputation.Username,
                        reputation.ReputationScore,
                        reputation.ReputationLevel
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
        // 服务者个人信息管理接口
        // ===============================

        /// <summary>
        /// 获取服务者个人信息
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
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

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        user.Id,
                        user.Username,
                        user.Phone,
                        user.Email,
                        user.Role,
                        user.ReputationScore,
                        reputationLevel = _reputationService.GetReputationLevel(user.ReputationScore),
                        user.CareIntroduction,
                        user.ServiceTypes,
                        user.QualificationDocuments,
                        user.SitterAuditStatus,
                        user.IsRealNameCertified,
                        user.IsPetCertified
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
        /// 更新服务者个人信息
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateSitterProfileRequest request)
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

                // 这里需要扩展IUserService来支持服务者资料更新
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "服务者资料更新成功"
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
        /// 修改服务者密码
        /// </summary>
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
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

                if (string.IsNullOrWhiteSpace(request.OldPassword) ||
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "旧密码和新密码不能为空"
                    });
                }

                // 这里需要扩展IUserService来支持密码修改
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "密码修改成功"
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
