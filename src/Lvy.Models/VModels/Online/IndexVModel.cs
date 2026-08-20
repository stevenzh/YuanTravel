using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Online
{

    /// <summary>
    /// 首页vmodel 
    /// </summary>
    public class IndexVModel : BaseVModel
    {

        /// <summary>
        /// 旧版目的地导航
        /// </summary>
        public List<DestNavVModel> DestsNav { get; set; }

        /// <summary>
        /// 公告栏信息
        /// </summary>
        public List<BaseArticleModel> NoticeModels { get; set; }
    }



}
