using System;
using System.Collections.Generic;
using System.Text;
using AssetInfo.DBA;
using AssetInfo.Help;

namespace AssetInfo.Entity
{
    /// <summary>
    /// 数据表: Asset
    /// </summary>
    public class AssetM : BaseModel
    {
        /// <summary>
        /// pk guid32 maxlen=32
        /// </summary>
        public string Id;
        /// <summary>
        /// 资产名称 maxlen=50
        /// </summary>
        [VStringLength(maxlen = 50)]
        [VNotNull]
        public string Title;
        /// <summary>
        /// 资产发行代码 maxlen=50
        /// </summary>
        [VStringLength(maxlen = 50)]
        [VNotNull]
        public string Code;
        /// <summary>
        /// 风险等级
        /// </summary>
        [VStringLength(len = 8)]
        [VNotNull]
        public string Risk;
        /// <summary>
        /// 成本
        /// </summary>
        public decimal Amount;
        /// <summary>
        /// 市值
        /// </summary>
        public decimal Value;
        /// <summary>
        /// 手续费
        /// </summary>
        public decimal Charge;
        /// <summary>
        /// 资产说明 maxlen=50
        /// </summary>
        [VStringLength(maxlen = 50)]
        [VNotNull]
        public string Remark;
        /// <summary>
        /// 收益
        /// </summary>
        public decimal Profit;
        /// <summary>
        /// 资产购买机构 maxlen=8
        /// </summary>
        [VStringLength(len = 8)]
        [VNotNull]
        public string ExcOrg;
        /// <summary>
        /// 资产品种 maxlen=8
        /// </summary>
        [VStringLength(len = 8)]
        [VNotNull]
        public string Kind;
        /// <summary>
        /// 购买日
        /// </summary>
        public DateTimeOffset BuyDate;
        /// <summary>
        /// 起息日
        /// </summary>
        public DateTimeOffset ValueDate;
        /// <summary>
        /// 到期日
        /// </summary>
        public DateTimeOffset ExpDate;
        /// <summary>
        /// 参考年利率
        /// </summary>
        public decimal Rate;
        /// <summary>
        /// 期限 天
        /// </summary>
        public int Term;
        /// <summary>
        /// 资产状态码 maxlen=8
        /// </summary>
        [VStringLength(len = 8)]
        [VNotNull]
        public string Status;
        /// <summary>
        /// 记录生成时间
        /// </summary>
        public DateTimeOffset Ctime;
        /// <summary>
        /// 同一个资产唯一编号(guid32) maxlen=32
        /// </summary>
        [VStringLength(len = 32)]
        [VNotNull]
        public string ItemCode;
        /// <summary>
        /// 0=停用,1=启用
        /// </summary>
        public int Enabled;
    }
}
