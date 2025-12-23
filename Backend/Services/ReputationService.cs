using petpal.API.Data;
using petpal.API.Models;

namespace petpal.API.Services
{
    /// <summary>
    /// 信誉评价服务实现
    /// 负责计算和管理用户的信誉分数和等级
    /// </summary>
    public class ReputationService : IReputationService
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 构造函数
        /// 通过依赖注入获取数据库上下文
        /// </summary>
        /// <param name="context">应用程序数据库上下文</param>
        public ReputationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 根据信誉分数获取对应的信誉等级
        /// 等级划分规则：
        /// - 500分以上：爱心达人
        /// - 200-499分：靠谱
        /// - 50-199分：新手
        /// - 50分以下：新手
        /// </summary>
        /// <param name="score">信誉分数</param>
        /// <returns>信誉等级字符串</returns>
        public string GetReputationLevel(int score)
        {
            if (score >= 500)
            {
                return "爱心达人"; // 高信誉用户，获得高级称号
            }
            else if (score >= 200)
            {
                return "靠谱"; // 中等信誉用户，获得中级称号
            }
            else if (score >= 50)
            {
                return "新手"; // 初级信誉用户，获得初级称号
            }
            else
            {
                return "新手"; // 信誉分数过低，仍为新手等级
            }
        }

        /// <summary>
        /// 根据订单评价计算信誉分变化值
        /// 计算规则：
        /// - 5星：+10分
        /// - 4星：+5分
        /// - 3星：0分
        /// - 2星：-5分
        /// - 1星：-10分
        /// - 如果有评价内容额外+2分
        /// </summary>
        /// <param name="evaluation">评价对象（OrderEvaluation）</param>
        /// <returns>信誉分变化值</returns>
        public int CalculateReputationChange(object evaluation)
        {
            // 类型检查，确保传入的是OrderEvaluation对象
            if (evaluation is not OrderEvaluation eval)
            {
                throw new ArgumentException("评价对象类型不正确", nameof(evaluation));
            }

            // 根据评分计算基础分数变化
            int baseChange = eval.Score switch
            {
                5 => 10,  // 5星好评，大幅加分
                4 => 5,   // 4星好评，中等加分
                3 => 0,   // 3星中评，不加分也不减分
                2 => -5,  // 2星差评，轻微减分
                1 => -10, // 1星差评，大幅减分
                _ => 0    // 无效评分，保持不变
            };

            // 如果用户提供了详细评价内容，给予额外奖励
            // 鼓励用户认真填写评价，提高评价质量
            if (!string.IsNullOrEmpty(eval.Content))
            {
                baseChange += 2;
            }

            return baseChange;
        }

        /// <summary>
        /// 记录用户信誉分数变化
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="oldScore">变化前的分数</param>
        /// <param name="newScore">变化后的分数</param>
        /// <param name="reason">变化原因</param>
        /// <returns>异步任务</returns>
        public async Task LogReputationChangeAsync(string userId, int oldScore, int newScore, string reason)
        {
            var log = new ReputationLog
            {
                UserId = userId,
                OldScore = oldScore,
                NewScore = newScore,
                Reason = reason
            };

            _context.ReputationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 异步更新用户的信誉分数
        /// 更新分数后自动重新计算并更新信誉等级
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="scoreChange">分数变化值</param>
        /// <returns>异步任务</returns>
        public async Task UpdateUserReputationAsync(string userId, int scoreChange)
        {
            // 根据用户ID查找用户记录
            var user = await _context.Users.FindAsync(userId);

            // 如果用户不存在，直接返回
            if (user == null)
            {
                return;
            }

            // 更新用户信誉分数
            user.ReputationScore += scoreChange;

            // 确保信誉分数不低于0
            // 信誉分数为0时，用户仍可使用基本功能，但信誉等级保持最低
            if (user.ReputationScore < 0)
            {
                user.ReputationScore = 0;
            }

            // 根据新的信誉分数重新计算信誉等级
            user.ReputationLevel = GetReputationLevel(user.ReputationScore);

            // 保存更改到数据库
            await _context.SaveChangesAsync();
        }
    }
}
