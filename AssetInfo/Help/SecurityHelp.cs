using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AssetInfo.Help
{
    public class SecurityHelp
    {
        /// <summary>
        /// MD5摘要,返回32位小写字符串
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StringMd5(string s)
        {
            MD5 md5 = MD5.Create();
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            byte[] data = md5.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 基于Sha1摘要.输入一个字符串,返回一个由40个字符组成的十六进制的哈希散列
        /// </summary>
        /// <param name="str">要加密的字符串</param>
        /// <returns>加密后的十六进制的哈希散列（字符串）</returns>
        public static string StringSHA1(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
        public static string StringSHA256(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA256Managed.Create().ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in data)
            {
                sb.Append(t.ToString("x2"));
            }
            return sb.ToString();
        }
        #region AES
        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="strKey">密钥:16位</param>
        /// <returns>返回加密后的密文字节数组</returns>
        public static byte[] AESEncryptToBytes(string plainText, string strKey)
        {
            //默认密钥向量 
            byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                //分组加密算法
                SymmetricAlgorithm des = Rijndael.Create();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
                //设置密钥及密钥向量
                des.Key = Encoding.UTF8.GetBytes(strKey);
                des.IV = _key1;
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                byte[] cipherBytes = ms.ToArray();//得到加密后的字节数组
                cs.Close();
                ms.Close();
                return cipherBytes;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字节数组</param>
        /// <param name="strKey">密钥:16位</param>
        /// <returns>返回解密后的字符串</returns>
        public static byte[] AESDecryptToBytes(byte[] cipherText, string strKey)
        {
            //默认密钥向量 
            byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                SymmetricAlgorithm des = Rijndael.Create();
                des.Key = Encoding.UTF8.GetBytes(strKey);
                des.IV = _key1;
                byte[] decryptBytes = new byte[cipherText.Length];
                MemoryStream ms = new MemoryStream(cipherText);
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
                cs.Read(decryptBytes, 0, decryptBytes.Length);
                cs.Close();
                ms.Close();
                return decryptBytes;
            }
            catch
            {
                return null;
            }
        }


        /// <summary>
        /// AES加密算法,返回加密后的16进制字符串值.发生异常时返回null
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <param name="strKey">密钥:16位长度</param>
        /// <returns>返回加密后的16进制字符串值</returns>
        public static string AESEncrypt(string plainText, string strKey)
        {
            //默认密钥向量 
            byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                //分组加密算法
                SymmetricAlgorithm des = Rijndael.Create();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
                //设置密钥及密钥向量
                des.Key = Encoding.UTF8.GetBytes(strKey);
                des.IV = _key1;
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                byte[] cipherBytes = ms.ToArray();//得到加密后的字节数组
                cs.Close();
                ms.Close();
                StringBuilder sb = new StringBuilder();
                foreach (byte item in cipherBytes)
                {
                    sb.Append(String.Format("{0:x2}", item));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// AES解密,返回解密后的字符串.发生异常时返回null
        /// </summary>
        /// <param name="cipherText">密文16进制字符串字节数组</param>
        /// <param name="strKey">密钥:16位长度</param>
        /// <returns>返回解密后的字符串</returns>
        public static string AESDecrypt(string cipherText, string strKey)
        {
            //默认密钥向量 
            byte[] _key1 = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                byte[] cipherByteAry = new byte[cipherText.Length / 2];
                for (int i = 0; i < cipherByteAry.Length; i++)
                {
                    cipherByteAry[i] = System.Convert.ToByte(cipherText.Substring(i * 2, 2), 16);
                }
                
                SymmetricAlgorithm des = Rijndael.Create();
                des.Key = Encoding.UTF8.GetBytes(strKey);
                des.IV = _key1;
                byte[] decryptBytes = new byte[cipherByteAry.Length];
                MemoryStream ms = new MemoryStream(cipherByteAry);
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read);
                cs.Read(decryptBytes, 0, decryptBytes.Length);
                cs.Close();
                ms.Close();
                return Encoding.UTF8.GetString(decryptBytes).TrimEnd('\0');
            }
            catch
            {
                return null;
            }
        }
        #endregion

        /// <summary>
        /// 字节转16进制字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string Bytes2HexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var d in data)
            {
                sb.Append(d.ToString("x02"));
            }
            return sb.ToString();
        }
        /// <summary>
        /// 16进度字符串,转为字节
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] HexString2Bytes(string str)
        {
            if (str.IndexOf("0x") == 0)
                str = str.Substring(2);
            byte[] outd = new byte[str.Length / 2];
            for (var i = 0; i < str.Length / 2; i++)
            {
                outd[i] = byte.Parse(str.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return outd;
        }
    }
   
}
