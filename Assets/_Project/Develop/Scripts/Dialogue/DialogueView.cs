using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueCharacter
{
    public GameObject Object;
    public TextMeshProUGUI Name;
}

public enum SpeakerSide { Left, Right }

public class DialogueView : MonoBehaviour, IDialogueView, IPauseable
{
    [field: SerializeField, ReadOnly] public DialogueCharacter CurrentSpeaker { get; private set; }
    [SerializeField] private DialogueCharacter leftCharacter;
    [SerializeField] private DialogueCharacter rightCharacter;

    private Dictionary<SpeakerSide, DialogueCharacter> characters;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Space(20)]
    [Header("Dialogue Continue method")]
    [InfoBox("Нажатие на кнопку или клавишу продолжит диалог. Если один из методов не задан, будет использоваться другой.")]
    [SerializeField] private Button ContinueButton;
    [SerializeField] private KeyCode ContinueKey = KeyCode.Space;

    [Header("Audio settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip letterSfx;
    [SerializeField] private AudioClip wordSfx;

    [Header("Choices")]
    [SerializeField] private GameObject choiceContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    private CancellationTokenSource revealCts;
    private bool isPaused;

    private void Awake()
    {
        characters = new Dictionary<SpeakerSide, DialogueCharacter>
        {
            { SpeakerSide.Left, leftCharacter },
            { SpeakerSide.Right, rightCharacter }
        };
    }
    private void Start()
    {
        if (G.PauseManager == null)
            return;

        G.PauseManager.Register(this);
    }
    private void OnDestroy()
    {
        if (G.PauseManager == null)
            return;

        G.PauseManager.Unregister(this);
    }

    #region Dialogue
    public void SkipReveal()
    {
        if (revealCts != null && !revealCts.IsCancellationRequested)
            revealCts.Cancel();
    }

    public void ClearSpeakerNames()
    {
        foreach (var character in characters.Values)
        {
            character.Name.text = "";
        }
    }
    public void ClearDialogueText() => dialogueText.text = "";
    public void SetSpeaker(SpeakerSide side, string newName)
    {
        if (!characters.TryGetValue(side, out var speaker)) return;

        speaker.Object.SetActive(true); //Под вопросом
        speaker.Name.text = newName;

        CurrentSpeaker = speaker;
    }

    public async UniTask ShowTextAsync(string text, string speaker, AudioClip voiceClip, TextRevealMode revealMode, float speed, DialogueTextAlignment alignment = DialogueTextAlignment.Left)
    {
        revealCts?.Cancel();
        revealCts = new CancellationTokenSource();
        var ct = revealCts.Token;

        ClearDialogueText();

        if (voiceClip != null)
            audioSource.PlayOneShot(voiceClip); 

        ApplyAlignment(alignment);

        text = text.Replace("{User}", UserData.UserName);

        switch (revealMode)
        {
            case TextRevealMode.Instant:
                dialogueText.text = text;
                break;

            case TextRevealMode.ByLetters:
                foreach (var c in text)
                {
                    if (ct.IsCancellationRequested) break;

                    await WaitWhilePaused();

                    dialogueText.text += c;

                    if (!char.IsWhiteSpace(c) && letterSfx != null)
                        audioSource.PlayOneShot(letterSfx); //Возможно вернуть громкость

                    await UniTask.Delay(TimeSpan.FromSeconds(speed), cancellationToken: ct);
                }
                break;

            case TextRevealMode.ByWords:
                var words = text.Split(' ');
                foreach (var word in words)
                {
                    if (ct.IsCancellationRequested) break;

                    await WaitWhilePaused();

                    dialogueText.text += word + " ";

                    if (wordSfx != null)
                        audioSource.PlayOneShot(wordSfx);//Возможно вернуть громкость

                    await UniTask.Delay(TimeSpan.FromSeconds(speed), cancellationToken: ct);
                }
                break;
        }

        revealCts = null;
    }

    private void ApplyAlignment(DialogueTextAlignment alignment) => dialogueText.alignment = (TextAlignmentOptions)alignment;

    //WIP
    public async UniTask<string> ShowChoicesAsync(List<DialogueChoice> choices)
    {
        choiceContainer.SetActive(true);

        var tcs = new UniTaskCompletionSource<string>();

        foreach (Transform child in choiceContainer.transform)
            Destroy(child.gameObject);

        foreach (var choice in choices)
        {
            var buttonObj = Instantiate(choiceButtonPrefab, choiceContainer.transform);
            var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = choice.text;

            var capturedChoice = choice;
            buttonObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                tcs.TrySetResult(capturedChoice.nextNodeGuid);
                choiceContainer.SetActive(false);
            });
        }

        return await tcs.Task;
    }

    public async UniTask WaitForContinue(CancellationToken ct)
    {
        var tcs = new UniTaskCompletionSource();

        void TryComplete()
        {
            if (!tcs.Task.Status.IsCompleted())
                tcs.TrySetResult();
        }

        if (ContinueButton != null)
        {
            void OnClick()
            {
                ContinueButton.onClick.RemoveListener(OnClick);
                TryComplete();
            }

            ContinueButton.onClick.AddListener(OnClick);

            ct.Register(() => ContinueButton.onClick.RemoveListener(OnClick));
        }

        while (!tcs.Task.Status.IsCompleted())
        {
            if (ContinueKey != KeyCode.None && Input.GetKeyDown(ContinueKey))
            {
                TryComplete();
                break;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        await tcs.Task;
    }

    #endregion


    #region IPauseable
    private async UniTask WaitWhilePaused() => await UniTask.WaitUntil(() => !isPaused, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
    public void OnPause() => isPaused = true;

    public void OnResume() => isPaused = false;
    #endregion
}
