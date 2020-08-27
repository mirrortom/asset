using AssetInfo.BLL;
using AssetInfo.DBA;
using AssetInfo.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AssetInfo.DAL
{
    class TotalDayDal
    {
        /// <summary>
        /// 获取最新日期总值.如果无值返回null
        /// 如果指定日期,则获取指定日期的总值
        /// </summary>
        /// <returns></returns>
        public static AssetM GetLastValue(int date=0)
        {
            string where = "";
            if (date != 0)
            {
                where = "where totaldate=@totaldate";
            }
            SQLServer db = new SQLServer();
            string last = $@"SELECT TOP 1 value,totaldate FROM totalday {where}
order by totaldate desc";
            AssetM[] lastVal = db.ExecuteQuery<AssetM>(last,date);
            return lastVal?[0];
        }
        /// <summary>
        /// 更新总值
        /// </summary>
        /// <param name="para"></param>
        public static void UpdateVal(AssetM para)
        {
            para.ErrorCode = 305;
            SQLServer db = new SQLServer();
            string del = @"delete totalday where totaldate=@totaldate";
            string sql = @"insert into totalday(id,value,totaldate,ctime)";
            db.BeginTransaction();
            if (db.ExecuteNoQuery(del, para.TotalDate, 1) < 0
                || db.Insert<AssetM>(sql, para) != 1)
            {
                db.RollBackTransaction();
                para.ErrorMsg = AlertMsg.数据库错误.ToString();
                return;
            }
            if (db.CommitTransaction())
            {
                para.ErrorCode = 200;
                return;
            }
            para.ErrorMsg = AlertMsg.数据库错误.ToString();
        }
    }
}
