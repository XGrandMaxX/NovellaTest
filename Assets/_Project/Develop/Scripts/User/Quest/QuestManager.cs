using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    private QuestLog questLog;
    public List<QuestSO> activeQuests = new();

    #region Initialize
    private void Awake()
    {
        if (G.QuestManager != null)
            Destroy(gameObject);
        else
            G.QuestManager = this;

        questLog = GetComponent<QuestLog>();
        questLog.OnQuestCreated += SubscribeQuestButtons;
        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        questLog.OnQuestCreated -= SubscribeQuestButtons;
    }

    private void SubscribeQuestButtons(QuestUI quest, QuestSO questSO)
    {
        quest.AcceptButton.onClick.RemoveAllListeners();
        quest.ClaimButton.onClick.RemoveAllListeners();

        quest.AcceptButton.onClick.AddListener(() => QuestAccept(quest, questSO));
        quest.ClaimButton.onClick.AddListener(() => ClaimReward(quest, questSO));
    }
    #endregion

    #region Quests
    public void AddQuest(QuestSO newQuest)
    {
        if (!activeQuests.Contains(newQuest))
        {
            activeQuests.Add(newQuest);
            RefreshQuestLogUI();
        }
    }

    private void QuestAccept(QuestUI quest, QuestSO questSO)
    {
        questSO.Accepted = true;

        //quest.AcceptButton.gameObject.SetActive(false);
        //quest.ClaimButton.gameObject.SetActive(true);
        //quest.ClaimButton.interactable = false;

        var buttonText = quest.ClaimButton.GetComponentInChildren<Text>();
        DOTweenAnimationManager.DotAnimation(buttonText, "InProgress", 0.5f);

        RefreshQuestLogUI(false);
    }

    public void RefreshQuestLogUI(bool withAnimation = true) => questLog.RefreshUI(withAnimation);

    private async void ClaimReward(QuestUI quest, QuestSO questSO)
    {
        //WIP
        questSO.Accepted = false;
        activeQuests.Remove(questSO);

        await DOTweenAnimationManager.AnimateHideAsync(quest.transform, ease: quest.SpawnEase);

        Destroy(quest.gameObject);
    }


    public void OnItemCollected(string itemID)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.Objectives.OfType<CollectItemObjective>())
            {
                obj.AddItem(itemID);
            }
        }
    }
    public void OnNPCTalked(string npcID)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.Objectives.OfType<TalkToNPCObjective>())
            {
                obj.MarkTalked(npcID);
            }
        }
    }
    public void ManuallyCompleteObjective(string questID)
    {
        var quest = activeQuests.FirstOrDefault(q => q.QuestID == questID);
        if (quest == null) return;

        foreach (var obj in quest.Objectives.OfType<ManualCompleteObjective>())
        {
            obj.CompleteManually();
        }
    }

    #endregion
}
