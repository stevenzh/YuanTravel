using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing.Drawing2D;
using System.Web;

namespace Arch.Common
{
    public class ValidateCode
    {
        /// <summary>
        ///  生成验证码
        /// </summary>
        /// <returns></returns>
        public static string CreateCheckCodeString()
        {
            char[] allCharArray = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };   //定义用于验证码的字符数组
            string randomCode = "";//定义验证码字符串
            Random rand = new Random();
            for (int i = 0; i < 4; i++)
                randomCode += allCharArray[rand.Next(allCharArray.Length)];
            return randomCode;//生成四个字符
        }
        /// <summary>
        /// 生成五位随机数
        /// </summary>
        /// <returns></returns>
        public static string CreateCodeString()
        {
            char[] allCharArray = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };   //定义用于验证码的字符数组
            string randomCode = "";//定义验证码字符串
            Random rand = new Random();
            for (int i = 0; i < 5; i++)
                randomCode += allCharArray[rand.Next(allCharArray.Length)];
            return randomCode;//生成四个字符
        }

        /// <summary>
        /// 生成6位随机数
        /// </summary>
        /// <returns></returns>
        public static string CreateCodeStringSix()
        {
            char[] allCharArray = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };   //定义用于验证码的字符数组
            string randomCode = "";//定义验证码字符串
            Random rand = new Random();
            for (int i = 0; i < 6; i++)
                randomCode += allCharArray[rand.Next(allCharArray.Length)];
            return randomCode;//生成四个字符
        }

        /// <summary>
        ///  生成验证码图片
        /// </summary>
        /// <param name="checkCode"></param>
        /// <returns></returns>
        public static byte[] CreateCheckCodeImages(String checkCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling((checkCode.Length * 16.5)), 30);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器				
                Random random = new Random();
                //清空图片背景色			
                g.Clear(Color.White);
                //画图片的背景噪音线				
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 14, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Blue, Color.DarkRed, 1.2f, true);

                g.DrawString(checkCode, font, brush, 4, 4);
                //画图片的前景噪音点				
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线				
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                MemoryStream ms = new MemoryStream();
                image.Save(ms, ImageFormat.Gif);

                //输出图片流
                return ms.ToArray();

            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }

        /// <summary>
        /// 生成随机验证码
        /// </summary>
        /// <returns></returns>
        public static int GetRandomPwd()
        {
            int iSeed = DateTime.Now.DayOfYear + DateTime.Now.Millisecond + DateTime.Now.Second;
            Random ro = new Random(iSeed);
            int iResult = ro.Next(100000, 999999);
            return iResult;
        }

        /// <summary>
        /// 生成随机验证码
        /// </summary>
        /// <returns></returns>
        public static int GetRandomPwd(int rand)
        {
            int iSeed = DateTime.Now.DayOfYear + DateTime.Now.Millisecond + DateTime.Now.Second + rand;
            Random ro = new Random(iSeed);
            int iResult = ro.Next(100000, 999999);
            return iResult;
        }


        /// <summary>
        /// 生成8位随机数 
        /// </summary>
        /// <returns></returns>
        public static string CreateCodeStringSix(int count)
        {
            int intSum = 8;
            if (count != 0)
                intSum = count;
            char[] allCharArray = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };   //定义用于验证码的字符数组
            string randomCode = "";//定义验证码字符串
            Random rand = new Random();
            for (int i = 0; i < intSum; i++)
                randomCode += allCharArray[rand.Next(allCharArray.Length)];
            return randomCode;//生成四个字符
        }



        #region 其它函数 

        /// <summary>
        /// 将图片直接输出到页面上
        /// </summary>
        /// <param name="oImg"></param>
        private static byte[] WriteImage(Bitmap oImg)
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                oImg.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //输出图片流
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 清除过期图片(一分钟前)
        /// </summary>
        private static void ClearImage()
        {
            //先取得chart文件夹中的文件列表 
            string[] fileEntries = System.IO.Directory.GetFiles(HttpContext.Current.Request.PhysicalApplicationPath + "\\Pic");
            //遍历文件列表 
            foreach (string sFile in fileEntries)
            {
                //将文件的生成日期与系统日期相比，如果是当日以前生成的文件，删除它 
                if (DateTime.Compare(System.IO.File.GetCreationTime(sFile).AddMinutes(1), DateTime.Now) < 0)
                {
                    System.IO.File.Delete(sFile);
                }
            }
        }

        #endregion
        #region 随机中文汉字验证码
        /// <summary>
        /// 获取随机中文汉字验证码图片
        /// </summary>
        /// <param name="Count"></param>
        /// <returns></returns>
        public static byte[] GetChsChkCodeImage(string ChsChkCode)
        {
            Int32 ccLen = ChsChkCode.Length;
            String ccFtFm = "宋体";
            Int32 ccFtSz = 12;
            Int32 ccWidth = ccLen * (ccFtSz + 8);
            Int32 ccHeight = ccFtSz + 10;
            using (Bitmap oImg = new Bitmap(ccWidth, ccHeight))
            {
                using (Graphics oGpc = Graphics.FromImage(oImg))
                {
                    HatchBrush hBrush = new HatchBrush(HatchStyle.DashedVertical,
                     Color.Yellow, Color.LightPink);
                    oGpc.FillRectangle(hBrush, 0, 0, ccWidth, ccWidth);
                    oGpc.DrawString(ChsChkCode, new System.Drawing.Font(ccFtFm, ccFtSz, FontStyle.Bold),
                     new System.Drawing.SolidBrush(Color.Green), 4, 2);

                    //边框   
                    Pen blackPen = new Pen(Color.LightPink, 1);
                    oGpc.DrawLine(blackPen, 0, ccHeight, 0, 0); // 左竖线   
                    oGpc.DrawLine(blackPen, 0, 0, ccWidth, 0); // 顶横线   
                    oGpc.DrawLine(blackPen, ccWidth - 1, 0, ccWidth - 1, 20); // 右竖线   
                    oGpc.DrawLine(blackPen, 0, ccHeight - 1, ccWidth, ccHeight - 1); // 底横线   

                    return WriteImage(oImg);
                }
            }
        }

        /// <summary>
        /// 生成随机中文汉字
        /// </summary>
        /// <param name="Count">个数</param>
        /// <returns></returns>
        public static string GetChineseString(int Count)
        {
            //获取GB2312编码页（表） 
            Encoding gb = Encoding.GetEncoding("gb2312");      //调用函数产生Count个随机中文汉字编码 
            object[] bytes = CreateRegionCode(Count);       //根据汉字编码的字节数组解码出中文汉字 
            string Result = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                Result += gb.GetString((byte[])Convert.ChangeType(bytes[i], typeof(byte[])));
            }   
            //返回结果
            return Result;
        }
        /* 
     此函数在汉字编码范围内随机创建含两个元素的十六进制字节数组，每个字节数组代表一个汉字，并将 
     四个字节数组存储在object数组中。 
     参数：strlength，代表需要产生的汉字个数 
    */
        private static object[] CreateRegionCode(int strlength)
        {
            //定义一个字符串数组储存汉字编码的组成元素 
            string[] rBase = new String[16] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f" };

            Random rnd = new Random();

            //定义一个object数组用来 
            object[] bytes = new object[strlength];
            /*每循环一次产生一个含两个元素的十六进制字节数组，并将其放入bject数组中 
       每个汉字有四个区位码组成 
       区位码第1位和区位码第2位作为字节数组第一个元素 
       区位码第3位和区位码第4位作为字节数组第二个元素 
     */
            for (int i = 0; i < strlength; i++)
            {
                //区位码第1位 
                int r1 = rnd.Next(11, 14);
                string str_r1 = rBase[r1].Trim();       //区位码第2位 
                rnd = new Random(r1 * unchecked((int)DateTime.Now.Ticks) + i);//更换随机数发生器的 种子避免产生重复值 
                int r2;
                if (r1 == 13)
                {
                    r2 = rnd.Next(0, 7);
                }
                else
                {
                    r2 = rnd.Next(0, 16);
                }
                string str_r2 = rBase[r2].Trim();       //区位码第3位 
                rnd = new Random(r2 * unchecked((int)DateTime.Now.Ticks) + i);
                int r3 = rnd.Next(10, 16);
                string str_r3 = rBase[r3].Trim();       //区位码第4位 
                rnd = new Random(r3 * unchecked((int)DateTime.Now.Ticks) + i);
                int r4;
                if (r3 == 10)
                {
                    r4 = rnd.Next(1, 16);
                }
                else if (r3 == 15)
                {
                    r4 = rnd.Next(0, 15);
                }
                else
                {
                    r4 = rnd.Next(0, 16);
                }
                string str_r4 = rBase[r4].Trim();       //定义两个字节变量存储产生的随机汉字区位码 
                byte byte1 = Convert.ToByte(str_r1 + str_r2, 16);
                byte byte2 = Convert.ToByte(str_r3 + str_r4, 16);
                //将两个字节变量存储在字节数组中 
                byte[] str_r = new byte[] { byte1, byte2 };       //将产生的一个汉字的字节数组放入object数组中 
                bytes.SetValue(str_r, i);

            }
            return bytes;
        }

        #endregion
    }
}
