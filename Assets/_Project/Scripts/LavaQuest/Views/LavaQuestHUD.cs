using UnityEngine;
using TMPro;

public class LavaQuestHUD : MonoBehaviour
{
    [Header("UI Text Elements")]
    [SerializeField] private TextMeshProUGUI levelText;    // e.g. "Level 1/7"
    [SerializeField] private TextMeshProUGUI playersText;  // e.g. "Players 90/100"
    [SerializeField] private TextMeshProUGUI infoText;     // e.g. "Stage Cleared!"

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