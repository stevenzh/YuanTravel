using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lvy.Models;
using Lvy.Models.BaseDB;

namespace Lvy.VModels.Base
{
    public class ArticleVModel : BaseVModel
    {

        public ArticleVModel()
        {
            if (Article == null)
                Article = new BaseArticleModel();
            if (ArticlePageList == null)
                ArticlePageList = new PagedList<BaseArticleModel>();
        }

        /// <summary>
        /// 查询对象
        /// </summary>
        public BaseArticleModel Article { get; set; }

        /// <summary>
        /// 集合
        /// </summary>
        public PagedList<BaseArticleModel> ArticlePageList { get; set; }
        /// <summary>
        /// 过滤范围 0-全部 1-外部 2-内部
        /// </summary>
        public int Scope { get; set; }
    }
}
