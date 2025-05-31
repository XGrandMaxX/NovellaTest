using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/DialogueSO")]
public class DialogueSO : ScriptableObject
{
    public List<DialogueNode> nodes;
}