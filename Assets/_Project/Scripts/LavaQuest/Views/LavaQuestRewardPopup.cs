using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class LavaQuestRewardPopup : UIScaleAnimation
{
    public event Action OnCollectClicked;

    [Header("Interaction")]
    [Tooltip("Button to claim the reward and close the popup.")]
    [SerializeField] private Button btnCollect;

    [Header("Layout Containers")]
    [Tooltip("The parent container where the Local Player's large avatar will be spawned.")]
    [SerializeField] private Transform mainWinnerContainer;

    [Tooltip("The parent container (Grid/Horizontal Layout) for other winners.")]
    [SerializeField] private Transform coWinnersContainer;

    [Header("Information Text")]
    [Tooltip("Text displaying: 'You are sharing the reward with X others'.")]
    [SerializeField] private TextMeshProUGUI txtSharingInfo;

    [Header("Visual Configuration")]
    [Tooltip("Maximum number of other winners to display in the small grid.")]
    [SerializeField] private int maxCoWinnersDisplay = 5;

    [Tooltip("Scale multiplier for the main player's avatar.")]
    [SerializeField] private float mainAvatarScale = 1.3f;

    [Tooltip("Scale multiplier for other winners' avatars.")]
    [SerializeField] private float coAvatarScale = 0.8f;

    private ObjectPooler objectPooler;
    private List<AvatarView> spawnedAvatars = new List<AvatarView>();

    private void Start()
    {
        if (btnCollect != null)
        {
            btnCollect.onClick.AddListener(() => OnCollectClicked?.Invoke());
        }
    }

    /// <summary>
    /// Initializes the popup with the list of winners.
    /// Separates the local player from the rest for visual hierarchy.
    /// </summary>
    /// <param name="winners">List of all participants who won.</param>
    /// <param name="pooler">Reference to the ObjectPooler.</param>
    public void Initialize(List<ParticipantData> winners, ObjectPooler pooler)
    {
        this.objectPooler = pooler;

        CleanupVisuals();

        if (winners == null || winners.Count == 0) return;

        // 1. Separate Local Player and Others
        ParticipantData localPlayer = winners.FirstOrDefault(p => p.IsMainPlayer);
        List<ParticipantData> otherWinners = winners.Where(p => !p.IsMainPlayer).ToList();

        // 2. Spawn Main Player (The Hero)
        if (localPlayer != null)
        {
            SpawnAvatar(localPlayer, mainWinnerContainer, mainAvatarScale);
        }

        // 3. Configure Sharing Text
        int othersCount = otherWinners.Count;
        if (othersCount > 0)
        {
            if (txtSharingInfo)
            {
                txtSharingInfo.gameObject.SetActive(true);
                txtSharingInfo.text = $"You are sharing the reward with <color=#FFD700><b>{othersCount}</b></color> other winners!";
            }
        }
        else
        {
            // Solo Win Scenario
            if (txtSharingInfo) txtSharingInfo.gameObject.SetActive(false);
        }

        // 4. Spawn Other Winners (The Crowd)
        int displayCount = Mathf.Min(othersCount, maxCoWinnersDisplay);
        for (int i = 0; i < displayCount; i++)
        {
            SpawnAvatar(otherWinners[i], coWinnersContainer, coAvatarScale);
        }
    }

    private void SpawnAvatar(ParticipantData data, Transform parent, float scale)
    {
        if (objectPooler == null) return;

        AvatarView avatar = objectPooler.GetAvatar(parent);

        // Reset transform properties to ensure layout group handles it correctly
        avatar.transform.localScale = Vector3.one * scale;
        avatar.transform.localPosition = Vector3.zero;
        avatar.transform.localRotation = Quaternion.identity;

        avatar.Configure(data);

        // Force the avatar to look "Idle" or "Winning" (Reset any fall rotation)
        avatar.ForceStop();

        spawnedAvatars.Add(avatar);
    }

    private void CleanupVisuals()
    {
        foreach (var avatar in spawnedAvatars)
        {
            if (avatar != null && objectPooler != null)
            {
                objectPooler.ReturnAvatar(avatar);
            }
        }
        spawnedAvatars.Clear();
    }
}