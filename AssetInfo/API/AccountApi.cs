using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AssetInfo.Help;
using AssetInfo.BLL;

namespace AssetInfo
{
    class AccountApi : ApiBase
    {
        [HTTPPOST]
        public async Task Login()
        {
            // 只能本机登录
            if (this.HttpContext.Connection.LocalIpAddress.ToString() != this.HttpContext.Connection.RemoteIpAddress.ToString())
            {
                await this.Json(new { errmsg = "login only localhost!" });
                return;
            }
            // 密码比对
            var para = this.ParaForm();
            if (CheckPwd(para.pwd))
            {
                // 生成token返回客户端
                string tk = Token.NewToken();
                await this.Json(new { errcode = 200, token = tk });
                return;
            }
            await this.Json(new { errmsg = "login error!" });
        }

        private static bool CheckPwd(string base64Pwd)
        {
            byte[] pwdBytes = Convert.FromBase64String(base64Pwd);
            string pwd = Encoding.UTF8.GetString(pwdBytes);
            DbOptions dbop = new DbOptions()
                .SetCreateIfMissing()
                .SetEnableWriteThreadAdaptiveYield(true);
            RocksDb db = RocksDb.Open(dbop, @"E:\db\rocksdb\asset");
            // db里的密码做了sha256
            string password = db.Get("assetPwd");
            db.Dispose();
            return SecurityHelp.StringSHA256(pwd) == password;
        }
    }
}
