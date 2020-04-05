using AssetInfo.BLL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using AssetInfo.Entity;
using AssetInfo.Help;
namespace AssetInfo
{
    [AUTH]
    class AssetApi : ApiBase
    {
        [HTTPPOST]
        public async Task List()
        {
            var para = this.ParaForm<AssetM>();
            List<AssetM> data = AssetBll.List(para);
            if (data.Count == 0)
            {
                await this.Json(new { errmsg = para.ErrorMsg, errcode = para.ErrorCode });
                return;
            }
            // 返回字段按需设定
            var redata = ToObjectList(data);
            // 按需字段列表
            if (!string.IsNullOrWhiteSpace(para.Fields) && para.Fields.Split(',').Length > 0)
            {
                var list = SerializeHelp.ObjectsToDicts(redata, para.Fields.Split(','));
                await this.Json(new { list, errcode = 200 });
                return;
            }
            // 所有字段
            await this.Json(new { list = redata, errcode = 200 });
        }
        [HTTPPOST]
        public async Task History()
        {
            var para = this.ParaForm<AssetM>();
            List<AssetM> data = AssetBll.HistoryList(para);
            if (data.Count == 0)
            {
                await this.Json(new { errmsg = para.ErrorMsg, errcode = para.ErrorCode });
                return;
            }
            // 返回字段按需设定
            var redata = ToObjectList(data);
            // 按需字段列表
            if (!string.IsNullOrWhiteSpace(para.Fields) && para.Fields.Split(',').Length > 0)
            {
                var list = SerializeHelp.ObjectsToDicts(redata, para.Fields.Split(','));
                await this.Json(new { list, errcode = 200 });
                return;
            }
            // 所有字段
            await this.Json(new { list = redata, errcode = 200 });
        }
        [HTTPPOST]
        public async Task Item()
        {
            var para = this.ParaForm<AssetM>();
            AssetM data = AssetBll.GetById(para);
            if (data == null)
            {
                await this.Json(new { errmsg = para.ErrorMsg, errcode = para.ErrorCode });
                return;
            }
            // 返回字段按需设定
            var redata = new
            {
                data.Id,
                data.Title,
                data.Code,
                data.Amount,
                data.Value,
                data.Charge,
                data.Remark,
                data.Profit,
                data.ExcOrg,
                data.Kind,
                data.Risk,
                BuyDate = data.BuyDate.ToString("yyyy/MM/dd HH:mm"),
                ValueDate = data.ValueDate.ToString("yyyy/MM/dd HH:mm"),
                ExpDate = data.ExpDate.ToString("yyyy/MM/dd HH:mm"),
                data.Rate,
                data.Term,
                data.Status,
                Ctime = data.Ctime.ToString("yyyy/MM/dd HH:mm"),
                data.ItemCode
            };
            await this.Json(new { item = redata, errcode = 200 });
        }

        [HTTPPOST]
        public async Task Add()
        {
            AssetM para = this.ParaForm<AssetM>();
            AssetBll.Add(para);
            await this.Json(new { errcode = para.ErrorCode, errmsg = para.ErrorMsg });
        }

        /// <summary>
        /// 资产统计
        /// </summary>
        /// <returns></returns>
        [HTTPPOST]
        public async Task Statistic()
        {
            AssetM data = AssetBll.ValueTotal();
            if (data.ErrorCode != 200)
            {
                await this.Json(new { errcode = 301, errmsg = data.ErrorMsg });
                return;
            }
            List<AssetM> byRisk = AssetBll.ValueTotalByRisk();
            List<AssetM> byExcOrg = AssetBll.ValueTotalByExcOrg();
            var redata = new
            {
                data.Value,
                data.Amount,
                data.Profit,
                data.Charge,
                byRisk= SerializeHelp.ObjectsToDicts(byRisk, nameof(AssetM.Risk), nameof(AssetM.Value),
                nameof(AssetM.Amount),nameof(AssetM.Profit), nameof(AssetM.Charge)),
                byExcOrg= SerializeHelp.ObjectsToDicts(byExcOrg, nameof(AssetM.ExcOrg), nameof(AssetM.Value),
                nameof(AssetM.Amount), nameof(AssetM.Profit), nameof(AssetM.Charge)),
                errcode = data.ErrorCode
            };
            await this.Json(redata);
        }
        private IEnumerable<object> ToObjectList(IEnumerable<AssetM> data)
        {
            var redata = data.Select(o => new
            {
                o.Id,
                o.Title,
                o.Code,
                o.Amount,
                o.Value,
                o.Charge,
                o.Remark,
                o.Profit,
                Risk = KeyValBll.CodeToTitle(o.Risk),
                ExcOrg = KeyValBll.CodeToTitle(o.ExcOrg),
                Kind = KeyValBll.CodeToTitle(o.Kind),
                Buydate = o.BuyDate.ToString("yyyy/MM/dd HH:mm"),
                Valuedate = o.ValueDate.ToString("yyyy/MM/dd HH:mm"),
                Expdate = o.ExpDate.ToString("yyyy/MM/dd HH:mm"),
                o.Rate,
                o.Term,
                Status = KeyValBll.CodeToTitle(o.Status),
                Ctime = o.Ctime.ToString("yyyy/MM/dd HH:mm"),
                o.ItemCode,
                o.Enabled
            });
            return redata;
        }
    }
}
