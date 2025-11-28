using UnityEngine;
using TMPro;

public class LavaQuestHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI clockText;

    public void UpdateLevelText(int current, int total)
    {
        if (levelText) levelText.text = $"{current}/{total}";
    }

    public void UpdatePlayersText(int current, int total)
    {
        if (playersText) playersText.text = $"{current}/{total}";
    }

    public void SetInfoMessage(string message)
    {
        if (infoText) infoText.text = message;
    }
    public void SetClockMessage(string message)
    {
        if (clockText) clockText.text = message;
    }
}