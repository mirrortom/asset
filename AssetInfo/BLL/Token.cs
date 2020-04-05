using AssetInfo.Entity;
using AssetInfo.Help;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AssetInfo.BLL
{
    public class Token
    {
        private const string aesKey16 = "iq29d9!=*7&8iv30";

        /// <summary>
        /// client发来的token,解析成对象.失败返回null
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static AccountM ParseToken(string token)
        {
            string sign = token.Substring(token.Length - 64);
            string userAes = token.Substring(0, token.Length - 64);

            // userjson
            string userjson = SecurityHelp.AESDecrypt(userAes, aesKey16);
            if (userjson == null) return null;
            //
            AccountM user = SerializeHelp.JsonToObject<AccountM>(userjson);
            user.Sign = sign;
            user.JsonStr = userjson;
            return user;
        }
        /// <summary>
        /// 检查token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool Check(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;
            if (token.Length < 64)
                return false;
            // 解析
            AccountM u = ParseToken(token);
            if (u == null) return false;

            // check sign
            string reSign = SecurityHelp.StringSHA256(u.JsonStr + aesKey16);
            if (reSign != u.Sign)
                return false;

            // 2小时过期
            if (DateTimeOffset.Now.UtcTicks > u.Expire)
                return false;
            //
            return true;
        }
        /// <summary>
        /// 生成新的token
        /// </summary>
        /// <returns></returns>
        public static string NewToken()
        {
            var user = new 
            {
                Id = "1",
                Name = "mirror",
                // 2小时过期
                Expire = DateTimeOffset.Now.AddHours(2).UtcTicks
            };
            // userjson
            string userjson = SerializeHelp.ObjectToJSON(user);
            // userjson + aeskey = aes
            string userAes = SecurityHelp.AESEncrypt(userjson, aesKey16);
            // sign sha256 len=64 
            string userSha256 = SecurityHelp.StringSHA256(userjson + aesKey16);
            // token
            string token = userAes + userSha256;
            return token;
        }
    }
}
