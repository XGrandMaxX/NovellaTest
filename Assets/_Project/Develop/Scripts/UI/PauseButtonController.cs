using UnityEngine;
using UnityEngine.UI;

public class PauseButtonController : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite pauseSprite;
    [SerializeField] private Sprite resumeSprite;

    private bool isPaused = false;

    private void Start()
    {
        pauseButton.onClick.RemoveAllListeners();
        pauseButton.onClick.AddListener(TogglePause);
        UpdateButtonVisual();
    }

    private void TogglePause()
    {
        if (G.PauseManager == null)
            return;

        isPaused = !isPaused;

        if (isPaused)
            G.PauseManager.Pause();
        else
            G.PauseManager.Resume();

        UpdateButtonVisual();
    }

    private void UpdateButtonVisual()
    {
        if (buttonImage != null)
            buttonImage.sprite = isPaused ? resumeSprite : pauseSprite;
    }
}
