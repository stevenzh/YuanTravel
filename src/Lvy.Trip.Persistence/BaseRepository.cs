using PetaPoco;

namespace Lvy.Trip.Dao
{
    public class BaseRepository
    {
        protected Database _repo;

        public int Execute(string sql, params object[] args)
        {
            return _repo.Execute(sql, args);
        }

        /// <summary>
        ///  varchar 类型的字符串需要调用该方法
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public AnsiString Ansi(string field)
        {
            return new AnsiString(field);
        }

        public AnsiString AnsiLike(string field)
        {
            return new AnsiString("%" + field + "%");
        }
    }
}