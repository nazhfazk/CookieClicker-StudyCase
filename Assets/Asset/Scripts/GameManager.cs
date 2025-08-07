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
                AddCookie(GetClickPower(), false); // Auto click doesn't count as manual click
                autoClickTimer = 0f;
            }
        }
    }

    // Add cookies to counter
    public void AddCookie(int amount, bool isManualClick = true)
    {
        cookieCount += amount;
        totalCookiesEarned += amount;

        UpdateCookieCounterUI();

        // Track quest progress
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.EarnCookies, amount);

            if (isManualClick)
            {
                totalClicks++;
                QuestManager.Instance.AddProgress(QuestType.ClickCertainAmount, 1);
            }
        }

        Debug.Log($"Added {amount} cookies. Total: {cookieCount}");
    }

    // Spend cookies (untuk shop)
    public bool SpendCookies(int amount)
    {
        Debug.Log($"Trying to spend {amount} cookies. Have: {cookieCount}");

        if (cookieCount >= amount)
        {
            cookieCount -= amount;
            totalCookiesSpent += amount;

            UpdateCookieCounterUI();

            // Track quest progress
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.AddProgress(QuestType.SpendCookies, amount);
            }

            Debug.Log($"Successfully spent {amount} cookies. Remaining: {cookieCount}");
            return true;
        }

        Debug.Log($"Not enough cookies! Need: {amount}, Have: {cookieCount}");
        return false;
    }

    // Get current cookie count
    public int GetCookieCount()
    {
        return cookieCount;
    }

    // Get current click power (base + multiplier)
    public int GetClickPower()
    {
        return baseClickPower + clickMultiplier;
    }

    // Add click multiplier from shop items
    public void AddClickMultiplier(int amount)
    {
        clickMultiplier += amount;
        multipliersBought++;

        // Track quest progress
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.BuyMultiplier, 1);
        }

        Debug.Log($"Click power increased! Now: {GetClickPower()} per click");
    }

    // Add auto click rate from shop items
    public void AddAutoClickRate(float rate)
    {
        autoClickRate += rate;
        autoClickersBought++;

        // Track quest progress
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.BuyAutoClicker, 1);
        }

        Debug.Log($"Auto click rate increased! Now: {autoClickRate} clicks per second");
    }

    // Track skin 
    public void OnSkinChanged()
    {
        skinsChanged++;

        // Track quest progress
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.AddProgress(QuestType.ChangeCookieSkin, 1);
        }

        Debug.Log($"Cookie skin changed! Total changes: {skinsChanged}");
    }

    // Update UI counter display
    private void UpdateCookieCounterUI()
    {
        if (cookieCounterText != null)
        {
            cookieCounterText.text = $"Cookies: {cookieCount}";
        }
    }

    // Method untuk manual click
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