using System;
using System.Collections.Generic;
using Arch.Common.Utils;
using Lvy.Models;
using NPOI.SS.Formula.Functions;
using PetaPoco;

namespace Lvy.Trip.Dao
{
    public class YuanDbRepository<T> : BaseRepository, IRepository<T>
    {

        public YuanDbRepository()
        {
            // _repo = new Database("YuanDB");
            //_repo = new Database("Server=112.124.7.61;Uid=yuan;Pwd=58f55f5s;Charset=utf8;Database=yuandb;Port=3306;", "MySql.Data.MySqlClient");
            _repo = new Database(MyHelper.connectionString, "MySql.Data.MySqlClient");
        }

        public ITransaction GetTransaction() { return _repo.GetTransaction(); }
        public bool IsNew(T obj) { return _repo.IsNew(obj); }
        public object Insert(T obj) { return _repo.Insert(obj); }
        public int Update(T obj, IEnumerable<string> columns) { return _repo.Update(obj, columns); }
        public int Update(T obj) { return _repo.Update(obj); }
        public int Update(string sql, params object[] args) { return _repo.Update<T>(sql, args); }

        public int Delete(T obj) { return _repo.Delete(obj); }
        public int Delete(string sql, params object[] args) { return _repo.Delete<T>(sql, args); }

        public int Delete(object primaryKey) { return _repo.Delete<T>(primaryKey); }
        public bool Exists(object primaryKey) { return _repo.Exists<T>(primaryKey); }

        public T SingleOrDefault(object primaryKey) { return _repo.SingleOrDefault<T>(primaryKey); }
        public T SingleOrDefault(string sql, params object[] args) { return _repo.SingleOrDefault<T>(sql, args); }

        public T FirstOrDefault(string sql, params object[] args) { return _repo.FirstOrDefault<T>(sql, args); }


        public T GetById(object primaryKey) { return _repo.Single<T>(primaryKey); }
        public T Single(string sql, params object[] args) { return _repo.Single<T>(sql, args); }
        public T Single(Sql sql) { return _repo.Single<T>(sql); }

        public T First(string sql, params object[] args) { return _repo.First<T>(sql, args); }


        public List<T> Fetch(string sql, params object[] args) { return _repo.Fetch<T>(sql, args); }

        public List<T> Fetch(long page, long itemsPerPage, string sql, params object[] args) { return _repo.Fetch<T>(page, itemsPerPage, sql, args); }

        public List<T> SkipTake(long skip, long take, string sql, params object[] args) { return _repo.SkipTake<T>(skip, take, sql, args); }
        /// <summary>
        /// 不支持多表分页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="itemsPerPage"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public PagedList<T> Pager(long pageIndex, long pageSize, string sql, params object[] args)
        {
            var obj = _repo.Page<T>(pageIndex, pageSize, sql, args);
            var list = new PagedList<T>
            {
                Items = obj.Items,
                Context = obj.Context,
                PageIndex = obj.CurrentPage,
                PageSize = obj.ItemsPerPage,
                PageCount = obj.TotalPages,
                TotalCount = obj.TotalItems
            };

            return list;
        }

        public PagedList<TModel> Pager<TModel>(long pageIndex, long pageSize, string sql, params object[] args)
        {
            var obj = _repo.Page<TModel>(pageIndex, pageSize, sql, args);
            var list = new PagedList<TModel>
            {
                Items = obj.Items,
                Context = obj.Context,
                PageIndex = obj.CurrentPage,
                PageSize = obj.ItemsPerPage,
                PageCount = obj.TotalPages,
                TotalCount = obj.TotalItems
            };

            return list;
        }

        public IEnumerable<T> Query(string sql, params object[] args)
        {
            return _repo.Query<T>(sql, args);
        }
        public IEnumerable<TModel> Query<TModel>(string sql, params object[] args)
        {
            return _repo.Query<TModel>(sql, args);
        }

        public TModel ExecuteScalar<TModel>(string sql, params object[] args)
        {
            return _repo.ExecuteScalar<TModel>(sql, args);
        }


        // Multi Query
        public IEnumerable<TRet> Query<T1, T2, TRet>(Func<T1, T2, TRet> cb, string sql, params object[] args)
        {
            return _repo.Query(cb, sql, args);
        }
        public IEnumerable<TRet> Query<T1, T2, T3, TRet>(Func<T1, T2, T3, TRet> cb, string sql, params object[] args)
        {
            return _repo.Query(cb, sql, args);
        }
        public IEnumerable<TRet> Query<T1, T2, T3, T4, TRet>(Func<T1, T2, T3, T4, TRet> cb, string sql, params object[] args)
        {
            return _repo.Query(cb, sql, args);
        }
        // Multi Query (Simple)
        public IEnumerable<T1> Query<T1, T2>(string sql, params object[] args)
        {
            return _repo.Query<T1, T2>(sql, args);
        }
        public IEnumerable<T1> Query<T1, T2, T3>(string sql, params object[] args)
        {
            return _repo.Query<T1, T2, T3>(sql, args);
        }

        public IEnumerable<T1> Query<T1, T2, T3, T4>(string sql, params object[] args)
        {
            return _repo.Query<T1, T2, T3, T4>(sql, args);
        }
    }
}