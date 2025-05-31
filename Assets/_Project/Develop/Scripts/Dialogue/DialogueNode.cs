using System;
using UnityEngine;
using NaughtyAttributes;
using System.Collections.Generic;
using TMPro;

public enum TextRevealMode { Instant, ByLetters, ByWords }
public enum DialogueNodeType { Text, Choice }
public enum DialogueTextAlignment
{
    TopLeft = TextAlignmentOptions.TopLeft,
    Left = TextAlignmentOptions.Left,
    Center = TextAlignmentOptions.Center,
    TopRight = TextAlignmentOptions.TopRight,
    Right = TextAlignmentOptions.Right
}


[Serializable]
public class DialogueNode
{
    public string GUID;
    public DialogueNodeType NodeType;

    [Header("Speaker Settings")]
    [InfoBox("Если спикер не установлен или установлено имя User, будет использован UserName, который установил игрок")]
    public string SpeakerName;
    public SpeakerSide SpeakerSide;

    [Header("Dialogue Settings")]
    [Space(20)]
    [MinValue(0), AllowNesting] public float DialogueStartDelay;
    [ResizableTextArea, AllowNesting, Label("Text (Press Enter)"), InfoBox("Используйте {User}, чтобы добавить имя пользователя в диалог")] 
    public string Text;
    public DialogueTextAlignment TextAlignment;
    public AudioClip TextPrintSound;

    [Header("Auto Change Settings")]
    public bool AutoChangeDialogue;
    [ShowIf(nameof(AutoChangeDialogue)), MinValue(0), AllowNesting] public float AutoChangeDelay;

    [Space(20)]

    [Header("Reveal Settings")]
    public TextRevealMode revealMode = TextRevealMode.ByLetters;
    [ShowIf(nameof(ShouldUseRevealSpeed)), MinValue(0.01f), AllowNesting]public float revealSpeed = 0.05f;

    [Header("Events")]
    public List<EventState> SceneEventKeysOnStart;
    public List<EventState> SceneEventKeysOnEnd;

    public List<DialogueChoice> Choices;
    [InfoBox("Квесты, которые выдаются при запуске диалога")]
    public List<QuestSO> Quests;

    [Header("NextNode GUID")]
    public string NextNodeGuid;
    private bool ShouldUseRevealSpeed() => revealMode == TextRevealMode.ByLetters || revealMode == TextRevealMode.ByWords;
}

[System.Serializable]
public class EventState
{
    public string Key;
    [MinValue(0), AllowNesting] public float DelayBeforeEvent;
    [MinValue(0), AllowNesting] public float DelayAfterEvent;
}