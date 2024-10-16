using System.Data;
using System.Reflection;
using Dapper;
using Npgsql;

namespace DapperRepositoryLib
{
    public class Repository<T> : IRepository<T>
    {
        private readonly string _connectionString;

        public Repository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private NpgsqlConnection Connection => new NpgsqlConnection(_connectionString);

        private static string TableName => typeof(T).Name.ToLower(); 

        public virtual async Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            using (var dbConnection = Connection)
            {
                await dbConnection.OpenAsync();
                var offset = (pageNumber - 1) * pageSize;

                return await dbConnection.QueryAsync<T>(
                    $"SELECT * FROM \"{TableName}\" ORDER BY \"id\" LIMIT @PageSize OFFSET @Offset", 
                    new { PageSize = pageSize, Offset = offset });
            }
        }

        public virtual async Task<T> GetByIdAsync(int id)
        {
            using (var dbConnection = Connection)
            {
                await dbConnection.OpenAsync();

                T? result = await dbConnection.QueryFirstOrDefaultAsync<T>(
                    $"SELECT * FROM \"{TableName}\" WHERE \"id\" = @Id",
                    new { Id = id });

                if (EqualityComparer<T>.Default.Equals(result, default))
                {
                    throw new InvalidOperationException($"Registro com ID {id} não encontrado.");
                }

                return result!;
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            using (var dbConnection = Connection)
            {
                await dbConnection.OpenAsync();
                using (var transaction = await dbConnection.BeginTransactionAsync())
                {
                    try
                    {
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var columnNames = string.Join(", ", properties.Select(p => $"\"{p.Name}\""));
                        var parameterNames = string.Join(", ", properties.Select(p => $"@{p.Name}"));

                        var sqlQuery = $"INSERT INTO \"{TableName}\" ({columnNames}) VALUES ({parameterNames})";
                        await dbConnection.ExecuteAsync(sqlQuery, entity, transaction);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw; 
                    }
                }
            }
        }

        public virtual async Task UpdateAsync(T entity)
        {
            using (var dbConnection = Connection)
            {
                await dbConnection.OpenAsync();
                using (var transaction = await dbConnection.BeginTransactionAsync())
                {
                    try
                    {
                        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        var setClause = string.Join(", ", properties.Select(p => $"\"{p.Name}\" = @{p.Name}"));

                        var sqlQuery = $"UPDATE \"{TableName}\" SET {setClause} WHERE \"id\" = @Id"; // Assumindo que a chave primária é "id"
                        await dbConnection.ExecuteAsync(sqlQuery, entity, transaction);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw; 
                    }
                }
            }
        }

        public virtual async Task DeleteAsync(int id)
        {
            using (var dbConnection = Connection)
            {
                await dbConnection.OpenAsync();
                using (var transaction = await dbConnection.BeginTransactionAsync())
                {
                    try
                    {
                        await dbConnection.ExecuteAsync($"DELETE FROM \"{TableName}\" WHERE \"id\" = @Id", new { Id = id }, transaction);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
    }

    public interface IRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync(int pageNumber = 1, int pageSize = 10);
        Task<T> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}
