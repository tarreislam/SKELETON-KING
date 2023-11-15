namespace ZORGATH.Upgrades;

public class ProductsResponseEntry
{
    public ProductsResponseEntry(Upgrade upgrade)
    {
        ProductCode = upgrade.Code;
        ProductName = upgrade.Name;
        Price = upgrade.Price;
        Purchasable = upgrade.Purchasable;
        Premium = upgrade.Premium;
        PriceInMMP = upgrade.PriceInMMP;
        LocalPath = upgrade.LocalContent;
    }

    [PhpProperty("name")]
    public string ProductCode { get; set; }

    [PhpProperty("cname")]
    public string? ProductName { get; set; }

    [PhpProperty("cost")]
    public int Price { get; set; }

    [PhpProperty("purchasable")]
    public bool Purchasable { get; set; }

    [PhpProperty("premium")]
    public bool Premium { get; set; }

    [PhpProperty("premium_mmp_cost")]
    public int PriceInMMP { get; set; }

    [PhpProperty("dynamic")]
    public int Dynamic { get; set; }

    [PhpProperty("local_path")]
    public string LocalPath { get; set; }
}