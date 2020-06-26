using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AssetInfo.Help
{
    #region 验证特性

    /// <summary>
    /// 验证特性基类
    /// </summary>
    public class ValidBaseAttribute : Attribute
    {
        /// <summary>
        /// 自定义错误信息.可以不填
        /// </summary>
        public string errMsg;

        /// <summary>
        /// 字符串是否为null或者''
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public bool HasValue(object val)
        {
            return val == null || val.ToString() == string.Empty;
        }
        public virtual string Check(object val)
        {
            return null;
        }
    }
    /// <summary>
    /// 必填
    /// </summary>
    public class VNotNullAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (val == null || string.IsNullOrWhiteSpace(val.ToString()))
                return "不能为空或空白字符";
            return null;
        }
    }

    /// <summary>
    /// Email
    /// </summary>
    public class VEmailAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsEmail(val.ToString()) == false)
                return "无效的Email";
            return null;
        }
    }

    /// <summary>
    /// 国内11位手机号
    /// [可匹配"(+86)013800138000",()号可以省略，+号可以省略，(+86)可以省略, 11位手机号前的0可以省略; 11位手机号第二位数可以是3~9中的任意一个]
    /// </summary>
    public class VMobileAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsMobile(val.ToString()) == false)
                return "无效的手机号码";
            return null;
        }
    }

    /// <summary>
    /// 限26个英文,大小写不限.
    /// </summary>
    public class VAbcAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsAbc(val.ToString()) == false)
                return "限26个英文,大小写不限";
            return null;
        }
    }

    /// <summary>
    /// 限0-9数字
    /// </summary>
    public class VDigitAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsDigit(val.ToString()) == false)
                return "限0-9数字";
            return null;
        }
    }

    /// <summary>
    /// 限26个英文字母(开头)和0-9整数(可选)
    /// </summary>
    public class VAbcDigitAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsAbcDigit(val.ToString()) == false)
                return "限26个英文字母(开头)和0-9整数(可选)";
            return null;
        }
    }

    /// <summary>
    /// 限26个英文字母和0-9整数(可选)和_下划线(可选),并且是字母或者下划线开头. 
    /// </summary>
    public class VAbcDigitUlineAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsAbcDigitUline(val.ToString()) == false)
                return "限26个英文字母和0-9整数(可选)和_下划线(可选),并且是字母或者下划线开头. ";
            return null;
        }
    }

    /// <summary>
    /// 限url
    /// </summary>
    public class VUrlAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsUrl(val.ToString()) == false)
                return "无效的Url";
            return null;
        }
    }

    /// <summary>
    /// 限Ipv4
    /// </summary>
    public class VIpv4Attribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsIpv4(val.ToString()) == false)
                return "无效的Ipv4";
            return null;
        }
    }

    /// <summary>
    /// 能转为DateTimeOffSet格式的字符串
    /// </summary>
    public class VDateAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            bool isok = DateTimeOffset.TryParse(val.ToString(), out DateTimeOffset d);
            if (isok == false)
                return "无效的日期格式";
            return null;
        }
    }

    /// <summary>
    /// 字符串长度限制
    /// </summary>
    public class VStringLengthAttribute : ValidBaseAttribute
    {
        /// <summary>
        /// 最大长度
        /// </summary>
        public int maxlen;
        /// <summary>
        /// 最小长度
        /// </summary>
        public int minlen;
        /// <summary>
        /// 等于长度
        /// </summary>
        public int len;

        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (maxlen > 0)
            {
                if (val.ToString().Length > maxlen)
                    return $"长度超过 {maxlen} 个字";
            }
            if (minlen > 0)
            {
                if (val.ToString().Length < minlen)
                    return $"长度少于 {minlen} 个字";
            }
            if (len > 0)
            {
                if (val.ToString().Length != len)
                    return $"长度必须 {len} 个字";
            }
            return null;
        }
    }

    /// <summary>
    /// 金额2位小数,可以负数
    /// </summary>
    public class VMoneyAttribute : ValidBaseAttribute
    {
        public override string Check(object val)
        {
            if (HasValue(val))
                return null;
            if (ValidHelp.IsMoney(val.ToString()))
                return $"数值必须是整数或带1-2位小数";
            return null;
        }
    }
    #endregion

    public class Validate
    {
        /// <summary>
        /// 验证实体类成员或者属性的值.成功返回null,失败返回错误信息.
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="names">验证指定成员或者属性名字</param>
        /// <returns></returns>
        public static string CheckEntity<T>(T entity, params string[] names)
        {
            // 获取field或者prop的值
            object getVal(MemberInfo member)
            {
                if (member is FieldInfo)
                    return ((FieldInfo)member).GetValue(entity);
                else
                    return ((PropertyInfo)member).GetValue(entity);
            }

            // 找出entity所有field和prop
            Type t = typeof(T);
            List<MemberInfo> members = new List<MemberInfo>();
            members.AddRange(t.GetProperties());
            members.AddRange(t.GetFields());

            // 验证 fields,props
            foreach (var item in members)
            {
                // 是否要验证
                if (IsToValid(item, names) == false)
                    continue;
                // 要验证的值
                object val = getVal(item);
                // 验证
                string res = ToValid(item, val);
                if (res == null)
                    continue;
                return res;
            }
            // 全部通过
            return null;
        }
        /// <summary>
        /// 判断field或者prop是否需要验证
        /// </summary>
        /// <param name="member"></param>
        /// <param name="names">需要验证的field或者prop名字</param>
        /// <returns></returns>
        private static bool IsToValid(MemberInfo member, string[] names)
        {
            // 是否贴有特性,没贴不验证.(用特性基类判断,贴子类也有效)
            bool hasValidAttr = Attribute.IsDefined(member, typeof(ValidBaseAttribute));
            if (hasValidAttr == false)
                return false;

            // 有特性
            // 1. names为空,验证
            if (names.Length == 0) return true;

            // 2. 在names里,验证
            if (names.Contains(member.Name))
                return true;
            
            // 3.不在names里,不验证
            return false;
        }

        /// <summary>
        /// 根据特性,验证实体类的field或者prop,成功返回null,失败返回出错信息
        /// </summary>
        /// <param name="member">field或者prop对象</param>
        /// <param name="value">field或者prop的值</param>
        /// <returns></returns>
        private static string ToValid(MemberInfo member, object value)
        {
            // 取出field,prop上的特性
            var vattrs = member.GetCustomAttributes<ValidBaseAttribute>();
            foreach (var item in vattrs)
            {
                string res = item.Check(value);
                if (res == null) continue;
                // 自定义信息优先
                string emsg = string.IsNullOrWhiteSpace(item.errMsg) ? res : item.errMsg;
                return $"{member.Name}: {emsg}";
            }
            return null;
        }

    }
}
