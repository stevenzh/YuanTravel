namespace Lvy.Web.Common.Cache
{
    public interface ILvyCache
    {
        object Get(string key);
        void Add(string key,object obj);
        void Remove(string key);
    }
}