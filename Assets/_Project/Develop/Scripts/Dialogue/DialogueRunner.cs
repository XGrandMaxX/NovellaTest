using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

[RequireComponent(typeof(DialogueView), typeof(DialogueSceneEvents))]
public class DialogueRunner : MonoBehaviour
{
    private DialogueView _view;

    private DialogueSceneEvents _sceneEvents;

    [field: Tooltip("Set the initial dialogue to play when the scene starts")]
    [field: SerializeField] public DialogueSO CurrentDialogue { get; private set; }

    private Dictionary<string, DialogueNode> _nodeMap;
    private void Awake()
    {
        _view = GetComponent<DialogueView>();
        _sceneEvents = GetComponent<DialogueSceneEvents>();
    }
    private void Start()
    {
        if (CurrentDialogue != null)
            PlayDialogueAsync(CurrentDialogue).Forget();
    }

    public async UniTask PlayDialogueAsync(DialogueSO dialogue)
    {
        CurrentDialogue = dialogue;
        _nodeMap = new Dictionary<string, DialogueNode>();

        foreach (var node in dialogue.nodes)
            _nodeMap[node.GUID] = node;

        if (dialogue.nodes.Count == 0) return;

        await PlayNodeAsync(dialogue.nodes[0].GUID);
    }
    private async UniTask PlayNodeAsync(string guid)
    {
        if (!_nodeMap.TryGetValue(guid, out var node)) return;

        var ct = this.GetCancellationTokenOnDestroy();

        await UniTask.Delay(TimeSpan.FromSeconds(node.DialogueStartDelay), cancellationToken: ct);

        AddQuests(node);
        InvokeSceneEvents(node.SceneEventKeysOnStart).Forget();

        await UniTask.DelayFrame(3, PlayerLoopTiming.Update, cancellationToken: ct);
        
        var speakerName = (string.IsNullOrWhiteSpace(node.SpeakerName) || node.SpeakerName == "User") ? UserData.UserName : node.SpeakerName;
        SetSpeaker(node.SpeakerSide, speakerName);

        TryToCompleteDialogueQuest(node);

        switch (node.NodeType)
        {
            case DialogueNodeType.Text:

                await _view.ShowTextAsync(node.Text, node.SpeakerName, node.TextPrintSound, node.revealMode, node.revealSpeed, node.TextAlignment);

                if (node.AutoChangeDialogue)
                    await UniTask.Delay(TimeSpan.FromSeconds(node.AutoChangeDelay), cancellationToken: ct);
                else
                    await _view.WaitForContinue(ct);

                InvokeSceneEvents(node.SceneEventKeysOnEnd).Forget();
                G.QuestManager.RefreshQuestLogUI();

                if (!string.IsNullOrEmpty(node.NextNodeGuid))
                    await PlayNodeAsync(node.NextNodeGuid);

                break;
            //WIP
            case DialogueNodeType.Choice:
                var chosenGuid = await _view.ShowChoicesAsync(node.Choices);

                InvokeSceneEvents(node.SceneEventKeysOnEnd).Forget();
                G.QuestManager.RefreshQuestLogUI();

                await PlayNodeAsync(chosenGuid);
                break;
        }
    }

    private async UniTaskVoid InvokeSceneEvents(List<EventState> events)
    {
        if (_sceneEvents == null) return;
        var ct = this.GetCancellationTokenOnDestroy();

        foreach (var @event in events)
        {
            if(string.IsNullOrWhiteSpace(@event.Key))
                continue;

            await UniTask.Delay(TimeSpan.FromSeconds(@event.DelayBeforeEvent), cancellationToken: ct);

            _sceneEvents.Invoke(@event.Key);

            await UniTask.Delay(TimeSpan.FromSeconds(@event.DelayAfterEvent), cancellationToken: ct);
        }
    }
    private void AddQuests(DialogueNode node)
    {
        foreach (var quest in node.Quests)
        {
            G.QuestManager.AddQuest(quest);
        }
    }
    private void TryToCompleteDialogueQuest(DialogueNode node)
    {
        string npcName = node.SpeakerName;

        G.QuestManager.OnNPCTalked(npcName);
    }

    #region Dialogue Control
    private void SetSpeaker(SpeakerSide side, string name) => _view.SetSpeaker(side, name);
    public void SetDialogue(DialogueSO newDialogue) => CurrentDialogue = newDialogue;
    public void SetAndPlayDialogue(DialogueSO newDialogue)
    {
        SetDialogue(newDialogue);
        PlayDialogueAsync(newDialogue).Forget();
    }
    #endregion
}
