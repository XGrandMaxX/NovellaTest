using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/New Quest")]
public class QuestSO : ScriptableObject
{
    public string QuestID;
    public string QuestTitle;
    [Label("Description (Press Enter)")]
    [ResizableTextArea] public string Description;
    [InfoBox("Создавайте задания через кнопки снизу")]
    [SerializeReference] public List<QuestObjective> Objectives;

    public List<QuestRewardSO> Rewards;
    public bool IsCompleted => Objectives != null && Objectives.TrueForAll(obj => obj.IsCompleted);

    [ReadOnly] public bool Accepted;

    private void OnEnable()
    {
        ResetQuest();
        UpdateAllPreviews();

        Accepted = false;
    }

    public void ResetQuest()
    {
        for (int i = 0; i < Objectives.Count; i++)
        {
            ResetObjective(i);
        }
        Debug.Log($"Квест '<color=cyan>{QuestTitle} ID:{QuestID}</color>' был сброшен");
    }

    public void ResetObjective(int objectiveIndex)
    {
        if (objectiveIndex >= 0 && objectiveIndex < Objectives.Count)
        {
            Objectives[objectiveIndex].Reset();
            Debug.Log($"Цель квеста '<color=green>{Objectives[objectiveIndex].Description}</color>' была сброшена");
        }
    }

    public void UpdateAllPreviews()
    {
        foreach (var objective in Objectives)
        {
            objective.UpdateCompletedPreview();
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(QuestSO))]
public class QuestSOEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add CollectItemObjective"))
        {
            var quest = (QuestSO)target;
            quest.Objectives ??= new List<QuestObjective>();
            quest.Objectives.Add(new CollectItemObjective());
            UnityEditor.EditorUtility.SetDirty(quest);
        }
        if (GUILayout.Button("Add TalkToNPCObjective"))
        {
            var quest = (QuestSO)target;
            quest.Objectives ??= new List<QuestObjective>();
            quest.Objectives.Add(new TalkToNPCObjective());
            UnityEditor.EditorUtility.SetDirty(quest);
        }

        if (GUILayout.Button("Add ManualObjective"))
        {
            var quest = (QuestSO)target;
            quest.Objectives ??= new List<QuestObjective>();
            quest.Objectives.Add(new ManualCompleteObjective());
            UnityEditor.EditorUtility.SetDirty(quest);
        }
    }
}
#endif