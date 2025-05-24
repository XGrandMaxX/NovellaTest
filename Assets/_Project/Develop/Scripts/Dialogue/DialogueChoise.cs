using UnityEngine.Events;

[System.Serializable]
public class DialogueChoice
{
    public string text;
    public UnityEvent onSelected;
    public string nextNodeGuid;
}
