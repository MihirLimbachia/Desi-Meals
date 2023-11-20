using System;
using Npgsql;
using System.Data;

namespace DesiMealsAbroad.Infra
{
    public class PostgresQueryRunner
    {
        private readonly string _connectionString;

        public PostgresQueryRunner(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ExecuteNonQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(sql, connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            command.ExecuteNonQuery();
        }

        public T ExecuteScalar<T>(string query, params NpgsqlParameter[] parameters)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    var result = command.ExecuteScalar();

                    return (result == DBNull.Value) ? default(T) : (T)result;
                }
            }
        }


        public DataTable ExecuteQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(sql, connection);

            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }

            connection.Open();
            var dataTable = new DataTable();
            dataTable.Load(command.ExecuteReader());

            return dataTable;
        }
}
  
}

