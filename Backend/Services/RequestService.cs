using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using petpal.API.Models.DTOs;

namespace petpal.API.Services
{
    /// <summary>
    /// 需求管理服务实现
    /// 提供需求发布、接单、审核相关的业务逻辑
    /// </summary>
    public class RequestService : IRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IGeolocationService _geolocationService;

        public RequestService(
            ApplicationDbContext context,
            IUserService userService,
            IGeolocationService geolocationService)
        {
            _context = context;
            _userService = userService;
            _geolocationService = geolocationService;
        }

        /// <summary>
        /// 获取宠物类型列表
        /// </summary>
        public async Task<List<PetType>> GetPetTypesAsync()
        {
            return new List<PetType>
            {
                new PetType { Value = "dog", Label = "狗狗 🐶", Description = "犬类宠物" },
                new PetType { Value = "cat", Label = "猫咪 🐱", Description = "猫类宠物" },
                new PetType { Value = "rabbit", Label = "兔兔 🐰", Description = "兔子等小型宠物" },
                new PetType { Value = "bird", Label = "鸟鸟 🐦", Description = "鸟类宠物" },
                new PetType { Value = "other", Label = "其他 🐾", Description = "其他宠物类型" }
            };
        }

        /// <summary>
        /// 获取服务类型列表
        /// </summary>
        public async Task<List<ServiceCategory>> GetServiceCategoriesAsync()
        {
            return new List<ServiceCategory>
            {
                new ServiceCategory { Value = "walking", Label = "遛狗", Description = "带宠物外出散步" },
                new ServiceCategory { Value = "feeding", Label = "喂食", Description = "定时喂食和照顾" },
                new ServiceCategory { Value = "grooming", Label = "美容", Description = "宠物美容服务" },
                new ServiceCategory { Value = "medical", Label = "就医陪同", Description = "陪同宠物就医" },
                new ServiceCategory { Value = "boarding", Label = "寄养", Description = "宠物寄养服务" },
                new ServiceCategory { Value = "other", Label = "其他", Description = "其他宠物服务" }
            };
        }

        /// <summary>
        /// 创建宠物信息
        /// </summary>
        public async Task<Pet> CreatePetProfileAsync(string userId, CreatePetRequest petInfo)
        {
            var pet = new Pet
            {
                UserId = userId,
                Name = petInfo.Name,
                Type = petInfo.Type,
                Breed = petInfo.Breed,
                Age = petInfo.Age,
                IsVaccinated = petInfo.IsVaccinated,
                Description = petInfo.Description,
                CreatedAt = DateTime.Now
            };

            _context.Pets.Add(pet);
            await _context.SaveChangesAsync();

            return pet;
        }

        /// <summary>
        /// 发布宠物服务需求
        /// </summary>
        public async Task<MutualOrder> CreateRequestAsync(string userId, CreateRequestData request)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ArgumentException("用户不存在");
            }

            // 认证检查已移除：发布需求无需实名认证或宠物认证

            var order = new MutualOrder
            {
                OwnerId = userId,
                Title = request.Title,
                PetType = request.ServiceType, // 这里应该是宠物类型，但API设计中用的是ServiceType
                ServiceType = request.ServiceType,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Description = request.Description,
                Status = OrderStatus.Pending,
                Longitude = request.Longitude,
                Latitude = request.Latitude,
                CreatedAt = DateTime.Now
            };

            // 根据位置查找社区
            if (request.Longitude.HasValue && request.Latitude.HasValue)
            {
                var community = await _geolocationService.FindCommunityByLocationAsync(
                    request.Longitude.Value, request.Latitude.Value);
                order.CommunityId = community?.Id;
            }

            _context.MutualOrders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        /// <summary>
        /// 获取可接单的需求列表
        /// </summary>
        public async Task<List<MutualOrder>> GetAvailableRequestsAsync(string sitterId, RequestFilters filters)
        {
            var sitter = await _userService.GetUserByIdAsync(sitterId);
            if (sitter == null || sitter.Role != UserRole.Sitter)
            {
                throw new UnauthorizedAccessException("只有服务者可以查看可接单需求");
            }

            var query = _context.MutualOrders
                .Include(o => o.Owner)
                .Where(o => o.Status == OrderStatus.Pending);

            if (!string.IsNullOrEmpty(filters.ServiceType))
            {
                query = query.Where(o => o.ServiceType == filters.ServiceType);
            }

            // 如果指定了最大距离，进行地理筛选
            if (filters.MaxDistance.HasValue && sitter.Longitude.HasValue && sitter.Latitude.HasValue)
            {
                // 这里简化实现，实际应该计算距离
                // 暂时返回所有符合条件的订单
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();
        }

        /// <summary>
        /// 获取需求的详细信息
        /// </summary>
        public async Task<RequestDetail> GetRequestDetailAsync(string requestId, string userId)
        {
            var request = await _context.MutualOrders
                .Include(o => o.Owner)
                .FirstOrDefaultAsync(o => o.Id == requestId);

            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == request.OwnerId);

            double distance = 0;
            var user = await _userService.GetUserByIdAsync(userId);
            if (user?.Longitude.HasValue == true && user.Latitude.HasValue == true &&
                request.Longitude.HasValue && request.Latitude.HasValue)
            {
                distance = _geolocationService.CalculateDistance(
                    (double)user.Latitude.Value, (double)user.Longitude.Value,
                    (double)request.Latitude.Value, (double)request.Longitude.Value);
            }

            return new RequestDetail
            {
                Request = request,
                Pet = pet,
                Owner = request.Owner,
                Distance = distance
            };
        }

        /// <summary>
        /// 接受需求（接单）
        /// </summary>
        public async Task<MutualOrder> AcceptRequestAsync(string sitterId, string requestId)
        {
            var sitter = await _userService.GetUserByIdAsync(sitterId);
            if (sitter == null || sitter.Role != UserRole.Sitter)
            {
                throw new UnauthorizedAccessException("只有服务者可以接受需求");
            }

            var request = await _context.MutualOrders.FindAsync(requestId);
            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            // 检查审核状态：必须先审核通过才能接单
            if (request.Status != OrderStatus.Approved)
            {
                throw new InvalidOperationException("该需求尚未通过审核，无法接单");
            }

            // 检查执行状态：只有开放状态才能接单
            if (request.ExecutionStatus != OrderExecutionStatus.Open)
            {
                throw new InvalidOperationException("该需求已被接受或已完成");
            }

            request.ExecutionStatus = OrderExecutionStatus.Accepted;
            request.AcceptedAt = DateTime.Now;
            // 这里可以添加SitterId字段来记录接受者

            await _context.SaveChangesAsync();

            return request;
        }

        /// <summary>
        /// 获取待审核的需求列表
        /// </summary>
        public async Task<List<RequestDto>> GetPendingReviewsAsync(ReviewFilters filters)
        {
            // 基于 filters.Status 进行状态过滤；如果未提供 status，默认返回 Pending（审核列表）
            var query = _context.MutualOrders.AsQueryable();

            if (!string.IsNullOrEmpty(filters.Status))
            {
                if (Enum.TryParse<OrderStatus>(filters.Status, true, out var parsedStatus))
                {
                    query = query.Where(o => o.Status == parsedStatus);
                }
                else
                {
                    // 如果无法解析，默认不应用状态过滤（保持原有行为）
                }
            }
            else
            {
                query = query.Where(o => o.Status == OrderStatus.Pending);
            }

            if (!string.IsNullOrEmpty(filters.ServiceType))
            {
                query = query.Where(o => o.ServiceType == filters.ServiceType);
            }

            var orders = await query
                .OrderBy(o => o.CreatedAt)
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .Select(o => new RequestDto
                {
                    Id = o.Id,
                    Title = o.Title,
                    PetType = o.PetType,
                    ServiceType = o.ServiceType,
                    StartTime = o.StartTime,
                    EndTime = o.EndTime,
                    Description = o.Description,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                    Longitude = o.Longitude,
                    Latitude = o.Latitude,
                    Distance = o.Distance,
                    User = o.Owner != null ? new UserSimpleDto
                    {
                        Id = o.Owner.Id,
                        Username = o.Owner.Username,
                        Name = o.Owner.Username,
                        Phone = o.Owner.Phone,
                        Role = o.Owner.Role,
                        ReputationScore = o.Owner.ReputationScore
                    } : null
                })
                .ToListAsync();

            return orders;
        }

        /// <summary>
        /// 获取审核详情
        /// </summary>
        public async Task<ReviewDetail> GetReviewDetailAsync(string requestId)
        {
            var request = await _context.MutualOrders
                .Include(o => o.Owner)
                .FirstOrDefaultAsync(o => o.Id == requestId);

            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            var pet = await _context.Pets
                .FirstOrDefaultAsync(p => p.UserId == request.OwnerId);

            var materials = await _context.AuditMaterials
                .Where(m => m.SitterId == request.OwnerId)
                .ToListAsync();

            return new ReviewDetail
            {
                Request = request,
                Pet = pet,
                Owner = request.Owner,
                Materials = materials
            };
        }

        /// <summary>
        /// 审核通过需求
        /// </summary>
        public async Task<MutualOrder> ApproveRequestAsync(string adminId, string requestId)
        {
            var admin = await _userService.GetUserByIdAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("只有管理员可以审核需求");
            }

            var request = await _context.MutualOrders.FindAsync(requestId);
            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            if (request.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("该需求已审核完成");
            }

            request.Status = OrderStatus.Approved;
            await _context.SaveChangesAsync();

            return request;
        }

        /// <summary>
        /// 审核拒绝需求
        /// </summary>
        public async Task<MutualOrder> RejectRequestAsync(string adminId, string requestId, string reason)
        {
            var admin = await _userService.GetUserByIdAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("只有管理员可以审核需求");
            }

            var request = await _context.MutualOrders.FindAsync(requestId);
            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            if (request.Status != OrderStatus.Pending)
            {
                throw new InvalidOperationException("该需求已审核完成");
            }

            request.Status = OrderStatus.Rejected;
            // 这里可以添加拒绝原因字段

            await _context.SaveChangesAsync();

            return request;
        }

        /// <summary>
        /// 重新审核需求
        /// </summary>
        public async Task<MutualOrder> RecheckRequestAsync(string adminId, string requestId)
        {
            var admin = await _userService.GetUserByIdAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("只有管理员可以重新审核需求");
            }

            var request = await _context.MutualOrders.FindAsync(requestId);
            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            if (request.Status != OrderStatus.Rejected)
            {
                throw new InvalidOperationException("只有被拒绝的需求可以重新审核");
            }

            request.Status = OrderStatus.Pending;
            await _context.SaveChangesAsync();

            return request;
        }

        /// <summary>
        /// 删除审核记录（管理员操作）
        /// </summary>
        public async Task DeleteReviewAsync(string adminId, string requestId)
        {
            var admin = await _userService.GetUserByIdAsync(adminId);
            if (admin == null || admin.Role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("只有管理员可以删除审核记录");
            }

            var request = await _context.MutualOrders
                .Include(o => o.Evaluations)
                .FirstOrDefaultAsync(o => o.Id == requestId);

            if (request == null)
            {
                throw new ArgumentException("需求不存在");
            }

            _context.MutualOrders.Remove(request);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 计算距离
        /// </summary>
        public async Task<double> CalculateDistanceAsync(string userId1, string userId2)
        {
            var user1 = await _userService.GetUserByIdAsync(userId1);
            var user2 = await _userService.GetUserByIdAsync(userId2);

            if (user1?.Longitude == null || user1.Latitude == null ||
                user2?.Longitude == null || user2.Latitude == null)
            {
                return double.MaxValue; // 返回最大值表示无法计算距离
            }

            return _geolocationService.CalculateDistance(
                (double)user1.Latitude.Value, (double)user1.Longitude.Value,
                (double)user2.Latitude.Value, (double)user2.Longitude.Value);
        }
    }
}
