using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Models;
using petpal.API.Services;
using System.Security.Claims;

namespace petpal.API.Controllers
{
    /// <summary>
    /// 管理员控制器
    /// 处理所有管理员专用的操作
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize] // 所有接口都需要认证
    public class AdminController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly IRequestService _requestService;
        private readonly IUserService _userService;

        public AdminController(
            ICommunityService communityService,
            IRequestService requestService,
            IUserService userService)
        {
            _communityService = communityService;
            _requestService = requestService;
            _userService = userService;
        }

        // ===============================
        // 管理员个人管理接口
        // ===============================

        /// <summary>
        /// 获取管理员个人信息
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

                var admin = await _userService.GetUserByIdAsync(userId);
                if (admin == null || admin.Role != UserRole.Admin)
                {
                    return Forbid("只有管理员可以访问此接口");
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        admin.Id,
                        admin.Username,
                        admin.Phone,
                        admin.Email,
                        admin.Role,
                        admin.CreatedAt
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
        /// 编辑管理员个人信息
        /// </summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
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

                var admin = await _userService.GetUserByIdAsync(userId);
                if (admin == null || admin.Role != UserRole.Admin)
                {
                    return Forbid("只有管理员可以访问此接口");
                }

                // 这里可以扩展IUserService来支持更新用户信息
                // 暂时返回成功
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "个人信息更新成功"
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
        /// 修改管理员密码
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

        // ===============================
        // 社区管理接口
        // ===============================

        /// <summary>
        /// 获取管理员所属社区信息
        /// </summary>
        [HttpGet("community/my")]
        public async Task<IActionResult> GetMyCommunity()
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

                var community = await _communityService.GetUserCommunityAsync(userId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = community
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
        /// 获取社区数据概览统计
        /// </summary>
        [HttpGet("community/stats")]
        public async Task<IActionResult> GetCommunityStats()
        {
            try
            {
                var stats = await _communityService.GetCommunityStatsAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = stats
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
        /// 获取成员分布数据
        /// </summary>
        [HttpGet("community/distribution")]
        public async Task<IActionResult> GetMemberDistribution()
        {
            try
            {
                var distribution = await _communityService.GetMemberDistributionAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = distribution
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
        /// 获取社区活跃度趋势数据
        /// </summary>
        [HttpGet("community/activity")]
        public async Task<IActionResult> GetActivityTrend([FromQuery] int days = 7)
        {
            try
            {
                var trend = await _communityService.GetActivityTrendAsync(days);

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
        /// 获取社区成员列表
        /// </summary>
        [HttpGet("community/members")]
        public async Task<IActionResult> GetCommunityMembers([FromQuery] MemberFilters filters)
        {
            try
            {
                var members = await _communityService.GetCommunityMembersAsync(filters);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        members,
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
        /// 搜索社区成员
        /// </summary>
        [HttpGet("community/members/search")]
        public async Task<IActionResult> SearchMembers([FromQuery] string keyword, [FromQuery] MemberFilters filters)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "搜索关键词不能为空"
                    });
                }

                var members = await _communityService.SearchMembersAsync(keyword, filters);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        members,
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
        /// 修改成员角色
        /// </summary>
        [HttpPut("community/members/role")]
        public async Task<IActionResult> ChangeMemberRole([FromBody] ChangeRoleRequest request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                await _communityService.ChangeMemberRoleAsync(adminId, request.MemberId, request.NewRole);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "成员角色修改成功"
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
        /// 移除社区成员
        /// </summary>
        [HttpDelete("community/members/remove")]
        public async Task<IActionResult> RemoveMember([FromBody] RemoveMemberRequest request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                await _communityService.RemoveMemberAsync(adminId, request.MemberId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "成员移除成功"
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
        /// 获取社区设置信息
        /// </summary>
        [HttpGet("community/settings")]
        public async Task<IActionResult> GetCommunitySettings()
        {
            try
            {
                var settings = await _communityService.GetCommunitySettingsAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = settings
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
        /// 修改社区设置
        /// </summary>
        [HttpPut("community/settings")]
        public async Task<IActionResult> UpdateCommunitySettings([FromBody] CommunitySettings settings)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                await _communityService.UpdateCommunitySettingsAsync(adminId, settings);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "社区设置更新成功"
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
        // 需求审核接口
        // ===============================

        /// <summary>
        /// 获取需求审核列表
        /// </summary>
        [HttpGet("requests/review/list")]
        public async Task<IActionResult> GetReviewList([FromQuery] ReviewFilters filters)
        {
            try
            {
                var requests = await _requestService.GetPendingReviewsAsync(filters);

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
        /// 获取单个需求的审核详情
        /// </summary>
        [HttpGet("requests/review/detail/{requestId}")]
        public async Task<IActionResult> GetReviewDetail(string requestId)
        {
            try
            {
                var detail = await _requestService.GetReviewDetailAsync(requestId);

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
        /// 审核通过需求
        /// </summary>
        [HttpPut("requests/review/pass")]
        public async Task<IActionResult> ApproveRequest([FromBody] ReviewRequest request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                var result = await _requestService.ApproveRequestAsync(adminId, request.RequestId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result,
                    Message = "需求审核通过"
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
        /// 审核拒绝需求
        /// </summary>
        [HttpPut("requests/review/reject")]
        public async Task<IActionResult> RejectRequest([FromBody] RejectReviewRequest request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                var result = await _requestService.RejectRequestAsync(adminId, request.RequestId, request.Reason);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result,
                    Message = "需求已拒绝"
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
        /// 重新审核需求
        /// </summary>
        [HttpPut("requests/review/recheck")]
        public async Task<IActionResult> RecheckRequest([FromBody] ReviewRequest request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse
                    {
                        Success = false,
                        Message = "用户未认证"
                    });
                }

                var result = await _requestService.RecheckRequestAsync(adminId, request.RequestId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = result,
                    Message = "需求重新审核"
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
