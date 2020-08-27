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
                data.Positions,
                data.Price,
                data.Remark,
                data.Profit,
                data.ExcOrg,
                data.Kind,
                data.Risk,
                ValueDate = data.ValueDate.ToString("yyyy/MM/dd HH:mm"),
                ExpDate = data.ExpDate.ToString("yyyy/MM/dd HH:mm"),
                data.Rate,
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

        [HTTPPOST]
        public async Task Titles()
        {
            var para = this.ParaForm<AssetM>();
            List<AssetM> data = AssetBll.HistoryTitles(para);
            if (data.Count == 0)
            {
                await this.Json(new { errmsg = para.ErrorMsg, errcode = para.ErrorCode });
                return;
            }
            // 字段
            var redata = data.Select(o => new
            {
                o.Title,
                o.Id
            });
            await this.Json(new { list = redata, errcode = 200 });
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
            // 风险等级
            Dictionary<string, object>[] byRisk = AssetBll.ValueTotalByRisk();
            // 机构
            List<AssetM> byExcOrg = AssetBll.ValueTotalByExcOrg();
            // 资产类型
            List<AssetM> byKind = AssetBll.ValueTotalByKind();
            // 近30更新日总值
            List<AssetM> lasttotal30 = AssetBll.Last30TotalVal();
            var redata = new
            {
                data.Value,
                byRisk = data.Value == 0 ? null : byRisk,
                byExcOrg = byExcOrg == null ? null :
                SerializeHelp.ObjectsToDicts(byExcOrg, nameof(AssetM.ExcOrg), nameof(AssetM.Value)),
                byKind = byKind == null ? null :
                SerializeHelp.ObjectsToDicts(byKind, nameof(AssetM.Kind), nameof(AssetM.Value)),
                lasttotal30= lasttotal30==null?null:
                SerializeHelp.ObjectsToDicts(lasttotal30, nameof(AssetM.Value), nameof(AssetM.TotalDate)),
                errcode = data.ErrorCode
            };
            await this.Json(redata);
        }

        /// <summary>
        /// 总资产更新
        /// </summary>
        /// <returns></returns>
        [HTTPPOST]
        public async Task TotalUp()
        {
            AssetM data = AssetBll.TotalUp();
            if (data.ErrorCode >= 300)
            {
                await this.Json(new { errcode = data.ErrorCode, errmsg = data.ErrorMsg });
                return;
            }
            var redata = new
            {
                date = data.TotalDate,
                time = data.Ctime.ToString("yyyy/MM/dd HH:mm:ss"),
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
                o.Positions,
                o.Price,
                o.Remark,
                o.Profit,
                Risk = KeyValBll.CodeToTitle(o.Risk),
                ExcOrg = KeyValBll.CodeToTitle(o.ExcOrg),
                Kind = KeyValBll.CodeToTitle(o.Kind),
                Valuedate = o.ValueDate.ToString("yyyy/MM/dd HH:mm"),
                Expdate = o.ExpDate.ToString("yyyy/MM/dd HH:mm"),
                o.Rate,
                Status = KeyValBll.CodeToTitle(o.Status),
                Ctime = o.Ctime.ToString("yyyy/MM/dd HH:mm"),
                o.ItemCode,
                o.Enabled
            });
            return redata;
        }
    }
}
