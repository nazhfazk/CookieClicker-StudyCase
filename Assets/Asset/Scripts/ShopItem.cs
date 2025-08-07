using UnityEngine;
using UnityEngine.UI;

// Data structure sederhana untuk shop items
[System.Serializable]
public class ShopItem
{
    [Header("Item Info")]
    public string itemName;
    public string description;
    public int basePrice;
    public Sprite itemIcon;

    [Header("Item Type")]
    public ShopItemType itemType;

    [Header("Upgrade Values")]
    public int clickMultiplier = 1; // Untuk click multiplier items
    public float autoClickRate = 0f; // Untuk auto clicker items (clicks per second)

    [Header("Cookie Skin")]
    public Sprite cookieSprite; // Untuk cookie skins

    // Current level untuk upgrades (auto clicker & multiplier)
    [HideInInspector]
    public int currentLevel = 0;

    // Method sederhana untuk hitung harga berdasarkan level
    public int GetCurrentPrice()
    {
        if (itemType == ShopItemType.CookieSkin)
        {
            return basePrice; // Cookie skin harga tetap
        }

        // Untuk upgrades, harga naik berdasarkan level menggunakan exponential growth
        // Formula: basePrice * (1.5^currentLevel)
        // Level 0: basePrice * 1 = basePrice
        // Level 1: basePrice * 1.5
        // Level 2: basePrice * 2.25
        // Level 3: basePrice * 3.375, etc
        float multiplier = Mathf.Pow(1.5f, currentLevel);
        return Mathf.RoundToInt(basePrice * multiplier);
    }

    // Method untuk cek apakah ini cookie skin
    public bool IsCookieSkin()
    {
        return itemType == ShopItemType.CookieSkin;
    }

    // Method untuk cek apakah ini upgrade item
    public bool IsUpgrade()
    {
        return itemType == ShopItemType.AutoClicker || itemType == ShopItemType.ClickMultiplier;
    }
}

// Enum sederhana untuk tipe item
public enum ShopItemType
{
    AutoClicker,     // Item yang auto click cookie
    ClickMultiplier, // Item yang increase click power
    CookieSkin      // Item untuk ganti sprite cookie
}