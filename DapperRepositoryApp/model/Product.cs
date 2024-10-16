namespace DapperRepositoryApp.Model
{

    public class Product
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Category { get; set; }

        public decimal Price { get; set; }

        public Product()
        {

        }

        public Product(string name, string category, decimal price)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Category = category ?? throw new ArgumentNullException(nameof(category));
            Price = price;
        }
    }

}