using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetInfo.BLL;
using AssetInfo.DBA;
using AssetInfo.Entity;
namespace AssetInfo
{
    public class AssetDal
    {
        /// <summary>
        /// 数据(分页):查找出符合条件的多个记录
        /// </summary>
        /// <param name="para">查询条件参数</param>
        /// <returns></returns>
        public static AssetM[] List(AssetM para)
        {
            StringBuilder sb = new StringBuilder("1=1");
            string where = sb.ToString();
            string sql = $@"SELECT id,title,code,amount,value,positions,price,remark,profit,excorg,risk,kind,valuedate,expdate,rate,status,ctime,itemCode
                            FROM Asset
                            WHERE {where}
                            ORDER BY ctime DESC
                            OFFSET @OffSetRows ROWS FETCH NEXT @PageSize ROWS ONLY";
            string sqlcount = $@"SELECT COUNT(id) FROM Asset WHERE {where}";
            //
            SQLServer db = new SQLServer();
            // 总条数(如果为0无需再查询)
            int count = 0;
            var listcount = db.ExecuteScalar<AssetM>(sqlcount, para);
            if (listcount == null || !int.TryParse(listcount.ToString(), out count) || count == 0)
                return null;
            para.ListCount = count;
            // 数据列表
            AssetM[] data = db.ExecuteQuery<AssetM, AssetM>(sql, para);
            return data;
        }

        /// <summary>
        /// 数据:查找出符合条件的所有记录
        /// </summary>
        /// <param name="para">查询条件参数</param>
        /// <returns></returns>
        public static AssetM[] All(AssetM para)
        {
            StringBuilder sb = new StringBuilder();
            // 条件:市值大于0的,如果市值为0,说明已经清仓
            sb.Append("AND value > 0");
            string where = sb.ToString();
            // 先排除禁用的记录,再开窗
            string sql = $@"
SELECT id,title,code,amount,value,positions,price,remark,profit,risk,excorg,kind,valuedate,expdate,rate,status,ctime,itemCode
FROM(
	SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
	FROM(
		SELECT a.*
		FROM Asset a
		LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset}'
		WHERE d.ctime IS NULL) b
	) s
WHERE s.rn=1 {where}
ORDER BY s.ctime DESC";
            //
            SQLServer db = new SQLServer();
            // 数据列表
            AssetM[] data = db.ExecuteQuery<AssetM, AssetM>(sql, para);
            return data ?? null;
        }

        /// <summary>
        /// 一个:查找指定ID(主键)的一个记录
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        public static AssetM GetById(AssetM para)
        {
            StringBuilder sb = new StringBuilder("1=1");
            string where = sb.ToString();
            string sql = $@"SELECT id,title,code,amount,value,positions,price,remark,risk,profit,excorg,kind,valuedate,expdate,rate,status,ctime,itemCode
                            FROM Asset
                            WHERE id=@id and {where}";
            //
            SQLServer db = new SQLServer();
            // 数据列表
            AssetM[] data = db.ExecuteQuery<AssetM>(sql, para.Id, 1);
            return data?[0];
        }

        /// <summary>
        /// 添加一个:增加一个新的记录.返回受影响行数
        /// </summary>
        /// <param name="para">新记录实体</param>
        /// <returns></returns>
        public static int Add(AssetM para)
        {
            string insert = @"INSERT INTO Asset (id,title,code,amount,value,positions,price,remark,risk,profit,excorg,kind,valuedate,expdate,rate,status,ctime,itemCode)";
            SQLServer db = new SQLServer();
            return db.Insert<AssetM>(insert, para);
        }

        /// <summary>
        /// 更新一个:查找指定ID(主键)的一个记录,然后更新之.返回受影响行数
        /// </summary>
        /// <param name="para">新记录实体</param>
        /// <returns></returns>
        public static int UpdateById(AssetM para)
        {
            StringBuilder sb = new StringBuilder("1=1");
            string where = sb.ToString();
            string update = $@"UPDATE Asset (id,title,code,amount,value,positions,price,remark,risk,profit,excorg,kind,valuedate,expdate,rate,status,ctime,itemCode) WHERE id=@id and {where}";
            SQLServer db = new SQLServer();
            return db.Update<AssetM>(update, para);
        }
        /// <summary>
        /// 今天是否更新过资产(只要有1条有效更新就是)
        /// 也可以指定日期yyyy-MM-dd
        /// </summary>
        /// <param name="dateDay">yyyy-MM-dd</param>
        /// <returns></returns>
        public static bool IsUpdateToday(string dateDay = null)
        {
            string day = dateDay ?? DateTimeOffset.Now.ToString("yyyy-MM-dd");
            DateTimeOffset dayStart = DateTimeOffset.Parse(day + " 00:00:00");
            DateTimeOffset dayEnd = DateTimeOffset.Parse(day + " 23:59:59");
            string sql = $@"SELECT count(b.id)
FROM(
	SELECT a.*
	FROM Asset a
	LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset}'
	WHERE d.ctime IS NULL 
	) b
where ctime >= @stime AND ctime <= @etime";
            SQLServer db = new SQLServer();
            object count = db.ExecuteScalar(sql, dayStart, dayEnd);
            if (count == null) return false;
            return int.Parse(count.ToString()) > 0;
        }

        /// <summary>
        /// 查询最后有效更新资产日期 返回一个yyyy-MM-dd的日期公式字符串.
        /// 失败返回 default
        /// </summary>
        /// <returns></returns>
        public static DateTimeOffset GetLastUpDay()
        {
            string sql = $@"SELECT TOP 1 a.ctime
FROM Asset a
LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset}'
WHERE d.ctime IS NULL
ORDER BY a.ctime DESC";
            SQLServer db = new SQLServer();
            object lastDate = db.ExecuteScalar(sql);
            if (lastDate == null) return default;
            DateTimeOffset date = (DateTimeOffset)lastDate;
            return date;
        }
    }
}
