using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using petpal.API.Data;
using petpal.API.Services;
using petpal.API.Models;
using Pomelo.EntityFrameworkCore.MySql;
using Serilog;
using System.Text;
using System;

var builder = WebApplication.CreateBuilder(args);

// 配置Serilog日志记录器
// Serilog提供结构化日志记录，便于追踪和调试应用程序行为
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// 添加数据库上下文配置
// 使用MySQL作为数据库，支持实体框架Core进行数据访问
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
                     new MySqlServerVersion(new Version(8, 0, 33))));

// 配置JWT身份认证
// JWT用于无状态的API身份验证，提供安全的token-based认证机制
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,           // 验证令牌发行者
            ValidateAudience = true,         // 验证令牌受众
            ValidateLifetime = true,         // 验证令牌生命周期
            ValidateIssuerSigningKey = true, // 验证签名密钥
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
        // 支持 SignalR 在查询参数中通过 access_token 传递 token（WebSocket / ServerSentEvents）
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// 添加Redis缓存服务
// Redis用于缓存热点数据，提高应用程序性能
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

    // 注册应用程序服务
    // 使用依赖注入容器管理服务生命周期
    builder.Services.AddScoped<IUserService, UserService>();           // 用户服务
    builder.Services.AddScoped<IJwtService, JwtService>();             // JWT令牌服务
    builder.Services.AddScoped<IOrderRatingService, OrderRatingService>(); // 订单评分服务（新）
    builder.Services.AddScoped<IGeolocationService, GeolocationService>(); // 地理位置服务
    builder.Services.AddScoped<ICommunityService, CommunityService>(); // 社区管理服务
    builder.Services.AddScoped<IRequestService, RequestService>();     // 需求管理服务
    builder.Services.AddScoped<IOrderService, OrderService>();         // 订单管理服务

// 添加控制器服务，并将枚举序列化为字符串以便前端接收可读角色名
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// 添加 SignalR 支持（用于实时聊天）
builder.Services.AddSignalR();

// 配置Swagger API文档
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "petpal API",
        Version = "v1",
        Description = "宠物互助平台API，提供用户管理、互助订单等功能"
    });

    // 添加JWT认证到Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 构建应用程序
var app = builder.Build();

// 配置HTTP请求管道中间件
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// 静态文件（用于聊天图片等上传资源）
app.UseStaticFiles();

// SignalR hubs
app.MapHub<petpal.API.Hubs.ChatHub>("/hubs/chat");

// 应用程序启动时执行数据库迁移
// 确保数据库结构与代码模型同步
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
                // 应用数据库迁移
                await db.Database.MigrateAsync();
                Log.Information("数据库迁移完成");

    }
    catch (Exception ex)
    {
        Log.Warning(ex, "数据库初始化警告，继续启动服务");
    }
}

// 启动应用程序
Log.Information("宠物互助平台API服务启动");
app.Run();
