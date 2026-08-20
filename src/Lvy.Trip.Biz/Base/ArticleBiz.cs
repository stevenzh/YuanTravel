using Arch.Common.Utils;
using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Models.JModels;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using Lvy.Web.Common;
using MySql.Data.MySqlClient;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 公告
    /// </summary>
    public class ArticleBiz : BaseBiz
    {
        private ArticleDao _dao = new ArticleDao();
        private ActicleBrowseDao _browseDao = new ActicleBrowseDao();
        public static string connectionString = ConfigurationManager.ConnectionStrings["YuanDB"].ConnectionString;

        /// <summary>
        /// 获取公告集合
        /// </summary>
        /// <param name="vModel"></param>
        /// <returns></returns>
        public PagedList<BaseArticleModel> GetPageList(ArticleVModel vModel)
        {
            var sql = new Sql();
            sql.Append("select * from BaseArticles where IsValid=1");

            if (!vModel.Article.Title.IsNullOrEmpty())
                sql.Append(" and Title like @0 ", AnsiLike(vModel.Article.Title));
            if (!vModel.Article.OwnerCode.IsNullOrEmpty())
                sql.Append(" and OwnerCode=@0 ", Ansi(vModel.Article.OwnerCode));
            if (vModel.Scope == 1)
                sql.Append(" and NoticeType<>1 ");

            sql.Append(" order by ModifiedTime DESC ");

            var list = _dao.Pager(vModel.ArticlePageList.PageIndex, vModel.ArticlePageList.PageSize, sql.SQL, sql.Arguments);
            return list;
        }

        public void AddBrowse(BaseArticleBrowseModel model)
        {
            _browseDao.Insert(model);
        }

        /// <summary>
        /// 获取公告前*条
        /// </summary>
        /// <param name="topCount"></param>
        /// <param name="type">1:内部 2国内短线 3 国内长线 4 出境5门票6签证</param>
        /// <returns></returns>
        public List<BaseArticleModel> GetArticleList(string ownerCode, int topCount, int type)
        {
            var sql = new Sql();
            sql.Append(@"SELECT * FROM BaseArticles WHERE OwnerCode=@0 and IsValid=1 ", ownerCode);
            if (type != 0)
                sql.Append(@" and NoticeType=@0", type);

            sql.Append(@" ORDER BY ModifiedTime DESC LIMIT " + topCount);

            return _dao.Fetch(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 添加公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int AddArticle(BaseArticleModel model)
        {
            return _dao.Insert(model).ToInt();
        }

        /// <summary>
        /// 更新公告
        /// </summary>
        /// <param name="noticeModel"></param>
        /// <returns>返回1：true 0:false</returns>
        public int UpdateArticle(BaseArticleModel noticeModel)
        {
            return _dao.Update(noticeModel);
        }

        /// <summary>
        /// 获取一个角色对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public BaseArticleModel GetById(int pageId, bool pageCnt = false)
        {
            var entity = _dao.GetById(pageId);
            if (entity != null && pageCnt)
            {
                entity.BrowseCnt = entity.BrowseCnt + 1;
            }
            _dao.Update(entity);
            return entity;
        }

        public List<StatData> StatTime(int id, DateTime today)
        {
            List<StatData> list = new List<StatData>();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT Dateadd(mi, ( Datediff(mi, CONVERT(VARCHAR(10), Dateadd(ss, 1, created), 120), Dateadd(ss, 1, created)) / 60 ) *60, CONVERT (VARCHAR(10), created, 120)) AS TimeSpan, ");
            sb.Append(" Count(browseid) AS Cnt ");
            sb.Append("FROM basearticlebrowses where ArticleID = '" + id + "' ");
            if (today != default(DateTime))
            {
                var d = today.AddDays(1); ;
                var dd = DateTime.Now.AddDays(-2);
                // sb.Append("and created > '" + today.ToShortDateString() + "' and created < '" + d.ToShortDateString() + "' " );
                sb.Append("and created > '" + String.Format("{0:yyyy-MM-dd HH:mm}", dd) + "' ");
            }
            sb.Append("GROUP BY Dateadd(mi, (Datediff(mi, CONVERT(VARCHAR(10), Dateadd(ss, 1, created ), 120), Dateadd(ss, 1, created)) / 60) *60, CONVERT(VARCHAR(10), created, 120)) ORDER BY TimeSpan");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (IDataReader dr = MySqlHelper.ExecuteReader(conn, sb.ToString()))
                {
                    while (dr.Read())
                    {
                        StatData lineIndex = new StatData();
                        lineIndex.UserName = String.Format("{0:dd日HH时}", MyHelper.GetDateTime(dr, "TimeSpan"));
                        lineIndex.AllFans = MyHelper.GetInt(dr, "Cnt");

                        list.Add(lineIndex);
                    }
                }
            }
            return list;
        }

        public List<StatData> StatRegion(int id, DateTime today)
        {
            List<StatData> list = new List<StatData>();
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT CityName, Count(browseid) AS Cnt ");
            sb.Append("FROM basearticlebrowses where ArticleID = '" + id + "' ");
            if (today != default(DateTime))
            {
                //var d = today.AddDays(1);
                var dd = DateTime.Now.AddDays(-2);
                //sb.Append("and created > '" + today.ToShortDateString() + "' and created < '" + d.ToShortDateString() + "' ");
                sb.Append("and created > '" + String.Format("{0:yyyy-MM-dd HH:mm}", dd) + "' ");
            }
            sb.Append("GROUP BY CityName ");

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                using (IDataReader dr = MySqlHelper.ExecuteReader(conn, sb.ToString()))
                {
                    while (dr.Read())
                    {
                        StatData lineIndex = new StatData();
                        lineIndex.UserName = MyHelper.GetString(dr, "CityName");
                        lineIndex.AllFans = MyHelper.GetInt(dr, "Cnt");

                        list.Add(lineIndex);
                    }
                }
            }
            return list;
        }
    }
}