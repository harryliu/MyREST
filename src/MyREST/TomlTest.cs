namespace MyREST
{
    using Nett;

    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class ProductList
    {
        [TomlTable("products")]
        public List<Product> Products { get; set; }
    }

    public static void Main(string[] args)
    {
        var tomlString = @"
        [[products]]
        name = ""Product 1""
        price = 10.99

        [[products]]
        name = ""Product 2""
        price = 19.99
    "
        ;

        var productList = Toml.ReadString<ProductList>(tomlString);

        foreach (var product in productList.Products)
        {
            Console.WriteLine($"Name: {product.Name}, Price: {product.Price}");
        }
    }
}