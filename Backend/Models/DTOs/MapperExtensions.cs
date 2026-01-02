using petpal.API.Models;

namespace petpal.API.Models.DTOs
{
    /// <summary>
    /// DTO映射扩展方法
    /// 提供实体到DTO的转换功能
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// 将User实体转换为UserDto
        /// </summary>
        public static UserDto ToUserDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Phone = user.Phone,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                ReputationScore = user.ReputationScore,
                ReputationLevel = user.ReputationLevel,
                // 认证字段已移除
                SitterAuditStatus = user.SitterAuditStatus,
                CareIntroduction = user.CareIntroduction,
                ServiceTypes = user.ServiceTypes,
                Community = user.Community?.ToCommunitySimpleDto(),
                Longitude = user.Longitude,
                Latitude = user.Latitude,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt
            };
        }

        /// <summary>
        /// 将Community实体转换为CommunitySimpleDto
        /// </summary>
        public static CommunitySimpleDto ToCommunitySimpleDto(this Community community)
        {
            return new CommunitySimpleDto
            {
                Id = community.Id,
                Name = community.Name,
                Description = community.Description,
                IsActive = community.IsActive
            };
        }

        /// <summary>
        /// 将User集合转换为UserDto集合
        /// </summary>
        public static IEnumerable<UserDto> ToUserDtos(this IEnumerable<User> users)
        {
            return users.Select(u => u.ToUserDto());
        }
    }
}
