using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI rewardText;
    public Slider progressSlider;
    public Button claimButton;
    public Image questIcon;

    [Header("Status Colors")]
    public Color inProgressColor = Color.white;
    public Color completedColor = Color.green;
    public Color claimedColor = Color.gray;

    private Quest currentQuest;

    private void Start()
    {
        if (claimButton != null)
        {
            claimButton.onClick.AddListener(OnClaimButtonClicked);
        }
    }

    public void SetData(Quest quest)
    {
        currentQuest = quest;
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (currentQuest == null || currentQuest.data == null) return;

        // Update basic info
        if (titleText != null)
            titleText.text = currentQuest.data.questTitle;

        if (descriptionText != null)
            descriptionText.text = currentQuest.data.GetFormattedDescription();

        if (rewardText != null)
            rewardText.text = $"+{currentQuest.data.rewardCookies} cookies";

        // Update progress
        if (progressSlider != null)
            progressSlider.value = currentQuest.GetProgressNormalized();

        if (progressText != null)
            progressText.text = currentQuest.GetProgressText();

        // Update claim button
        UpdateClaimButton();

        // Update colors based on status
        UpdateStatusColors();
    }

    private void UpdateClaimButton()
    {
        if (claimButton == null) return;

        bool canClaim = currentQuest.isCompleted && !currentQuest.isRewarded;
        claimButton.gameObject.SetActive(canClaim);
        claimButton.interactable = canClaim;

        // Debug untuk troubleshoot button
        Debug.Log($"[QuestUI] Quest: {currentQuest.data.questTitle} - Completed: {currentQuest.isCompleted} - Rewarded: {currentQuest.isRewarded} - CanClaim: {canClaim}");

        if (canClaim)
        {
            var buttonText = claimButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = "CLAIM";
        }
    }

    private void UpdateStatusColors()
    {
        Color statusColor;

        if (currentQuest.isCompleted && currentQuest.isRewarded)
            statusColor = claimedColor;
        else if (currentQuest.isCompleted)
            statusColor = completedColor;
        else
            statusColor = inProgressColor;

        // Apply color to various elements
        if (titleText != null)
            titleText.color = statusColor;

        if (progressSlider != null)
        {
            var fillImage = progressSlider.fillRect?.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = statusColor;
        }
    }

    private void OnClaimButtonClicked()
    {
        Debug.Log($"[QuestUI] Claim button clicked for quest: {currentQuest.data.questTitle}");

        if (currentQuest != null && currentQuest.isCompleted && !currentQuest.isRewarded)
        {
            // Use QuestManager's reward claiming system instead of doing it manually
            if (QuestManager.Instance != null)
            {
                bool success = QuestManager.Instance.ClaimQuestReward(currentQuest);
                if (success)
                {
                    Debug.Log($"[QuestUI] Successfully claimed quest reward: {currentQuest.data.rewardCookies} cookies");
                    // QuestManager will handle UI updates through events
                }
                else
                {
                    Debug.LogWarning($"[QuestUI] Failed to claim quest reward for: {currentQuest.data.questTitle}");
                }
            }
            else
            {
                Debug.LogError("[QuestUI] QuestManager.Instance is null!");
            }
        }
        else
        {
            Debug.LogWarning($"[QuestUI] Cannot claim reward - Quest: {(currentQuest != null ? currentQuest.data.questTitle : "NULL")}, Completed: {(currentQuest != null ? currentQuest.isCompleted.ToString() : "NULL")}, Rewarded: {(currentQuest != null ? currentQuest.isRewarded.ToString() : "NULL")}");
        }
    }

    // ADD MISSING METHOD: GetProgressInfo for debugging
    public string GetProgressInfo()
    {
        if (currentQuest == null || currentQuest.data == null)
            return "Quest is NULL";

        return $"{currentQuest.data.questTitle}: {currentQuest.currentProgress}/{currentQuest.data.targetAmount} - Completed: {currentQuest.isCompleted} - Rewarded: {currentQuest.isRewarded}";
    }

    // Helper method to check if this quest UI is ready to claim
    public bool IsReadyToClaim()
    {
        return currentQuest != null && currentQuest.isCompleted && !currentQuest.isRewarded;
    }

    // Get the current quest reference
    public Quest GetQuest()
    {
        return currentQuest;
    }
}