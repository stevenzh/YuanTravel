using Lvy.Models.CrmDB;

namespace Lvy.Trip.Dao.Crm
{
    public class SysPlatformDao : YuanDbRepository<SysPlatformModel>
    {
        public SysPlatformModel GetByUrl(string url)
        {
            string sql = " select * from SysPlatform where Url like @0";
            return _repo.FirstOrDefault<SysPlatformModel>(sql, AnsiLike(url));
        }
    }
}