using NaughtyAttributes;
using UnityEngine;


[System.Serializable]
public abstract class QuestObjective : IQuestObjective
{
    [field: ResizableTextArea, Label("Description (Press Enter)")]
    [field: SerializeField] public string Description { get; private set; }

    //Только для инспектора
    [ReadOnly, AllowNesting]
    [Label("Completed")]
    [SerializeField] private bool completedPreview;

    public abstract bool IsCompleted { get; }

    public void UpdateCompletedPreview() => completedPreview = IsCompleted;

    public abstract void Reset();
}


[System.Serializable]
public class CollectItemObjective : QuestObjective
{
    public string ItemID;
    public int RequiredAmount = 1;
    private int CurrentAmount;

    public override bool IsCompleted => CurrentAmount >= RequiredAmount;

    public CollectItemObjective() => Reset();

    public void AddItem(string collectedItemID)
    {
        if (collectedItemID == ItemID)
        {
            CurrentAmount++;
            if (IsCompleted)
                UpdateCompletedPreview();
        }
    }

    public override void Reset()
    {
        CurrentAmount = 0;
        UpdateCompletedPreview();
    }

    public int GetCurrentAmount() => CurrentAmount;
    public float GetProgress() => RequiredAmount > 0 ? (float)CurrentAmount / RequiredAmount : 0f;
}

[System.Serializable]
public class TalkToNPCObjective : QuestObjective
{
    public string NpcID;
    private bool Talked = false;

    public override bool IsCompleted => Talked;

    public TalkToNPCObjective() => Reset();

    public void MarkTalked(string npc)
    {
        if (npc == NpcID)
        {
            Talked = true;
            Debug.Log($"Вы поговорили с NPC {npc}");
            UpdateCompletedPreview();
        }
    }

    public override void Reset()
    {
        Talked = false;
        UpdateCompletedPreview();
    }
}

[System.Serializable]
public class ManualCompleteObjective : QuestObjective
{
    private bool isCompleted = false;

    public override bool IsCompleted => isCompleted;

    public ManualCompleteObjective() => Reset();

    public void CompleteManually()
    {
        isCompleted = true;
        UpdateCompletedPreview();
    }

    public override void Reset()
    {
        isCompleted = false;
        UpdateCompletedPreview();
    }
}