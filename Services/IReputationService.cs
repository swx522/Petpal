namespace petpal.API.Services
{
    /// <summary>
    /// 信誉评价服务接口
    /// 定义信誉分计算和等级管理的相关方法
    /// </summary>
    public interface IReputationService
    {
        /// <summary>
        /// 根据信誉分数获取信誉等级
        /// </summary>
        /// <param name="score">信誉分数</param>
        /// <returns>信誉等级字符串</returns>
        string GetReputationLevel(int score);

        /// <summary>
        /// 根据评价计算信誉分变化值
        /// </summary>
        /// <param name="evaluation">评价信息（包含评分和内容）</param>
        /// <returns>信誉分变化值（可正可负）</returns>
        int CalculateReputationChange(object evaluation);

        /// <summary>
        /// 更新用户信誉分数
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="scoreChange">分数变化值</param>
        /// <returns>异步任务</returns>
        Task UpdateUserReputationAsync(string userId, int scoreChange);
    }
}
