using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
SELECT id,title,code,amount,value,charge,remark,profit,risk,excorg,kind,buydate,valuedate,expdate,rate,term,status,a.ctime,itemCode,CASE WHEN d.ctime IS NULL THEN 1 ELSE 0 END [enabled]
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
            string sql = "insert into Asset(id,title,code,amount,value,charge,risk,remark,profit,excorg,kind,buydate,valuedate,expdate,rate,term,status,ctime,itemCode)";
            SQLServer db = new SQLServer();
            int re = db.Insert<AssetM>(sql, para);
            if (re == 1)
                para.ErrorCode = 200;
        }

        /// <summary>
        /// ItemCode值标识同一个资产,首次添加资产时生成,后面更新这个资产时会传来资产Id,据此查得ItemCode值
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
        /// 计算总市值.含字段: amount,value,charge,profit
        /// </summary>
        /// <returns></returns>
        public static AssetM ValueTotal()
        {
            string sql = $@"
SELECT SUM(amount) amount,SUM(value) value,SUM(charge) charge,SUM(profit) profit
FROM(
	SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
	FROM(
		SELECT a.*
		FROM Asset a
		LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset.ToString()}'
		WHERE d.ctime IS NULL) b
	) s
WHERE s.rn=1";
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
        /// 计算按风险等级分组的总市值.含字段:risk, amount,value,charge,profit
        /// </summary>
        /// <returns></returns>
        public static List<AssetM> ValueTotalByRisk()
        {
            string sql = $@"
SELECT kv.title risk,s.amount,s.charge,s.profit,s.value
FROM(
	SELECT title,code FROM keyval
	WHERE category={(int)KVEnumKind.风险等级}) kv
LEFT JOIN(
	SELECT risk, SUM(amount) amount,SUM(value) value,SUM(charge) charge,SUM(profit) profit
	FROM(
		SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
		FROM(
			SELECT a.*
			FROM Asset a
			LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset.ToString()}'
			WHERE d.ctime IS NULL) a
		) b
	WHERE b.rn=1
	GROUP BY b.risk) s
ON kv.code=s.risk
ORDER BY kv.title";
            SQLServer db = new SQLServer();
            var data = db.ExecuteQuery<AssetM>(sql);
            if (data == null)
            {
                return null;
            }
            return data.ToList();
        }
        /// <summary>
        /// 计算按机构分组的总市值.含字段:excorg, amount,value,charge,profit
        /// </summary>
        /// <returns></returns>
        public static List<AssetM> ValueTotalByExcOrg()
        {
            string sql = $@"
SELECT kv.title excorg,s.amount,s.charge,s.profit,s.value FROM(
	SELECT excOrg, SUM(amount) amount,SUM(value) value,SUM(charge) charge,SUM(profit) profit
	FROM(
		SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
		FROM(
			SELECT a.*
			FROM Asset a
			LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset.ToString()}'
			WHERE d.ctime IS NULL) a
		) b
	WHERE b.rn=1
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
        
    }
}
