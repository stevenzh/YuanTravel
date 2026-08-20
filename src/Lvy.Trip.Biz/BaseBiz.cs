using PetaPoco;
using System.Configuration;

namespace Lvy.Trip.Biz
{
    public class BaseBiz
    {

        /// <summary>
        /// 交通类型为巴士的值（常量）
        /// </summary>
        public const int BusInTrafficType = 1;

        /// <summary>
        ///  varchar 类型的字符串需要调用该方法
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public AnsiString Ansi(string field)
        {
            return new AnsiString(field);
        }

        public AnsiString AnsiLike(string field)
        {
            return new AnsiString("%" + field + "%");
        }

        public AnsiString AnsiLeftLike(string field)
        {
            return new AnsiString(field + "%");
        }
    }
}