using Cysharp.Threading.Tasks;

public interface IUIEffect
{
    UniTask PlayEnterEffectAsync();
    UniTask PlayExitEffectAsync();
}