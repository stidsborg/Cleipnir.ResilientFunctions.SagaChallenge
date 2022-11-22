using Cleipnir.ResilientFunctions.PostgreSQL;

namespace OrderWebApi.Cli;

public static class ClearDatabase
{
    public static void Do()
    {
        DatabaseHelper.RecreateDatabase(Settings.ConnectionString).Wait();
    }
}