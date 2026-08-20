using System;
using System.Web;

namespace Lvy.Web.Common.Cache
{
    public class CacheContext : ILvyCache
    {

        private static CacheContext _context = null;
        public static CacheContext Current
        {
            get
            {
                if (_context == null)
                {
                    return _context = new CacheContext();
                }

                return _context;
            }
        }

        public object Get(string key)
        {
            return HttpRuntime.Cache.Get(key);
        }

        public void Add(string key, object obj)
        {
            HttpRuntime.Cache.Insert(key, obj);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="cacheTime">秒为单位</param>
        public void Add(string key, object obj, int cacheTime)
        {
            HttpRuntime.Cache.Insert(key, obj, null, DateTime.Now.AddSeconds(cacheTime), TimeSpan.Zero);
        }

        public void Remove(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }
    }
}
