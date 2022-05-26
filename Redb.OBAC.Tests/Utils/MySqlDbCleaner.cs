using MySql.Data.MySqlClient;
using System;

namespace Redb.OBAC.Tests.Utils
{
    internal class MySqlDbCleaner
    {
        private readonly string _connectionString;

        public MySqlDbCleaner(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void CleanDb()
        {
            MySqlConnection connection;
            try
            {
                connection = new MySqlConnection(_connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                if (ex.Message.ToLower().Contains("unknown database"))
                    return; // no need to drop anything :)
                throw;
            }

            var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE IF EXISTS {connection.Database};";
            command.ExecuteNonQuery();

            command.CommandText = $"CREATE DATABASE {connection.Database};";
            command.ExecuteNonQuery();
        }
    }
}
