using System;
using System.Collections.Generic;
using System.Text;

namespace AssetInfo.Help
{
    class RandHelp
    {
        private static Random rand = new Random();
        /// <summary>
        /// 生成一个随机的密码组成可选
        /// </summary>
        /// <param name="len">长度</param>
        /// <param name="level">20=纯数字 21=纯小写字母 22=纯大写字母 23=特殊字符 24=数字小写字母 25=数字字母大小写 26=数字字母大小写下划线加{!@#$*&amp;+=})(</param>
        /// <returns></returns>
        public static string NewPassWord(int len, int level)
        {
            string[] datasource = new string[27];
            datasource[0] = "0123456789";
            datasource[1] = "abcdefghijklmnopqrstuvwxyz";
            datasource[2] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            datasource[3] = "!@#$*&+=_}{)(";
            //
            datasource[20] = datasource[0];
            datasource[21] = datasource[1];
            datasource[22] = datasource[2];
            datasource[23] = datasource[3];
            datasource[24] = datasource[0] + datasource[1];
            datasource[25] = datasource[0] + datasource[1] + datasource[2];
            datasource[26] = datasource[0] + datasource[1] + datasource[2] + datasource[3];
            //
            if (level > datasource.Length - 1 || level < 20)
                level = 20;
            // 产生随机密码
            if (level >= 20 && level <= 23)
            {
                if (len <= 0) len = 1;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < len; i++)
                {
                    sb.Append(datasource[level][rand.Next(0, datasource[level].Length)]);
                }
                return sb.ToString();
            }
            // 如果是组合形的,则需要判断是否有效
            if (level > 23)
            {
                while (true)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < len; i++)
                    {
                        sb.Append(datasource[level][rand.Next(0, datasource[level].Length)]);
                    }
                    string tpwd = sb.ToString();
                    // 判断随机数是否符合要求// 如果未随机出要求的字符,则需要重新随机.
                    if (level == 24)
                    {
                        if (len < 2) len = 2;
                        bool hasint = HasChar(tpwd, datasource[0]);
                        bool haschar = HasChar(tpwd, datasource[1]);
                        if (hasint && haschar)
                            return tpwd;
                    }
                    else if (level == 25)
                    {
                        if (len < 3) len = 3;
                        bool hasint = HasChar(tpwd, datasource[0]);
                        bool haschar = HasChar(tpwd, datasource[1]);
                        bool hasCHAR = HasChar(tpwd, datasource[2]);
                        if (hasint && haschar && hasCHAR)
                            return tpwd;
                    }
                    else if (level == 26)
                    {
                        if (len < 4) len = 4;
                        bool hasint = HasChar(tpwd, datasource[0]);
                        bool haschar = HasChar(tpwd, datasource[1]);
                        bool hasCHAR = HasChar(tpwd, datasource[2]);
                        bool hasother = HasChar(tpwd, datasource[3]);
                        if (hasint && haschar && hasCHAR && hasother)
                            return tpwd;
                    }
                }
            }
            return "";
        }
        /// <summary>
        /// 检查指定字符串中是否包含另一字符串中的至少一个字符
        /// </summary>
        /// <param name="sourcestring">要检测的字符串</param>
        /// <param name="checkstring">是否包含这个字符串中的字符</param>
        /// <returns></returns>
        private static bool HasChar(string sourcestring, string checkstring)
        {
            foreach (var item in checkstring)
            {
                if (sourcestring.Contains(item)) return true;
            }
            return false;
        }

        /// <summary>
        /// 返回一个32位guid字符串,字母小写
        /// </summary>
        /// <returns></returns>
        public static string NewGuid()
        {
            return Guid.NewGuid().ToString("N");
        }
    }

}
