using System;
using System.Collections.Generic;
using System.Text;

namespace AssetInfo.Entity
{
    public class AccountM
    {
        public string Id;
        public string Name;
        public long Expire;
        /// <summary>
        /// 签名 json + mi aes
        /// </summary>
        public string Sign;
        /// <summary>
        /// Id Name Expire3个字段的json字符串形式
        /// </summary>
        public string JsonStr;
    }
}
