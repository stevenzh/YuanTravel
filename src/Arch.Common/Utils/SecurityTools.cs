using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Arch.Common.Utils
{
    /// <summary>
    ///  加密解密工具类
    /// </summary>
    public class SecurityTools
    {
        private string EncryptKey
        {
            get
            {
                var key = ConfigurationManager.AppSettings["EncryptKey"];
                if (key.IsNullOrEmpty())
                    return "songguosoft";
                else
                    return key;
            }
        }

        #region DES加密/解密

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string ToEncrypt(string text)
        {
            return ToEncrypt(text, EncryptKey);
        }

        /// <summary>
        /// 加密数据
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public string ToEncrypt(string text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(text);
            des.Key = ASCIIEncoding.ASCII.GetBytes(ToMD5Encrypt1(sKey).Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(ToMD5Encrypt1(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string ToDecrypt(string text)
        {
            return ToDecrypt(text, EncryptKey);
        }

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public string ToDecrypt(string text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            len = text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(ToMD5Encrypt1(sKey).Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(ToMD5Encrypt1(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion DES加密/解密

        #region Hash加密

        /// <summary>
        /// 得到随机哈希加密字符串
        /// 得到随机安全码
        /// </summary>
        /// <returns></returns>
        public string GetSecurity()
        {
            string Security = HashEncoding(GetRandomValue());
            return Security;
        }

        /// <summary>
        /// 得到一个随机数值
        /// </summary>
        /// <returns></returns>
        public string GetRandomValue()
        {
            Random Seed = new Random();
            string RandomVaule = Seed.Next(1, int.MaxValue).ToString();
            return RandomVaule;
        }

        /// <summary>
        /// 哈希加密一个字符串
        /// </summary>
        /// <param name="security"></param>
        /// <returns></returns>
        public string HashEncoding(string security)
        {
            byte[] Value;
            UnicodeEncoding Code = new UnicodeEncoding();
            byte[] Message = Code.GetBytes(security);
            SHA512Managed Arithmetic = new SHA512Managed();
            Value = Arithmetic.ComputeHash(Message);
            security = "";
            foreach (byte o in Value)
            {
                security += (int)o + "O";
            }
            return security;
        }

        #endregion Hash加密

        #region MD5加密

        /// <summary>
        /// MD5加密方法
        /// </summary>
        /// <param name="myString">要加密的字符串</param>
        /// <returns>加密后的字符串</returns>
        public string ToMD5Encrypt(string myString)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.Unicode.GetBytes(myString);
            byte[] targetData = md5.ComputeHash(fromData);
            return BitConverter.ToString(targetData).Replace("-", "");
        }

        /// <summary>
        /// 用于替换原有 FormsAuthentication.HashPasswordForStoringInConfigFile
        /// </summary>
        /// <param name="myString"></param>
        /// <returns></returns>
        public string ToMD5Encrypt1(string myString)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(myString));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "").ToUpper();
            }
        }

        #endregion MD5加密
    }
}