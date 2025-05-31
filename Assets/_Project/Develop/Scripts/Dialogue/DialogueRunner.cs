using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DialogueView), typeof(DialogueSceneEvents))]
public class DialogueRunner : MonoBehaviour, IPauseable
{
    private DialogueView _view;

    private DialogueSceneEvents _sceneEvents;

    [field: Tooltip("Set the initial dialogue to play when the scene starts")]
    [field: SerializeField] public DialogueSO CurrentDialogue { get; private set; }

    private Dictionary<string, DialogueNode> _nodeMap;
    [SerializeField] private DialogueSO ItemDialogue;

    [SerializeField] private bool isPaused;
    private void Awake()
    {
        _view = GetComponent<DialogueView>();
        _sceneEvents = GetComponent<DialogueSceneEvents>();
    }
    private void Start()
    {
        if (UserData.HasQuestItem && ItemDialogue != null)
            SetDialogue(ItemDialogue);

        if (CurrentDialogue != null)
            PlayDialogueAsync(CurrentDialogue).Forget();

        if (G.PauseManager == null)
            return;

        G.PauseManager.Register(this);
    }
    private void OnDestroy()
    {
        if (G.PauseManager == null)
            return;

        G.PauseManager.Unregister(this);
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

        await WaitWhilePaused();
        await UniTask.Delay(TimeSpan.FromSeconds(node.DialogueStartDelay), cancellationToken: ct);
        await WaitWhilePaused();

        AddQuests(node);
        InvokeSceneEvents(node.SceneEventKeysOnStart).Forget();

        await UniTask.DelayFrame(3, PlayerLoopTiming.Update, cancellationToken: ct);
        await WaitWhilePaused();

        var speakerName = (string.IsNullOrWhiteSpace(node.SpeakerName) || node.SpeakerName == "User") ? UserData.UserName : node.SpeakerName;
        SetSpeaker(node.SpeakerSide, speakerName);

        TryToCompleteDialogueQuest(node);

        switch (node.NodeType)
        {
            case DialogueNodeType.Text:
                await WaitWhilePaused();

                await _view.ShowTextAsync(node.Text, node.SpeakerName, node.TextPrintSound, node.revealMode, node.revealSpeed, node.TextAlignment);

                await WaitWhilePaused();

                if (node.AutoChangeDialogue)
                    await UniTask.Delay(TimeSpan.FromSeconds(node.AutoChangeDelay), cancellationToken: ct);
                else
                    await _view.WaitForContinue(ct);

                await WaitWhilePaused();

                InvokeSceneEvents(node.SceneEventKeysOnEnd).Forget();
                G.QuestManager.RefreshQuestLogUI();

                if (!string.IsNullOrEmpty(node.NextNodeGuid))
                    await PlayNodeAsync(node.NextNodeGuid);

                break;
            //WIP
            case DialogueNodeType.Choice:
                await WaitWhilePaused();

                var chosenGuid = await _view.ShowChoicesAsync(node.Choices);

                await WaitWhilePaused();

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

            await WaitWhilePaused();
            await UniTask.Delay(TimeSpan.FromSeconds(@event.DelayBeforeEvent), cancellationToken: ct);
            await WaitWhilePaused();

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


    #region IPauseable
    private async UniTask WaitWhilePaused() => await UniTask.WaitUntil(() => !isPaused, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
    public void OnPause() => isPaused = true;

    public void OnResume() => isPaused = false;
    #endregion
}
