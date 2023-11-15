using System.ComponentModel.DataAnnotations.Schema;

namespace PUZZLEBOX;

public class Upgrade
{
    public enum Type
    {
        ChatNameColour,
        ChatSymbol,
        AccountIcon,
        AlternativeAvatar,
        AnnouncerVoice,
        Taunt,
        Courier,
        Hero,
        EAP,
        Status,
        Miscellaneous,
        Ward,
        Enhancement,
        Coupon,
        Mastery,
        Creep,
        Building,
        TauntBadge,
        TPEffect,
        SelectionCircle,
        Bundle
    }

    private static string GetPrefix(Type type)
    {
        return type switch
        {
            Type.ChatNameColour => "cc.",
            Type.ChatSymbol => "cs.",
            Type.AccountIcon => "ai.",
            Type.AlternativeAvatar => "aa.",
            Type.AnnouncerVoice => "av.",
            Type.Taunt => "t.",
            Type.Courier => "c.",
            Type.Hero => "h.",
            Type.EAP => "eap.",
            Type.Status => "s.",
            Type.Miscellaneous => "m.",
            Type.Ward => "w.",
            Type.Enhancement => "en.",
            Type.Coupon => "cp.",
            Type.Mastery => "ma.",
            Type.Creep => "cr.",
            Type.Building => "bu.",
            Type.TauntBadge => "tb.",
            Type.TPEffect => "te.",
            Type.SelectionCircle => "sc.",
            Type.Bundle => ""
        };
    }

    private static string GetFullUpgradeCategory(Type type)
    {
        return type switch
        {
            Type.ChatNameColour => "Chat Color",
            Type.ChatSymbol => "Chat Symbol",
            Type.AccountIcon => "Account Icon",
            Type.AlternativeAvatar => "Alt Avatar",
            Type.AnnouncerVoice => "Alt Announcement",
            Type.Taunt => "Taunt",
            Type.Courier => "Couriers",
            Type.Miscellaneous => "Misc",
            Type.Ward => "Ward",
            Type.Creep => "Creep",
            Type.TauntBadge => "Taunt Badge",
            Type.TPEffect => "TP Effect",
            Type.SelectionCircle => "Selection Circle"
        };
    }

    [Required]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    // Unique ID of this Upgrade.
    // Example: 1082
    public int UpgradeId { get; set; }

    [Required]
    // The type of this Upgrade.
    // Example: UpgradeType.Taunt
    public Type UpgradeType { get; set; }

    // Unique name of this Upgrade.
    // Example: "Kongor Taunt"
    public string? Name { get; set; }

    [Required]
    // Unique code of this Upgrade.
    // Example: "Kongor_Taunt"
    public string Code { get; set; }

    // Unique name of this Upgrade, prefixed with the Upgrade type.
    // Example: "t.Kongor_Taunt"
    public string PrefixedCode
    {
        get => GetPrefix(UpgradeType) + Code;
    }

    public string UpgradeFullCategory
    {
        get => GetFullUpgradeCategory(UpgradeType);
    }

    [Required]
    // Icon or EffectPath that is displayed to user in the Store.
    // Example: "/ui/fe2/store/icons/taunt_kongor.tga"
    public string LocalContent { get; set; }

    [Required]
    // Whether a user can buy this Upgrade in the Store.
    public bool Purchasable { get; set; }

    [Required]
    // Price of this Upgrade in gold coins.
    // Example: 10
    public int Price { get; set; }

    [Required]
    // Price of this Upgrade in silver coins.
    // Example: 100
    public int PriceInMMP { get; set; }

    [Required]
    // Whether this Upgrade is Premium. Not sure what this actually means.
    public bool Premium { get; set; }

    [Required]
    // Whether this Upgrade is allowed to be used in-game. For example, buggy Upgrades can be disabled until they are
    // fixed.
    public bool Enabled { get; set; }

    [Required]
    // Whether this Upgrade is Bundle. For bundle tab.
    public bool IsBundle { get; set; }

    // Product IDs required to buy the itesm (if the items is locked)
    public string? ProductsRequired { get; set; }

    // The following properties are also defined by the game but currently ignored:
    // int productQuantity
    // int productWebContent
    // int[5] specialBundle
    // int[6] productCharge
    // int[7] productDuration
    // int[2] productTime
    // (int,string,int,int) productStats
    // string productEnhancement
    // string productEnhancementID
    // int[3] chargesRemaining
    // int durationsRemaining
    // string productDescription
}