using System;
using System.Collections.Generic;
using System.Text;

namespace AssetInfo.BLL
{
    /// <summary>
    /// 所有数据表名称.要小写的
    /// </summary>
    internal enum Table
    {
        keyval,
        asset
    }
    /// <summary>
    /// keyval分类类型
    /// </summary>
    internal enum KVEnumKind
    {
        空位,
        购买机构,
        资产种类,
        资产状态,
        风险等级
    }
    /// <summary>
    /// 常用提示语
    /// </summary>
    internal enum AlertMsg
    {
        没有数据,
        服务器错误,
        数据库错误,
        当前时段禁止该操作
    }
}
