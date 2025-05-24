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
    public DialogueNodeType nodeType;

    [Header("Speaker Settings")]
    public string speakerName;
    public SpeakerSide speakerSide;

    [Space(20)]
    [ResizableTextArea, AllowNesting, Label("Text (Press Enter)")]public string text;
    public DialogueTextAlignment textAlignment;
    public AudioClip textPrintSound;
    [Space(20)]

    [Header("Auto Change Settings")]
    public bool autoChangeDialogue;
    [ShowIf(nameof(autoChangeDialogue)), MinValue(0), AllowNesting]public float autoChangeDelay;

    [Header("Reveal Settings")]
    public TextRevealMode revealMode = TextRevealMode.ByLetters;
    [ShowIf(nameof(ShouldUseRevealSpeed)), MinValue(0.01f), AllowNesting]public float revealSpeed = 0.05f;

    [Header("Events")]
    public List<EventState> sceneEventKeysOnStart;
    public List<EventState> sceneEventKeysOnEnd;

    public List<DialogueChoice> choices;

    [Header("NextNode GUID")]
    public string nextNodeGuid;

    private bool ShouldUseRevealSpeed() => revealMode == TextRevealMode.ByLetters || revealMode == TextRevealMode.ByWords;
}

[System.Serializable]
public class EventState
{
    public string Key;
    [MinValue(0), AllowNesting] public float DelayBeforeEvent;
    [MinValue(0), AllowNesting] public float DelayAfterEvent;
}