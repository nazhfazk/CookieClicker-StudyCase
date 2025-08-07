using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Complete Quest Panel Manager - Fixed version with proper reward claiming
/// Fixes: GetProgressInfo method + Button click handling + Event management
/// </summary>
public class QuestPanelManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform questParent; // This should be the "Content" GameObject
    public GameObject questUIPrefab;
    public TextMeshProUGUI countdownText;

    [Header("Layout Settings")]
    [SerializeField] private float questPrefabHeight = 150f; // Height for each quest prefab
    [SerializeField] private bool debugLayout = true; // Enable layout debugging
    [SerializeField] private bool debugQuestProgress = true; // Enable quest progress debugging

    private List<QuestUI> questUIs = new List<QuestUI>();
    private bool isInitialized = false;

    #region Unity Lifecycle
    private void Start()
    {
        // Set countdown text reference in QuestManager
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SetCountdownText(countdownText);
        }

        // Verify layout components
        VerifyLayoutComponents();
    }

    private void OnEnable()
    {
        // Subscribe to events when panel becomes active
        InitPanel();
    }

    private void OnDisable()
    {
        // Unsubscribe from events when panel becomes inactive
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshUI;
            QuestManager.Instance.OnQuestUpdated -= UpdateQuestProgress;
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnQuestUpdated -= RefreshUI;
            QuestManager.Instance.OnQuestUpdated -= UpdateQuestProgress;
        }
    }
    #endregion

    #region Layout System Fix
    /// <summary>
    /// Verify that all necessary layout components are present and configured
    /// </summary>
    private void VerifyLayoutComponents()
    {
        if (questParent == null)
        {
            Debug.LogError("[QuestPanelManager] Quest Parent (Content) is not assigned!");
            return;
        }

        // Check for Vertical Layout Group
        VerticalLayoutGroup layoutGroup = questParent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            Debug.LogWarning("[QuestPanelManager] Adding missing VerticalLayoutGroup to Content");
            layoutGroup = questParent.gameObject.AddComponent<VerticalLayoutGroup>();
        }

        // Configure Vertical Layout Group - IMPORTANT SETTINGS!
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false; // CRITICAL: False to prevent overlap
        layoutGroup.spacing = 20f;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;

        // Check for Content Size Fitter
        ContentSizeFitter sizeFitter = questParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            Debug.LogWarning("[QuestPanelManager] Adding missing ContentSizeFitter to Content");
            sizeFitter = questParent.gameObject.AddComponent<ContentSizeFitter>();
        }

        // Configure Content Size Fitter - IMPORTANT SETTINGS!
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // CRITICAL

        if (debugLayout)
        {
            Debug.Log("[QuestPanelManager] Layout components verified and configured");
        }
    }

    /// <summary>
    /// Force rebuild layout to fix positioning issues
    /// </summary>
    private void ForceLayoutRebuild()
    {
        if (questParent == null) return;

        RectTransform contentRect = questParent.GetComponent<RectTransform>();
        if (contentRect != null)
        {
            // Force layout rebuild
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

            // If parent has layout, rebuild that too
            Transform parentOfParent = questParent.parent;
            if (parentOfParent != null)
            {
                RectTransform parentRect = parentOfParent.GetComponent<RectTransform>();
                if (parentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                }
            }

            if (debugLayout)
            {
                Debug.Log("[QuestPanelManager] Forced layout rebuild");
            }
        }
    }
    #endregion

    #region Quest System Integration
    public void InitPanel()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("[QuestPanelManager] QuestManager.Instance is null!");
            return;
        }

        if (debugQuestProgress)
        {
            Debug.Log("[QuestPanelManager] Initializing panel...");
        }

        // Unsubscribe first to prevent duplicate subscriptions
        QuestManager.Instance.OnQuestUpdated -= RefreshUI;
        QuestManager.Instance.OnQuestUpdated -= UpdateQuestProgress;

        // Subscribe to events for quest progress updates
        QuestManager.Instance.OnQuestUpdated += RefreshUI;
        QuestManager.Instance.OnQuestUpdated += UpdateQuestProgress;

        // Initial UI setup
        RefreshUI();
        isInitialized = true;
    }

    private void RefreshUI()
    {
        if (debugQuestProgress)
        {
            Debug.Log("[QuestPanelManager] RefreshUI called");
        }

        if (QuestManager.Instance == null || QuestManager.Instance.activeQuests == null)
        {
            Debug.LogError("[QuestPanelManager] QuestManager or activeQuests is null!");
            return;
        }

        // Clear existing UI properly
        ClearQuestUIs();

        // Wait one frame to ensure cleanup is complete, then create new UIs
        StartCoroutine(CreateQuestUIsDelayed());
    }

    /// <summary>
    /// Create quest UIs with proper delay for layout system
    /// </summary>
    private IEnumerator CreateQuestUIsDelayed()
    {
        yield return new WaitForEndOfFrame();

        // Create new quest UIs
        foreach (var quest in QuestManager.Instance.activeQuests)
        {
            CreateQuestUI(quest);
        }

        if (debugQuestProgress)
        {
            Debug.Log($"[QuestPanelManager] Created {questUIs.Count} quest UIs");
        }

        // Force layout rebuild after creating all UIs
        yield return new WaitForEndOfFrame();
        ForceLayoutRebuild();
    }

    private void ClearQuestUIs()
    {
        // Clear our list first
        questUIs.Clear();

        // Destroy all children in the quest parent
        if (questParent != null)
        {
            for (int i = questParent.childCount - 1; i >= 0; i--)
            {
                Transform child = questParent.GetChild(i);
                if (child != null)
                {
                    if (Application.isPlaying)
                        Destroy(child.gameObject);
                    else
                        DestroyImmediate(child.gameObject);
                }
            }
        }

        if (debugLayout)
        {
            Debug.Log("[QuestPanelManager] Cleared all quest UIs");
        }
    }

    private void CreateQuestUI(Quest quest)
    {
        if (questUIPrefab == null || questParent == null)
        {
            Debug.LogError("[QuestPanelManager] QuestUI prefab or parent is null!");
            return;
        }

        if (quest == null || quest.data == null)
        {
            Debug.LogError("[QuestPanelManager] Quest or quest data is null!");
            return;
        }

        // Instantiate quest UI prefab
        GameObject questObj = Instantiate(questUIPrefab, questParent);

        // Configure Layout Element for proper sizing (FIX LAYOUT)
        LayoutElement layoutElement = questObj.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = questObj.AddComponent<LayoutElement>();
        }

        // Configure layout element settings
        layoutElement.ignoreLayout = false;
        layoutElement.minHeight = questPrefabHeight;
        layoutElement.preferredHeight = questPrefabHeight;
        layoutElement.flexibleHeight = 0f;
        layoutElement.flexibleWidth = 1f;

        // Configure RectTransform for proper anchoring (FIX LAYOUT)
        RectTransform rectTransform = questObj.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(0, questPrefabHeight);
        }

        // Setup quest UI component (FIX QUEST COMPLETION)
        QuestUI questUI = questObj.GetComponent<QuestUI>();
        if (questUI == null)
        {
            Debug.LogError("[QuestPanelManager] QuestUI component not found on prefab!");
            Destroy(questObj);
            return;
        }

        // Initialize quest UI with data
        questUI.SetData(quest);
        questUIs.Add(questUI);

        if (debugQuestProgress)
        {
            Debug.Log($"[QuestPanelManager] Created UI for quest: {quest.data.questTitle} " +
                     $"(Progress: {quest.currentProgress}/{quest.data.targetAmount}, " +
                     $"Completed: {quest.isCompleted}, Rewarded: {quest.isRewarded})");
        }
    }

    /// <summary>
    /// Update quest progress for all active quest UIs (FIX QUEST COMPLETION)
    /// This method is called when quest progress changes
    /// </summary>
    public void UpdateQuestProgress()
    {
        if (!isInitialized) return;

        if (debugQuestProgress)
        {
            Debug.Log("[QuestPanelManager] Updating quest progress for all UIs");
        }

        // Update each quest UI with current progress
        for (int i = 0; i < questUIs.Count; i++)
        {
            if (questUIs[i] != null)
            {
                questUIs[i].UpdateDisplay();

                if (debugQuestProgress)
                {
                    Debug.Log($"[QuestPanelManager] Updated UI {i}: {questUIs[i].GetProgressInfo()}");
                }
            }
        }

        // Check for completed quests that are ready to claim
        CheckForReadyToClaim();
    }

    /// <summary>
    /// Check and highlight quests that are ready to claim rewards
    /// </summary>
    private void CheckForReadyToClaim()
    {
        int readyToClaimCount = 0;

        foreach (var questUI in questUIs)
        {
            if (questUI != null && questUI.IsReadyToClaim())
            {
                readyToClaimCount++;
            }
        }

        if (readyToClaimCount > 0 && debugQuestProgress)
        {
            Debug.Log($"[QuestPanelManager] {readyToClaimCount} quests are ready to claim rewards");
        }
    }
    #endregion

    #region Debug & Utility Methods
    /// <summary>
    /// Force refresh all quest UIs (useful for debugging)
    /// </summary>
    [ContextMenu("Force Refresh UI")]
    public void ForceRefreshUI()
    {
        RefreshUI();
    }

    /// <summary>
    /// Debug method to check layout status
    /// </summary>
    [ContextMenu("Debug Layout Info")]
    public void DebugLayoutInfo()
    {
        Debug.Log("=== QUEST PANEL LAYOUT DEBUG ===");
        Debug.Log($"Quest Parent: {(questParent != null ? questParent.name : "NULL")}");
        Debug.Log($"Active Quest UIs: {questUIs.Count}");
        Debug.Log($"Children in Parent: {(questParent != null ? questParent.childCount : 0)}");

        if (questParent != null)
        {
            VerticalLayoutGroup layout = questParent.GetComponent<VerticalLayoutGroup>();
            ContentSizeFitter fitter = questParent.GetComponent<ContentSizeFitter>();

            Debug.Log($"Layout Group: {(layout != null ? "Present" : "Missing")}");
            Debug.Log($"Content Size Fitter: {(fitter != null ? "Present" : "Missing")}");

            if (layout != null)
            {
                Debug.Log($"Layout - Control Height: {layout.childControlHeight}, Force Expand Height: {layout.childForceExpandHeight}");
            }
        }
    }

    /// <summary>
    /// Debug method to check quest progress
    /// </summary>
    [ContextMenu("Debug Quest Progress")]
    public void DebugQuestProgress()
    {
        Debug.Log("=== QUEST PROGRESS DEBUG ===");

        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager.Instance is NULL!");
            return;
        }

        Debug.Log($"Active Quests in Manager: {QuestManager.Instance.activeQuests.Count}");
        Debug.Log($"Active Quest UIs in Panel: {questUIs.Count}");

        for (int i = 0; i < questUIs.Count; i++)
        {
            if (questUIs[i] != null)
            {
                Debug.Log($"Quest UI {i}: {questUIs[i].GetProgressInfo()}");
            }
        }
    }

    /// <summary>
    /// Debug method to force complete a quest for testing
    /// </summary>
    [ContextMenu("DEBUG: Complete First Quest")]
    public void DebugCompleteFirstQuest()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.activeQuests.Count > 0)
        {
            var firstQuest = QuestManager.Instance.activeQuests[0];
            if (!firstQuest.isCompleted)
            {
                firstQuest.AddProgress(firstQuest.data.targetAmount - firstQuest.currentProgress);
                Debug.Log($"[DEBUG] Completed quest: {firstQuest.data.questTitle}");

                // Trigger UI update
                QuestManager.Instance.TriggerQuestUpdate();
            }
            else
            {
                Debug.Log($"[DEBUG] Quest {firstQuest.data.questTitle} is already completed");
            }
        }
    }

    /// <summary>
    /// Debug method to check button states
    /// </summary>
    [ContextMenu("DEBUG: Check Button States")]
    public void DebugButtonStates()
    {
        Debug.Log("=== BUTTON STATES DEBUG ===");
        for (int i = 0; i < questUIs.Count; i++)
        {
            if (questUIs[i] != null && questUIs[i].claimButton != null)
            {
                bool buttonActive = questUIs[i].claimButton.gameObject.activeInHierarchy;
                bool buttonInteractable = questUIs[i].claimButton.interactable;
                bool questCompleted = questUIs[i].GetQuest().isCompleted;
                bool questRewarded = questUIs[i].GetQuest().isRewarded;

                Debug.Log($"Quest {i} ({questUIs[i].GetQuest().data.questTitle}): " +
                         $"Button Active: {buttonActive}, Interactable: {buttonInteractable}, " +
                         $"Completed: {questCompleted}, Rewarded: {questRewarded}");
            }
        }
    }

    /// <summary>
    /// Get the number of currently displayed quest UIs
    /// </summary>
    public int GetActiveQuestUICount()
    {
        return questUIs.Count;
    }
    #endregion
}