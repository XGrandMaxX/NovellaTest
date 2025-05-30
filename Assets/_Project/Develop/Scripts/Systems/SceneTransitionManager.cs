using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionManager : MonoBehaviour
{
    [SerializeField] private Image FadeScreen;
    [Header("FadeOut")]
    [SerializeField] private float FadeOutDuration;
    [SerializeField] private DG.Tweening.Ease FadeOutEase;
    [Header("FadeIn")]
    [SerializeField] private float FadeInDuration;
    [SerializeField] private DG.Tweening.Ease FadeInEase;

    private void Awake()
    {
        if (G.SceneTransitionManager != null)
            Destroy(gameObject);
        else
        {
            G.SceneTransitionManager = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public async void LoadSceneWithFade(string sceneName)
    {
        await DOTweenAnimationManager.FadeOutScreenAsync(FadeScreen, FadeOutDuration, FadeOutEase);

        await LoadSceneProperlyAsync(sceneName);

        await UniTask.DelayFrame(2);

        await DOTweenAnimationManager.FadeInScreenAsync(FadeScreen, FadeInDuration, FadeInEase);
    }
    public async void LoadSceneWithoutFade(string sceneName) => await LoadSceneProperlyAsync(sceneName);

    private async UniTask LoadSceneProperlyAsync(string sceneName)
    {
        var loadOp = SceneManager.LoadSceneAsync(sceneName);
        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.90f)
            await UniTask.Yield();

        await UniTask.Delay(100);

        loadOp.allowSceneActivation = true;

        await UniTask.NextFrame();
    }

}
