using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string itemID;
    [SerializeField] private Transform targetPoint;

    private void OnEnable() => FlyToTarget();
    public async void OnPointerClick(PointerEventData eventData)
    {
        gameObject.SetActive(false);
        UserData.HasQuestItem = true;
        G.QuestManager.OnItemCollected(itemID);

        var mapTransition = FindObjectOfType<MapTransition>(true);
        mapTransition.SetMapByID("Map2"); //Last map saved
        mapTransition.AutoTransition(0);

        await UniTask.DelayFrame(2);
        mapTransition.SetMapByID("Next");
        mapTransition.AutoTransition(1.5f);
    }


    private void FlyToTarget()
    {
        if (targetPoint == null)
            return;

        Vector3 startPos = transform.position;
        Vector3 endPos = targetPoint.position;

        float duration = 1f;
        float arcHeight = 2f;

        Vector3 midPoint = (startPos + endPos) / 2;
        midPoint.y += arcHeight;

        Vector3[] path = new Vector3[] {startPos, midPoint, endPos};

        transform.DOPath(path, duration, PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                RectTransform rect = GetComponent<RectTransform>();
                Sequence seq = DOTween.Sequence();
                seq.Join(rect.DOShakeAnchorPos(
                    duration: 1f,
                    strength: new Vector2(15f, 15f),
                    vibrato: 10,
                    randomness: 0f,
                    snapping: false,
                    fadeOut: true
                ));
                seq.Join(rect.DOScale(1.1f, 0.5f).SetLoops(2, LoopType.Yoyo));
                seq.Play();
            });

    }
}
