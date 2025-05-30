using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using Cysharp.Threading.Tasks;

public class UIHoverEffect : MonoBehaviour, IUIEffect, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Hover Offset Settings")]
    [Tooltip("≈сли true Ч смещение будет случайным в пределах диапазона.")]
    public bool useRandomOffset = false;

    [Tooltip("ћаксимальное смещение по X при наведении.")]
    public float offsetX = 10f;

    [Tooltip("ћаксимальное смещение по Y при наведении.")]
    public float offsetY = 5f;

    [Tooltip("ƒлительность анимации смещени€.")]
    public float duration = 0.3f;

    [Tooltip("“ип плавности анимации.")]
    public Ease ease = Ease.OutQuad;

    private Vector3 _originalPosition;
    private Vector3 _targetPosition;

    private void Awake() => _originalPosition = transform.localPosition;

    public void OnPointerEnter(PointerEventData eventData) => PlayEnterEffectAsync().Forget();

    public void OnPointerExit(PointerEventData eventData) => PlayExitEffectAsync().Forget();

    public async UniTask PlayEnterEffectAsync()
    {
        if (useRandomOffset)
        {
            float randomX = Random.Range(-offsetX, offsetX);
            float randomY = Random.Range(-offsetY, offsetY);
            _targetPosition = _originalPosition + new Vector3(randomX, randomY, 0);
        }
        else
        {
            _targetPosition = _originalPosition + new Vector3(offsetX, offsetY, 0);
        }

        await transform.DOLocalMove(_targetPosition, duration)
                       .SetEase(ease)
                       .AsyncWaitForCompletion();
    }

    public async UniTask PlayExitEffectAsync()
    {
        await transform.DOLocalMove(_originalPosition, duration)
                       .SetEase(ease)
                       .AsyncWaitForCompletion();
    }
}
