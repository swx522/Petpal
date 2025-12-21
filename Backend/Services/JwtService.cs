using Microsoft.IdentityModel.Tokens;
using petpal.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace petpal.API.Services
{
    /// <summary>
    /// JWT令牌服务实现
    /// 负责生成和验证JWT访问令牌
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        /// <summary>
        /// 构造函数
        /// 通过依赖注入获取配置信息
        /// </summary>
        /// <param name="configuration">应用程序配置</param>
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// 生成JWT访问令牌
        /// 创建包含用户基本信息和权限的JWT令牌
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <returns>JWT令牌字符串</returns>
        public string GenerateToken(User user)
        {
            // 创建用户声明列表
            // 这些声明将在令牌中携带用户信息
            var claims = new[]
            {
                // 用户ID声明 - 用于标识用户身份
                new Claim(ClaimTypes.NameIdentifier, user.Id),

                // 用户名声明 - 用于显示用户名称
                new Claim(ClaimTypes.Name, user.Username),

                // 用户角色声明 - 用于权限控制
                new Claim(ClaimTypes.Role, user.Role.ToString()),

                // 信誉等级声明 - 用于业务逻辑判断
                new Claim("ReputationLevel", user.ReputationLevel),

                // 认证状态声明 - 表示用户是否完成双重认证
                new Claim("IsCertified", (user.IsRealNameCertified && user.IsPetCertified).ToString())
            };

            // 获取JWT配置信息
            var keyString = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            // 创建对称安全密钥
            // 使用配置文件中的密钥字符串
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

            // 创建签名凭据
            // 使用HMAC-SHA256算法进行签名
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 创建JWT令牌
            var token = new JwtSecurityToken(
                issuer: issuer,           // 令牌发行者
                audience: audience,       // 令牌受众
                claims: claims,           // 用户声明
                expires: DateTime.Now.AddDays(7), // 令牌过期时间（7天）
                signingCredentials: creds // 签名凭据
            );

            // 将JWT令牌序列化为字符串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 验证JWT令牌
        /// 解析令牌并验证其有效性
        /// </summary>
        /// <param name="token">JWT令牌字符串</param>
        /// <returns>包含用户声明的ClaimsPrincipal对象</returns>
        /// <exception cref="SecurityTokenException">令牌无效时抛出异常</exception>
        public ClaimsPrincipal ValidateToken(string token)
        {
            // 创建JWT令牌处理器
            var tokenHandler = new JwtSecurityTokenHandler();

            // 获取配置信息
            var keyString = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            // 创建令牌验证参数
            var validationParameters = new TokenValidationParameters
            {
                // 启用各种验证
                ValidateIssuerSigningKey = true,    // 验证签名密钥
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
                ValidateIssuer = true,              // 验证发行者
                ValidIssuer = issuer,
                ValidateAudience = true,            // 验证受众
                ValidAudience = audience,
                ValidateLifetime = true,            // 验证生命周期
                ClockSkew = TimeSpan.Zero           // 时钟偏移设为0，提高安全性
            };

            // 验证令牌并返回声明主体
            // 如果令牌无效，此方法会抛出异常
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
    }
}
