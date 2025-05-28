using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public interface IDialogueView
{
    UniTask ShowTextAsync(string text, string speaker, AudioClip sound, TextRevealMode mode, float speed, DialogueTextAlignment alignment = DialogueTextAlignment.Left);
    UniTask<string> ShowChoicesAsync(List<DialogueChoice> choices);
}
