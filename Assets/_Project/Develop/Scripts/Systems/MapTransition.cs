using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using NaughtyAttributes;
using Cysharp.Threading.Tasks;

public class MapTransition : MonoBehaviour
{
    [Header("Maps Configuration")]
    [SerializeField] private Map[] maps;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.8f;
    [SerializeField] private float mapSpacing = 300f;
    [SerializeField] private Ease easeType = Ease.OutQuart;
    [SerializeField] private float scaleCenter = 1.2f;
    [SerializeField] private float scaleSide = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private bool useFadeAnimation = true;
    [SerializeField] private float centerAlpha = 1f;
    [SerializeField] private float sideAlpha = 0.6f;

    [SerializeField] private Button startMapButton;
    [SerializeField, ReadOnly] private Map selectedMap;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private int currentMapIndex = 0;
    private Vector3[] originalPositions;
    private bool isTransitioning = false;

    private Vector3 baseCenterPosition;
    private float baseTransitionSpeed;

    private void OnEnable()
    {
        transitionDuration = baseTransitionSpeed;

        if(startMapButton != null ) 
            startMapButton.gameObject.SetActive(true);

        if(leftButton != null )
            leftButton.gameObject.SetActive(true);

        if(rightButton != null )
            rightButton.gameObject.SetActive(true);

        foreach (var map in maps)
        {
            map.Button.interactable = true;
        }
    }

    private async void Start()
    {
        InitializeMaps();

        SetupInitialState();

        await TransitionToIndexAsync(0, true);

        gameObject.SetActive(false);
    }

    private void InitializeMaps()
    {
        originalPositions = new Vector3[maps.Length];
        baseTransitionSpeed = transitionDuration;

        SubscribeSideButtons();

        for (int i = 0; i < maps.Length; i++)
        {
            maps[i].Init();
            originalPositions[i] = maps[i].Transform.localPosition;

            int index = i;
            maps[i].Button.onClick.AddListener(async () => await TransitionToIndexAsync(index));
        }

        baseCenterPosition = maps[0].Transform.localPosition;
    }

    private void SubscribeSideButtons()
    {
        leftButton.onClick.RemoveAllListeners();
        rightButton.onClick.RemoveAllListeners();

        leftButton.onClick.AddListener(MoveLeft);
        rightButton.onClick.AddListener(MoveRight);
    }

    private async void SetupInitialState() => await UpdateMapPositionsAsync(currentMapIndex);

    public void SetMapByID(string ID)
    {
        if (maps == null || maps.Length == 0)
            return;

        if (ID.Contains("Next", System.StringComparison.OrdinalIgnoreCase))
        {
            int nextIndex = Mathf.Clamp(currentMapIndex + 1, 0, maps.Length - 1);

            currentMapIndex = nextIndex;
            selectedMap = maps[nextIndex];
        }
        else if (ID.Contains("Previous", System.StringComparison.OrdinalIgnoreCase))
        {
            int prevIndex = Mathf.Clamp(currentMapIndex - 1, 0, maps.Length - 1);

            currentMapIndex = prevIndex;
            selectedMap = maps[prevIndex];
        }
        else
        {
            var targetMap = maps.FirstOrDefault(m => m.ID == ID);
            if (targetMap != null)
            {
                selectedMap = targetMap;
                currentMapIndex = System.Array.IndexOf(maps, targetMap);
            }
            else
            {
                Debug.LogWarning($"SetMapByID: Map with ID '{ID}' not found!");
            }
        }
    }

    public async void TransitionTo(string mapID)
    {
        int targetIndex = System.Array.FindIndex(maps, m => m.ID == mapID);
        if (targetIndex >= 0)
        {
            await TransitionToIndexAsync(targetIndex);
        }
        else
        {
            Debug.LogWarning($"Map with ID '{mapID}' not found!");
        }
    }
    /// <summary>
    /// Использовать в квестах пока не будет новой системы ивентов
    /// </summary>
    public async void AutoTransition(float newTransitionSpeed)
    {
        if (selectedMap == null || string.IsNullOrWhiteSpace(selectedMap.ID))
            return;

        gameObject.SetActive(true);

        if(startMapButton != null) 
            startMapButton.gameObject.SetActive(false);

        if(leftButton != null)
            leftButton.gameObject.SetActive(false);

        if(rightButton != null)
            rightButton.gameObject.SetActive(false);

        foreach (var map in maps)
        {
            map.Button.interactable = false;
        }

        SetTransitionSpeed(newTransitionSpeed);

        int targetIndex = System.Array.FindIndex(maps, m => m.ID == selectedMap.ID);
        await TransitionToIndexAsync(targetIndex, false, false);

        await UniTask.DelayFrame(2);

        await UpdateMapPositionsAsync(targetIndex);

        LoadMap();
    }
    public void SetTransitionSpeed(float newTransitionSpeed) => transitionDuration = newTransitionSpeed;    

    public async UniTask TransitionToIndexAsync(int targetIndex, bool ignoreSameMap = false, bool enableStartButton = true)
    {
        if (isTransitioning || targetIndex < 0 || targetIndex >= maps.Length || (targetIndex == currentMapIndex && !ignoreSameMap))
            return;

        isTransitioning = true;
        startMapButton.interactable = enableStartButton;
        startMapButton.gameObject.SetActive(enableStartButton);

        await UpdateMapPositionsAsync(targetIndex);

        currentMapIndex = targetIndex;
        selectedMap = maps[targetIndex];

        isTransitioning = false;
        OnTransitionComplete();
    }


    private async UniTask UpdateMapPositionsAsync(int centerIndex)
    {
        if (startMapButton != null)
            startMapButton.interactable = false;

        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < maps.Length; i++)
        {
            Map map = maps[i];

            Vector3 targetPosition = CalculateTargetPosition(i, centerIndex);
            float targetScale = (i == centerIndex) ? scaleCenter : scaleSide;
            float targetAlpha = (i == centerIndex) ? centerAlpha : sideAlpha;

            sequence.Join(map.Transform.DOLocalMove(targetPosition, transitionDuration).SetEase(easeType));

            if (useScaleAnimation)
                sequence.Join(map.Transform.DOScale(targetScale, transitionDuration).SetEase(easeType));

            if (useFadeAnimation && map.CanvasGroup != null)
                sequence.Join(map.CanvasGroup.DOFade(targetAlpha, transitionDuration).SetEase(easeType));
        }

        await sequence.AsyncWaitForCompletion();

        if(startMapButton != null)
            startMapButton.interactable = true;
    }

    private Vector3 CalculateTargetPosition(int mapIndex, int centerIndex)
    {
        int offset = mapIndex - centerIndex;
        return baseCenterPosition + Vector3.right * (offset * mapSpacing);
    }
    public async void MoveLeft()
    {
        int targetIndex = Mathf.Max(0, currentMapIndex - 1);
        await TransitionToIndexAsync(targetIndex);
    }

    public async void MoveRight()
    {
        int targetIndex = Mathf.Min(maps.Length - 1, currentMapIndex + 1);
        await TransitionToIndexAsync(targetIndex);
    }
    public Map GetCurrentMap() => maps[currentMapIndex];

    public int GetCurrentMapIndex() => currentMapIndex;

    private void OnTransitionComplete()
    {
        //Debug.Log($"Transition completed. Current map: {maps[currentMapIndex].ID}");
    }

    [System.Serializable]
    public class Map
    {
        [Header("Map Data")]
        [Label("ID (Scene Name)"), AllowNesting]
        public string ID;
        public Image Image;
        public Button Button;

        [Header("Components (Auto-assigned)")]
        [ReadOnly, AllowNesting] public Transform Transform;
        [ReadOnly, AllowNesting] public CanvasGroup CanvasGroup;

        public void Init()
        {
            if (Image != null && Transform == null)
                Transform = Image.transform;

            if (Image != null && CanvasGroup == null)
                CanvasGroup = Image.GetComponent<CanvasGroup>();
        }

        public Transform GetTransform()
        {
            if (Transform == null && Image != null)
                Transform = Image.transform;
            return Transform;
        }

        public CanvasGroup GetCanvasGroup()
        {
            if (CanvasGroup == null && Image != null)
            {
                CanvasGroup = Image.GetComponent<CanvasGroup>();
                if (CanvasGroup == null)
                    CanvasGroup = Image.gameObject.AddComponent<CanvasGroup>();
            }
            return CanvasGroup;
        }
    }

    public void LoadMap()
    {
        if (selectedMap == null || string.IsNullOrWhiteSpace(selectedMap.ID) || G.SceneTransitionManager == null)
            return;

        G.SceneTransitionManager.LoadSceneWithFade(selectedMap.ID);
    }
}
