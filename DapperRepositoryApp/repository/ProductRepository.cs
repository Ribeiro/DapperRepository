using Dapper;
using DapperRepositoryApp.Model;
using DapperRepositoryLib;
using Npgsql;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DapperRepositoryApp.Repository
{
    public class ProductRepository : Repository<Product>
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString) : base(connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Product>> GetByConditionAsync(string condition)
        {
            using (var dbConnection = new NpgsqlConnection(_connectionString))
            {
                await dbConnection.OpenAsync();
                return await dbConnection.QueryAsync<Product>($"SELECT * FROM \"products\" WHERE {condition}");
            }
        }

        public async Task<Product> GetFirstOrDefaultAsync(string condition)
        {
            using (var dbConnection = new NpgsqlConnection(_connectionString))
            {
                await dbConnection.OpenAsync();
                var result = await dbConnection.QueryFirstOrDefaultAsync<Product>($"SELECT * FROM \"products\" WHERE {condition}");
                if(null == result)
                {
                    throw new InvalidOperationException("O produto não foi encontrado.");
                }
                return result;
            }
        }

        public override async Task<IEnumerable<Product>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
        {
            // Utilize a implementação do método base, se desejado.
            return await base.GetAllAsync(pageNumber, pageSize);
        }

        public override async Task<Product> GetByIdAsync(int id)
        {
            // Utilize a implementação do método base, se desejado.
            return await base.GetByIdAsync(id);
        }

        public override async Task AddAsync(Product entity)
        {
            // Utilize a implementação do método base, se desejado.
            await base.AddAsync(entity);
        }

        public override async Task UpdateAsync(Product entity)
        {
            // Utilize a implementação do método base, se desejado.
            await base.UpdateAsync(entity);
        }

        public override async Task DeleteAsync(int id)
        {
            // Utilize a implementação do método base, se desejado.
            await base.DeleteAsync(id);
        }
    }
}