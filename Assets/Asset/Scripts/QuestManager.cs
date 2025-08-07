using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

/// <summary>
/// Fixed Quest Manager - Handles quest logic, progress tracking, and rewards
/// Fixes: Proper quest completion flow, progress updates, and reward claiming
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Config")]
    public List<QuestData> allQuests;
    public int activeQuestCount = 3;
    public float refreshInterval = 300f; // 5 minutes in seconds

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Active Quests (Runtime)")]
    public List<Quest> activeQuests = new List<Quest>();

    // Events for UI updates
    public delegate void QuestUpdated();
    public event QuestUpdated OnQuestUpdated;

    private float nextRefreshTime;
    private bool isInitialized = false;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeQuests();
    }

    private void Update()
    {
        if (!isInitialized) return;

        UpdateCountdownUI();

        // Check for automatic quest refresh (every 5 minutes)
        if (Time.time >= nextRefreshTime)
        {
            Debug.Log("[QuestManager] Auto-refreshing quests due to timer");
            RefreshQuests();
            nextRefreshTime = Time.time + refreshInterval;
        }
    }
    #endregion

    #region Initialization
    private void InitializeQuests()
    {
        Debug.Log("[QuestManager] Initializing quest system...");

        CreateDefaultQuests();
        RefreshQuests();
        nextRefreshTime = Time.time + refreshInterval;
        isInitialized = true;

        Debug.Log($"[QuestManager] Initialized with {activeQuests.Count} active quests");
    }

    private void CreateDefaultQuests()
    {
        if (allQuests == null) allQuests = new List<QuestData>();

        // Clear existing quests to prevent duplicates
        allQuests.Clear();

        // Create 6 different quests programmatically
        allQuests.Add(CreateQuestData("Cookie Clicker Novice", "Click cookies {target} times", QuestType.ClickCertainAmount, 50, 100));
        allQuests.Add(CreateQuestData("Big Spender", "Spend {target} cookies", QuestType.SpendCookies, 200, 150));
        allQuests.Add(CreateQuestData("Cookie Collector", "Earn {target} cookies total", QuestType.EarnCookies, 500, 250));
        allQuests.Add(CreateQuestData("Automation Enthusiast", "Buy {target} Auto Clickers", QuestType.BuyAutoClicker, 2, 200));
        allQuests.Add(CreateQuestData("Power User", "Buy {target} Click Multipliers", QuestType.BuyMultiplier, 3, 300));
        allQuests.Add(CreateQuestData("Style Master", "Change cookie skin {target} time", QuestType.ChangeCookieSkin, 1, 100));

        Debug.Log($"[QuestManager] Created {allQuests.Count} quest templates");
    }

    private QuestData CreateQuestData(string title, string description, QuestType type, int target, int reward)
    {
        QuestData quest = ScriptableObject.CreateInstance<QuestData>();
        quest.questTitle = title;
        quest.description = description;
        quest.questType = type;
        quest.targetAmount = target;
        quest.rewardCookies = reward;
        return quest;
    }
    #endregion

    #region Quest Management
    private void RefreshQuests()
    {
        Debug.Log("[QuestManager] Refreshing quests...");

        activeQuests.Clear();

        // Shuffle and select random quests
        List<QuestData> shuffled = new List<QuestData>(allQuests);
        for (int i = 0; i < shuffled.Count; i++)
        {
            QuestData temp = shuffled[i];
            int randomIndex = Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        // Take up to activeQuestCount quests
        int questsToTake = Mathf.Min(activeQuestCount, shuffled.Count);
        for (int i = 0; i < questsToTake; i++)
        {
            activeQuests.Add(new Quest(shuffled[i]));
        }

        Debug.Log($"[QuestManager] Created {activeQuests.Count} active quests");

        // Notify UI to update
        OnQuestUpdated?.Invoke();
    }

    /// <summary>
    /// Add progress to quests of specific type
    /// Fixed: Now properly triggers UI updates when progress changes
    /// </summary>
    public void AddProgress(QuestType type, int amount = 1)
    {
        bool hasProgress = false;

        foreach (var quest in activeQuests)
        {
            if (quest.data.questType == type && !quest.isCompleted)
            {
                int oldProgress = quest.currentProgress;
                quest.AddProgress(amount);

                if (quest.currentProgress > oldProgress)
                {
                    hasProgress = true;
                    Debug.Log($"[QuestManager] Quest '{quest.data.questTitle}' progress: {quest.currentProgress}/{quest.data.targetAmount}");

                    // Check if quest just completed
                    if (quest.isCompleted && !quest.isRewarded)
                    {
                        Debug.Log($"[QuestManager] Quest '{quest.data.questTitle}' completed! Ready to claim reward.");
                    }
                }
            }
        }

        // Trigger UI update if any progress was made
        if (hasProgress)
        {
            Debug.Log("[QuestManager] Triggering UI update due to quest progress");
            OnQuestUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Claim reward for a completed quest
    /// Fixed: Proper reward claiming flow
    /// </summary>
    public bool ClaimQuestReward(Quest quest)
    {
        if (quest == null || !quest.isCompleted || quest.isRewarded)
        {
            Debug.LogWarning("[QuestManager] Cannot claim reward - quest is null, not completed, or already rewarded");
            return false;
        }

        // Give reward to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCookie(quest.data.rewardCookies, false); // false = not manual click
            Debug.Log($"[QuestManager] Claimed quest reward: {quest.data.rewardCookies} cookies for '{quest.data.questTitle}'");
        }

        // Mark as rewarded
        quest.isRewarded = true;

        // Check if all quests are completed and claimed
        bool allQuestsClaimedOrIncomplete = true;
        foreach (var activeQuest in activeQuests)
        {
            if (activeQuest.isCompleted && !activeQuest.isRewarded)
            {
                allQuestsClaimedOrIncomplete = false;
                break;
            }
        }

        // If all completed quests are claimed, refresh quest list
        if (allQuestsClaimedOrIncomplete && HasAnyCompletedQuests())
        {
            Debug.Log("[QuestManager] All quests completed and claimed, refreshing quest list");
            RefreshQuests();
            nextRefreshTime = Time.time + refreshInterval; // Reset timer
        }
        else
        {
            // Just update UI
            OnQuestUpdated?.Invoke();
        }

        return true;
    }

    private bool HasAnyCompletedQuests()
    {
        foreach (var quest in activeQuests)
        {
            if (quest.isCompleted)
                return true;
        }
        return false;
    }
    #endregion

    #region UI Updates
    private void UpdateCountdownUI()
    {
        if (countdownText != null)
        {
            float timeLeft = nextRefreshTime - Time.time;

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
            }

            int minutes = Mathf.FloorToInt(timeLeft / 60f);
            int seconds = Mathf.FloorToInt(timeLeft % 60f);
            countdownText.text = $"Next Refresh: {minutes:00}:{seconds:00}";
        }
    }

    public void SetCountdownText(TextMeshProUGUI text)
    {
        countdownText = text;
    }

    /// <summary>
    /// Helper method to trigger quest update event manually
    /// </summary>
    public void TriggerQuestUpdate()
    {
        Debug.Log("[QuestManager] Manually triggering quest update");
        OnQuestUpdated?.Invoke();
    }
    #endregion

    #region Debug & Testing
    /// <summary>
    /// Force refresh for testing purposes
    /// </summary>
    [ContextMenu("Force Refresh Quests")]
    public void ForceRefreshQuests()
    {
        Debug.Log("[QuestManager] Force refreshing quests");
        RefreshQuests();
        nextRefreshTime = Time.time + refreshInterval;
    }

    public float GetTimeUntilRefresh()
    {
        return nextRefreshTime - Time.time;
    }

    /// <summary>
    /// Debug method to check quest status
    /// </summary>
    [ContextMenu("Debug Quest Status")]
    public void DebugQuestStatus()
    {
        Debug.Log($"[QuestManager] === QUEST STATUS DEBUG ===");
        Debug.Log($"Active Quests Count: {activeQuests.Count}");

        for (int i = 0; i < activeQuests.Count; i++)
        {
            var quest = activeQuests[i];
            Debug.Log($"Quest {i}: {quest.data.questTitle} - Progress: {quest.currentProgress}/{quest.data.targetAmount} - Completed: {quest.isCompleted} - Rewarded: {quest.isRewarded}");
        }
    }
    #endregion
}