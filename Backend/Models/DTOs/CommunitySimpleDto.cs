namespace petpal.API.Models.DTOs
{
    /// <summary>
    /// 社区简易数据传输对象
    /// 只包含社区的基本信息，避免循环引用
    /// </summary>
    public class CommunitySimpleDto
    {
        /// <summary>
        /// 社区ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 社区名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 社区描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 社区状态
        /// </summary>
        public bool IsActive { get; set; }
    }
}
