using System.Collections.Generic;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// 复制数据到另一个TDictionary
        /// </summary>
        /// <param name="source">赋值对象</param>
        /// <param name="copy">copy对象</param>
        /// <returns>赋值后的TDictionary</returns>
        public static TDictionary CopyFrom<TDictionary, TKey, TValue>(
            this TDictionary source,
            IDictionary<TKey, TValue> copy)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var pair in copy)
            {
                source.Add(pair.Key, pair.Value);
            }

            return source;
        }
        /// <summary>
        /// 把copy对象的数据复制到source对象中
        /// </summary>
        /// <param name="source">被copy对象</param>
        /// <param name="copy">copy对象</param>
        /// <param name="keys">唯一键集合</param>
        /// <returns>被赋值后的TDictionary对相关</returns>
        public static TDictionary CopyFrom<TDictionary, TKey, TValue>(
            this TDictionary source,
            IDictionary<TKey, TValue> copy,
            IEnumerable<TKey> keys)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var key in keys)
            {
                source.Add(key, copy[key]);
            }

            return source;
        }
        /// <summary>
        /// 移除数据源中的指定数据
        /// </summary>
        /// <param name="source">数据源</param>
        /// <param name="keys">键</param>
        /// <returns>移除后的数据源</returns>
        public static TDictionary RemoveKeys<TDictionary, TKey, TValue>(
            this TDictionary source,
            IEnumerable<TKey> keys)
            where TDictionary : IDictionary<TKey, TValue>
        {
            foreach (var key in keys)
            {
                source.Remove(key);
            }

            return source;
        }
        /// <summary>
        /// 从字典类型中移除指定键的值
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source">字典类型数据源</param>
        /// <param name="keys">键</param>
        /// <returns>移除指定数据后的数据源</returns>
        public static IDictionary<TKey, TValue> RemoveKeys<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                source.Remove(key);
            }

            return source;
        }
    }

}