using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using petpal.API.Services;
using petpal.API.Models.DTOs;
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
        private readonly ApplicationDbContext _context;

        public AdminController(
            ICommunityService communityService,
            IRequestService requestService,
            IUserService userService,
            ApplicationDbContext context)
        {
            _communityService = communityService;
            _requestService = requestService;
            _userService = userService;
            _context = context;
        }

        // 管理员个人 profile 已统一到 `api/profile`，此处相关路由已移除以避免重复实现。

        // 管理员的密码修改入口已合并到 `api/user/password`，此处移除以避免重复实现。

        // ===============================
        // 社区管理接口
        // ===============================


        /// <summary>
        /// 获取社区数据概览统计
        /// </summary>
        [HttpGet("community/stats")]
        public async Task<IActionResult> GetCommunityStats()
        {
            try
            {
                // 按管理员所属社区统计（若管理员无社区，则返回全站统计）
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? communityId = null;
                if (!string.IsNullOrEmpty(adminId))
                {
                    var adminUser = await _userService.GetUserByIdAsync(adminId);
                    if (adminUser != null && adminUser.CommunityId.HasValue)
                    {
                        communityId = adminUser.CommunityId;
                    }
                }

                var stats = await _communityService.GetCommunityStatsAsync(communityId);

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
                // 尝试获取当前管理员所属社区，并按社区统计
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? communityId = null;
                if (!string.IsNullOrEmpty(adminId))
                {
                    var adminUser = await _userService.GetUserByIdAsync(adminId);
                    if (adminUser != null && adminUser.CommunityId.HasValue)
                    {
                        communityId = adminUser.CommunityId;
                    }
                }

                var distribution = await _communityService.GetMemberDistributionAsync(communityId);

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
                // 限定为当前管理员所属社区的成员（若管理员属于某社区）
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(adminId))
                {
                    var adminUser = await _userService.GetUserByIdAsync(adminId);
                    if (adminUser != null && adminUser.CommunityId.HasValue)
                    {
                        filters.CommunityId = adminUser.CommunityId;
                    }
                }

                // 获取包含统计信息的成员列表
                var membersWithStats = await _communityService.GetCommunityMembersWithStatsAsync(filters);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        members = membersWithStats,
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
                var memberDtos = members.Select(u => u.ToUserDto()).ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        members = memberDtos,
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
        [HttpDelete("community/members/remove/{memberId}")]
        public async Task<IActionResult> RemoveMember(string memberId)
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

                await _communityService.RemoveMemberAsync(adminId, memberId);

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

        /// <summary>
        /// 删除审核记录
        /// </summary>
        [HttpDelete("requests/review/delete")]
        public async Task<IActionResult> DeleteReview([FromBody] ReviewRequest request)
        {
            try
            {
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized(new ApiResponse { Success = false, Message = "用户未认证" });
                }

                await _requestService.DeleteReviewAsync(adminId, request.RequestId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "删除审核记录成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse { Success = false, Message = ex.Message });
            }
        }

        /// <summary>
        /// 获取待审核的服务者申请列表
        /// </summary>
        [HttpGet("sitter/applications/pending")]
        public async Task<IActionResult> GetPendingSitterApplications()
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

                var admin = await _userService.GetUserByIdAsync(adminId);
                if (admin == null || admin.Role != UserRole.Admin)
                {
                    return Forbid();
                }

                var applications = await _context.SitterApplications
                    .Include(a => a.User)
                    .Where(a => a.Status == SitterAuditStatus.Pending)
                    .OrderBy(a => a.AppliedAt)
                    .Select(a => new
                    {
                        id = a.Id,
                        userId = a.UserId,
                        username = a.User.Username,
                        realName = a.RealName,
                        idCardNumber = a.IdCardNumber,
                        joinReason = a.JoinReason,
                        appliedAt = a.AppliedAt
                    })
                    .ToListAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        applications,
                        totalCount = applications.Count
                    },
                    Message = "获取待审核申请成功"
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
        /// 审核服务者资格申请
        /// </summary>
        [HttpPost("sitter/applications/review")]
        public async Task<IActionResult> ReviewSitterApplication([FromBody] ReviewSitterApplicationRequest request)
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

                var admin = await _userService.GetUserByIdAsync(adminId);
                if (admin == null || admin.Role != UserRole.Admin)
                {
                    return Forbid();
                }

                var application = await _context.SitterApplications
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == request.ApplicationId);

                if (application == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "申请记录不存在"
                    });
                }

                if (application.Status != SitterAuditStatus.Pending)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "该申请已被处理"
                    });
                }

                // 更新申请状态
                application.Status = request.Approved ? SitterAuditStatus.Approved : SitterAuditStatus.Rejected;
                application.ReviewComment = request.ReviewComment?.Trim();
                application.ReviewedAt = DateTime.Now;

                // 更新用户状态
                if (request.Approved)
                {
                    application.User.Role = UserRole.Sitter;
                    application.User.SitterAuditStatus = SitterAuditStatus.Approved;
                }
                else
                {
                    application.User.SitterAuditStatus = SitterAuditStatus.Rejected;
                }

                _context.SitterApplications.Update(application);
                _context.Users.Update(application.User);

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = request.Approved ? "申请已通过" : "申请已拒绝"
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
        /// 获取所有服务者申请记录
        /// </summary>
        [HttpGet("sitter/applications")]
        public async Task<IActionResult> GetAllSitterApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

                var admin = await _userService.GetUserByIdAsync(adminId);
                if (admin == null || admin.Role != UserRole.Admin)
                {
                    return Forbid();
                }

                var query = _context.SitterApplications
                    .Include(a => a.User)
                    .OrderByDescending(a => a.AppliedAt);

                var totalCount = await query.CountAsync();

                var applications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        id = a.Id,
                        userId = a.UserId,
                        username = a.User.Username,
                        realName = a.RealName,
                        idCardNumber = a.IdCardNumber,
                        joinReason = a.JoinReason,
                        status = a.Status.ToString(),
                        appliedAt = a.AppliedAt,
                        reviewedAt = a.ReviewedAt,
                        reviewComment = a.ReviewComment
                    })
                    .ToListAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        applications,
                        pagination = new
                        {
                            page,
                            pageSize,
                            totalCount,
                            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                        }
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
    }

}
