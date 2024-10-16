using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using DapperRepositoryApp.Model;
using DapperRepositoryApp.Repository;
using DapperRepositoryLib;

namespace DapperRepositoryApp
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // Configurar o host e a injeção de dependência
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    var connectionString = "Host=localhost;Port=5432;Database=your_database;Username=your_user;Password=your_password";

                    services.AddScoped<IRepository<Product>>(provider => new Repository<Product>(connectionString)); // Registro do repositório genérico
                    services.AddScoped<ProductRepository>(); // Registro do repositório específico
                })
                .Build();

            // Resolver o serviço ProductRepository
            var productRepository = host.Services.GetRequiredService<ProductRepository>();

            // Exemplo de uso do ProductRepository
            await RunProductOperations(productRepository);
        }

        private static async Task RunProductOperations(ProductRepository productRepository)
        {
            var newProduct = new Product
            {
                Name = "Produto Exemplo",
                Category = "Categoria Exemplo",
                Price = 99.99m
            };

            await productRepository.AddAsync(newProduct);
            Console.WriteLine("Produto adicionado com sucesso.");

            var products = await productRepository.GetAllAsync(1, 10);
            Console.WriteLine($"Total de produtos: {products.Count()}");

            var condition = $"\"category\" = 'Categoria Exemplo'";
            var filteredProducts = await productRepository.GetByConditionAsync(condition);
            Console.WriteLine($"Total de produtos na categoria 'Categoria Exemplo': {filteredProducts.Count()}");

            var firstProduct = await productRepository.GetFirstOrDefaultAsync(condition);
            if (firstProduct != null)
            {
                Console.WriteLine($"Produto encontrado: {firstProduct.Name}");
            }
            else
            {
                Console.WriteLine("Nenhum produto encontrado.");
            }

            if (firstProduct != null)
            {
                firstProduct.Price = 79.99m; // Atualizando o preço
                await productRepository.UpdateAsync(firstProduct);
                Console.WriteLine("Produto atualizado com sucesso.");
            }

            if (firstProduct != null)
            {
                await productRepository.DeleteAsync(firstProduct.Id);
                Console.WriteLine("Produto excluído com sucesso.");
            }
        }
    }
}