using Cleipnir.ResilientFunctions.PostgreSQL;
using Npgsql;

namespace OrderWebApi.Cli;

public static class ClearDatabase
{
    public static void Do()
    {
        DatabaseHelper.CreateDatabaseIfNotExists(Settings.ConnectionString).Wait();
        var store = new PostgreSqlFunctionStore(Settings.ConnectionString);
        store.Initialize().Wait();

        //clear existing tables
        var connection = new NpgsqlConnection(Settings.ConnectionString);
        connection.Open();
        var sqlCommand = new NpgsqlCommand(
            "TRUNCATE TABLE rfunctions; TRUNCATE TABLE events;",
            connection
        );
        sqlCommand.ExecuteNonQuery();
    }
}