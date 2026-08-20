using System.Collections.Generic;
using Lvy.Models;
using PetaPoco;

namespace Lvy.Trip.Dao
{
    public interface IRepository<T>
    {
        ITransaction GetTransaction();
        //void Complete();
        bool IsNew(T obj);
        object Insert(T obj);
        int Update(T obj, IEnumerable<string> columns);
        int Update(T obj);
        int Update(string sql, params object[] args);
        int Delete(T obj);
        int Delete(string sql, params object[] args);
        int Delete(object primaryKey);
        bool Exists(object primaryKey);
        T SingleOrDefault(object primaryKey);
        T SingleOrDefault(string sql, params object[] args);
        T FirstOrDefault(string sql, params object[] args);
        T GetById(object primaryKey);
        T Single(string sql, params object[] args);

        T First(string sql, params object[] args);

        List<T> Fetch(string sql, params object[] args);

        List<T> Fetch(long page, long itemsPerPage, string sql, params object[] args);

        List<T> SkipTake(long skip, long take, string sql, params object[] args);

        PagedList<T> Pager(long pageIndex, long pageSize, string sql, params object[] args);

        IEnumerable<T> Query(string sql, params object[] args);

        TModel ExecuteScalar<TModel>(string sql, params object[] args);

        //int SetValidState(object primaryKey);
    }
}