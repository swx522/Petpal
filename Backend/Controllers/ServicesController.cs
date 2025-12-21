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
    [Route("api/v1/services")]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IGeolocationService _geolocationService;

        public ServicesController(
            ApplicationDbContext context,
            IUserService userService,
            IGeolocationService geolocationService)
        {
            _context = context;
            _userService = userService;
            _geolocationService = geolocationService;
        }

        /// <summary>
        /// 获取服务列表接口（带筛选）
        /// 获取待接单的服务列表，支持按服务类型、状态筛选，实现社区+距离的本地化服务
        /// </summary>
        /// <param name="type">服务类型筛选（可选）</param>
        /// <param name="distanceRange">距离范围（公里，默认3公里）</param>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页数量</param>
        /// <returns>服务列表</returns>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetServices([FromQuery] string? type, [FromQuery] double distanceRange = 3.0, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

                // 验证用户是否为Sitter
                var user = await _context.Users
                    .Include(u => u.Community)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || user.Role != UserRole.Sitter || user.SitterAuditStatus != SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有审核通过的Sitter才能查看服务列表"
                    });
                }

                // 检查用户是否有定位信息
                if (!user.Longitude.HasValue || !user.Latitude.HasValue)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "请先更新您的定位信息"
                    });
                }

                var userLng = (double)user.Longitude.Value;
                var userLat = (double)user.Latitude.Value;

                // 实现社区优先的筛选逻辑
                List<MutualOrder> allServices;

                if (user.CommunityId.HasValue)
                {
                    // 1. 优先获取同社区的服务（按距离排序）
                    var communityServices = await _geolocationService.GetServicesInCommunityAsync(
                        user.CommunityId.Value, userLat, userLng);

                    // 2. 获取跨社区的附近服务（排除同社区，按距离排序）
                    var nearbyServices = await _geolocationService.GetNearbyServicesAcrossCommunitiesAsync(
                        userLat, userLng, distanceRange, user.CommunityId.Value);

                    // 3. 合并结果：同社区在前，跨社区在后
                    allServices = communityServices.Concat(nearbyServices).ToList();
                }
                else
                {
                    // 如果用户没有社区信息，只获取距离范围内的服务
                    allServices = await _geolocationService.GetNearbyServicesAcrossCommunitiesAsync(
                        userLat, userLng, distanceRange);
                }

                // 服务类型筛选
                if (!string.IsNullOrEmpty(type) && Enum.TryParse<HelpType>(type, true, out var helpType))
                {
                    allServices = allServices.Where(o => o.HelpType == helpType).ToList();
                }

                // 排除自己的订单
                allServices = allServices.Where(o => o.RequesterId != userId).ToList();

                var totalCount = allServices.Count;
                var services = allServices
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // 构建响应数据
                var serviceList = services.Select(o => new
                {
                    serviceId = o.Id,
                    requester = new
                    {
                        userId = o.Requester.Id,
                        username = o.Requester.Username,
                        reputationScore = o.Requester.ReputationScore,
                        reputationLevel = o.Requester.ReputationLevel
                    },
                    pet = new
                    {
                        petId = o.Pet.Id,
                        name = o.Pet.Name,
                        type = o.Pet.Type,
                        breed = o.Pet.Breed,
                        age = o.Pet.Age,
                        isVaccinated = o.Pet.IsVaccinated,
                        description = o.Pet.Description
                    },
                    helpType = o.HelpType.ToString(),
                    startTime = o.StartTime,
                    endTime = o.EndTime,
                    duration = (o.EndTime - o.StartTime).TotalHours,
                    longitude = o.Longitude,
                    latitude = o.Latitude,
                    remark = o.Remark,
                    orderImages = ParseOrderImages(o.OrderImages),
                    createdAt = o.CreatedAt
                });

                var responseData = new
                {
                    services = serviceList,
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
                    Message = "获取服务列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取服务列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取服务详情接口
        /// 获取单个服务的详细信息
        /// </summary>
        /// <param name="serviceId">服务ID</param>
        /// <returns>服务详情</returns>
        [HttpGet("{serviceId}")]
        [Authorize]
        public async Task<IActionResult> GetServiceDetail(string serviceId)
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

                // 验证用户是否为Sitter
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter || user.SitterAuditStatus != SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有审核通过的Sitter才能查看服务详情"
                    });
                }

                // 获取服务详情
                var service = await _context.MutualOrders
                    .Include(o => o.Requester)
                    .Include(o => o.Pet)
                    .FirstOrDefaultAsync(o => o.Id == serviceId && o.Status == OrderStatus.Pending);

                if (service == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "服务不存在或已接单"
                    });
                }

                // 构建响应数据
                var responseData = new
                {
                    serviceId = service.Id,
                    requester = new
                    {
                        userId = service.Requester.Id,
                        username = service.Requester.Username,
                        phone = MaskPhoneNumber(service.Requester.Phone),
                        reputationScore = service.Requester.ReputationScore,
                        reputationLevel = service.Requester.ReputationLevel,
                        isRealNameCertified = service.Requester.IsRealNameCertified,
                        isPetCertified = service.Requester.IsPetCertified
                    },
                    pet = new
                    {
                        petId = service.Pet.Id,
                        name = service.Pet.Name,
                        type = service.Pet.Type,
                        breed = service.Pet.Breed,
                        age = service.Pet.Age,
                        isVaccinated = service.Pet.IsVaccinated,
                        description = service.Pet.Description
                    },
                    helpType = service.HelpType.ToString(),
                    startTime = service.StartTime,
                    endTime = service.EndTime,
                    duration = (service.EndTime - service.StartTime).TotalHours,
                    longitude = service.Longitude,
                    latitude = service.Latitude,
                    remark = service.Remark,
                    orderImages = ParseOrderImages(service.OrderImages),
                    createdAt = service.CreatedAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取服务详情成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取服务详情失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 接单操作接口
        /// Sitter接受服务订单
        /// </summary>
        /// <param name="serviceId">服务ID</param>
        /// <returns>接单结果</returns>
        [HttpPost("{serviceId}/accept")]
        [Authorize]
        public async Task<IActionResult> AcceptService(string serviceId)
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

                // 验证用户是否为Sitter
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter || user.SitterAuditStatus != SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有审核通过的Sitter才能接单"
                    });
                }

                // 获取服务
                var service = await _context.MutualOrders
                    .Include(o => o.Requester)
                    .FirstOrDefaultAsync(o => o.Id == serviceId);

                if (service == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "服务不存在"
                    });
                }

                // 验证服务状态
                if (service.Status != OrderStatus.Pending)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "服务已被其他Sitter接受或已完成"
                    });
                }

                // 验证不能接受自己的服务
                if (service.RequesterId == userId)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "不能接受自己的服务"
                    });
                }

                // 更新服务状态
                service.Status = OrderStatus.Accepted;
                service.HelperId = userId;
                service.AcceptedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "成功接受服务，请按约定时间提供服务"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"接受服务失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 解析订单图片列表
        /// </summary>
        private List<string> ParseOrderImages(string? imagesJson)
        {
            if (string.IsNullOrEmpty(imagesJson))
                return new List<string>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(imagesJson) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
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
    }
}
