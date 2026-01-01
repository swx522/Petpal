using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Services;
using petpal.API.Models;
using petpal.API.Models.DTOs;
using System.Security.Claims;

namespace petpal.API.Controllers
{
    /// <summary>
    /// 社区控制器
    /// 处理社区相关的查询功能（非管理性质）
    /// </summary>
    [ApiController]
    [Route("api/community")]
    [Authorize] // 需要认证但不限于管理员
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityService _communityService;
        private readonly IGeolocationService _geolocationService;

        public CommunityController(
            ICommunityService communityService,
            IGeolocationService geolocationService)
        {
            _communityService = communityService;
            _geolocationService = geolocationService;
        }

        // ===============================
        // 社区信息查询接口
        // ===============================

        /// <summary>
        /// 获取当前用户所属社区信息
        /// </summary>
        [HttpGet("my")]
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

                if (community == null)
                {
                    return NotFound(new ApiResponse
                    {
                        Success = false,
                        Message = "未找到所属社区"
                    });
                }

                var communityDto = community.ToCommunitySimpleDto();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = communityDto
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
        /// 根据经纬度查找社区
        /// </summary>
        [HttpGet("find")]
        public async Task<IActionResult> FindCommunity([FromQuery] decimal longitude, [FromQuery] decimal latitude)
        {
            try
            {
                var community = await _geolocationService.FindCommunityByLocationAsync(longitude, latitude);

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
        /// 获取社区内的服务列表
        /// </summary>
        [HttpGet("services/{communityId}")]
        public async Task<IActionResult> GetCommunityServices(int communityId, [FromQuery] double userLat, [FromQuery] double userLng)
        {
            try
            {
                var services = await _geolocationService.GetServicesInCommunityAsync(communityId, userLat, userLng);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = services
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
        /// 获取跨社区的附近服务
        /// </summary>
        [HttpGet("services/nearby")]
        public async Task<IActionResult> GetNearbyServices([FromQuery] double userLat, [FromQuery] double userLng, [FromQuery] double radiusKm = 3, [FromQuery] int? excludeCommunityId = null)
        {
            try
            {
                var services = await _geolocationService.GetNearbyServicesAcrossCommunitiesAsync(userLat, userLng, radiusKm, excludeCommunityId);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = services
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
