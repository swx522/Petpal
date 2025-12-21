namespace petpal.API.Models
{
    /// <summary>
    /// 统一API响应格式
    /// 所有API接口都使用此格式返回响应
    /// </summary>
    public class ApiResponse
    {
        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应数据
        /// 成功时包含返回的数据，失败时为null
        /// </summary>
        public object? Data { get; set; }

        /// <summary>
        /// 响应消息
        /// 包含操作结果的描述信息
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
