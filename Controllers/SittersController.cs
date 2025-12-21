using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using System.Security.Claims;

namespace petpal.API.Controllers
{
    [ApiController]
    [Route("api/v1/sitters")]
    public class SittersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SittersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取审核进度接口
        /// 返回当前审核阶段、预计完成时间、信誉等级等
        /// </summary>
        /// <param name="sitterId">Sitter用户ID</param>
        /// <returns>审核进度信息</returns>
        [HttpGet("{sitterId}/audit/status")]
        [Authorize]
        public async Task<IActionResult> GetAuditStatus(string sitterId)
        {
            try
            {
                // 验证用户权限（只能查看自己的审核状态或管理员查看所有）
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != sitterId && userRole != UserRole.Admin.ToString())
                {
                    return Forbid("只能查看自己的审核状态");
                }

                // 获取Sitter信息
                var sitter = await _context.Users.FirstOrDefaultAsync(u => u.Id == sitterId && u.Role == UserRole.Sitter);
                if (sitter == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Sitter不存在"
                    });
                }

                // 计算审核进度信息
                var auditStatus = GetAuditStatusInfo(sitter);
                var submittedMaterials = await _context.AuditMaterials
                    .Where(m => m.SitterId == sitterId)
                    .CountAsync();

                var responseData = new
                {
                    sitterId = sitter.Id,
                    username = sitter.Username,
                    auditStatus = auditStatus,
                    submittedMaterialsCount = submittedMaterials,
                    reputationLevel = sitter.ReputationLevel,
                    appliedAt = sitter.CreatedAt,
                    lastAuditAt = sitter.AuditAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取审核进度成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取审核进度失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 提交/上传审核资料接口
        /// 上传身份证、宠物健康证明、服务环境照片等审核材料
        /// </summary>
        /// <param name="sitterId">Sitter用户ID</param>
        /// <param name="request">审核资料请求</param>
        /// <returns>上传结果</returns>
        [HttpPost("{sitterId}/audit/materials")]
        [Authorize]
        public async Task<IActionResult> SubmitAuditMaterials(string sitterId, [FromBody] SubmitAuditMaterialsRequest request)
        {
            try
            {
                // 验证用户权限
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (currentUserId != sitterId)
                {
                    return Forbid("只能提交自己的审核资料");
                }

                // 获取Sitter信息
                var sitter = await _context.Users.FirstOrDefaultAsync(u => u.Id == sitterId && u.Role == UserRole.Sitter);
                if (sitter == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "Sitter不存在"
                    });
                }

                // 验证审核状态
                if (sitter.SitterAuditStatus == SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "审核已通过，无需重复提交资料"
                    });
                }

                // 创建审核材料记录
                var auditMaterial = new AuditMaterial
                {
                    SitterId = sitterId,
                    MaterialType = request.MaterialType,
                    MaterialName = request.MaterialName,
                    FilePath = request.FilePath,
                    FileSize = request.FileSize,
                    ContentType = request.ContentType,
                    Status = AuditMaterialStatus.Pending
                };

                _context.AuditMaterials.Add(auditMaterial);

                // 如果是从审核拒绝状态重新提交，更新Sitter状态
                if (sitter.SitterAuditStatus == SitterAuditStatus.Rejected)
                {
                    sitter.SitterAuditStatus = SitterAuditStatus.Resubmitted;
                }

                await _context.SaveChangesAsync();

                var responseData = new
                {
                    materialId = auditMaterial.Id,
                    materialType = auditMaterial.MaterialType.ToString(),
                    materialName = auditMaterial.MaterialName,
                    uploadedAt = auditMaterial.UploadedAt,
                    status = auditMaterial.Status.ToString()
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "审核资料提交成功，等待管理员审核"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"提交审核资料失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取已提交资料接口
        /// 返回已提交的审核资料列表
        /// </summary>
        /// <param name="sitterId">Sitter用户ID</param>
        /// <returns>审核资料列表</returns>
        [HttpGet("{sitterId}/audit/materials")]
        [Authorize]
        public async Task<IActionResult> GetAuditMaterials(string sitterId)
        {
            try
            {
                // 验证用户权限（只能查看自己的资料或管理员查看所有）
                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (currentUserId != sitterId && userRole != UserRole.Admin.ToString())
                {
                    return Forbid("只能查看自己的审核资料");
                }

                // 获取审核资料
                var materials = await _context.AuditMaterials
                    .Where(m => m.SitterId == sitterId)
                    .OrderByDescending(m => m.UploadedAt)
                    .ToListAsync();

                // 构建响应数据
                var materialList = materials.Select(m => new
                {
                    materialId = m.Id,
                    materialType = m.MaterialType.ToString(),
                    materialName = m.MaterialName,
                    filePath = m.FilePath,
                    fileSize = m.FileSize,
                    contentType = m.ContentType,
                    status = m.Status.ToString(),
                    reviewComment = m.ReviewComment,
                    uploadedAt = m.UploadedAt,
                    reviewedAt = m.ReviewedAt
                });

                var responseData = new
                {
                    sitterId = sitterId,
                    materials = materialList,
                    totalCount = materials.Count
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取审核资料成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取审核资料失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取审核状态信息
        /// </summary>
        private object GetAuditStatusInfo(User sitter)
        {
            var statusInfo = new
            {
                currentStage = sitter.SitterAuditStatus.ToString(),
                stageDescription = GetStageDescription(sitter.SitterAuditStatus),
                estimatedCompletion = GetEstimatedCompletion(sitter.SitterAuditStatus),
                progress = GetProgressPercentage(sitter.SitterAuditStatus)
            };

            return statusInfo;
        }

        /// <summary>
        /// 获取审核阶段描述
        /// </summary>
        private string GetStageDescription(SitterAuditStatus status)
        {
            return status switch
            {
                SitterAuditStatus.NotApplied => "未申请",
                SitterAuditStatus.Pending => "资料审核中",
                SitterAuditStatus.Approved => "审核通过",
                SitterAuditStatus.Rejected => "审核未通过",
                SitterAuditStatus.Resubmitted => "资料重新提交审核中",
                _ => "未知状态"
            };
        }

        /// <summary>
        /// 获取预计完成时间
        /// </summary>
        private string GetEstimatedCompletion(SitterAuditStatus status)
        {
            return status switch
            {
                SitterAuditStatus.Pending => "1-3个工作日",
                SitterAuditStatus.Resubmitted => "1-3个工作日",
                SitterAuditStatus.Approved => "已完成",
                SitterAuditStatus.Rejected => "需要重新提交资料",
                _ => "未知"
            };
        }

        /// <summary>
        /// 获取进度百分比
        /// </summary>
        private int GetProgressPercentage(SitterAuditStatus status)
        {
            return status switch
            {
                SitterAuditStatus.NotApplied => 0,
                SitterAuditStatus.Pending => 25,
                SitterAuditStatus.Resubmitted => 50,
                SitterAuditStatus.Approved => 100,
                SitterAuditStatus.Rejected => 0,
                _ => 0
            };
        }

        /// <summary>
        /// 审核资料提交请求模型
        /// </summary>
        public class SubmitAuditMaterialsRequest
        {
            public AuditMaterialType MaterialType { get; set; }
            public string MaterialName { get; set; } = string.Empty;
            public string FilePath { get; set; } = string.Empty;
            public long FileSize { get; set; }
            public string? ContentType { get; set; }
        }
    }
}
