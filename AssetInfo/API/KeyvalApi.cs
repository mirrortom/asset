using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using AssetInfo.BLL;
using AssetInfo.Entity;
using AssetInfo.Help;

namespace AssetInfo
{
    [AUTH]
    class KeyvalApi : ApiBase
    {
        [HTTPPOST]
        public async Task List()
        {
            KeyvalM para = this.ParaForm<KeyvalM>();
            List<KeyvalM> data = KeyValBll.All(para);
            if (data.Count == 0)
            {
                await this.Json(new { errmsg = "no data find", errcode = para.ErrorCode });
                return;
            }
            // 返回字段按需设定
            var redata = data.Select(o => new
            {
                o.Code,
                o.Title,
                o.Category,
                o.Comment,
                o.Ctime,
                o.Enabled
            });
            await this.Json(new { list = redata, errcode = 200 });
        }
        /// <summary>
        /// 所有表记录的onoff操作都走这个API.
        /// </summary>
        /// <returns></returns>
        [HTTPPOST]
        public async Task OnOff()
        {
            // 客户端要传tabid(表名),colid(记录id),on(1=on,0=off) 3个参数
            var para = this.ParaForm<OnOff>();

            bool isok = KeyValBll.Enabled(para);
            await this.Json(new
            {
                errcode = isok ? 200 : 401
            });
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <returns></returns>
        [HTTPPOST]
        public async Task OrderBy()
        {
            var para = this.ParaForm();
            KeyvalM data = KeyValBll.OrderBy(para.orderby);
            await this.Json(new { errcode = data.ErrorCode, errmsg = data.ErrorMsg });
        }
        [HTTPPOST]
        public async Task Item()
        {
            var para = this.ParaForm<KeyvalM>();
            KeyvalM data = KeyValBll.GetById(para.Code);
            if (data == null)
            {
                await this.Json(new { msg = para.ErrorMsg });
                return;
            }
            // 返回字段按需设定
            var redata = new
            {
                data.Code,
                data.Title,
                data.Category,
                data.Comment,
                data.Ctime
            };
            await this.Json(redata);
        }

        [HTTPPOST]
        public async Task Add()
        {
            var para = this.ParaForm();
            string parajson = SerializeHelp.ObjectToJSON(para);
            var model = SerializeHelp.JsonToObject<KeyvalM>(parajson);
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                await this.Json(new { errcode = 300, errmsg = "Title is notnull" });
                return;
            }
            if (string.IsNullOrWhiteSpace(model.Comment))
            {
                await this.Json(new { errcode = 300, errmsg = "Comment is notnull" });
                return;
            }
            string result = KeyValBll.Add(model);
            await this.Json(new { errmsg = result, errcode = result.Length != 8 ? 400 : 200 });
        }

        [HTTPPOST]
        public async Task UpdateItemById()
        {
            var para = this.ParaForm();
            //var result = KeyValBll.UpdateItemById(para);
            await this.Json(new { msg = para.ErrorMsg });
        }
    }
}