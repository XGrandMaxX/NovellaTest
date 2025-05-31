using Cysharp.Threading.Tasks;
using DG.Tweening;
using NaughtyAttributes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class CardGameManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform cardParent;
    [SerializeField] private List<CardData> cardDatas;

    [SerializeField, MinValue(0), MaxValue(2)] private float cardSpawnSpeed;

    [Space(20)]
    public UnityEvent OnGameEnded;

    private CardController firstCard;
    private CardController secondCard;

    private int totalPairs;
    private int matchedPairs;

    public bool IsProcessing { get; private set; } = false;
    public async void InitGame()
    {
        await ClearAllCards();

        if (G.PauseManager != null)
            G.PauseManager.Pause();

        firstCard = null;
        secondCard = null;
        matchedPairs = 0;
        totalPairs = cardDatas.Count;
        IsProcessing = false;

        List<CardData> allCards = new List<CardData>();
        foreach (var data in cardDatas)
        {
            allCards.Add(data);
            allCards.Add(data);
        }

        allCards = allCards.OrderBy(x => Random.value).ToList();

        foreach (var data in allCards)
        {
            var cardObj = Instantiate(cardPrefab, cardParent);
            cardObj.transform.localScale = Vector3.zero;

            var card = cardObj.GetComponentInChildren<CardController>();
            card.Init(data.Id, data.FrontImage, this);

            await cardObj.transform.DOScale(Vector3.one, cardSpawnSpeed).SetEase(Ease.OutBack).AsyncWaitForCompletion();
        }
    }

    private async UniTask ClearAllCards()
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in cardParent)
            children.Add(child);

        foreach (var child in children)
        {
            await child.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).AsyncWaitForCompletion();
            Destroy(child.gameObject);
        }
    }

    public async UniTask TryRevealCard(CardController card)
    {
        if (IsProcessing || card.IsMatched || card == firstCard || card == secondCard)
            return;

        if (firstCard == null)
        {
            firstCard = card;
            await firstCard.FlipToFront();
        }
        else if (secondCard == null)
        {
            secondCard = card;
            IsProcessing = true;
            await secondCard.FlipToFront();

            if (firstCard.CardId == secondCard.CardId)
            {
                firstCard.SetMatched();
                secondCard.SetMatched();
                matchedPairs++;

                await UniTask.Delay(300);

                if (matchedPairs >= totalPairs)
                    OnGameWin();
            }
            else
            {
                await UniTask.Delay(1000);
                await firstCard.FlipToBack();
                await secondCard.FlipToBack();
            }

            firstCard = null;
            secondCard = null;
            IsProcessing = false;
        }
    }

    private void OnGameWin()
    {
        if (G.PauseManager != null)
            G.PauseManager.Resume();

        IsProcessing = true;
        Debug.Log("онаедю!");
        OnGameEnded?.Invoke();
    }
}
