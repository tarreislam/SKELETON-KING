namespace ZORGATH.Upgrades;

public interface IUpgradeRepository
{
    public List<Upgrade> GetAllUpgrades();
    public ClientProductsPhpResponse GetProductsForClient();
}