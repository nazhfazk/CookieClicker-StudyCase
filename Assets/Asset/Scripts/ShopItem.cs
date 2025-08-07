using UnityEngine;
using UnityEngine.UI;


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

    
    [HideInInspector]
    public int currentLevel = 0;

    
    public int GetCurrentPrice()
    {
        if (itemType == ShopItemType.CookieSkin)
        {
            return basePrice; 
        }

        // Untuk upgrades, harga naik berdasarkan level menggunakan exponential growth
        float multiplier = Mathf.Pow(1.5f, currentLevel);
        return Mathf.RoundToInt(basePrice * multiplier);
    }

 
    public bool IsCookieSkin()
    {
        return itemType == ShopItemType.CookieSkin;
    }


    public bool IsUpgrade()
    {
        return itemType == ShopItemType.AutoClicker || itemType == ShopItemType.ClickMultiplier;
    }
}

// Enum tipe item
public enum ShopItemType
{
    AutoClicker,     // Item yang auto click cookie
    ClickMultiplier, // Item yang increase click power
    CookieSkin      // Item untuk ganti sprite cookie
}