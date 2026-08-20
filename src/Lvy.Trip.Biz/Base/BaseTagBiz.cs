using Lvy.Models;
using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Base;
using Lvy.VModels.Base;
using PetaPoco;
using System;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 产品标签
    /// </summary>
    public class BaseTagBiz : BaseBiz
    {
        private readonly BaseTagDao _dao = new BaseTagDao();

        private delegate void TagHandler(int tagId);

        private delegate void TagMultiHandler(string[] tagIds);

        public PagedList<BaseTagModel> GetTagPagedList(TagVModel vModel)
        {
            var sql = new Sql();
            sql.Append("SELECT * FROM BaseTag WHERE IsValid=1 AND OwnerCode=@0 ", vModel.OwnerCode);
            if (vModel.TagModel.ProductType != 0)
            {
                sql.Append(" AND ProductType = @0", vModel.TagModel.ProductType);
            }
            if (!vModel.TagModel.TagName.IsNullOrEmpty())
            {
                sql.Append(" AND TagName LIKE @0", AnsiLike(vModel.TagModel.TagName));
            }

            return _dao.Pager(vModel.TagPagedList.PageIndex, vModel.TagPagedList.PageSize, sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得标签列表
        /// 0:线路 1:门票 3:文章
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns></returns>
        public List<BaseTagModel> GetTags(string ownerCode, int type = 1)
        {
            return _dao.Fetch("SELECT * FROM BaseTag WHERE OwnerCode=@0 AND ProductType=@1", ownerCode,  type);
        }

        /// <summary>
        /// 增加命中次数
        /// </summary>
        /// <param name="tagId"></param>
        public void AddHit(int tagId)
        {
            _dao.Update("SET Hit=Hit+1 WHERE Id=@0", tagId);
        }

        /// <summary>
        /// 增加命中次数
        /// </summary>
        /// <param name="tagIds"></param>
        public void AddHitMulti(string[] tagIds)
        {
            foreach (string tagId in tagIds)
            {
                if (!tagId.IsNullOrEmpty())
                    _dao.Update(@" SET Hit=Hit+1 WHERE Id=@0", Convert.ToInt32(tagId));
            }
        }

        public BaseTagModel GetTagById(int id)
        {
            return _dao.GetById(id);
        }

        public void Add(BaseTagModel model)
        {
            _dao.Insert(model);
        }

        /// <summary>
        /// 增加命中次数 （异步）
        /// </summary>
        /// <param name="tagId"></param>
        public void AsynAddHit(int tagId)
        {
            TagHandler handler = AddHit;
            IAsyncResult ar = handler.BeginInvoke(tagId, null, null);
        }

        public void Update(BaseTagModel model)
        {
            _dao.Update(model);
        }

        /// <summary>
        /// 增加命中次数 （异步）
        /// </summary>
        /// <param name="tagIds"></param>
        public void AsynAddHitMulti(string[] tagIds)
        {
            TagMultiHandler handler = AddHitMulti;
            IAsyncResult ar = handler.BeginInvoke(tagIds, null, null);
        }

        public bool CheckName(string name, int tagId, string ownerCode)
        {
            var sql = new Sql();
            sql.Append("SELECT COUNT(1) FROM BaseTag WHERE OwnerCode=@0 AND TagName=@1 ", Ansi(ownerCode),  Ansi(name));
            if (tagId != 0)
                sql.Append(" AND ID<>@0", tagId);

            long cnt = _dao.ExecuteScalar<long>(sql.ToString(), sql.Arguments);
            return cnt > 0;
        }

        public int Delete(int id)
        {
            return _dao.Update("SET IsValid=0 WHERE Id=@0 ", id);
        }

        /// <summary>
        /// 增加点击次数
        /// </summary>
        /// <param name="tagId"></param>
        public void AddClick(int tagId)
        {
            _dao.Update(@"SET ClickCnt=ClickCnt+1 WHERE Id=@0", tagId);
        }

        /// <summary>
        /// 增加点击次数 （异步）
        /// </summary>
        /// <param name="tagId"></param>
        public void AsynAddClick(int tagId)
        {
            TagHandler handler = AddClick;
            IAsyncResult ar = handler.BeginInvoke(tagId, null, null);
        }
    }
}