using System.Data.Common;

namespace JinRi.LogCenter
{
    public class ConnectionStringFactory
    {
        public static string CreateConnectionString(DatabaseEnum databaseEnum)
        {
            return CreateConnectionString(databaseEnum.ToString());
        }
        public static string CreateConnectionString(string databaseEnum)
        {
            return CreateConnectionString(databaseEnum, AppSetting.EncryptKey);
        }

        public static string CreateConnectionString(string databaseEnum, string dbkey)
        {
            return ConnectionHelper.GetConnectionString(databaseEnum, dbkey);
        }
    }
}
