using UnityEngine;

public class Quest
{
    public QuestData data;
    public int currentProgress;
    public bool isCompleted;
    public bool isRewarded; // Track if reward has been given

    public Quest(QuestData data)
    {
        this.data = data;
        this.currentProgress = 0;
        this.isCompleted = false;
        this.isRewarded = false;
    }

    public void AddProgress(int amount)
    {
        if (isCompleted) return;

        currentProgress += amount;
        if (currentProgress >= data.targetAmount)
        {
            currentProgress = data.targetAmount;
            isCompleted = true;
            Debug.Log($"Quest '{data.questTitle}' completed!");
        }
    }

    public float GetProgressNormalized()
    {
        return Mathf.Clamp01((float)currentProgress / data.targetAmount);
    }

    public string GetProgressText()
    {
        if (isCompleted)
        {
            return isRewarded ? "COMPLETED" : "CLAIM REWARD!";
        }
        return $"{currentProgress}/{data.targetAmount}";
    }

    public string GetStatusText()
    {
        if (isCompleted && isRewarded)
            return "COMPLETED";
        else if (isCompleted && !isRewarded)
            return "READY TO CLAIM";
        else
            return "IN PROGRESS";
    }
}