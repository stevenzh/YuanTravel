using Lvy.Models.BaseDB;
using Lvy.Trip.Dao.Crm;
using PetaPoco;
using System.Collections.Generic;

namespace Lvy.Trip.Biz.Base
{
    /// <summary>
    /// 数据字典
    /// </summary>
    public class DictBiz : BaseBiz
    {
        private DictionaryDao _dicDao = new DictionaryDao();
        private DictionaryDetailDao _dicDetailDao = new DictionaryDetailDao();

        public List<BaseDictionaryModel> GetList()
        {
            return _dicDao.Fetch("SELECT * FROM BaseDictionary ORDER BY TableName ");
        }

        public BaseDictionaryModel GetById(int id)
        {
            return _dicDao.GetById(id);
        }

        public int AddDictionary(BaseDictionaryModel model)
        {
            model.IsValid = 1;
            return _dicDao.Insert(model) == null ? 0 : 1;
        }

        public int UpdateDictionary(BaseDictionaryModel model)
        {
            return _dicDao.Update(model);
        }

        public int SetValidStateByDictionary(int id)
        {
            var obj = GetById(id);

            obj.IsValid = obj.IsValid == 1 ? 0 : 1;

            return _dicDao.Update(obj);
        }

        #region DictionaryDetail

        public BaseDictionaryDetailModel GetByDetailId(int detailId)
        {
            return _dicDetailDao.GetById(detailId);
        }

        public int SetValidStateByDictionaryDetail(int detailId)
        {
            var obj = _dicDetailDao.GetById(detailId);

            obj.IsValid = obj.IsValid == 1 ? 0 : 1;

            //   CacheContext.Current.Remove(obj.Name);
            return _dicDetailDao.Update(obj);
        }

        public List<BaseDictionaryDetailModel> GetDetailList(int dicId)
        {
            return _dicDetailDao.Fetch("select * from BaseDictionaryDetail where DicId=@0", dicId);
        }

        public int AddDictionaryDetail(BaseDictionaryDetailModel model)
        {
            //  CacheContext.Current.Remove(model.Name);
            return _dicDetailDao.Insert(model) == null ? 0 : 1;
        }

        public int UpdateDictionaryDetail(BaseDictionaryDetailModel model)
        {
            // var obj = GetById(model.Id);

            // 清空key缓存
            //  CacheContext.Current.Remove(model.Name);
            return _dicDetailDao.Update(model);
        }

        /// <summary>
        /// 取得出境城市
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public BaseDictionaryDetailModel GetOutCityEnum(string key)
        {
            var sql = new Sql();
            sql.Append(" select * from BaseDictionaryDetail where name='OutCityEnum' and `key`=@0 ", key);

            return _dicDetailDao.FirstOrDefault(sql.SQL, sql.Arguments);
        }

        /// <summary>
        /// 取得产品类型
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseDictionaryDetailModel> GetLineType()
        {
            var sql = new Sql();
            sql.Append(" select * from BaseDictionaryDetail where name='LineTypeEnum' and  IsValid=1  ");

            return _dicDetailDao.Query<BaseDictionaryDetailModel>(sql.SQL, sql.Arguments);
        }

        #endregion DictionaryDetail
    }
}