using AssetInfo.Help;
using System;
using System.Collections.Generic;
using System.Text;
using AssetInfo.DBA;
using AssetInfo.Entity;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace AssetInfo.BLL
{
    /// <summary>
    /// 基本数据表 资产类型,状态,其它枚举值等等
    /// </summary>
    public class KeyValBll
    {
        private static readonly MemoryCache cache = CacheHelp.cache;
        /// <summary>
        /// 生成新code 8位
        /// </summary>
        /// <returns></returns>
        public static string NewCode()
        {
            return RandHelp.NewPassWord(8, 24);
        }
        /// <summary>
        /// 添加一个新的"K-V对",返回新KV对的code(8长度的字符串).如果添加失败返回错误信息
        /// </summary>
        /// <returns></returns>
        public static string Add(KeyvalM para)
        {
            // code是PK,不能重复,尝试10次
            string insert = "insert into KeyVal(code,title,category,comment,ctime,orderby)";
            SQLServer db = new SQLServer();
            for (int i = 0; i < 10; i++)
            {
                para.Code = NewCode();
                para.Ctime = DateTimeOffset.Now;
                int re = db.Insert<KeyvalM>(insert, para);
                if (re == 1)
                {
                    return para.Code;
                }
            }
            return "db error add is failed!";
        }
        /// <summary>
        /// 停用/启用 tabId(表名),colId(记录id),on(1=on,0=off) 3个参数
        /// </summary>
        public static bool Enabled(OnOff para)
        {
            if (string.IsNullOrWhiteSpace(para.tableid) || string.IsNullOrWhiteSpace(para.colid) || string.IsNullOrWhiteSpace(para.onoff))
                return false;

            string sql = $@"SELECT COUNT(*) FROM disabled WHERE tableid=@tableid and colid=@colid";
            string on = $@"DELETE disabled WHERE tableid=@tableid and colid=@colid";
            string off = $@"INSERT disabled(tableid,colid,ctime)";
            SQLServer db = new SQLServer();

            para.tableid = para.tableid.ToLower();
            // 查询状态
            object count = db.ExecuteScalar<OnOff>(sql, para);
            bool isDisabled = int.Parse(count.ToString()) > 0;
            bool isok = true;
            if (para.onoff == "1" && isDisabled)
            {
                // 启用
                isok = db.ExecuteNoQuery<OnOff>(on, para) > 0;
            }
            else if (para.onoff == "0" && !isDisabled)
            {
                para.ctime = DateTimeOffset.Now;
                // 停用
                isok = db.Insert<OnOff>(off, para) > 0;
            }
            return isok;
        }
        /// <summary>
        /// 获取keyval表所有数据,一个code为键,KeyvalM为值的字典.数据会加入缓存
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static Dictionary<string, KeyvalM> Cache()
        {
            // 读取缓存
            Dictionary<string, KeyvalM> dicts = cache.Get<Dictionary<string, KeyvalM>>(Table.keyval.ToString());
            // 无缓存时,查库缓存
            if (dicts == null)
            {
                List<KeyvalM> data = All(new KeyvalM());
                dicts = new Dictionary<string, KeyvalM>();
                foreach (var item in data)
                {
                    dicts.Add(item.Code, item);
                }
                cache.Set<Dictionary<string, KeyvalM>>(Table.keyval.ToString(), dicts);
            }
            return dicts;
        }
        /// <summary>
        /// 返回code对应的标题
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static string CodeToTitle(string code)
        {
            Dictionary<string, KeyvalM> dicts = Cache();
            if (dicts == null) return null;
            return dicts[code].Title;
        }
        /// <summary>
        /// 数据:查找出符合条件的所有记录
        /// </summary>
        /// <param name="para">查询条件参数</param>
        /// <returns></returns>
        public static List<KeyvalM> All(KeyvalM para)
        {
            StringBuilder sb = new StringBuilder("1=1");
            if (para.Category > 0)
            {
                sb.Append(" and category=@category");
            }
            string where = sb.ToString();
            string sql = $@"SELECT code,title,category,comment,a.ctime,
                                   CASE WHEN d.ctime IS NULL THEN 1 ELSE 0 END [enabled]
                            FROM Keyval a 
                            LEFT JOIN disabled d
                            ON d.colid=a.code AND d.tableid='{Table.keyval.ToString()}'
                            WHERE {where}
                            ORDER BY a.orderby , a.ctime DESC";
            //
            SQLServer db = new SQLServer();
            // 数据列表
            KeyvalM[] data = db.ExecuteQuery<KeyvalM, KeyvalM>(sql, para);
            if (data == null)
                return new List<KeyvalM>();
            para.ErrorCode = 200;
            return data.ToList();
        }

        /// <summary>
        /// 一个:查找指定ID(主键)的一个记录
        /// </summary>
        /// <param name="id">主键ID</param>
        /// <returns></returns>
        public static KeyvalM GetById(string id)
        {
            StringBuilder sb = new StringBuilder("1=1");
            string where = sb.ToString();
            string sql = $@"SELECT code,title,category,ctime,comment
                            FROM Keyval
                            WHERE id=@id and {where}";
            //
            SQLServer db = new SQLServer();
            // 数据列表
            KeyvalM[] data = db.ExecuteQuery<KeyvalM>(sql, id, 1);
            return data?[0];
        }
        /// <summary>
        /// 判断指定id是否为表中记录.enabled参数为true时,除了是表中记录还要是开启的
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static bool CheckByCode(string code, bool enabled = true)
        {
            StringBuilder sb = new StringBuilder("1=1");
            string where = sb.ToString();
            string sql = $@"SELECT COUNT(code)
                            FROM Keyval
                            WHERE code=@code";
            string isEnabled = $@"SELECT COUNT(*) FROM disabled WHERE colid=@colid and tableid='{Table.keyval.ToString()}'";
            //
            SQLServer db = new SQLServer();
            object res = db.ExecuteScalar(sql, code, 1);

            if (int.Parse(res.ToString()) <= 0)
                return false;
            if (enabled == false)
                return true;
            // id不在disabled表中就是已开启的记录,否则为停用记录
            res = db.ExecuteScalar(isEnabled, code, 1);
            return int.Parse(res.ToString()) == 0;
        }
        /// <summary>
        /// 修改排序.
        /// </summary>
        /// <param name="orderString">code字符串,逗号隔开的.</param>
        /// <returns></returns>
        public static KeyvalM OrderBy(string orderString)
        {
            KeyvalM data = new KeyvalM();
            if (string.IsNullOrWhiteSpace(orderString))
            {
                data.ErrorMsg = "排序参数不能为空!";
                return data;
            }
            string[] order = orderString.Split(',');
            string sql = @"UPDATE Keyval SET orderby=@orderby WHERE code=@code";
            SQLServer db = new SQLServer();
            db.BeginTransaction();
            for (int i = 0; i < order.Length; i++)
            {
                int re = db.ExecuteNoQuery(sql, i+1, order[i]);
                if (re == -999)
                {
                    db.RollBackTransaction();
                    data.ErrorMsg = AlertMsg.数据库错误.ToString();
                    return data;
                }
            }
            if (db.CommitTransaction() == false)
            {
                data.ErrorMsg = AlertMsg.数据库错误.ToString();
                return data;
            }
            data.ErrorCode = 200;
            return data;
        }
    }
}
