using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Orchestrates the event flow, bot simulation, and UI state management.
/// Acts as the central controller for the Lava Quest event.
/// </summary>
public class LavaQuestGameManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private AvatarView avatarPrefab;
    [SerializeField] private int totalParticipantCount = 20;
    [SerializeField] private List<Sprite> botAvatarsLibrary;

    [Header("Scene References")]
    [SerializeField] private Transform stepsContainer;
    [SerializeField] private Transform avatarsContainer;
    [SerializeField] private ScrollRect scrollRect;

    [Header("UI Panels")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject startPopupPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject fakeGamePanel;

    [Header("UI Controllers")]
    [SerializeField] private LavaQuestHUD hudController;

    [Header("Buttons")]
    [SerializeField] private Button popupStartButton;
    [SerializeField] private Button mapPlayButton;
    [SerializeField] private Button gameWinButton;
    [SerializeField] private Button gameFailButton;

    // Runtime State
    private List<AvatarView> activeAvatars = new List<AvatarView>();
    private List<Transform> steps = new List<Transform>();
    private int currentStepIndex = 0;

    private void Start()
    {
        InitializeLevel();
        InitializeParticipants();
        RegisterEvents();

        // Initial State
        SwitchState(GameState.Lobby);
    }

    private void InitializeLevel()
    {
        foreach (Transform step in stepsContainer)
            steps.Add(step);
    }

    private void InitializeParticipants()
    {
        // 1. Create Main Player
        CreateAvatar(new ParticipantData
        {
            Id = "player_main",
            IsMainPlayer = true,
            ProfileSprite = null // Or fetch from a user profile system
        });

        // 2. Create Bots
        for (int i = 0; i < totalParticipantCount - 1; i++)
        {
            CreateAvatar(new ParticipantData
            {
                Id = $"bot_{i}",
                IsMainPlayer = false,
                ProfileSprite = GetRandomSprite()
            });
        }
    }

    private void CreateAvatar(ParticipantData data)
    {
        if (steps.Count == 0) return;

        AvatarView avatar = Instantiate(avatarPrefab, avatarsContainer);
        avatar.Setup(data);

        // Add random horizontal offset to avoid stacking
        Vector3 startPos = steps[0].position;
        startPos.x += Random.Range(-80f, 80f);

        avatar.SetPosition(startPos);

        activeAvatars.Add(avatar);
    }

    private void RegisterEvents()
    {
        popupStartButton.onClick.AddListener(() => SwitchState(GameState.Map));
        mapPlayButton.onClick.AddListener(() => SwitchState(GameState.FakeGame));
        gameWinButton.onClick.AddListener(() => ProcessRound(true));
        gameFailButton.onClick.AddListener(() => ProcessRound(false));
    }

    private void SwitchState(GameState state)
    {
        lobbyPanel.SetActive(false);
        startPopupPanel.SetActive(false);
        mapPanel.SetActive(false);
        fakeGamePanel.SetActive(false);

        switch (state)
        {
            case GameState.Lobby:
                lobbyPanel.SetActive(true);
                startPopupPanel.SetActive(true);
                break;
            case GameState.Map:
                mapPanel.SetActive(true);
                UpdateHUD("Event Started!");
                break;
            case GameState.FakeGame:
                fakeGamePanel.SetActive(true);
                break;
        }
    }

    private void ProcessRound(bool playerWon)
    {
        SwitchState(GameState.Map);

        int survivors = 0;

        foreach (var avatar in activeAvatars)
        {
            if (avatar.Data.IsEliminated) continue;

            // Logic: Determine round result for each participant
            bool passed = false;

            if (avatar.Data.IsMainPlayer)
                passed = playerWon;
            else
                passed = Random.value > 0.3f; // 70% chance for bots to pass

            // Action: Move or Eliminate
            if (passed && currentStepIndex < steps.Count - 1)
            {
                Transform targetStep = steps[currentStepIndex + 1];
                Vector3 targetPos = targetStep.position;
                targetPos.x += Random.Range(-80f, 80f);

                avatar.PlayJump(targetPos);
                survivors++;
            }
            else
            {
                avatar.Data.IsEliminated = true;
                Vector3 dropPos = avatar.transform.position + (Vector3.down * 3000f);
                avatar.PlayFall(dropPos);
            }
        }

        if (playerWon)
        {
            currentStepIndex++;
            UpdateHUD("Stage Completed!");
            StartCoroutine(FocusCameraRoutine());
        }
        else
        {
            UpdateHUD("You Fell!");
        }

        UpdatePlayerCount(survivors);
    }

    // --- Helpers ---

    private void UpdateHUD(string status)
    {
        if (hudController == null) return;
        hudController.SetInfoMessage(status);
        hudController.UpdateLevelText(currentStepIndex, steps.Count);
    }

    private void UpdatePlayerCount(int count)
    {
        if (hudController != null)
            hudController.UpdatePlayersText(count, totalParticipantCount);
    }

    private Sprite GetRandomSprite()
    {
        if (botAvatarsLibrary == null || botAvatarsLibrary.Count == 0) return null;
        return botAvatarsLibrary[Random.Range(0, botAvatarsLibrary.Count)];
    }

    private IEnumerator FocusCameraRoutine()
    {
        yield return new WaitForSeconds(0.1f);

        if (scrollRect != null && steps.Count > 0)
        {
            // Calculate normalized scroll position (Inverted because vertical scroll starts at 1)
            float targetScroll = 1f - ((float)currentStepIndex / (float)steps.Count);
            float startScroll = scrollRect.verticalNormalizedPosition;
            float t = 0;

            // Smooth linear interpolation
            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                scrollRect.verticalNormalizedPosition = Mathf.Lerp(startScroll, targetScroll, t);
                yield return null;
            }
        }
    }

    private enum GameState { Lobby, Map, FakeGame }
}