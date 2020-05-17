using RocksDbSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using AssetInfo.Help;
using AssetInfo.BLL;
using Microsoft.Extensions.Primitives;

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
                LoginLog(true);
                await this.Json(new { errcode = 200, token = tk });
                return;
            }
            LoginLog(false);
            await this.Json(new { errmsg = "login error!" });
        }

        /// <summary>
        /// 登录成功时,到数据库加一条日志
        /// </summary>
        /// <param name="isok">true=登录成功</param>
        private void LoginLog(bool isok)
        {
            // 取出HOST,USERAGENT信息
            this.Request.Headers.TryGetValue("User-Agent", out StringValues useragent);
            this.Request.Headers.TryGetValue("Host", out StringValues host);
            string ipport = $"{this.HttpContext.Connection.RemoteIpAddress.ToString()}:{this.HttpContext.Connection.RemotePort.ToString()}";
            string insert = $"insert into loginlog(host,ip,ctime,useragent,info)";
            DBA.SQLServer db = new DBA.SQLServer();
            db.Insert(insert, host.ToString(), ipport.ToString(), DateTimeOffset.Now, useragent.ToString(), isok ? "1" : "0");
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
