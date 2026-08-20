using Lvy.Models.BaseDB;

namespace Lvy.Trip.Dao.Base
{
    public class BaseTagDao : YuanDbRepository<BaseTagModel>
    {
        /// <summary>
        /// 点击次数更新
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int UpdateClickCnt(int id)
        {
            string sql = " set ClickCnt=ClickCnt+1 where Id=@0";
            return _repo.Update<BaseTagModel>(sql, id);
        }

        /// <summary>
        /// 是否存在相同的标签名
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public bool HasTagName(string tagName)
        {
            string sql = "select 1 from BaseTag where TagName=@0";
            return _repo.FirstOrDefault<int>(sql, new PetaPoco.AnsiString(tagName)) > 0;
        }
    }

    public class BrandDao : YuanDbRepository<BrandModel>
    {
    }

    public class BaseFileResDao : YuanDbRepository<BaseFileResModel>
    {
    }
}