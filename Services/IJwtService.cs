using petpal.API.Models;
using System.Security.Claims;

namespace petpal.API.Services
{
    /// <summary>
    /// JWT令牌服务接口
    /// 定义JWT令牌相关的操作方法
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// 生成JWT访问令牌
        /// 根据用户信息创建包含必要声明的JWT令牌
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>JWT令牌字符串</returns>
        string GenerateToken(User user);

        /// <summary>
        /// 验证JWT令牌
        /// 解析并验证令牌的有效性，返回声明主体
        /// </summary>
        /// <param name="token">JWT令牌字符串</param>
        /// <returns>包含用户声明的ClaimsPrincipal对象</returns>
        ClaimsPrincipal ValidateToken(string token);
    }
}
