using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    public Text TitleText;
    public Text DescriptionText;
    public Image Icon;
    public Slider QuestSlider;

    public Button ClaimButton;
    public Button AcceptButton;

    public Transform RewardParentObject;

    public DG.Tweening.Ease SpawnEase;
}
