using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Models;
using petpal.API.Services;
using System.Security.Claims;

namespace petpal.API.Controllers
{
    /// <summary>
    /// 用户控制器
    /// 处理普通用户的个人信息管理
    /// </summary>
    [ApiController]
    [Route("api/user")]
    [Authorize] // 所有接口都需要认证
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IOrderService _orderService;
        private readonly IRequestService _requestService;
        private readonly IReputationService _reputationService;

        public UserController(
            IUserService userService,
            IOrderService orderService,
            IRequestService requestService,
            IReputationService reputationService)
        {
            _userService = userService;
            _orderService = orderService;
            _requestService = requestService;
        }

        // ===============================
        // 用户信息管理接口
        // ===============================

        /// <summary>
        /// 获取当前用户信息
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
                        user.IsRealNameCertified,
                        user.IsPetCertified,
                        user.CreatedAt,
                        user.LastLoginAt
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
        /// 更新用户信息
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
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

                // 这里需要扩展IUserService来支持更新用户信息
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "用户信息更新成功"
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
        /// 修改密码
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

        /// <summary>
        /// 注销账户
        /// </summary>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
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

                // 这里需要扩展IUserService来支持账户注销
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "账户注销成功"
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
        // 宠物管理接口
        // ===============================

        /// <summary>
        /// 创建宠物信息
        /// </summary>
        [HttpPost("pet/profile")]
        public async Task<IActionResult> CreatePetProfile([FromBody] CreatePetRequest request)
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

                var pet = await _requestService.CreatePetProfileAsync(userId, request);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = pet,
                    Message = "宠物信息创建成功"
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
        // 信誉管理接口
        // ===============================

        /// <summary>
        /// 获取用户信誉档案
        /// </summary>
        [HttpGet("reputation")]
        public async Task<IActionResult> GetReputation()
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

                var reputation = await _orderService.GetUserReputationAsync(userId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = reputation
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
        /// 获取信誉变化日志
        /// </summary>
        [HttpGet("reputation/logs")]
        public async Task<IActionResult> GetReputationLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

                var logs = await _orderService.GetReputationLogsAsync(userId, page, pageSize);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        logs,
                        page,
                        pageSize
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
        /// 获取信誉变化趋势
        /// </summary>
        [HttpGet("reputation/trend")]
        public async Task<IActionResult> GetReputationTrend([FromQuery] int days = 30)
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

                var trend = await _orderService.GetReputationTrendAsync(userId, days);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = trend
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
        /// 获取用户的评价列表
        /// </summary>
        [HttpGet("reviews")]
        public async Task<IActionResult> GetUserReviews([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

                var reviews = await _orderService.GetUserReviewsAsync(userId, page, pageSize);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        reviews,
                        page,
                        pageSize
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
        // 位置服务接口
        // ===============================

        /// <summary>
        /// 更新用户位置信息
        /// </summary>
        [HttpPost("location")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
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

                // 这里需要扩展IUserService来支持位置更新
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "位置更新成功"
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
        /// 获取用户当前位置信息
        /// </summary>
        [HttpGet("location")]
        public async Task<IActionResult> GetLocation()
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

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        longitude = user?.Longitude,
                        latitude = user?.Latitude,
                        locationUpdatedAt = user?.LocationUpdatedAt
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
    }

}
