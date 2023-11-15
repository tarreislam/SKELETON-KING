using Newtonsoft.Json;

namespace ZORGATH.Upgrades;

public class UpgradeRepository : IUpgradeRepository
{
    private readonly string _sourcePath = "Upgrades.JSON";
    private List<Upgrade> _allUpgrades = new();
    private Dictionary<string, Dictionary<int, ProductsResponseEntry>> _upgradesForClient = new();

    public UpgradeRepository()
    {
        // Load upgrades
        if (!File.Exists(_sourcePath))
        {
            throw new FileNotFoundException($"Make sure the file \"{_sourcePath}\" is located next to the executable");
        }

        /*
         * decode and setup new objects for upgrades
         */
        foreach (var upgradeGetDto in
                 JsonConvert.DeserializeObject<UpgradeFileContentDto[]>(
                     File.ReadAllText(_sourcePath))) // Upgrades.JSON saved in SKELETON-KING repo
        {
            _allUpgrades.Add(new Upgrade
            {
                UpgradeId = upgradeGetDto.UpgradeId,
                UpgradeType = upgradeGetDto.UpgradeType,
                Name = upgradeGetDto.Name,
                Code = upgradeGetDto.Code,
                LocalContent = upgradeGetDto.LocalContent,
                Purchasable = upgradeGetDto.Purchasable,
                Price = upgradeGetDto.Price,
                PriceInMMP = upgradeGetDto.PriceInMMP,
                Premium = upgradeGetDto.Premium,
                Enabled = upgradeGetDto.Enabled,
                IsBundle = upgradeGetDto.Bundle,
                ProductsRequired = upgradeGetDto.ProductsRequired
            });
        }

        /*
         * Add products to categories
         */
        var enabledUpgrades = _allUpgrades.Where(u => u.Enabled).ToList();
        _AddProducts(enabledUpgrades, "Alt Avatar", Upgrade.Type.AlternativeAvatar);
        _AddProducts(enabledUpgrades, "Taunt", Upgrade.Type.Taunt);
        _AddProducts(enabledUpgrades, "Misc", Upgrade.Type.Miscellaneous);
        _AddProducts(enabledUpgrades, "Alt Announcement", Upgrade.Type.AnnouncerVoice);
        _AddProducts(enabledUpgrades, "Couriers", Upgrade.Type.Courier);
        _AddProducts(enabledUpgrades, "Hero", Upgrade.Type.Hero);
        _AddProducts(enabledUpgrades, "Ward", Upgrade.Type.Ward);
        _AddProducts(enabledUpgrades, "EAP", Upgrade.Type.EAP);
    }

    public List<Upgrade> GetAllUpgrades()
    {
        return _allUpgrades;
    }

    public ClientProductsPhpResponse GetProductsForClient()
    {
        return new ClientProductsPhpResponse(_upgradesForClient);
    }

    // internals
    private void _AddProducts(List<Upgrade> enabledUpgrades, string categoryName, Upgrade.Type upgradeType)
    {
        Dictionary<int, ProductsResponseEntry> productsInCategory = new();
        foreach (Upgrade upgrade in enabledUpgrades.Where(upgrade => upgrade.UpgradeType == upgradeType))
        {
            productsInCategory[upgrade.UpgradeId] = new(upgrade);
        }

        _upgradesForClient[categoryName] = productsInCategory;
    }
}