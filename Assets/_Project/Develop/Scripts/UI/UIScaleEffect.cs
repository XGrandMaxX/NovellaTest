using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;

public class UIScaleEffect : MonoBehaviour, IUIEffect, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float scaleMultiplier = 1.1f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private Vector3 _originalScale;

    private void Awake() => _originalScale = transform.localScale;

    public void OnPointerEnter(PointerEventData eventData) => PlayEnterEffectAsync().Forget();

    public void OnPointerExit(PointerEventData eventData) => PlayExitEffectAsync().Forget();

    public async UniTask PlayEnterEffectAsync()
    {
        await transform.DOScale(_originalScale * scaleMultiplier, duration)
                       .SetEase(ease)
                       .AsyncWaitForCompletion();
    }

    public async UniTask PlayExitEffectAsync()
    {
        await transform.DOScale(_originalScale, duration)
                       .SetEase(ease)
                       .AsyncWaitForCompletion();
    }

}
