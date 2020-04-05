using AssetInfo.DBA;
using AssetInfo.Help;
using System;

namespace AssetInfo.Entity
{
    /// <summary>
    /// 数据表: Keyval
    /// </summary>
    public class KeyvalM : BaseModel
    {
        /// <summary>
        /// 唯一编号 maxlen=8
        /// </summary>
        public string Code;
        /// <summary>
        /// 种类名 maxlen=50
        /// </summary>
        [VNotNull]
        [VStringLength(maxlen = 50)]
        public string Title;
        /// <summary>
        /// 所属分类
        /// </summary>
        [VNotNull]
        [VDigit]
        public int Category;
        /// <summary>
        /// 说明 maxlen=50
        /// </summary>
        [VNotNull]
        [VStringLength(maxlen = 50)]
        public string Comment;
        /// <summary>
        /// 记录产生时间
        /// </summary>
        public DateTimeOffset Ctime;
        /// <summary>
        /// 0=停用,1=启用
        /// </summary>
        public int Enabled;
        /// <summary>
        /// 排序字段
        /// </summary>
        public int OrderBy;
    }
}