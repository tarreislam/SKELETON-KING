namespace ZORGATH.Upgrades;

public class ClientProductsPhpResponse
{
    [PhpProperty("products")]
    public Dictionary<string, Dictionary<int, ProductsResponseEntry>> Products { get; set; }

    public ClientProductsPhpResponse(Dictionary<string, Dictionary<int, ProductsResponseEntry>> products)
    {
        Products = products;
    }
    
}