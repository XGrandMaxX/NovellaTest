using NaughtyAttributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private MenuButton[] menuButtons;
    [SerializeField] private PlayerNameInputUI playerNameInputUI;

    [SerializeField] private TMP_Text StartNameText;
    [SerializeField, Scene] private string LoadedScene;
    private void Awake()
    {
        if (G.MainMenu != null)
            Destroy(gameObject);
        else
            G.MainMenu = this;
    }
    private void Start()
    {
        foreach (var entry in menuButtons)
        {
            entry.Button.onClick.RemoveAllListeners();
        }

        var startButton = menuButtons.FirstOrDefault(b => b.ID.Contains("start", StringComparison.OrdinalIgnoreCase)).Button;

        if (!UserData.HasUser)
            startButton.onClick.AddListener(ShowPlayerNameFiled);
        else
        {
            startButton.onClick.AddListener(StartGame);
            StartNameText.text = $"Welcome {UserData.UserName}";
        }
    }

    private async void ShowPlayerNameFiled()
    {
        foreach (var entry in menuButtons)
        {
            entry.Button.gameObject.SetActive(false);
        }
        
        await playerNameInputUI.SetActiveAsync(true);
    }

    public void StartGame()
    {
        foreach (var entry in menuButtons)
        {
            entry.Button.gameObject.SetActive(false);
        }

        SceneManager.LoadSceneAsync(LoadedScene);
    }
}

[Serializable]
public class MenuButton
{
    public string ID;
    public Button Button;
}
