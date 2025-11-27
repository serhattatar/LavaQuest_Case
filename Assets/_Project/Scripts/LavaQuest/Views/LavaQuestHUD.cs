using UnityEngine;
using TMPro;

public class LavaQuestHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI playersText;
    [SerializeField] private TextMeshProUGUI infoText;

    public void UpdateLevelText(int current, int total)
    {
        if (levelText) levelText.text = $"Level {current}/{total}";
    }

    public void UpdatePlayersText(int current, int total)
    {
        if (playersText) playersText.text = $"Players {current}/{total}";
    }

    public void SetInfoMessage(string message)
    {
        if (infoText) infoText.text = message;
    }
}