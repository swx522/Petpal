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
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IGeolocationService _geolocationService;

        public RequestsController(
            ApplicationDbContext context,
            IUserService userService,
            IGeolocationService geolocationService)
        {
            _context = context;
            _userService = userService;
            _geolocationService = geolocationService;
        }

        // ===============================
        // 宠物主人 - 需求发布相关接口
        // ===============================

        /// <summary>
        /// 获取宠物类型列表
        /// 宠物主人专用
        /// </summary>
        [HttpGet("pet/types")]
        [Authorize]
        public IActionResult GetPetTypes()
        {
            var petTypes = new[]
            {
                new { value = "dog", label = "狗狗 🐶", description = "犬类宠物" },
                new { value = "cat", label = "猫咪 🐱", description = "猫类宠物" },
                new { value = "rabbit", label = "兔兔 🐰", description = "兔子等小型宠物" },
                new { value = "bird", label = "鸟鸟 🐦", description = "鸟类宠物" },
                new { value = "other", label = "其他 🐾", description = "其他宠物类型" }
            };

            return Ok(new ApiResponse
            {
                Success = true,
                Data = petTypes,
                Message = "获取宠物类型列表成功"
            });
        }

        /// <summary>
        /// 提交宠物信息
        /// 宠物主人专用
        /// </summary>
        [HttpPost("pet/profile")]
        [Authorize]
        public async Task<IActionResult> SubmitPetProfile([FromBody] SubmitPetProfileRequest request)
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
                        Message = "只有宠物主人才能提交宠物信息"
                    });
                }

                // 创建宠物信息
                var pet = new Pet
                {
                    UserId = userId,
                    Name = request.Name,
                    Type = request.Type,
                    Breed = request.Breed,
                    Age = request.Age,
                    IsVaccinated = request.IsVaccinated,
                    Description = request.Description
                };

                _context.Pets.Add(pet);
                await _context.SaveChangesAsync();

                var responseData = new
                {
                    petId = pet.Id,
                    name = pet.Name,
                    type = pet.Type,
                    breed = pet.Breed,
                    age = pet.Age,
                    isVaccinated = pet.IsVaccinated,
                    description = pet.Description,
                    createdAt = pet.CreatedAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "宠物信息提交成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"提交宠物信息失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取服务类型列表
        /// 宠物主人专用
        /// </summary>
        [HttpGet("service/categories")]
        [Authorize]
        public IActionResult GetServiceCategories()
        {
            var serviceCategories = new[]
            {
                new { value = "walk", label = "遛狗服务 🚶", description = "帮您遛狗，保持宠物健康" },
                new { value = "feed", label = "喂食照顾 🍽️", description = "定时喂食，照顾宠物饮食" },
                new { value = "medical", label = "就医陪伴 🏥", description = "陪同宠物就医，提供照顾" },
                new { value = "groom", label = "美容护理 ✂️", description = "洗澡、修剪、美容服务" },
                new { value = "other", label = "其他服务 🐾", description = "其他宠物服务需求" }
            };

            return Ok(new ApiResponse
            {
                Success = true,
                Data = serviceCategories,
                Message = "获取服务类型列表成功"
            });
        }

        /// <summary>
        /// 发布宠物服务需求
        /// 宠物主人专用
        /// </summary>
        [HttpPost("request/create")]
        [Authorize]
        public async Task<IActionResult> CreateRequest([FromBody] CreateRequestRequest request)
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

                // 验证用户角色和认证状态
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.User)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有宠物主人才能发布需求"
                    });
                }

                var isCertified = await _userService.ValidateCertificationAsync(userId);
                if (!isCertified)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "发布需求需要完成实名认证和宠物认证"
                    });
                }

                // 验证时间
                if (request.StartTime >= request.EndTime)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "开始时间必须早于结束时间"
                    });
                }

                // 获取用户位置信息
                if (!user.Longitude.HasValue || !user.Latitude.HasValue)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "请先设置您的位置信息"
                    });
                }

                // 根据用户位置查找社区
                var community = await _geolocationService.FindCommunityByLocationAsync(user.Longitude.Value, user.Latitude.Value);

                // 创建需求（订单）
                var order = new MutualOrder
                {
                    OwnerId = userId,
                    Title = request.Title,
                    PetType = request.PetType,
                    ServiceType = request.ServiceType,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Description = request.Description,
                    CommunityId = community?.Id
                };

                _context.MutualOrders.Add(order);
                await _context.SaveChangesAsync();

                var responseData = new
                {
                    requestId = order.Id,
                    title = order.Title,
                    petType = order.PetType,
                    serviceType = order.ServiceType,
                    startTime = order.StartTime,
                    endTime = order.EndTime,
                    description = order.Description,
                    status = order.Status.ToString(),
                    communityName = community?.Name,
                    createdAt = order.CreatedAt
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "宠物服务需求发布成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"发布需求失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 提交需求的开始/结束时间
        /// 宠物主人专用
        /// </summary>
        [HttpPost("schedule/set")]
        [Authorize]
        public async Task<IActionResult> SetSchedule([FromBody] SetScheduleRequest request)
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

                // 验证时间格式和逻辑
                if (request.StartTime >= request.EndTime)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "开始时间必须早于结束时间"
                    });
                }

                if (request.StartTime <= DateTime.Now)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "开始时间必须晚于当前时间"
                    });
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "时间设置验证通过"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"时间设置失败: {ex.Message}"
                });
            }
        }

        // ===============================
        // 服务者 - 接单相关接口
        // ===============================

        /// <summary>
        /// 获取可接单的需求列表
        /// 服务者专用
        /// </summary>
        [HttpGet("requests/available")]
        [Authorize]
        public async Task<IActionResult> GetAvailableRequests(
            [FromQuery] string? type = null,
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

                // 验证用户是否为审核通过的服务者
                var user = await _context.Users
                    .Include(u => u.Community)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || user.Role != UserRole.Sitter || user.SitterAuditStatus != SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有审核通过的服务者才能查看可接单需求"
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

                // 获取附近的可接单需求
                List<MutualOrder> availableRequests;

                if (user.CommunityId.HasValue)
                {
                    // 优先获取同社区的服务
                    var communityRequests = await _geolocationService.GetServicesInCommunityAsync(
                        user.CommunityId.Value, userLat, userLng);

                    // 获取跨社区的附近服务
                    var nearbyRequests = await _geolocationService.GetNearbyServicesAcrossCommunitiesAsync(
                        userLat, userLng, 3.0, user.CommunityId.Value); // 3公里范围内

                    availableRequests = communityRequests.Concat(nearbyRequests).ToList();
                }
                else
                {
                    availableRequests = await _geolocationService.GetNearbyServicesAcrossCommunitiesAsync(
                        userLat, userLng, 3.0);
                }

                // 筛选可接单状态的需求
                availableRequests = availableRequests
                    .Where(o => o.Status == OrderStatus.Pending)
                    .ToList();

                // 服务类型筛选
                if (!string.IsNullOrEmpty(type))
                {
                    availableRequests = availableRequests
                        .Where(o => o.ServiceType.Equals(type, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                // 排除自己的订单
                availableRequests = availableRequests
                    .Where(o => o.OwnerId != userId)
                    .ToList();

                var totalCount = availableRequests.Count;
                var requests = availableRequests
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var requestList = requests.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    petType = o.PetType,
                    serviceType = o.ServiceType,
                    startTime = o.StartTime,
                    endTime = o.EndTime,
                    description = o.Description,
                    distance = o.Distance,
                    communityName = o.Community?.Name,
                    createdAt = o.CreatedAt
                });

                var responseData = new
                {
                    requests = requestList,
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
                    Message = "获取可接单需求列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取可接单需求失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 查看单个可接需求的详情
        /// 服务者专用
        /// </summary>
        [HttpGet("requests/detail/{id}")]
        [Authorize]
        public async Task<IActionResult> GetRequestDetail(string id)
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

                // 验证用户是否为审核通过的服务者
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter || user.SitterAuditStatus != SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有审核通过的服务者才能查看需求详情"
                    });
                }

                // 获取需求详情
                var request = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .Include(o => o.Community)
                    .FirstOrDefaultAsync(o => o.Id == id && o.Status == OrderStatus.Pending);

                if (request == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "需求不存在或已接单"
                    });
                }

                // 获取宠物主人的信誉信息
                var ownerReputation = await GetUserReputationSummary(request.OwnerId);

                var responseData = new
                {
                    id = request.Id,
                    title = request.Title,
                    petType = request.PetType,
                    serviceType = request.ServiceType,
                    startTime = request.StartTime,
                    endTime = request.EndTime,
                    description = request.Description,
                    communityName = request.Community?.Name,
                    createdAt = request.CreatedAt,
                    owner = new
                    {
                        user = request.Owner?.ToUserDto(),
                        reputationSummary = ownerReputation
                    }
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取需求详情成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取需求详情失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 接受需求（完成接单操作）
        /// 服务者专用
        /// </summary>
        [HttpPost("requests/accept/{id}")]
        [Authorize]
        public async Task<IActionResult> AcceptRequest(string id)
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

                // 验证用户是否为审核通过的服务者
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || user.Role != UserRole.Sitter || user.SitterAuditStatus != SitterAuditStatus.Approved)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "只有审核通过的服务者才能接单"
                    });
                }

                // 获取需求
                var request = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (request == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "需求不存在"
                    });
                }

                // 验证需求状态
                if (request.Status != OrderStatus.Pending)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "需求已被其他服务者接受或已完成"
                    });
                }

                // 验证不能接受自己的需求
                if (request.OwnerId == userId)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "不能接受自己的需求"
                    });
                }

                // 更新需求状态
                request.Status = OrderStatus.Accepted;
                request.AcceptedAt = DateTime.Now;
                request.AcceptedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "成功接受需求，请按约定时间提供服务"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"接受需求失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 计算服务者与需求发布者的距离
        /// 服务者专用
        /// </summary>
        [HttpGet("location/distance")]
        [Authorize]
        public async Task<IActionResult> CalculateDistance([FromQuery] string requestId)
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
                if (user == null || !user.Longitude.HasValue || !user.Latitude.HasValue)
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "用户位置信息不完整"
                    });
                }

                var request = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .FirstOrDefaultAsync(o => o.Id == requestId);

                if (request == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "需求不存在"
                    });
                }

                // 计算距离（简化的计算方式）
                var distance = CalculateDistance(
                    (double)user.Latitude.Value, (double)user.Longitude.Value,
                    (double)request.Owner.Latitude.GetValueOrDefault(0),
                    (double)request.Owner.Longitude.GetValueOrDefault(0));

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new { distance = Math.Round(distance, 2) },
                    Message = "距离计算成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"计算距离失败: {ex.Message}"
                });
            }
        }

        // ===============================
        // 管理员 - 需求审核相关接口
        // ===============================

        /// <summary>
        /// 获取需求审核列表
        /// 管理员专用
        /// </summary>
        [HttpGet("requests/review/list")]
        [Authorize]
        public async Task<IActionResult> GetReviewList(
            [FromQuery] string? status = null,
            [FromQuery] string? serviceType = null,
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

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.Moderator))
                {
                    return Forbid("需要管理员权限");
                }

                if (!user.CommunityId.HasValue)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "社区不存在"
                    });
                }

                var query = _context.MutualOrders
                    .Include(o => o.Owner)
                    .Include(o => o.Community)
                    .Where(o => o.CommunityId == user.CommunityId.Value);

                // 状态筛选
                if (!string.IsNullOrEmpty(status))
                {
                    if (Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
                    {
                        query = query.Where(o => o.Status == orderStatus);
                    }
                }

                // 服务类型筛选
                if (!string.IsNullOrEmpty(serviceType))
                {
                    query = query.Where(o => o.ServiceType.Equals(serviceType, StringComparison.OrdinalIgnoreCase));
                }

                var totalCount = await query.CountAsync();
                var requests = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var requestList = requests.Select(o => new
                {
                    id = o.Id,
                    title = o.Title,
                    petType = o.PetType,
                    serviceType = o.ServiceType,
                    status = o.Status.ToString(),
                    owner = o.Owner?.ToUserDto(),
                    createdAt = o.CreatedAt
                });

                var responseData = new
                {
                    requests = requestList,
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
                    Message = "获取需求审核列表成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取需求审核列表失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 获取单个需求的审核详情
        /// 管理员专用
        /// </summary>
        [HttpGet("requests/review/detail/{id}")]
        [Authorize]
        public async Task<IActionResult> GetReviewDetail(string id)
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
                if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.Moderator))
                {
                    return Forbid("需要管理员权限");
                }

                if (!user.CommunityId.HasValue)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "社区不存在"
                    });
                }

                var request = await _context.MutualOrders
                    .Include(o => o.Owner)
                    .Include(o => o.Community)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CommunityId == user.CommunityId.Value);

                if (request == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "需求不存在"
                    });
                }

                var responseData = new
                {
                    id = request.Id,
                    title = request.Title,
                    petType = request.PetType,
                    serviceType = request.ServiceType,
                    startTime = request.StartTime,
                    endTime = request.EndTime,
                    description = request.Description,
                    status = request.Status.ToString(),
                    createdAt = request.CreatedAt,
                    owner = new
                    {
                        user = request.Owner?.ToUserDto()
                    }
                };

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = responseData,
                    Message = "获取需求审核详情成功"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"获取需求审核详情失败: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// 审核通过需求
        /// 管理员专用
        /// </summary>
        [HttpPut("requests/review/pass")]
        [Authorize]
        public async Task<IActionResult> ApproveRequest([FromBody] ReviewRequestRequest request)
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
                if (admin == null || (admin.Role != UserRole.Admin && admin.Role != UserRole.Moderator))
                {
                    return Forbid("需要管理员权限");
                }

                // 这里可以实现需求的审核逻辑
                // 目前需求发布后自动可见，所以这个接口可能用于特殊情况

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "需求审核通过"
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
        /// 拒绝需求
        /// 管理员专用
        /// </summary>
        [HttpPut("requests/review/reject")]
        [Authorize]
        public async Task<IActionResult> RejectRequest([FromBody] ReviewRequestRequest request)
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
                if (admin == null || (admin.Role != UserRole.Admin && admin.Role != UserRole.Moderator))
                {
                    return Forbid("需要管理员权限");
                }

                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "拒绝审核必须填写拒绝原因"
                    });
                }

                // 这里可以实现拒绝需求的逻辑
                // 可以取消需求或标记为拒绝状态

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "需求已拒绝"
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
        /// 重新审核已拒绝的需求
        /// 管理员专用
        /// </summary>
        [HttpPut("requests/review/recheck")]
        [Authorize]
        public async Task<IActionResult> RecheckRequest([FromBody] RecheckRequestRequest request)
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
                if (admin == null || (admin.Role != UserRole.Admin && admin.Role != UserRole.Moderator))
                {
                    return Forbid("需要管理员权限");
                }

                // 这里可以实现重新审核的逻辑

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "需求重新审核完成"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = $"重新审核失败: {ex.Message}"
                });
            }
        }

        // ===============================
        // 工具方法
        // ===============================

        /// <summary>
        /// 获取用户信誉摘要
        /// </summary>
        private async Task<object> GetUserReputationSummary(string userId)
        {
            var evaluations = await _context.OrderEvaluations
                .Where(e => e.EvaluatedUserId == userId)
                .ToListAsync();

            var totalEvaluations = evaluations.Count;
            var positiveEvaluations = evaluations.Count(e => e.Score >= 4);
            var averageScore = totalEvaluations > 0 ? evaluations.Average(e => e.Score) : 0;

            return new
            {
                totalEvaluations = totalEvaluations,
                positiveRate = totalEvaluations > 0 ? Math.Round((double)positiveEvaluations / totalEvaluations * 100, 1) : 0,
                averageScore = Math.Round(averageScore, 1)
            };
        }

        /// <summary>
        /// 计算两点间的距离（简化的Haversine公式）
        /// </summary>
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // 地球半径，单位：公里
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        // ===============================
        // 请求模型
        // ===============================

        public class SubmitPetProfileRequest
        {
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public string Breed { get; set; } = string.Empty;
            public int Age { get; set; }
            public bool IsVaccinated { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public class CreateRequestRequest
        {
            public string Title { get; set; } = string.Empty;
            public string PetType { get; set; } = string.Empty;
            public string ServiceType { get; set; } = string.Empty;
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Description { get; set; } = string.Empty;
        }

        public class SetScheduleRequest
        {
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        public class ReviewRequestRequest
        {
            public string RequestId { get; set; } = string.Empty;
            public string Comment { get; set; } = string.Empty;
            public string? RejectionReason { get; set; }
        }

        public class RecheckRequestRequest
        {
            public string RequestId { get; set; } = string.Empty;
        }
    }
}
