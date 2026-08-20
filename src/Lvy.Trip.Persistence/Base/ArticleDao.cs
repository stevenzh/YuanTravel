using Lvy.Models.BaseDB;

namespace Lvy.Trip.Dao.Base
{
    public class BizLogDao : YuanDbRepository<BizLogModel> { }

    public class ArticleDao : YuanDbRepository<BaseArticleModel> { }

    public class ActicleCommentDao : YuanDbRepository<BaseArticleCommentModel> { }

    public class ActicleBrowseDao : YuanDbRepository<BaseArticleBrowseModel> { }

    public class BaseFileDao : YuanDbRepository<BaseFileModel> { }
}