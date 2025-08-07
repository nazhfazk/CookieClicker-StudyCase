using System;
using System.Collections.Generic;
using UnityEngine;


public class SaveData
{
    [Header("Core Game Data")]
    public int cookieCount = 0;
    public int clickMultiplier = 0;
    public float autoClickRate = 0f;

    [Header("Statistics")]
    public int totalCookiesEarned = 0;
    public int totalCookiesSpent = 0;
    public int totalClicks = 0;
    public int autoClickersBought = 0;
    public int multipliersBought = 0;
    public int skinsChanged = 0;

    [Header("Shop Data")]
    public List<ShopItemSaveData> shopItems = new List<ShopItemSaveData>();
    public List<int> ownedCookieSkins = new List<int>();
    public int currentCookieSkinIndex = -1; 
    [Header("Save Info")]
    public string saveTimestamp;
    public int saveVersion = 1; 

    public SaveData()
    {
      
        saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

  
    public static SaveData CreateFromGameState()
    {
        SaveData data = new SaveData();

        if (GameManager.Instance != null)
        {
            
            data.cookieCount = GameManager.Instance.GetCookieCount();
            data.clickMultiplier = GameManager.Instance.GetClickMultiplier();
            data.autoClickRate = GameManager.Instance.GetAutoClickRate();

       
            data.totalCookiesEarned = GameManager.Instance.GetTotalCookiesEarned();
            data.totalCookiesSpent = GameManager.Instance.GetTotalCookiesSpent();
            data.totalClicks = GameManager.Instance.GetTotalClicks();
            data.autoClickersBought = GameManager.Instance.GetAutoClickersBought();
            data.multipliersBought = GameManager.Instance.GetMultipliersBought();
            data.skinsChanged = GameManager.Instance.GetSkinsChanged();
        }

        
        ShopManager shopManager = GameObject.FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            
            data = shopManager.PopulateShopSaveData(data);
        }

        data.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        Debug.Log($"[SaveData] Created save data - Cookies: {data.cookieCount}, Multiplier: {data.clickMultiplier}, Auto Rate: {data.autoClickRate}");

        return data;
    }


    public string ToJson()
    {
        try
        {
            string json = JsonUtility.ToJson(this, true);
            Debug.Log($"[SaveData] Converted to JSON: {json.Length} characters");
            return json;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveData] Failed to convert to JSON: {e.Message}");
            return null;
        }
    }

   
    public static SaveData FromJson(string json)
    {
        try
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[SaveData] JSON string is null or empty");
                return new SaveData();
            }

            SaveData data = JsonUtility.FromJson<SaveData>(json);
            Debug.Log($"[SaveData] Loaded from JSON - Cookies: {data.cookieCount}, Multiplier: {data.clickMultiplier}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveData] Failed to parse JSON: {e.Message}");
            return new SaveData();
        }
    }


    public bool IsValid()
    {
        
        if (cookieCount < 0 || clickMultiplier < 0 || autoClickRate < 0)
        {
            Debug.LogWarning("[SaveData] Invalid negative values detected");
            return false;
        }

        if (cookieCount > 999999999) 
        {
            Debug.LogWarning("[SaveData] Cookie count seems too high, possible cheating");
            return false;
        }

        
        return true;
    }

    
    public string GetSaveSummary()
    {
        return $"Cookies: {cookieCount:N0} | Click Power: {clickMultiplier + 1} | Auto Rate: {autoClickRate:F1}/s | Saved: {saveTimestamp}";
    }
}


[Serializable]
public class ShopItemSaveData
{
    public int itemIndex;
    public int currentLevel;
    public ShopItemType itemType;

    public ShopItemSaveData(int index, int level, ShopItemType type)
    {
        itemIndex = index;
        currentLevel = level;
        itemType = type;
    }
}