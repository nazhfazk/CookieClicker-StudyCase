using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest Data")]
public class QuestData : ScriptableObject
{
    [Header("Quest Info")]
    public string questTitle;
    [TextArea(2, 4)]
    public string description;

    [Header("Quest Requirements")]
    public QuestType questType;
    public int targetAmount;

    [Header("Rewards")]
    public int rewardCookies;

    //[Header("Display")]
    //public Sprite questIcon;

    public string GetFormattedDescription()
    {
        return description.Replace("{target}", targetAmount.ToString());
    }
}