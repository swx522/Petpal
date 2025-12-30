using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 用户服务接口
    /// 定义用户管理相关的业务逻辑方法
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 用户注册
        /// 创建新用户账户
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="phone">手机号码</param>
        /// <param name="email">邮箱地址</param>
        /// <param name="role">用户角色</param>
        /// <returns>注册成功的用户信息</returns>
        Task<User> RegisterAsync(string username, string password, string phone, string email, UserRole role);

        /// <summary>
        /// 用户登录
        /// 验证用户凭据并返回用户信息
        /// </summary>
        /// <param name="account">账号（手机号或邮箱）</param>
        /// <param name="password">密码</param>
        /// <returns>登录成功的用户信息</returns>
        Task<User> LoginAsync(string account, string password);

        /// <summary>
        /// 根据ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<User> GetUserByIdAsync(string userId);

        /// <summary>
        /// 验证用户认证状态
        /// 检查用户是否已完成实名认证和宠物认证
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>认证状态</returns>
        Task<bool> ValidateCertificationAsync(string userId);

        /// <summary>
        /// 提交用户认证信息
        /// 处理实名认证或宠物认证的提交
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="certType">认证类型（"user"或"pet"）</param>
        /// <param name="certImages">认证图片URL列表</param>
        Task UpdateUserCertificationAsync(string userId, string certType, List<string> certImages);

        /// <summary>
        /// 密码哈希处理
        /// 将明文密码转换为哈希值存储
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <returns>密码哈希字符串</returns>
        string HashPassword(string password);

        /// <summary>
        /// 密码验证
        /// 验证明文密码是否与哈希值匹配
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="hash">密码哈希值</param>
        /// <returns>验证结果</returns>
        bool VerifyPassword(string password, string hash);

        /// <summary>
        /// 重置密码
        /// 通过手机号验证身份后重置密码
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="password">新密码</param>
        Task ResetPasswordAsync(string phone, string password);

        /// <summary>
        /// 更新通用个人资料（用户名 / 手机 / 邮箱）
        /// 只更新非空的字段，验证唯一性（用户名、手机号）
        /// </summary>
        Task UpdateCommonProfileAsync(string userId, string? username, string? phone, string? email);
    }
}
