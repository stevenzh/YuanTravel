using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    ///  数组扩展
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        ///  动态数组
        /// </summary>
        /// <param name="array">数组名称</param>
        /// <param name="len">数组长度</param>
        /// <returns></returns>
        public static string[] ReNew(this string[] array, int len)
        {
            string[] temp = new string[len];
            array.CopyTo(temp, 0);
            return temp;
        }

        /// <summary>
        ///  追加一组数据
        /// </summary>
        /// <param name="array">原数组</param>
        /// <param name="values">追加的数组</param>
        /// <returns></returns>
        public static string[] Add(this string[] array, string[] values)
        {
            array = array.ReNew(array.Length + values.Length);
            values.CopyTo(array, 0);
            return array;
        }
        public static string[] Add(this string[] array, string value)
        {
            string[] temp = new string[0];
            temp.ReNew(temp.Length + value.Length);
            temp.SetValue(value, temp.Length);
            return temp;
        }

    }
}
