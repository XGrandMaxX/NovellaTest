using Cysharp.Threading.Tasks;

public interface IDialogueEvent
{
    UniTask InvokeAsync();
}