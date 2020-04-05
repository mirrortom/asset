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
            string sql = $@"SELECT id,title,code,amount,value,charge,remark,profit,excorg,risk,kind,buydate,valuedate,expdate,rate,term,status,ctime,itemCode
                            FROM Asset
                            WHERE {where}
                            ORDER BY ctime DESC
                            OFFSET @OffSetRows ROWS FETCH NEXT @PageSize ROWS ONLY";
            string sqlcount = $@"SELECT COUNT(id) FROM Asset WHERE {where}";
            //
            SQLServer db = new SQLServer();
            // 总条数(如果为0无需再查询)
            int count=0;
            var listcount = db.ExecuteScalar<AssetM>(sqlcount, para);
            if (listcount == null || !int.TryParse(listcount.ToString(),out count) || count==0)
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
            StringBuilder sb = new StringBuilder("1=1");
            string where = sb.ToString();
            // 先排除禁用的记录,再开窗
            string sql = $@"
SELECT id,title,code,amount,value,charge,remark,profit,risk,excorg,kind,buydate,valuedate,expdate,rate,term,status,ctime,itemCode
FROM(
	SELECT *,ROW_NUMBER() OVER(PARTITION BY itemCode ORDER BY ctime DESC) rn
	FROM(
		SELECT a.*
		FROM Asset a
		LEFT JOIN disabled d ON d.colid=a.id AND d.tableid='{Table.asset.ToString()}'
		WHERE d.ctime IS NULL) b
	) s
WHERE s.rn=1 
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
            string sql = $@"SELECT id,title,code,amount,value,charge,remark,risk,profit,excorg,kind,buydate,valuedate,expdate,rate,term,status,ctime,itemCode
                            FROM Asset
                            WHERE id=@id and {where}";
            //
            SQLServer db = new SQLServer();
            // 数据列表
            AssetM[] data = db.ExecuteQuery<AssetM>(sql, para.Id,1);
            return data?[0];
        }

        /// <summary>
        /// 添加一个:增加一个新的记录.返回受影响行数
        /// </summary>
        /// <param name="para">新记录实体</param>
        /// <returns></returns>
        public static int Add(AssetM para)
        {
            string insert = @"INSERT INTO Asset (id,title,code,amount,value,charge,remark,risk,profit,excorg,kind,buydate,valuedate,expdate,rate,term,status,ctime,itemCode)";
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
            string update = $@"UPDATE Asset (id,title,code,amount,value,charge,remark,risk,profit,excorg,kind,buydate,valuedate,expdate,rate,term,status,ctime,itemCode) WHERE id=@id and {where}";
            SQLServer db = new SQLServer();
            return db.Update<AssetM>(update, para);
        }
    }
}
