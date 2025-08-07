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

                    // Track skin change for quests
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
}