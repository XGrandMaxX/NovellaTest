using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public static class DOTweenAnimationManager
{
    private static readonly Dictionary<Transform, Tween> _activeTweens = new();
    public static void DotAnimation(Text targetText, string message, float interval = 0.3f)
    {
        if (targetText == null) return;

        StopTweenIfExists(targetText.transform);

        int dotCount = 0;
        var tween = DOTween.To(() => dotCount, x =>
        {
            dotCount = x;
            targetText.text = message + new string('.', dotCount);
        }, 3, interval * 3f)
        .SetLoops(-1, LoopType.Restart)
        .SetEase(Ease.Linear);

        _activeTweens[targetText.transform] = tween;
    }
    public static async UniTask AnimateShowAsync(Transform target, float duration = 0.5f, float delay = 0f, Ease ease = Ease.OutBack)
    {
        if (target == null) return;

        StopTweenIfExists(target);

        target.localScale = Vector3.zero;
        target.gameObject.SetActive(true);

        var tcs = new TaskCompletionSource<bool>();

        var tween = target.DOScale(Vector3.one, duration)
                          .SetDelay(delay)
                          .SetEase(ease)
                          .OnComplete(() => tcs.TrySetResult(true))
                          .OnKill(() => _activeTweens.Remove(target));

        _activeTweens[target] = tween;

        await tcs.Task;
    }

    public static async UniTask AnimateHideAsync(Transform target, float duration = 0.3f, Ease ease = Ease.InBack)
    {
        if (target == null) return;

        StopTweenIfExists(target);

        var tcs = new TaskCompletionSource<bool>();

        var tween = target.DOScale(Vector3.zero, duration)
                          .SetEase(ease)
                          .OnComplete(() =>
                          {
                              target.gameObject.SetActive(false);
                              tcs.TrySetResult(true);
                          })
                          .OnKill(() => _activeTweens.Remove(target));

        _activeTweens[target] = tween;

        await tcs.Task;
    }

    public static void StopTweenIfExists(Transform target)
    {
        if (_activeTweens.TryGetValue(target, out var existingTween) && existingTween.IsActive())
        {
            existingTween.Kill();
            _activeTweens.Remove(target);
        }
    }
}
