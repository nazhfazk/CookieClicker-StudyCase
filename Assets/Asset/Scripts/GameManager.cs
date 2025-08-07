using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Cookie Counter")]
    [SerializeField] private TextMeshProUGUI cookieCounterText;

    [Header("Click Settings")]
    [SerializeField] private int baseClickPower = 1;

    // Game data
    private int cookieCount = 0;
    private int clickMultiplier = 0;
    private float autoClickRate = 0f;

    // Auto click system
    private float autoClickTimer = 0f;

    // Quest tracking
    private int totalCookiesEarned = 0;
    private int totalCookiesSpent = 0;
    private int totalClicks = 0;
    private int autoClickersBought = 0;
    private int multipliersBought = 0;
    private int skinsChanged = 0;

    
    [Header("Save/Load System")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float saveOnProgressInterval = 5f; // Save setiap 5 progress
    private float lastSaveProgress = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateCookieCounterUI();
    }

    private void Update()
    {
        // Handle auto clicking
        if (autoClickRate > 0f)
        {
            autoClickTimer += Time.deltaTime;

            if (autoClickTimer >= 1f / autoClickRate)
            {
                AddCookie(GetClickPower(), false); 
                autoClickTimer = 0f;
            }
        }
    }

   
    public void LoadGameData(SaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[GameManager] Attempted to load null save data");
            return;
        }

        Debug.Log("[GameManager] Loading game data from save file...");

       
        cookieCount = data.cookieCount;
        clickMultiplier = data.clickMultiplier;
        autoClickRate = data.autoClickRate;

        
        totalCookiesEarned = data.totalCookiesEarned;
        totalCookiesSpent = data.totalCookiesSpent;
        totalClicks = data.totalClicks;
        autoClickersBought = data.autoClickersBought;
        multipliersBought = data.multipliersBought;
        skinsChanged = data.skinsChanged;

        
        UpdateCookieCounterUI();

        Debug.Log($"[GameManager] Game data loaded successfully - Cookies: {cookieCount}, Click Power: {GetClickPower()}, Auto Rate: {autoClickRate}");

       
        if (enableAutoSave && PlayFabManager.Instance != null && PlayFabManager.Instance.IsLoggedIn())
        {
            Invoke(nameof(TriggerSave), 2f);
        }
    }

    private void TriggerSave()
    {
        if (PlayFabManager.Instance != null && PlayFabManager.Instance.IsLoggedIn() && !PlayFabManager.Instance.IsSaving())
        {
            PlayFabManager.Instance.SaveGameData();
        }
    }

    private void CheckAutoSave()
    {
        if (!enableAutoSave) return;

        float currentProgress = totalCookiesEarned + totalCookiesSpent + (totalClicks * 0.1f);

        if (currentProgress - lastSaveProgress >= saveOnProgressInterval)
        {
            lastSaveProgress = currentProgress;
            TriggerSave();
        }
    }

    //Tambah cookie ke counter
    public void AddCookie(int amount, bool isManualClick = true)
    {
        cookieCount += amount;
        totalCookiesEarned += amount;

        UpdateCookieCounterUI();

        
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.EarnCookies, amount);

            if (isManualClick)
            {
                totalClicks++;
                QuestManager.Instance.AddProgress(QuestType.ClickCertainAmount, 1);
            }
        }

        
        CheckAutoSave();

        Debug.Log($"Added {amount} cookies. Total: {cookieCount}");
    }

    // Spend cookie
    public bool SpendCookies(int amount)
    {
        Debug.Log($"Trying to spend {amount} cookies. Have: {cookieCount}");

        if (cookieCount >= amount)
        {
            cookieCount -= amount;
            totalCookiesSpent += amount;

            UpdateCookieCounterUI();

           
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AddProgress(QuestType.SpendCookies, amount);
            }

            
            CheckAutoSave();

            Debug.Log($"Successfully spent {amount} cookies. Remaining: {cookieCount}");
            return true;
        }

        Debug.Log($"Not enough cookies! Need: {amount}, Have: {cookieCount}");
        return false;
    }

    
    public int GetCookieCount()
    {
        return cookieCount;
    }

    
    public int GetClickPower()
    {
        return baseClickPower + clickMultiplier;
    }

    
    public void AddClickMultiplier(int amount)
    {
        clickMultiplier += amount;
        multipliersBought++;

       
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.BuyMultiplier, 1);
        }

        
        TriggerSave();

        Debug.Log($"Click power increased! Now: {GetClickPower()} per click");
    }

   
    public void AddAutoClickRate(float rate)
    {
        autoClickRate += rate;
        autoClickersBought++;

       
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.BuyAutoClicker, 1);
        }

       
        TriggerSave();

        Debug.Log($"Auto click rate increased! Now: {autoClickRate} clicks per second");
    }

   
    public void OnSkinChanged()
    {
        skinsChanged++;

       
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.ChangeCookieSkin, 1);
        }

        
        TriggerSave();

        Debug.Log($"Cookie skin changed! Total changes: {skinsChanged}");
    }

    private void UpdateCookieCounterUI()
    {
        if (cookieCounterText != null)
        {
            cookieCounterText.text = $"Cookies: {cookieCount}";
        }
    }

   
    public void OnCookieClicked()
    {
        AddCookie(GetClickPower(), true);
    }

    
    public int GetClickMultiplier() => clickMultiplier;
    public float GetAutoClickRate() => autoClickRate;
    public int GetTotalCookiesEarned() => totalCookiesEarned;
    public int GetTotalCookiesSpent() => totalCookiesSpent;
    public int GetTotalClicks() => totalClicks;
    public int GetAutoClickersBought() => autoClickersBought;
    public int GetMultipliersBought() => multipliersBought;
    public int GetSkinsChanged() => skinsChanged;
}
