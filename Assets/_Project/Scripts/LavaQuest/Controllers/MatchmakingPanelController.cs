using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchmakingPanelController : MonoBehaviour
{
    public event Action OnContinueRequested;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI textCounter;
    [SerializeField] private Transform avatarPileContainer;
    [SerializeField] private Button btnTapToContinue;

    [Header("Config")]
    [SerializeField] private float totalDuration = 3.0f;
    [SerializeField] private int targetPlayerCount = 100;

    [Header("Layout Settings")]
    [SerializeField] private float rowHeight = 70f;  // Vertical distance between rows
    [SerializeField] private float colWidth = 90f;   // Horizontal distance between items
    [SerializeField] private float randomness = 15f; // Jitter to avoid perfect grid look
    [SerializeField] private float startYOffset = -200f; // Start position (Bottom of the container)

    private ObjectPooler objectPooler;
    private List<AvatarView> spawnedAvatars = new List<AvatarView>();

    // Defines the "Egg/Diamond" shape: Number of avatars per row, from front to back
    private readonly int[] rowCapacityPattern = { 1, 2, 3, 4, 7, 7, 4, 3, 2,1};

    private void Start()
    {
        btnTapToContinue.onClick.AddListener(() => OnContinueRequested?.Invoke());
    }

    public void Initialize(List<ParticipantData> mockData, ObjectPooler pooler)
    {
        this.objectPooler = pooler;

        btnTapToContinue.gameObject.SetActive(false);

        CleanupVisuals();

        StartCoroutine(RoutineSimulateMatchmaking(mockData));
    }

    private void CleanupVisuals()
    {
        foreach (var avatar in spawnedAvatars)
        {
            if (avatar != null && objectPooler != null)
                objectPooler.ReturnAvatar(avatar);
        }
        spawnedAvatars.Clear();
    }

    private IEnumerator RoutineSimulateMatchmaking(List<ParticipantData> data)
    {
        int currentCount = 0;
        float timer = 0f;
        int visualsSpawned = 0;

        // Calculate max visuals based on our pattern capacity
        int maxVisuals = 0;
        foreach (int count in rowCapacityPattern) maxVisuals += count;
        // Limit data if we have fewer slots than data
        maxVisuals = Mathf.Min(data.Count, maxVisuals);

        while (currentCount < targetPlayerCount)
        {
            timer += Time.deltaTime;
            float progress = timer / totalDuration;

            currentCount = Mathf.FloorToInt(Mathf.Lerp(0, targetPlayerCount, progress));
            UpdateCounter(currentCount);

            // Spawn logic
            int visualsTarget = Mathf.FloorToInt(progress * maxVisuals);

            while (visualsTarget > visualsSpawned && visualsSpawned < data.Count)
            {
                SpawnVisualAvatar(data[visualsSpawned], visualsSpawned);
                visualsSpawned++;
            }

            yield return null;
        }

        UpdateCounter(targetPlayerCount);
        yield return new WaitForSeconds(0.2f);
        btnTapToContinue.gameObject.SetActive(true);
    }

    private void SpawnVisualAvatar(ParticipantData data, int visualIndex)
    {
        AvatarView avatar = objectPooler.GetAvatar(avatarPileContainer);
        avatar.Configure(data);
        avatar.PlayPopAnimation();

        // 1. Calculate Diamond/Egg Position
        Vector2 targetPos = CalculateEggPosition(visualIndex);
        avatar.SetPosition(targetPos);

        // 2. Random Rotation (Organic feel)
        float randomAngle = UnityEngine.Random.Range(-10f, 10f);
        avatar.transform.localRotation = Quaternion.Euler(0, 0, randomAngle);

        // 3. Layering Logic (Critical for the look)
        // The first avatar (Player) must be at the FRONT (Last Sibling).
        // New avatars are behind, so they go to the BACK (First Sibling).
        if (visualIndex == 0)
        {
            avatar.transform.SetAsLastSibling(); // Front
            // Optional: Make player slightly bigger
            avatar.transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            avatar.transform.SetAsFirstSibling(); // Behind previous ones
        }

        spawnedAvatars.Add(avatar);
    }

    private Vector2 CalculateEggPosition(int index)
    {
        int currentRow = 0;
        int itemsInCurrentRow = 0;
        int countConsumed = 0;

        // Find which row this index belongs to
        for (int i = 0; i < rowCapacityPattern.Length; i++)
        {
            if (index < countConsumed + rowCapacityPattern[i])
            {
                currentRow = i;
                itemsInCurrentRow = rowCapacityPattern[i];
                break;
            }
            countConsumed += rowCapacityPattern[i];
        }

        // Index relative to the row (0, 1, 2...)
        int indexInRow = index - countConsumed;

        // Calculate Y (Depth) - Moves UP as rows go back
        float yPos = startYOffset + (currentRow * rowHeight);

        // Calculate X (Width) - Centered
        // Formula: (Index * Width) - (TotalWidth / 2) + (HalfItemWidth)
        float totalRowWidth = itemsInCurrentRow * colWidth;
        float xPos = (indexInRow * colWidth) - (totalRowWidth / 2f) + (colWidth / 2f);

        // Add Random Jitter for organic look
        xPos += UnityEngine.Random.Range(-randomness, randomness);
        yPos += UnityEngine.Random.Range(-randomness, randomness);

        return new Vector2(xPos, yPos);
    }

    private void UpdateCounter(int count)
    {
        if (textCounter) textCounter.text = $"{count}/{targetPlayerCount}";
    }
}