using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetInfo.DAL;
using AssetInfo.DBA;
using AssetInfo.Entity;
using AssetInfo.Help;

namespace AssetInfo.BLL
{
    public class AssetBll
    {
        /// <summary>
        /// 列表:查找出符合条件的多个记录.如果没找到返回空列表
        /// </summary>
        /// <param name="para">查询条件参数</param>
        /// <returns></returns>
        public static List<AssetM> List(AssetM para)
        {
            AssetM[] data = null;
            if (para.IsPagePart == 0)
            {
                data = AssetDal.All(para);
            }
            else
            {
                // 若分页则要验证分页参数
                para.SetPageIndexAndSize();
                data = AssetDal.List(para);
            }
            if (data == null)
            {
                para.ErrorMsg = AlertMsg.没有数据.ToString();
                return new List<AssetM>();
            }
            for (int i = 0; i < data.Length; i++)
            {
                AssetM item = data[i];
                item.RowNumber = para.IsPagePart == 0
                    ? 1 + i : para.StartRowIndex + i;
            }
            return data.ToList();
        }

        /// <summary>
        /// 查找一个资产的所有更新记录.根据资产id
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public static List<AssetM> HistoryList(AssetM para)
        {
            if (string.IsNullOrWhiteSpace(para.Id))
            {
                para.ErrorMsg = "资产id无效";
                return new List<AssetM>();
            }
            bool hasItemcode = AssetItemCode(para);
            if (hasItemcode == false)
            {
                para.ErrorMsg = "资产itemCode无效";
                return new List<AssetM>();
            }
            string sql = $@"
SELECT id,title,code,amount,positions,price,value,remark,profit,risk,excorg,kind,valuedate,expdate,rate,status,a.ctime,itemCode,CASE WHEN d.ctime IS NULL THEN 1 ELSE 0 END [enabled]
FROM Asset a
LEFT JOIN disabled d
ON d.colid=a.id AND d.tableid='{Table.asset.ToString()}'
WHERE a.itemCode=@itemCode
ORDER BY a.ctime DESC";
            SQLServer db = new SQLServer();
            AssetM[] data = db.ExecuteQuery<AssetM, AssetM>(sql, para);
            if (data == null)
            {
                para.ErrorMsg = AlertMsg.没有数据.ToString();
                return new List<AssetM>();
            }
            return data.ToList();
        }

        /// <summary>
        /// 返回所有资产的标题.按资产itemcode分组,返回title,id(其中一个记录的id)
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public static List<AssetM> HistoryTitles(AssetM para)
        {
            string sql = $@"
SELECT a.title,a.id FROM
(
	SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
	FROM {Table.asset.ToString()}) a
WHERE a.rn=1";
            SQLServer db = new SQLServer();
            AssetM[] data = db.ExecuteQuery<AssetM, AssetM>(sql, para);
            if (data == null)
            {
                para.ErrorMsg = AlertMsg.没有数据.ToString();
                return new List<AssetM>();
            }
            return data.ToList();
        }

        /// <summary>
        /// 一个:查找指定ID(主键)的一个记录.如果没找到返回null
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        public static AssetM GetById(AssetM para)
        {
            return AssetDal.GetById(para);
        }
        /// <summary>
        /// 新增资产 资产id设置在para.id,成功返回true,失败返回false
        /// </summary>
        /// <param name="para"></param>
        /// <returns></returns>
        public static void Add(AssetM para)
        {
            if (FormCheck(para) == false)
                return;
            //
            para.Id = RandHelp.NewGuid();
            para.Ctime = DateTime.Now;
            //
            string sql = "insert into Asset(id,title,code,amount,value,positions,price,risk,remark,profit,excorg,kind,valuedate,expdate,rate,status,ctime,itemCode)";
            SQLServer db = new SQLServer();
            int re = db.Insert<AssetM>(sql, para);
            if (re == 1)
                para.ErrorCode = 200;
        }

        /// <summary>
        /// ItemCode值标识同一个资产的更新周期,从首次添加起到清仓止,首次添加资产时生成.
        /// 以后更新这个资产时会传来资产Id,据此查得ItemCode值.
        /// 举例说明:510300这个资产,在第一次添加时生成ItemCode "xxx",后续更新资产时,
        /// 此值都是"xxx",一直到清仓都是.
        /// 假如下一次又添加510300时,会生成新的ItemCode "xxx2".
        /// </summary>
        /// <param name="data"></param>
        private static bool AssetItemCode(AssetM data)
        {
            if (string.IsNullOrWhiteSpace(data.Id))
            {
                data.ItemCode = RandHelp.NewGuid();
                return true;
            }
            string sql = "SELECT itemCode FROM Asset WHERE Id=@Id";
            SQLServer db = new SQLServer();
            object itemcode = db.ExecuteScalar(sql, data.Id, 1);
            if (itemcode == null || string.IsNullOrWhiteSpace(itemcode.ToString()))
                return false;
            data.ItemCode = itemcode.ToString();
            return true;
        }
        /// <summary>
        /// add 验证表单数据
        /// </summary>
        /// <returns></returns>
        private static bool FormCheck(AssetM data)
        {
            if (AssetItemCode(data) == false)
            {
                data.ErrorMsg = $"{data.Title}的itemCode获取失败.";
                return false;
            }
            string verrmsg = Validate.CheckEntity<AssetM>(data);
            if (verrmsg != null)
            {
                data.ErrorMsg = verrmsg;
                return false;
            }
            foreach (string item in new string[] { data.Status, data.ExcOrg, data.Kind })
            {
                if (KeyValBll.CheckByCode(item) == false)
                {
                    data.ErrorMsg = $"{item}无效.";
                    return false;
                }
            }
            // 
            return true;
        }

        /// <summary>
        /// 计算当前(最后一次更新后)总资产.含字段: value
        /// </summary>
        /// <returns></returns>
        public static AssetM ValueTotal()
        {
            string sql = $@"
SELECT SUM(value) value
FROM(
	SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
	FROM(
		SELECT a.*
		FROM Asset a
		LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset}'
		WHERE d.ctime IS NULL) b
	) s
WHERE s.rn=1 and s.value>0";
            SQLServer db = new SQLServer();
            var data = db.ExecuteQuery<AssetM>(sql);
            if (data == null)
            {
                return new AssetM() { ErrorMsg = AlertMsg.没有数据.ToString() };
            }
            data[0].ErrorCode = 200;
            return data[0];
        }
        /// <summary>
        /// 更新总资产数据表
        /// </summary>
        /// <returns></returns>
        public static AssetM TotalUp()
        {
            AssetM m = new AssetM();
            // 1.过时检查
            TotalUp1_LastTime(m);
            if (m.ErrorCode > 300) return m;
            // 2.补录情况
            TotalUp2_BuLu(m);
            if (m.ErrorCode > 300) return m;
            // 2.1补录成功情况
            if (m.ErrorCode == 201) return m;
            // 写入数据
            AssetM totalVal = ValueTotal();
            if (totalVal.ErrorCode != 200)
            {
                m.ErrorCode = 306;
                m.ErrorMsg = "更新失败,获取总值出错!";
                return m;
            }
            m.Id = RandHelp.NewGuid();
            m.Value = totalVal.Value;
            m.Ctime = DateTimeOffset.Now;
            m.TotalDate = int.Parse(DateTimeOffset.Now.ToString("yyyyMMdd"));
            TotalDayDal.UpdateVal(m);
            return m;
        }
        /// <summary>
        /// 补录情况
        /// </summary>
        /// <param name="para"></param>
        private static void TotalUp2_BuLu(AssetM para)
        {
            // 查询最后更新资产日期
            DateTimeOffset lastUpDay = AssetDal.GetLastUpDay();
            // 表里没有有效更新数据,这种情况也不更新总值.
            if (lastUpDay == default)
            {
                para.ErrorCode = 302;
                para.ErrorMsg = "没有任何更新记录,不可更新总值!";
                return;
            }
            // 如果日期不是今天,(也就是今天没有更新过资产).再查询这个日期的总值记录,
            // 如果没有记录,那么补录(总值日期为这天).
            // 如果有记录,那么不可以更新.
            int lastday = int.Parse(lastUpDay.ToString("yyyyMMdd"));
            if (lastday < int.Parse(DateTimeOffset.Now.ToString("yyyyMMdd")))
            {
                AssetM totalByDay = TotalDayDal.GetLastValue(lastday);
                if (totalByDay == null)
                {
                    // 补录
                    AssetM totalVal = ValueTotal();
                    if (totalVal.ErrorCode != 200)
                    {
                        para.ErrorCode = 304;
                        para.ErrorMsg = "补录失败,获取总值出错!";
                        return;
                    }
                    para.Id = RandHelp.NewGuid();
                    para.Value = totalVal.Value;
                    para.Ctime = DateTimeOffset.Now;
                    para.TotalDate = lastday;
                    TotalDayDal.UpdateVal(para);
                    if (para.ErrorCode == 200)
                    {
                        para.ErrorCode = 201;
                        para.ErrorMsg = "补录成功!";
                    }
                }
                else
                {
                    para.ErrorCode = 303;
                    para.ErrorMsg = "今天没更新资产,不可更新总值!";
                }
            }
            // 
        }
        /// <summary>
        /// 总值更新只在每天23.50分前操作
        /// </summary>
        /// <param name="para"></param>
        private static void TotalUp1_LastTime(AssetM para)
        {
            // 为了不跨天,总值更新只在每天23.50分前操作
            string limitTime = DateTimeOffset.Now.ToString("yyyy/MM/dd 23:50:00");
            if (DateTimeOffset.Now > DateTimeOffset.Parse(limitTime))
            {
                para.ErrorCode = 301;
                para.ErrorMsg = AlertMsg.当前时段禁止该操作.ToString();
            }
        }
        /// <summary>
        /// 计算按风险等级分组的总市值.含字段:risk, value ,Comment
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object>[] ValueTotalByRisk()
        {
            string sql = $@"
SELECT kv.title Risk,s.Value,kv.Comment
FROM(
	SELECT title,code,comment FROM keyval
	WHERE category={(int)KVEnumKind.风险等级}) kv
LEFT JOIN(
	SELECT risk,SUM(value) value,SUM(profit) profit
	FROM(
		SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
		FROM(
			SELECT a.*
			FROM Asset a
			LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset}'
			WHERE d.ctime IS NULL) a
		) b
	WHERE b.rn=1 and b.value>0
	GROUP BY b.risk) s
ON kv.code=s.risk
ORDER BY kv.title";
            SQLServer db = new SQLServer();
            var data = db.ExecuteQuery(sql);
            if (data != null)
            {
                foreach (var item in data)
                {
                    if (item["Value"] == DBNull.Value)
                        item["Value"] = 0;
                }
            }
            return data;
        }

        /// <summary>
        /// 计算按机构分组的总市值.含字段:excorg, value
        /// </summary>
        /// <returns></returns>
        public static List<AssetM> ValueTotalByExcOrg()
        {
            string sql = $@"
SELECT kv.title excorg,s.value FROM(
	SELECT excOrg, SUM(value) value,SUM(profit) profit
	FROM(
		SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
		FROM(
			SELECT a.*
			FROM Asset a
			LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset}'
			WHERE d.ctime IS NULL) a
		) b
	WHERE b.rn=1 and b.value>0
	GROUP BY b.excOrg) s
JOIN KeyVal kv
ON kv.code=s.excOrg";
            SQLServer db = new SQLServer();
            var data = db.ExecuteQuery<AssetM>(sql);
            if (data == null)
            {
                return null;
            }
            return data.ToList();
        }

        /// <summary>
        /// 计算按资产种类的市值.含字段:kind, value
        /// </summary>
        /// <returns></returns>
        public static List<AssetM> ValueTotalByKind()
        {
            string sql = $@"
SELECT kv.title kind,s.value FROM(
	SELECT kind, SUM(value) value,SUM(profit) profit
	FROM(
		SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
		FROM(
			SELECT a.*
			FROM Asset a
			LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset.ToString()}'
			WHERE d.ctime IS NULL) a
		) b
	WHERE b.rn=1 and b.value>0
	GROUP BY b.kind) s
JOIN KeyVal kv
ON kv.code=s.kind";
            SQLServer db = new SQLServer();
            var data = db.ExecuteQuery<AssetM>(sql);
            if (data == null)
            {
                return null;
            }
            return data.ToList();
        }

        /// <summary>
        /// 获取最近30个总值数据,(用于首页显示)
        /// </summary>
        /// <returns></returns>
        public static List<AssetM> Last30TotalVal()
        {
            string sql = @"SELECT TOP 31 value,totaldate FROM totalday ORDER BY totaldate DESC";
            SQLServer db = new SQLServer();
            var data = db.ExecuteQuery<AssetM>(sql);
            if (data == null)
            {
                return null;
            }
            return data.ToList();
        }
    }
}
