using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("Shop Items Data")]
    [SerializeField] private ShopItem[] shopItems;

    [Header("Manual Shop Buttons")]
    [SerializeField] private ShopItemUI[] shopItemUIButtons;

    [Header("Cookie References")]
    [SerializeField] private Clicker cookieClicker;

    private GameManager gameManager;
    private List<int> ownedCookieSkins = new List<int>();

    void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("[ShopManager] GameManager.Instance is null!");
            return;
        }

        SetupManualShop();
    }

    private void SetupManualShop()
    {
        if (shopItemUIButtons.Length != shopItems.Length)
        {
            Debug.LogError("[ShopManager] Jumlah UI button dan item tidak sama!");
            return;
        }

        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i] != null && shopItemUIButtons[i] != null)
            {
                shopItemUIButtons[i].Initialize(shopItems[i], i, this);
            }
        }
    }

    public void BuyItem(int index)
    {
        if (index >= shopItems.Length)
        {
            Debug.LogError($"[ShopManager] Invalid item index: {index}");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogError("[ShopManager] GameManager is null!");
            return;
        }

        ShopItem item = shopItems[index];
        int price = item.GetCurrentPrice();

        if (gameManager.GetCookieCount() < price)
        {
            Debug.Log("[ShopManager] Not enough cookies!");
            return;
        }

        if (item.IsCookieSkin() && ownedCookieSkins.Contains(index))
        {
            Debug.Log("[ShopManager] Cookie skin already owned!");
            return;
        }

        bool success = gameManager.SpendCookies(price);
        if (success)
        {
            ApplyEffect(item, index);
            RefreshShop();
        }
    }

    // Save/Load
    public void LoadGameData(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[ShopManager] Attempted to load null save data");
            return;
        }

        Debug.Log("[ShopManager] Loading shop data from save file...");

        // Load cookie skin
        ownedCookieSkins = new List<int>(data.ownedCookieSkins);

        // Load shop item level
        if (data.shopItems != null && data.shopItems.Count > 0)
        {
            foreach (var saveItemData in data.shopItems)
            {
                if (saveItemData.itemIndex < shopItems.Length)
                {
                    shopItems[saveItemData.itemIndex].currentLevel = saveItemData.currentLevel;
                }
            }
        }

        // Apply current cookie skin if saved
        if (data.currentCookieSkinIndex >= 0 && data.currentCookieSkinIndex < shopItems.Length)
        {
            var skinItem = shopItems[data.currentCookieSkinIndex];
            if (skinItem.IsCookieSkin() && skinItem.cookieSprite != null && cookieClicker != null)
            {
                cookieClicker.ChangeCookieSprite(skinItem.cookieSprite);
                Debug.Log($"[ShopManager] Applied saved cookie skin: {skinItem.itemName}");
            }
        }

    
        RefreshShop();

        Debug.Log($"[ShopManager] Shop data loaded - Owned skins: {ownedCookieSkins.Count}");
    }

    public SaveData PopulateShopSaveData(SaveData saveData)
    {
        if (saveData == null) return null;

        // Save skin yang dimiliki
        saveData.ownedCookieSkins = new List<int>(ownedCookieSkins);

        // Save shop item level
        saveData.shopItems.Clear();
        for (int i = 0; i < shopItems.Length; i++)
        {
            if (shopItems[i].currentLevel > 0)
            {
                var itemSaveData = new ShopItemSaveData(i, shopItems[i].currentLevel, shopItems[i].itemType);
                saveData.shopItems.Add(itemSaveData);
            }
        }

       
        saveData.currentCookieSkinIndex = GetCurrentCookieSkinIndex();

        Debug.Log($"[ShopManager] Populated save data - {saveData.shopItems.Count} shop items, {saveData.ownedCookieSkins.Count} owned skins");

        return saveData;
    }

    private int GetCurrentCookieSkinIndex()
    {
        if (ownedCookieSkins.Count > 0)
        {
            return ownedCookieSkins[ownedCookieSkins.Count - 1];
        }
        return -1;
    }

  
    private void ApplyEffect(ShopItem item, int index)
    {
        switch (item.itemType)
        {
            case ShopItemType.AutoClicker:
                item.currentLevel++;
                gameManager.AddAutoClickRate(item.autoClickRate);
                Debug.Log($"[ShopManager] Bought Auto Clicker level {item.currentLevel}");
                break;

            case ShopItemType.ClickMultiplier:
                item.currentLevel++;
                gameManager.AddClickMultiplier(item.clickMultiplier);
                Debug.Log($"[ShopManager] Bought Click Multiplier level {item.currentLevel}");
                break;

            case ShopItemType.CookieSkin:
                if (!ownedCookieSkins.Contains(index))
                {
                    ownedCookieSkins.Add(index);
                }

                if (item.cookieSprite != null && cookieClicker != null)
                {
                    cookieClicker.ChangeCookieSprite(item.cookieSprite);

                    if (gameManager != null)
                    {
                        gameManager.OnSkinChanged();
                    }

                    Debug.Log($"[ShopManager] Changed cookie skin to: {item.itemName}");
                }
                break;
        }
    }

    public void RefreshShop()
    {
        for (int i = 0; i < shopItemUIButtons.Length; i++)
        {
            if (shopItemUIButtons[i] != null)
            {
                shopItemUIButtons[i].UpdateDisplay();
            }
        }
    }

    public List<int> GetOwnedCookieSkins()
    {
        return new List<int>(ownedCookieSkins);
    }

    public void SetOwnedCookieSkins(List<int> list)
    {
        ownedCookieSkins = new List<int>(list);
        RefreshShop();
    }

    public void SetCurrentCookieSkin(int skinIndex)
    {
        if (skinIndex < 0 || skinIndex >= shopItems.Length) return;

        var skinItem = shopItems[skinIndex];
        if (skinItem.IsCookieSkin() && skinItem.cookieSprite != null && cookieClicker != null)
        {
            cookieClicker.ChangeCookieSprite(skinItem.cookieSprite);
            Debug.Log($"[ShopManager] Set current cookie skin to: {skinItem.itemName}");
        }
    }
}
