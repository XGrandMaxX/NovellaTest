using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private DialogueView _view;

    [SerializeField] private DialogueSceneEvents _sceneEvents;

    [field: Tooltip("Set the initial dialogue to play when the scene starts")]
    [field: SerializeField] public DialogueSO CurrentDialogue { get; private set; }

    private Dictionary<string, DialogueNode> _nodeMap;

    private async void Start()
    {
        if (CurrentDialogue != null)
            await PlayDialogueAsync(CurrentDialogue);
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

        InvokeSceneEvents(node.sceneEventKeysOnStart);

        await UniTask.DelayFrame(3, PlayerLoopTiming.Update, cancellationToken: ct);

        SetSpeaker(node.speakerSide, node.speakerName);


        switch (node.nodeType)
        {
            case DialogueNodeType.Text:
                await _view.ShowTextAsync(node.text, node.speakerName, node.textPrintSound, node.revealMode, node.revealSpeed, node.textAlignment);

                if (node.autoChangeDialogue)
                    await UniTask.Delay(TimeSpan.FromSeconds(node.autoChangeDelay), cancellationToken: ct);
                else
                    await UniTask.WaitUntil(() => Input.GetKeyDown(KeyCode.Space), cancellationToken: ct); //или кнопка

                InvokeSceneEvents(node.sceneEventKeysOnEnd);

                if (!string.IsNullOrEmpty(node.nextNodeGuid))
                    await PlayNodeAsync(node.nextNodeGuid);

                break;
            //WIP
            case DialogueNodeType.Choice:
                var chosenGuid = await _view.ShowChoicesAsync(node.choices);
                InvokeSceneEvents(node.sceneEventKeysOnEnd);

                await PlayNodeAsync(chosenGuid);
                break;
        }
    }

    private async void InvokeSceneEvents(List<EventState> events)
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

    private void SetSpeaker(SpeakerSide side, string name) => _view.SetSpeaker(side, name);
}
