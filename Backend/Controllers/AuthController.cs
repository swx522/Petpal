using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using petpal.API.Models;
using petpal.API.Services;

namespace petpal.API.Controllers
{
    /// <summary>
    /// 认证控制器
    /// 处理用户认证相关的所有操作
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;

        public AuthController(IUserService userService, IJwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        /// <summary>
        /// 用户注册接口
        /// 支持手机号注册，自动验证唯一性
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.Phone))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "用户名、密码和手机号不能为空"
                    });
                }

                var user = await _userService.RegisterAsync(request.Username, request.Password, request.Phone, request.Email, request.Role);

                var token = _jwtService.GenerateToken(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        userId = user.Id,
                        username = user.Username,
                        token = token
                    },
                    Message = "注册成功，欢迎加入宠物互助平台"
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
        /// 用户登录接口
        /// 支持手机号或邮箱登录
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Account) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "账号和密码不能为空"
                    });
                }

                var user = await _userService.LoginAsync(request.Account, request.Password);

                var token = _jwtService.GenerateToken(user);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        userId = user.Id,
                        username = user.Username,
                        role = user.Role.ToString(),
                        token = token
                    },
                    Message = "登录成功"
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
        /// 退出登录接口
        /// 客户端清除token即可，服务端无需特殊处理
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(new ApiResponse
            {
                Success = true,
                Message = "退出登录成功"
            });
        }


        /// <summary>
        /// 重置密码接口
        /// 通过手机号验证重置密码（覆盖原密码）
        /// </summary>
        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Phone) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new ApiResponse
                    {
                        Success = false,
                        Message = "手机号和密码不能为空"
                    });
                }

                // 通过手机号重置密码（覆盖原密码，无需验证旧密码）
                await _userService.ResetPasswordAsync(request.Phone, request.Password);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "密码重置成功，请使用新密码登录"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse
                {
                    Success = false,
                    Message = "密码重置失败：" + ex.Message
                });
            }
        }
    }

}
