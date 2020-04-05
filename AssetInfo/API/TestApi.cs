using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AssetInfo.API
{
    class TestApi : ApiBase
    {
        [HTTPPOST]
        public async Task list()
        {
            string aa=this.Request.Headers["aaa"];
            string token = this.Request.Headers["webClientToken"];
            await this.Json(new { errcode = 200, errmsg = "it ok!", aa = aa, token = token });
        }
    }
}
