using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Web;

namespace Arch.Common.Utils
{
    /// <summary>
    /// 从UCS-2到UTF-8的编码方式如下：
    /// UCS-2编码(16进制)	UTF-8 字节流(二进制) 
    ///	0000 - 007F			0xxxxxxx 
    ///	0080 - 07FF			110xxxxx 10xxxxxx 
    ///	0800 - FFFF			1110xxxx 10xxxxxx 10xxxxxx 
    ///	
    ///	EncryptString/DecryptString 中的变量说明：
    ///	i -	每个整数代表了一个Unicode字符；
    ///	xDouble - 每个字符加密单元，每单元包含两个加密字符。
    ///		Ascii 字符占 1 个单元，Unicode 占 1/2/3 个单元。
    ///	k - 确定该加密单元是“奇型”加密还是“偶型”加密。
    /// </summary>
    public class Crypto
    {
        public Crypto()
        {
            //
            // TODO: 在此处添加构造函数逻辑
            //
        }

        //		public static string EncryptString(string src)
        //		{
        //			int iLen;
        //			string xWCU;
        //			char xChar;
        //			int intChar;
        //			int yintChar;
        //			char yChar;
        //			int xChar1;
        //			int xChar2;
        //			char yChar1;
        //			char yChar2;
        //			int i;
        //			string ret = "";
        //
        //			iLen =  src.Length; 
        //			i = 1;
        //			xWCU = src;
        //			for(i = 1;i <= iLen;i ++)
        //			{
        //				xChar =  char.Parse(xWCU.Substring(i-1,1));
        //				intChar = (int)xChar;
        //				yintChar = intChar;
        //				if(i%2 == 0)
        //				{
        //					yintChar = intChar + 2;
        //				}
        //				else
        //				{
        //					yintChar = intChar + 1;
        //				}
        //				yintChar = yintChar ^ 11;
        //				yChar = (char)yintChar;
        //				
        //				xChar1 = int.Parse(System.Math.Floor(yintChar / 15).ToString());
        //				if(xChar1 >= 10)
        //				{
        //					yChar1 = (char)((int)('A') + xChar1-10);
        //				}
        //				else
        //				{
        //					yChar1 =  char.Parse(xChar1.ToString().Trim());  
        //				}
        //
        //				xChar2 = yintChar % 15;
        //				if(xChar2 >= 10)
        //				{
        //					yChar2 = (char)((int)('A') + xChar2-10);
        //				}
        //				else
        //				{
        //					yChar2 =  char.Parse(xChar2.ToString().Trim());  
        //				}
        //
        //				ret = yChar2.ToString()  + yChar1.ToString()  + ret;
        //			}
        //			
        //			return HttpUtility.UrlEncode(HttpUtility.UrlEncode(ret));
        //			//return HttpUtility.UrlEncodeUnicode(HttpUtility.UrlEncodeUnicode(ret));
        //			
        //		}
        //
        //		public static string DecryptString(string src)
        //		{
        //
        //			int iLen;
        //			string xWCU;
        //			char xChar;
        //			int intChar;
        //			int yintChar;
        //			int xChar1;
        //			int xChar2;
        //			char yChar1;
        //			char yChar2;
        //			int i;
        //			string ret = "";
        //			src = HttpUtility.UrlDecode(HttpUtility.UrlDecode(src));
        //			iLen =  src.Length; 
        //			i = 1;
        //			xWCU = src;
        //			xChar1 = 0;
        //			xChar2 = 0;
        //			for(i = iLen;i > 0;i=i-2)
        //			{
        //				yChar1 =  char.Parse(xWCU.Substring(i-1,1));
        //				yChar2 =  char.Parse(xWCU.Substring(i-2,1));
        //				if(yChar1 >= 'A')
        //				{
        //					xChar1 = 10 + (int)yChar1 - (int)'A';
        //				}
        //				else
        //				{
        //					xChar1 = int.Parse(yChar1.ToString());
        //				}
        //				if(yChar2 >= 'A')
        //				{
        //					xChar2 = 10 + (int)yChar2 - (int)'A';
        //				}
        //				else
        //				{
        //					xChar2 = int.Parse(yChar2.ToString());
        //				} 
        //				yintChar = xChar1 * 15 + xChar2;
        //				yintChar = yintChar ^ 11 ;
        //				if((iLen-i+1) % 4 == 0 || (iLen-i+1) % 4 == 3  )
        //				{
        //					intChar = yintChar - 2;
        //				}
        //				else
        //				{
        //					intChar = yintChar - 1;
        //				}
        //				xChar = (char)intChar;
        //				ret = ret + xChar.ToString(); 
        //			}
        //			return ret;
        //		}

        public static string EnCodeString(string src)
        {
            int k = 0;
            string ret = "";

            char xChar;
            int intChar = 0;
            int ucs = 0;
            int utf = 0;

            for (int i = 1; i <= src.Length; i++)
            {
                k++;
                xChar = char.Parse(src.Substring(i - 1, 1));
                intChar = (int)xChar;
                if (intChar >= 0 && intChar <= 127)							//UCS-2
                {
                    ret = EncryptChar(xChar, k) + ret;
                }
                else if (intChar >= 128 && intChar <= 2047)					//UCS-4
                {
                    ucs = (intChar >> 6) | 192;
                    intChar = (intChar & 63) | 128;
                    ret = EncryptChar((char)(intChar), k + 1) + EncryptChar((char)ucs, k) + ret;
                    k++;
                }
                else														//UTF-8
                {
                    utf = (intChar >> 12) | 224;
                    ucs = (intChar >> 6) & 63 | 128;
                    intChar = (intChar & 63) | 128;
                    ret = EncryptChar((char)intChar, k + 2) + EncryptChar((char)ucs, k + 1) + EncryptChar((char)utf, k) + ret;
                    k = k + 2;
                }
            }

            //return HttpUtility.UrlEncode(ret);
            return ret;
        }

        public static string DeCodeString(string src)
        {
            int k = 0;
            string ret = "";
            string xDouble;

            char xChar;
            byte intChar;
            char xChar2;
            byte intChar2;
            char xChar3;
            byte intChar3;

            //src = HttpUtility.UrlDecode(src);
            if (src == "undefined")
            {
                return "";
            }

            if (src.Length % 2 != 0)
            {
                throw new Exception("Decrypt failed. Data may have been destroyed.");
            }

            for (int i = src.Length; i >= 1; i = i - 2)
            {
                k++;
                xDouble = src.Substring(i - 2, 2);
                xChar = char.Parse(DecryptChar(xDouble, k));
                intChar = (byte)xChar;
                if (intChar >= 0 && intChar <= 127)							//UCS-2
                {
                    ret += xChar;
                }
                else if (intChar >= 192 && intChar <= 223)					//UCS-4		
                {
                    k++;
                    i = i - 2;
                    xDouble = src.Substring(i - 2, 2);
                    xChar2 = char.Parse(DecryptChar(xDouble, k));
                    intChar2 = (byte)xChar2;

                    ret += (char)((intChar & 31) << 6 | intChar2 & 63);
                }
                else														//UTF-8
                {
                    k++;
                    i = i - 2;
                    xDouble = src.Substring(i - 2, 2);
                    xChar2 = char.Parse(DecryptChar(xDouble, k));
                    intChar2 = (byte)xChar2;

                    k++;
                    i = i - 2;
                    xDouble = src.Substring(i - 2, 2);
                    xChar3 = char.Parse(DecryptChar(xDouble, k));
                    intChar3 = (byte)xChar3;

                    ret += (char)((intChar & 15) << 12 | (intChar2 & 63) << 6 | intChar3 & 63);
                }
            }

            return ret;
        }

        private static string EncryptChar(char src, int i)
        {
            char xChar;
            int intChar;
            int yintChar;
            char yChar;
            int xChar1;
            int xChar2;
            char yChar1;
            char yChar2;
            string ret = "";

            xChar = src;
            intChar = (int)xChar;
            yintChar = intChar;
            if (i % 2 == 0)
            {
                yintChar = intChar + 2;
            }
            else
            {
                yintChar = intChar + 1;
            }
            yintChar = yintChar ^ 11;
            yChar = (char)yintChar;

            xChar1 = yintChar / 15;
            if (xChar1 >= 10)
            {
                yChar1 = (char)((int)('A') + xChar1 - 10);
            }
            else
            {
                yChar1 = char.Parse(xChar1.ToString().Trim());
            }

            xChar2 = yintChar % 15;
            if (xChar2 >= 10)
            {
                yChar2 = (char)((int)('A') + xChar2 - 10);
            }
            else
            {
                yChar2 = char.Parse(xChar2.ToString().Trim());
            }

            ret = yChar2.ToString() + yChar1.ToString();

            return ret;
        }

        private static string DecryptChar(string src, int i)
        {
            char xChar;
            int intChar;
            int yintChar;
            int xChar1;
            int xChar2;
            char yChar1;
            char yChar2;
            string ret = "";

            xChar1 = 0;
            xChar2 = 0;

            yChar1 = char.Parse(src.Substring(1, 1));
            yChar2 = char.Parse(src.Substring(0, 1));
            if (yChar1 >= 'A')
            {
                xChar1 = 10 + (int)yChar1 - (int)'A';
            }
            else
            {
                xChar1 = int.Parse(yChar1.ToString());
            }
            if (yChar2 >= 'A')
            {
                xChar2 = 10 + (int)yChar2 - (int)'A';
            }
            else
            {
                xChar2 = int.Parse(yChar2.ToString());
            }
            yintChar = xChar1 * 15 + xChar2;
            yintChar = yintChar ^ 11;

            if (i % 2 == 0)
            {
                intChar = yintChar - 2;
            }
            else
            {
                intChar = yintChar - 1;
            }

            xChar = (char)intChar;
            ret = xChar.ToString();

            return ret;
        }

        public static String Base64Encode(String s)
        {
            byte[] x = Encoding.UTF8.GetBytes(s);
            return Convert.ToBase64String(x);
        }

        public static String Base64Decode(String s)
        {
            byte[] x = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(x);
        }
    }
}

