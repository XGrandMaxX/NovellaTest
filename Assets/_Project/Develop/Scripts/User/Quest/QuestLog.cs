using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class QuestLog : MonoBehaviour
{
    [SerializeField] private QuestUI questEntryPrefab;
    [SerializeField] private Transform questListParent;
    [Space(20)]
    [SerializeField] private QuestReward rewardEntryPrefab;
    [SerializeField] private Text completedObjectivesCountText;

    public event Action<QuestUI, QuestSO> OnQuestCreated;
    private void Awake() => gameObject.SetActive(false);
    private void OnEnable() => RefreshUI();
    public void UpdateCompletedObjectivesUI(List<QuestSO> quests)
    {
        int savedCompletedObjectives = PlayerPrefs.GetInt("completedObjectives", 0);
        completedObjectivesCountText.text = $"Completed objectives: {savedCompletedObjectives}";

        int actualCompletedObjectives = 0;
        foreach (var quest in quests)
        {
            actualCompletedObjectives += quest.Objectives.Count(obj => obj.IsCompleted);
        }

        if (actualCompletedObjectives != savedCompletedObjectives && actualCompletedObjectives > 0)
        {
            PlayerPrefs.SetInt("completedObjectives", actualCompletedObjectives);
            PlayerPrefs.Save();

            completedObjectivesCountText.text = $"Completed objectives: {actualCompletedObjectives}";
        }
    }

    public void RefreshUI(bool withAnimation = true)
    {
        foreach (Transform child in questListParent)
            Destroy(child.gameObject);

        var quests = G.QuestManager != null ? G.QuestManager.activeQuests : null;
        if (quests == null || quests.Count == 0)
            return;

        UpdateCompletedObjectivesUI(quests);

        foreach (var quest in quests)
        {
            var entry = Instantiate(questEntryPrefab, questListParent);
            if(withAnimation)
                DOTweenAnimationManager.AnimateShowAsync(entry.transform, ease: entry.SpawnEase).Forget();

            var questId = quests.IndexOf(quest);
            entry.TitleText.text = $"{questId+1}.{quest.QuestTitle}";

            
            bool isCompleted = quest.IsCompleted;
            bool accepted = quest.Accepted;

            entry.ClaimButton.gameObject.SetActive(accepted);
            entry.AcceptButton.gameObject.SetActive(!accepted && !isCompleted);

            var objective = quest.Objectives.FirstOrDefault(obj => obj != null && !obj.IsCompleted && !string.IsNullOrWhiteSpace(obj.Description));
            if (objective != null)
                entry.DescriptionText.text = objective.Description;
            else if (objective == null && isCompleted)
                entry.DescriptionText.text = "Completed! Claim your reward";
            else
                entry.DescriptionText.text = quest.Description;

            if (accepted && !isCompleted)
            {
                entry.AcceptButton.gameObject.SetActive(false);
                entry.ClaimButton.interactable = false;
                SetInProgressAnimation(entry.ClaimButton);
            }
            else if (isCompleted)
            {
                quest.Accepted = false;
                entry.ClaimButton.gameObject.SetActive(true);
                entry.ClaimButton.interactable = true;

                DOTweenAnimationManager.StopTweenIfExists(entry.ClaimButton.transform);
                entry.ClaimButton.GetComponentInChildren<Text>().text = "CLAIM";
            }

            if (quest.Rewards != null)
            {
                foreach (var reward in quest.Rewards)
                {
                    var rewardEntry = Instantiate(rewardEntryPrefab, entry.RewardParentObject);
                    rewardEntry.Initialize(reward);
                }
            }

            OnQuestCreated?.Invoke(entry, quest);
        }
    }

    private void SetInProgressAnimation(Button button)
    {
        var text = button.GetComponentInChildren<Text>();
        if (text != null)
            DOTweenAnimationManager.DotAnimation(text, "InProgress", 0.5f);
    }
}
