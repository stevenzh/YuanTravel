using System;
using Arch.Common.Utils;
using System.Configuration;

namespace Arch.Common
{
    public class Toolkit
    {

        //private static Toolkit _current;
        //public static Toolkit Current
        //{
        //    get { return _current ?? (_current = Activator.CreateInstance<Toolkit>()); }
        //}

        /// <summary>
        /// 数据加密工具Z
        /// </summary>
        private static SecurityTools _security;
        public static SecurityTools Security { get { return _security ?? (_security = Activator.CreateInstance<SecurityTools>()); } }

        /// <summary>
        /// Rmb转换工具类
        /// </summary>
        private static RmbTools _rmb;
        public static RmbTools Rmb { get { return _rmb ?? (_rmb = Activator.CreateInstance<RmbTools>()); } }

        /// <summary>
        /// 图片处理
        /// </summary>
        private static ImageTools _image;
        public static ImageTools Image { get { return _image ?? (_image = Activator.CreateInstance<ImageTools>()); } }

        /// <summary>
        /// 对象映射工具类
        /// </summary>
        //private static MapperTools _mapper;
        //public static MapperTools Mapper { get { return _mapper ?? (_mapper = Activator.CreateInstance<MapperTools>()); } }


        private static NpoiHelper _npoi;
        public static NpoiHelper Npoi { get { return _npoi ?? (_npoi = Activator.CreateInstance<NpoiHelper>()); } }


        /// <summary>
        /// 根据key获取配置
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>获取配置节点内容</returns>
        public static string GetAppSetting(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key].ToString();
            }
            catch
            {
                //日志
                return "";
            }
        }

    }
}
