using System.Text.RegularExpressions;
using Npgsql;

namespace Redb.OBAC.Tests.Utils
{
    public class PgSqlDbCleaner 
    {
        private readonly string _connectionString;

        public PgSqlDbCleaner(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CleanDb()
        {
            NpgsqlConnection connection;
            try
            {
                connection = new NpgsqlConnection(_connectionString);
                connection.Open();
            }
            catch (PostgresException ex)
            {
                if (ex.MessageText.ToLower().Contains("does not exist"))
                    return; // no need to drop anything :)
                throw;
            }

            var command = connection.CreateCommand();
            command.CommandText = "drop schema if exists public cascade;";
            command.ExecuteNonQuery();

            command.CommandText = "CREATE SCHEMA public;";
            command.ExecuteNonQuery();
        }
    }
}