using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private AvatarController playerAvatar;
    [SerializeField] private Transform stepsContainer;

    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject gameSimPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button winDebugButton;
    [SerializeField] private Button failDebugButton;

    private List<Transform> steps = new List<Transform>();
    private int currentStepIndex = 0;

    private void Start()
    {
        InitializeLevel();
        RegisterEvents();
    }

    private void InitializeLevel()
    {
        foreach (Transform step in stepsContainer)
        {
            steps.Add(step);
        }

        if (steps.Count > 0)
        {
            playerAvatar.SetPosition(steps[0].position);
        }
    }

    private void RegisterEvents()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        winDebugButton.onClick.AddListener(OnWinClicked);
        failDebugButton.onClick.AddListener(OnFailClicked);
    }

    private void OnPlayClicked()
    {
        if (playerAvatar.IsAnimating) return;
        gameSimPanel.SetActive(true);
    }

    private void OnWinClicked()
    {
        gameSimPanel.SetActive(false);
        mapPanel.SetActive(true);

        if (currentStepIndex < steps.Count - 1)
        {
            currentStepIndex++;
            playerAvatar.PlayJumpAnimation(steps[currentStepIndex].position, OnJumpCompleted);
        }
    }

    private void OnFailClicked()
    {
        gameSimPanel.SetActive(false);
        mapPanel.SetActive(true);

        Vector3 dropTarget = playerAvatar.transform.position + (Vector3.down * 2000f);
        playerAvatar.PlayFallAnimation(dropTarget, OnLevelFailed);
    }

    private void OnJumpCompleted()
    {
        // TODO: Implement SFX or particle effects here
    }

    private void OnLevelFailed()
    {
        // TODO: Implement Game Over screen logic
    }
}