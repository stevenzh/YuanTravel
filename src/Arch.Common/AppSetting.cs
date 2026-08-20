using System.Configuration;

namespace Arch.Common
{
    public class AppSetting
    {
        public static string Get(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
