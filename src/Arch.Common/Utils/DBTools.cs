using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.OleDb;
//using System.Data.SqlClient;

namespace Arch.Common.Utils
{

    /// <summary>
    ///  通过DB获取数据的工具类
    /// </summary>
    public class DBTools
    {

        public static string[] GetSeqNo(string type, int count, string headPrefix, string footPrefix)
        {
            string[] temp = new string[count];
            string[] returnTemp = GetSeqNo(type, count, 6);
            if (!string.IsNullOrEmpty(headPrefix) && !string.IsNullOrEmpty(footPrefix))
            {
                for (var i = 0; i < returnTemp.Length; i++)
                {
                    temp[i] = headPrefix + returnTemp[i] + footPrefix;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(headPrefix) && string.IsNullOrEmpty(footPrefix))
                {
                    for (var i = 0; i < returnTemp.Length; i++)
                    {
                        temp[i] = headPrefix + returnTemp[i];
                    }
                }
                else if (string.IsNullOrEmpty(headPrefix) && !string.IsNullOrEmpty(footPrefix))
                {
                    for (var i = 0; i < returnTemp.Length; i++)
                    {
                        temp[i] = returnTemp[i] + footPrefix;
                    }
                }
            }
            return temp;
        }


        public static string[] GetSeqNo(string type, int count, int zerofix = 6)
        {
            DateTime cal = DateTime.Now;
            int year = cal.Year;
            int month = cal.Month;

            string sYear = year.ToString();
            string sMonth = month.ToString();

            if (sMonth.Length == 1)
            {
                sMonth = "0" + sMonth;
            }

            string sql = "SELECT * FROM SysSequence WHERE Type='{0}' AND Year='{1}' AND Month='{2}'".With(type, sYear, sMonth);


            int currentCnt = 0;
            int endCnt = 0;
            string opSql = "";
            using (MySqlConnection conn = new MySqlConnection(MyHelper.connectionString))
            {
                conn.Open();
                using (MySqlDataReader dr = MySqlHelper.ExecuteReader(conn, sql))
                {
                    if (dr.Read())
                    {
                        currentCnt = Int32.Parse(dr["SeqNo"].ToString());
                        endCnt = currentCnt + count;
                        opSql = "UPDATE SysSequence SET SeqNo={0} WHERE Type='{1}' AND Year='{2}' AND Month='{3}' ".With(endCnt, type, sYear, sMonth);
                    }
                    else
                    {
                        opSql = "INSERT INTO SysSequence (Type,Year,Month,SeqNo ) VALUES ('{0}','{1}','{2}',{3})".With(type, sYear, sMonth, count);
                    }
                }

                MySqlHelper.ExecuteNonQuery(conn, opSql);
                conn.Close();
            }


            sYear = sYear.Substring(2, 2);
            string[] cnos = new string[count];


            for (int i = 0; i < count; i++)
            {
                currentCnt++;
                string computerNo = "";

                computerNo = sYear + sMonth + currentCnt.ToString().PadLeft(6, '0');

                cnos[i] = computerNo;
            }

            return cnos;
        }

        /// <summary>
        ///  取序列号
        /// </summary>
        /// <param name="type">表名</param>
        /// <param name="zerofix"></param>
        /// <returns></returns>
        public static string GetSeqNo(string type, int zerofix = 6)
        {
            return GetSeqNo(type, 1, zerofix)[0];
        }
        public static string GetSeqNo(string type, string headPrefix, string footPrefix = "")
        {
            return GetSeqNo(type, 1, headPrefix, footPrefix)[0];
        }


        /// <summary>
        /// 获取系统时间
        /// </summary>
        /// <returns></returns>
        public static DateTime GetSysDate()
        {
            return Convertor.ToDateTime(MyHelper.GetSingle("select now() "));
        }


        #region 取序列号

        #region 变量属性

        private const string BasicNumber = "0123456789";
        private const string Base36String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string BasicString = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFG";

        #endregion

        #region 产生随机数
        /// <summary>
        /// 随机数种子
        /// </summary>
        private static readonly System.Random Random = new Random();

        /// <summary>
        /// 获得随机数种子
        /// </summary>
        /// <returns></returns>
        private static int GetNormalRandom()
        {
            return Random.Next();
        }



        /// <summary>
        /// 取值范围内随机数
        /// </summary>
        /// <returns></returns>
        public static int GetMaxRandom(int max)
        {
            return Random.Next(max);
        }

        /// <summary>
        /// 取值范围内随机数
        /// </summary>
        /// <returns></returns>
        public static int GetRangeRandom(int min, int max)
        {
            return Random.Next(min, max);
        }
        #endregion

        #region 进制转化
        /// <summary>
        /// 将十进制整数转化为36进制整数
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string DemTo36(int number)
        {
            try
            {
                string Value = "";
                int Len = Base36String.Length, Mod;
                while (number >= Base36String.Length)
                {
                    Mod = number % Base36String.Length;
                    Value = Base36String.Substring(Mod, 1) + Value;
                    number = (number - Mod) / Len;
                }
                Value = Base36String.Substring(number, 1) + Value;
                return Value;
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// 将十进制整数转化为36进制整数
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string DemTo36(string number)
        {
            if (number == null) throw new ArgumentNullException("Number");
            try
            {
                int tempNumber = Convert.ToInt32(number);
                return DemTo36(tempNumber);
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        #endregion

        #region 随机字符串

        /// <summary>
        /// 根据源字符串随机生成字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="baseString"></param>
        /// <returns></returns>
        private static string ExportRandomString(int length, string baseString)
        {
            int max = baseString.Length * 1000;
            string randomString = "";
            for (int i = 0; i < length; i++)
                randomString += baseString.Substring(GetMaxRandom(max) % baseString.Length, 1);

            return randomString;
        }

        /// <summary>
        /// 随机数字
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomNumber(int length)
        {
            return ExportRandomString(length, BasicNumber);
        }

        /// <summary>
        /// 随机编号
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomCode(int length)
        {
            return ExportRandomString(length, Base36String);
        }

        /// <summary>
        /// 随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            return ExportRandomString(length, BasicString);
        }

        /// <summary>
        /// 根据时间产生随机字符串
        /// </summary>
        /// <param name="Random"></param>
        /// <returns></returns>
        public static string GetSeq(int Random = 3)
        {
            DateTime Time = DateTime.Now;
            string serialCode = "";
            serialCode += DemTo36(Time.ToString("yy"));
            serialCode += DemTo36(Time.Month);
            serialCode += DemTo36(Time.Day);

            string second = "000" + DemTo36(Time.Hour * 3600 + Time.Minute * 60 + Time.Second);
            serialCode += second.Substring(second.Length - 4);

            string millSecond = "0" + DemTo36(Time.Millisecond);
            serialCode += millSecond.Substring(millSecond.Length - 2);

            string randomCode = "";
            for (int i = 0; i < Random; i++)
                randomCode += DemTo36(GetMaxRandom(36));

            return serialCode + randomCode;
        }
        #endregion

        #endregion


        /// <summary>
        ///  得到年月日 8位 YYYYMMDD
        /// </summary>
        /// <param name="headPrefix"></param>
        /// <param name="footPrefix"></param>
        /// <returns></returns>
        public static string GetYearMDSeqNo(string headPrefix, string footPrefix)
        {
            string seqNos = null;
            DateTime cal = DateTime.Now;
            int year = cal.Year;
            int month = cal.Month;
            int day = cal.Day;
            string sYear = year.ToString();
            string sMonth = month.ToString();
            string sDay = day.ToString();

            if (sMonth.Length == 1)
            {
                sMonth = "0" + sMonth;
            }
            if (sDay.Length == 1)
            {
                sDay = "0" + sDay;
            }
            seqNos = sYear + sMonth + sDay;
            if (!headPrefix.IsNullOrEmpty())
                seqNos = headPrefix + seqNos;
            if (!footPrefix.IsNullOrEmpty())
                seqNos = seqNos + footPrefix;
            return seqNos;
        }

        /// <summary>
        /// 线路产品采番
        /// </summary>
        /// <returns></returns>
        public static string GetLineSeqNo()
        {
            return GetProductSeqNoByVisaInfo("01", 6);
        }
        /// <summary>
        /// 门票产品采番
        /// </summary>
        /// <returns></returns>
        public static string GetTicketSeqNo()
        {
            return GetProductSeqNoByVisaInfo("02", 5);
        }
        /// <summary>
        /// 签证产品采番
        /// </summary>
        /// <returns></returns>
        public static string GetProductSeqNoByVisaInfo()
        {
            return GetProductSeqNoByVisaInfo("03", 4);
        }
        /// <summary>
        /// 酒店产品采番号
        /// </summary>
        /// <returns></returns>
        public static string GetHotelSeqNo()
        {
            return GetProductSeqNoByVisaInfo("05", 4);
        }
        /// <summary>
        /// 得到产品编码4位以 0001 开始累计
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetProductSeqNoByVisaInfo(string type, int chatLen)
        {
            string sql = "SELECT * FROM SysSequence WHERE Type='{0}'".With(type);

            int currentCnt = 1;
            string opSql = "";
            using (MySqlConnection conn = new MySqlConnection(MyHelper.connectionString))
            {
                conn.Open();
                using (MySqlDataReader dr = MySqlHelper.ExecuteReader(conn, sql))
                {
                    if (dr.Read())
                    {
                        currentCnt = Int32.Parse(dr["SeqNo"].ToString());
                        currentCnt = currentCnt + 1;
                        opSql = "UPDATE SysSequence SET SeqNo={0} WHERE Type='{1}'  ".With(currentCnt, type);
                    }
                    else
                    {
                        opSql = "INSERT INTO SysSequence (Type,SeqNo ) VALUES ('{0}',{1})".With(type, 1);
                    }
                }

                MySqlHelper.ExecuteNonQuery(conn, opSql);
            }

            return currentCnt.ToString().PadLeft(chatLen, '0');
        }

    }

}
