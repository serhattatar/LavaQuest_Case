using UnityEngine;

/// <summary>
/// [CORE SYSTEM] 
/// This script mocks the existing Main Game Manager.
/// </summary>
public class MainCanvasManager : MonoBehaviour
{
    [Space(10)]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Header("   [ CORE SYSTEM - MOCK ARCHITECTURE ]")]
    [Header("   This script simulates the Main Game flow.")]
    [Header("   Please focus evaluation on 'LavaQuest' modules.")]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Space(20)]
    [Header("Modules")]
    [SerializeField] private LavaQuestPanelController eventController;
    [SerializeField] private FakeGamePanel gameSimulation;
    [SerializeField] private LavaQuestPopup lavaQuestPopup;
    [SerializeField] private LavaQuestRewardPopup popupReward;
    [SerializeField] private MatchmakingPanelController matchmakingPanel;

    [Header("Core Dependencies")]
    [SerializeField] private ObjectPooler objectPooler;

    [Header("Panels")]
    [SerializeField] private GameObject panelEventMap;
    [SerializeField] private GameObject panelFakeGame;
    [SerializeField] private GameObject panelMatchmaking;

    private System.Collections.Generic.List<ParticipantData> cachedSessionData;

    private void Start()
    {
        lavaQuestPopup.Show();
        popupReward.Hide();
        panelEventMap.SetActive(false);
        panelFakeGame.SetActive(false);
        panelMatchmaking.SetActive(false);

        // --- Wiring ---
        lavaQuestPopup.OnStartClicked += OnStartEvent;
        lavaQuestPopup.OnCloseClicked += () =>
        {
            lavaQuestPopup.Hide();
        };

        matchmakingPanel.OnContinueRequested += OnMatchmakingCompleted;

        eventController.OnPlayRequested += () => {
            panelEventMap.SetActive(false);
            panelFakeGame.SetActive(true);
        };

        eventController.OnRestartRequested += () => {
            panelEventMap.SetActive(false);
            lavaQuestPopup.Show();
        };

        eventController.OnQuestCompleted += () => {
            popupReward.Show();
        };

        popupReward.OnCollectClicked += () => {
            popupReward.Hide(() =>
            {
                panelEventMap.SetActive(false);
                lavaQuestPopup.Show();
            });
        };

        gameSimulation.OnWinSelected += () => FinishGame(true);
        gameSimulation.OnFailSelected += () => FinishGame(false);
    }

    private void OnStartEvent()
    {
        cachedSessionData = GameSessionBridge.Instance.GetMockSessionData(100);

        lavaQuestPopup.Hide(() =>
        {
            panelMatchmaking.SetActive(true);
            matchmakingPanel.Initialize(cachedSessionData, objectPooler);
        });
    }

    private void OnMatchmakingCompleted()
    {
        panelMatchmaking.SetActive(false);
        panelEventMap.SetActive(true);
        eventController.Initialize(cachedSessionData, objectPooler);
    }

    private void FinishGame(bool isWin)
    {
        panelFakeGame.SetActive(false);
        panelEventMap.SetActive(true);
        eventController.ExecuteRoundLogic(isWin);
    }
}