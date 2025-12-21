using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using System.Security.Claims;
using System.Linq;

namespace petpal.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取待审核Sitter列表接口（管理员）
        /// 获取所有待审核状态的Sitter用户
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>待审核Sitter列表</returns>
        [HttpGet("sitters/pending")]
        [Authorize]
        public async Task<IActionResult> GetPendingSitters([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // 验证管理员权限
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != UserRole.Admin.ToString())
                {
                    return Forbid("需要管理员权限");
                }

                // 获取待审核的Sitter
                var query = _context.Users
                    .Where(u => u.Role == UserRole.Sitter &&
                               (u.SitterAuditStatus == SitterAuditStatus.Pending ||
                                u.SitterAuditStatus == SitterAuditStatus.Resubmitted));

                var totalCount = await query.CountAsync();
                var sitters = await query
                    .OrderBy(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 构建响应数据
                var sitterList = sitters.Select(s => new
                {
                    userId = s.Id,
                    username = s.Username,
                    phone = s.Phone, // 管理员可以看到完整手机号
                    email = s.Email,
                    role = s.Role.ToString(),
                    sitterAuditStatus = s.SitterAuditStatus.ToString(),
                    careIntroduction = s.CareIntroduction,
                    serviceTypes = s.ServiceTypes,
                    qualificationDocuments = ParseQualificationDocuments(s.QualificationDocuments),
                    createdAt = s.CreatedAt,
                    lastLoginAt = s.LastLoginAt
                });

                var responseData = new
                {
                    sitters = sitterList,
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
                    Message = "获取待审核Sitter列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取待审核Sitter列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取Sitter详情接口（管理员）
        /// 获取指定Sitter的详细信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>Sitter详情</returns>
        [HttpGet("sitters/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetSitterDetail(string userId)
        {
            try
            {
                // 验证管理员权限
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != UserRole.Admin.ToString())
                {
                    return Forbid("需要管理员权限");
                }

                // 获取Sitter信息
                var sitter = await _context.Users
                    .Include(u => u.Pets)
                    .Include(u => u.OrdersAsHelper)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Sitter);

                if (sitter == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Sitter不存在"
                    });
                }

                // 构建响应数据
                var responseData = new
                {
                    userId = sitter.Id,
                    username = sitter.Username,
                    phone = sitter.Phone,
                    email = sitter.Email,
                    role = sitter.Role.ToString(),
                    status = sitter.Status.ToString(),
                    reputationScore = sitter.ReputationScore,
                    reputationLevel = sitter.ReputationLevel,
                    isRealNameCertified = sitter.IsRealNameCertified,
                    isPetCertified = sitter.IsPetCertified,
                    sitterAuditStatus = sitter.SitterAuditStatus.ToString(),
                    careIntroduction = sitter.CareIntroduction,
                    serviceTypes = sitter.ServiceTypes,
                    qualificationDocuments = ParseQualificationDocuments(sitter.QualificationDocuments),
                    auditAt = sitter.AuditAt,
                    auditBy = sitter.AuditBy,
                    auditComment = sitter.AuditComment,
                    rejectionReason = sitter.RejectionReason,
                    createdAt = sitter.CreatedAt,
                    lastLoginAt = sitter.LastLoginAt,
                    pets = sitter.Pets.Select(p => new
                    {
                        petId = p.Id,
                        name = p.Name,
                        type = p.Type,
                        breed = p.Breed,
                        age = p.Age,
                        isVaccinated = p.IsVaccinated,
                        description = p.Description,
                        createdAt = p.CreatedAt
                    }),
                    completedOrdersCount = sitter.OrdersAsHelper.Count(o => o.Status == OrderStatus.Completed)
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取Sitter详情成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取Sitter详情失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 审核通过Sitter资质接口（管理员）
        /// 管理员审核通过Sitter的资质申请
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">审核请求体</param>
        /// <returns>审核结果</returns>
        [HttpPut("sitters/{userId}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveSitter(string userId, [FromBody] AuditSitterRequest request)
        {
            try
            {
                // 验证管理员权限
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != UserRole.Admin.ToString())
                {
                    return Forbid("需要管理员权限");
                }

                // 获取Sitter信息
                var sitter = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Sitter);
                if (sitter == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Sitter不存在"
                    });
                }

                // 验证审核状态
                if (sitter.SitterAuditStatus != SitterAuditStatus.Pending &&
                    sitter.SitterAuditStatus != SitterAuditStatus.Resubmitted)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "该Sitter的审核状态不允许此操作"
                    });
                }

                // 更新审核信息
                sitter.SitterAuditStatus = SitterAuditStatus.Approved;
                sitter.AuditAt = DateTime.Now;
                sitter.AuditBy = adminId;
                sitter.AuditComment = request.Comment;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Sitter资质审核通过，该用户现在可以接受互助订单"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"审核通过失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 审核拒绝Sitter资质接口（管理员）
        /// 管理员拒绝Sitter的资质申请
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="request">审核请求体</param>
        /// <returns>审核结果</returns>
        [HttpPut("sitters/{userId}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectSitter(string userId, [FromBody] AuditSitterRequest request)
        {
            try
            {
                // 验证管理员权限
                var adminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != UserRole.Admin.ToString())
                {
                    return Forbid("需要管理员权限");
                }

                // 验证拒绝原因
                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "拒绝审核必须填写拒绝原因"
                    });
                }

                // 获取Sitter信息
                var sitter = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.Role == UserRole.Sitter);
                if (sitter == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Sitter不存在"
                    });
                }

                // 验证审核状态
                if (sitter.SitterAuditStatus != SitterAuditStatus.Pending &&
                    sitter.SitterAuditStatus != SitterAuditStatus.Resubmitted)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "该Sitter的审核状态不允许此操作"
                    });
                }

                // 更新审核信息
                sitter.SitterAuditStatus = SitterAuditStatus.Rejected;
                sitter.AuditAt = DateTime.Now;
                sitter.AuditBy = adminId;
                sitter.AuditComment = request.Comment;
                sitter.RejectionReason = request.RejectionReason;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Sitter资质审核已拒绝，用户可以修改资料后重新申请"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"审核拒绝失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取所有Sitter列表接口（管理员）
        /// 获取所有Sitter用户，支持状态过滤和分页
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="status">审核状态过滤</param>
        /// <returns>Sitter列表</returns>
        [HttpGet("sitters")]
        [Authorize]
        public async Task<IActionResult> GetAllSitters([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            try
            {
                // 验证管理员权限
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole != UserRole.Admin.ToString())
                {
                    return Forbid("需要管理员权限");
                }

                // 构建查询
                var query = _context.Users.Where(u => u.Role == UserRole.Sitter);

                // 状态过滤
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<SitterAuditStatus>(status, true, out var auditStatus))
                {
                    query = query.Where(u => u.SitterAuditStatus == auditStatus);
                }

                var totalCount = await query.CountAsync();
                var sitters = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 构建响应数据
                var sitterList = sitters.Select(s => new
                {
                    userId = s.Id,
                    username = s.Username,
                    phone = s.Phone,
                    email = s.Email,
                    role = s.Role.ToString(),
                    status = s.Status.ToString(),
                    reputationScore = s.ReputationScore,
                    reputationLevel = s.ReputationLevel,
                    sitterAuditStatus = s.SitterAuditStatus.ToString(),
                    careIntroduction = s.CareIntroduction,
                    serviceTypes = s.ServiceTypes,
                    qualificationDocuments = ParseQualificationDocuments(s.QualificationDocuments),
                    auditAt = s.AuditAt,
                    auditComment = s.AuditComment,
                    rejectionReason = s.RejectionReason,
                    createdAt = s.CreatedAt,
                    lastLoginAt = s.LastLoginAt
                });

                var responseData = new
                {
                    sitters = sitterList,
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
                    Message = "获取Sitter列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取Sitter列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 解析资质文件列表
        /// 将JSON字符串解析为文件列表
        /// </summary>
        private List<string> ParseQualificationDocuments(string? documentsJson)
        {
            if (string.IsNullOrEmpty(documentsJson))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(documentsJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Sitter审核请求模型
        /// </summary>
        public class AuditSitterRequest
        {
            public string Comment { get; set; } = string.Empty;
            public string? RejectionReason { get; set; }
        }
    }
}
