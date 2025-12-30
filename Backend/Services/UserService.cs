using Microsoft.EntityFrameworkCore;
using petpal.API.Data;
using petpal.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace petpal.API.Services
{
    /// <summary>
    /// 用户服务实现
    /// 提供用户注册、登录、认证等核心功能
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        /// <summary>
        /// 构造函数
        /// 通过依赖注入获取所需的服务
        /// </summary>
        /// <param name="context">数据库上下文</param>
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 更新通用个人资料（用户名 / 手机 / 邮箱）
        /// 仅会更新非null字段；用户名/手机号唯一性会被验证
        /// </summary>
        public async Task UpdateCommonProfileAsync(string userId, string? username, string? phone, string? email)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 验证用户名唯一性（如果有变更）
            if (!string.IsNullOrEmpty(username) && username != user.Username)
            {
                if (await _context.Users.AnyAsync(u => u.Username == username && u.Id != userId))
                {
                    throw new Exception("用户名已存在");
                }
                user.Username = username;
            }

            // 验证手机号唯一性（如果有变更）
            if (!string.IsNullOrEmpty(phone) && phone != user.Phone)
            {
                if (await _context.Users.AnyAsync(u => u.Phone == phone && u.Id != userId))
                {
                    throw new Exception("手机号已被注册");
                }
                user.Phone = phone;
            }

            // 邮箱无需严格唯一性校验（保留现有行为），但仍可更新
            if (email != null)
            {
                user.Email = email;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>~
        /// 用户注册实现
        /// 创建新用户账户，验证输入数据唯一性
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="phone">手机号码</param>
        /// <param name="email">邮箱地址</param>
        /// <param name="role">用户角色</param>
        /// <returns>新创建的用户信息</returns>
        /// <exception cref="Exception">用户名或手机号已存在时抛出异常</exception>
        public async Task<User> RegisterAsync(string username, string password, string phone, string email, UserRole role)
        {
            // 验证用户名唯一性
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                throw new Exception("用户名已存在，请选择其他用户名");
            }

            // 验证手机号唯一性
            if (await _context.Users.AnyAsync(u => u.Phone == phone))
            {
                throw new Exception("该手机号已被注册，请使用其他手机号或直接登录");
            }

            // 创建新用户对象
            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password), // 对密码进行哈希处理
                Phone = phone,
                Email = email,
                Role = role // 设置用户角色
                // ReputationLevel 使用模型中的默认值"新手"
            };

            // 将新用户添加到数据库
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// 用户登录实现
        /// 支持手机号或邮箱登录，验证密码正确性
        /// </summary>
        /// <param name="account">账号（手机号或邮箱）</param>
        /// <param name="password">密码</param>
        /// <returns>登录成功的用户信息</returns>
        /// <exception cref="Exception">账号不存在或密码错误时抛出异常</exception>
        public async Task<User> LoginAsync(string account, string password)
        {
            // 根据账号查找用户（支持手机号或邮箱登录）
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Phone == account || u.Email == account);

            // 检查用户是否存在
            if (user == null)
            {
                throw new Exception("账号不存在，请检查输入或先注册账号");
            }

            // 验证密码
            if (!VerifyPassword(password, user.PasswordHash))
            {
                throw new Exception("密码错误，请重试");
            }

            // 更新最后登录时间
            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// 根据用户ID获取用户信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        /// <summary>
        /// 验证用户认证状态
        /// 检查用户是否同时完成了实名认证和宠物认证
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>认证状态（true表示已完成双重认证）</returns>
        public async Task<bool> ValidateCertificationAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null && user.IsRealNameCertified && user.IsPetCertified;
        }

        /// <summary>
        /// 更新用户认证信息
        /// 处理实名认证或宠物认证的提交
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="certType">认证类型</param>
        /// <param name="certImages">认证图片URL列表</param>
        /// <exception cref="Exception">用户不存在时抛出异常</exception>
        public async Task UpdateUserCertificationAsync(string userId, string certType, List<string> certImages)
        {
            // 查找用户
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 根据认证类型更新相应的认证状态
            // 注意：这里是简化实现，实际项目中应该调用第三方认证服务进行审核
            if (certType == "user")
            {
                // 实名认证
                user.IsRealNameCertified = true;
            }
            else if (certType == "pet")
            {
                // 宠物认证
                user.IsPetCertified = true;
            }

            // 保存更改
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 密码哈希处理
        /// 使用SHA256算法对密码进行哈希
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <returns>Base64编码的哈希字符串</returns>
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// 密码验证
        /// 将输入密码哈希后与存储的哈希值比较
        /// </summary>
        /// <param name="password">明文密码</param>
        /// <param name="hash">存储的哈希值</param>
        /// <returns>验证结果</returns>
        public bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        /// <summary>
        /// 重置密码实现
        /// 通过手机号验证身份后重置密码
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="password">新密码</param>
        /// <exception cref="Exception">用户不存在时抛出异常</exception>
        public async Task ResetPasswordAsync(string phone, string password)
        {
            // 根据手机号查找用户
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == phone);

            // 检查用户是否存在
            if (user == null)
            {
                throw new Exception("用户不存在");
            }

            // 更新密码
            user.PasswordHash = HashPassword(password);
            await _context.SaveChangesAsync();
        }
    }
}
