using UnityEngine;
using UnityEngine.UI;

public class QuestReward : MonoBehaviour
{
    public string RewardID;

    public int Value;
    public Text ValueText;

    public Image RewardImage;

    public void Initialize(QuestRewardSO questRewardSO)
    {
        RewardID = questRewardSO.RewardID;

        Value = questRewardSO.Value;
        ValueText.text = $"x {questRewardSO.Value}";

        RewardImage.sprite = questRewardSO.RewardIcon;
    }
}
