using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using petpal.API.Models;

namespace petpal.API.Data
{
    /// <summary>
    /// 应用程序数据库上下文
    /// 继承自Entity Framework Core的DbContext类
    /// 负责管理数据库连接和实体映射
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// 注入DbContextOptions配置
        /// </summary>
        /// <param name="options">数据库上下文配置选项</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// 用户表DbSet
        /// 提供对用户实体的CRUD操作
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// 宠物表DbSet
        /// 提供对宠物实体的CRUD操作
        /// </summary>
        public DbSet<Pet> Pets { get; set; }

        /// <summary>
        /// 互助订单表DbSet
        /// 提供对互助订单实体的CRUD操作
        /// </summary>
        public DbSet<MutualOrder> MutualOrders { get; set; }

        /// <summary>
        /// 订单评价表DbSet
        /// 提供对订单评价实体的CRUD操作
        /// </summary>
        public DbSet<OrderEvaluation> OrderEvaluations { get; set; }

        /// <summary>
        /// 审核材料表DbSet
        /// 提供对审核材料实体的CRUD操作
        /// </summary>
        public DbSet<AuditMaterial> AuditMaterials { get; set; }

        /// <summary>
        /// 服务者资格申请表DbSet
        /// 提供对服务者资格申请实体的CRUD操作
        /// </summary>
        public DbSet<SitterApplication> SitterApplications { get; set; }

        /// <summary>
        /// 社区表DbSet
        /// 提供对社区实体的CRUD操作
        /// </summary>
        public DbSet<Community> Communities { get; set; }

        /// <summary>
        /// 信誉日志表DbSet
        /// 提供对信誉日志实体的CRUD操作
        /// </summary>
        public DbSet<ReputationLog> ReputationLogs { get; set; }

        /// <summary>
        /// 配置实体模型
        /// 在这里定义实体间的关系、约束和索引
        /// </summary>
        /// <param name="modelBuilder">模型构建器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置用户表的索引
            // 为用户名创建唯一索引，确保用户名唯一性
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // 为手机号创建唯一索引，确保手机号唯一性
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Phone)
                .IsUnique();

            // 为邮箱创建索引（非唯一，因为邮箱是可选字段）
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email);

            // 配置互助订单表的索引
            // 复合索引：状态+创建时间，用于查询不同状态的订单列表
            modelBuilder.Entity<MutualOrder>()
                .HasIndex(o => new { o.Status, o.CreatedAt });

            // 配置执行状态的默认值
            modelBuilder.Entity<MutualOrder>()
                .Property(o => o.ExecutionStatus)
                .HasDefaultValue(OrderExecutionStatus.Open);


            // 配置外键关系
            // 虽然使用了[ForeignKey]特性，但这里可以添加额外的约束配置

            // 配置级联删除行为
            // 当用户被删除时，相关的宠物记录也被删除
            modelBuilder.Entity<Pet>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.Pets)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 配置订单与用户的关系
            modelBuilder.Entity<MutualOrder>()
                .HasOne(o => o.Owner)
                .WithMany(u => u.OrdersAsRequester) // 复用现有的导航属性
                .HasForeignKey(o => o.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // 不级联删除，保持数据完整性

            // 配置订单与服务者的关系
            modelBuilder.Entity<MutualOrder>()
                .HasOne(o => o.Sitter)
                .WithMany(u => u.OrdersAsHelper) // 使用现有的导航属性
                .HasForeignKey(o => o.SitterId)
                .OnDelete(DeleteBehavior.SetNull); // 服务者删除时，订单的服务者ID设为null

            // 配置评价与订单和用户的关系
            modelBuilder.Entity<OrderEvaluation>()
                .HasOne(e => e.Order)
                .WithMany(o => o.Evaluations)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderEvaluation>()
                .HasOne(e => e.Evaluator)
                .WithMany()
                .HasForeignKey(e => e.EvaluatorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderEvaluation>()
                .HasOne(e => e.EvaluatedUser)
                .WithMany()
                .HasForeignKey(e => e.EvaluatedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // 配置社区相关的外键关系
            modelBuilder.Entity<User>()
                .HasOne(u => u.Community)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MutualOrder>()
                .HasOne(o => o.Community)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CommunityId)
                .OnDelete(DeleteBehavior.SetNull);

            // 为社区表添加索引
            modelBuilder.Entity<Community>()
                .HasIndex(c => c.Name);

            modelBuilder.Entity<Community>()
                .HasIndex(c => new { c.MinLng, c.MaxLng, c.MinLat, c.MaxLat });

            // 为用户地理位置添加索引
            modelBuilder.Entity<User>()
                .HasIndex(u => new { u.Longitude, u.Latitude })
                .HasFilter("[Longitude] IS NOT NULL AND [Latitude] IS NOT NULL");

            // 为订单社区添加索引
            modelBuilder.Entity<MutualOrder>()
                .HasIndex(o => o.CommunityId);

            // 为订单社区添加索引
            modelBuilder.Entity<MutualOrder>()
                .HasIndex(o => o.CommunityId);
        }
    }
}
