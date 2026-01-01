using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Models;
using petpal.API.Services;
using petpal.API.Models.DTOs;
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

        public UserController(
            IUserService userService,
            IOrderService orderService,
            IRequestService requestService)
        {
            _userService = userService;
            _orderService = orderService;
            _requestService = requestService;
        }

        // ===============================
        // 用户信息管理接口
        // ===============================

        /// <summary>
        /// 获取当前用户的通用资料（用户名 / 手机 / 邮箱 等）
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "用户未认证" });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new ApiResponse { Success = false, Message = "用户不存在" });
            }

            var userDto = user.ToUserDto();

            return Ok(new ApiResponse
            {
                Success = true,
                Data = userDto
            });
        }

        /// <summary>
        /// 更新当前用户的通用资料（部分更新）
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCommonProfileRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ApiResponse { Success = false, Message = "用户未认证" });
            }

            try
            {
                await _userService.UpdateCommonProfileAsync(userId, request.Username, request.Phone, request.Email);
                return Ok(new ApiResponse { Success = true, Message = "更新成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
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

        // 信誉接口已移除；请使用订单评价接口（/api/orders/*）获取评价统计或明细。

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
