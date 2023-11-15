namespace ZORGATH.Upgrades;

public class UpgradeFileContentDto
{
    public int UpgradeId; 
    public Upgrade.Type UpgradeType; 
    public string Code = ""; // "Consider using nullable" warning
    public string? Name; 
    public string LocalContent = "";  // "Consider using nullable" warning
    public bool Purchasable; 
    public int Price; 
    public int PriceInMMP; 
    public bool Premium; 
    public bool Enabled; 
    public bool Bundle;
    public string? ProductsRequired;
}