using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AssetInfo.Help
{
    class ValidHelp
    {
        /// <summary>
        /// 指示一个字符串是否为数值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNumber(string str)
        {
            string rege = @"^(?:-?\d+|-?\d{1,3}(?:,\d{3})+)?(?:\.\d+)?$";
            return Regex.IsMatch(str, rege);
        }

        /// <summary>
        /// 指示一个字符串是否为email地址
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsEmail(string str)
        {
            string rege = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
            return Regex.IsMatch(str, rege);
        }
        /// <summary>
        /// 指示一个字符串是否为国内11位手机号
        /// [可匹配"(+86)013800138000",()号可以省略，+号可以省略，(+86)可以省略,11位手机号前的0可以省略;11位手机号第二位数可以是3~9中的任意一个]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsMobile(string str)
        {
            string rege = @"^(\((\+)?86\)|((\+)?86)?)0?1[^012]\d{9}$";
            return Regex.IsMatch(str, rege);
        }
        /// <summary>
        /// 指示一个字符串是否为26个英文字母组成,大小写不限.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAbc(string str)
        {
            string rege = @"[^a-zA-Z]";
            return !Regex.IsMatch(str, rege);
        }
        /// <summary>
        /// 指示一个字符串是否为0-9整数组成
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsDigit(string str)
        {
            string rege = @"^\d+$";
            return Regex.IsMatch(str, rege);
        }
        /// <summary>
        /// 指示一个字符串是否为26个英文字母和0-9整数(可选)组成,但必须是字母开头.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAbcDigit(string str)
        {
            string rege = @"^[a-zA-Z][a-zA-Z\d]*$";
            return Regex.IsMatch(str, rege);
        }

        /// <summary>
        /// 指示一个字符串是否为26个英文字母和0-9整数(可选)和_下划线(可选)组成,并且是字母或者下划线开头.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsAbcDigitUline(string str)
        {
            string rege = @"^[a-zA-Z_][a-zA-Z\d_]*$";
            return Regex.IsMatch(str, rege);
        }

        /// <summary>
        /// 指示一个字符串是否为url
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsUrl(string str)
        {
            string rege = @"^(?:(?:(?:https?|ftp):)?\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)(?:\.(?:[a-z\u00a1-\uffff0-9]-*)*[a-z\u00a1-\uffff0-9]+)*(?:\.(?:[a-z\u00a1-\uffff]{2,})).?)(?::\d{2,5})?(?:[/?#]\S*)?$";
            return Regex.IsMatch(str, rege);
        }

        public static bool IsIpv4(string str)
        {
            string rege = @"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            return Regex.IsMatch(str, rege);
        }

        /// <summary>
        /// 指示一个字符串是否为1~3位小数,或者正数 (d | d.d | d.dd | d.ddd),可用于金额
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsMoney(string str)
        {
            string rege = @"^[0-9]+([.]{1}[0-9]{1,3})?$";
            return Regex.IsMatch(str, rege);
        }
    }
}
