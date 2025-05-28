using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInputUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button confirmButton;

    [Header("Show/Hide animation settings")]
    [SerializeField] private DG.Tweening.Ease ease;
    [SerializeField, MinValue(0)] private float duration = 0.5f;
    [SerializeField, MinValue(0)] private float showDelay = 0f; 

    private void Start()
    {
        nameInputField.text = UserData.UserName;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmNameClicked);
    }


    private async void OnConfirmNameClicked()
    {
        string enteredName = nameInputField.text.Trim();

        if (!string.IsNullOrWhiteSpace(enteredName))
        {
            UserData.UserName = enteredName;
            await SetActiveAsync(false);
        }

        G.MainMenu.StartGame();
    }

    public async UniTask SetActiveAsync(bool value)
    {
        if (value)
            await DOTweenAnimationManager.AnimateShowAsync(gameObject.transform, duration, showDelay, ease);
        else
            await DOTweenAnimationManager.AnimateHideAsync(gameObject.transform, duration, ease);
    }
}
