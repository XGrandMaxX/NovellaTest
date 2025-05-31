using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    [SerializeField] private Image frontImage;
    [SerializeField] private Image backImage;
    [SerializeField] private Transform visualRoot;
    public int CardId { get; private set; }
    public bool IsMatched { get; private set; }

    private CardGameManager gameManager;

    public void Init(int id, Sprite front, CardGameManager manager)
    {
        CardId = id;
        frontImage.sprite = front;
        gameManager = manager;
        IsMatched = false;
        frontImage.enabled = false;
        backImage.enabled = true;
    }

    public async void OnClick()
    {
        await gameManager.TryRevealCard(this);
    }

    public async UniTask FlipToFront()
    {
        await visualRoot.DOScaleX(0, 0.2f).AsyncWaitForCompletion();
        backImage.enabled = false;
        frontImage.enabled = true;
        await visualRoot.DOScaleX(1, 0.2f).AsyncWaitForCompletion();
    }

    public async UniTask FlipToBack()
    {
        await visualRoot.DOScaleX(0, 0.2f).AsyncWaitForCompletion();
        frontImage.enabled = false;
        backImage.enabled = true;
        await visualRoot.DOScaleX(1, 0.2f).AsyncWaitForCompletion();
    }
    public async UniTask AnimateShow()
    {
        visualRoot.localScale = Vector3.zero;
        await visualRoot.DOScale(Vector3.one, 0.25f)
            .SetEase(Ease.OutBack)
            .AsyncWaitForCompletion();
    }
    public void SetMatched() => IsMatched = true;
}
