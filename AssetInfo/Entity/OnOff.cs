using System;
using System.Collections.Generic;
using System.Text;

namespace AssetInfo.Entity
{
    public class OnOff
    {
        /// <summary>
        /// 表名字(小写).可来自枚举AssetInfo.BLL.Table
        /// </summary>
        public string tableid;
        /// <summary>
        /// 记录id(pk)
        /// </summary>
        public string colid;
        /// <summary>
        /// 1=on , 0=off
        /// </summary>
        public string onoff;
        public DateTimeOffset ctime;
    }
}
