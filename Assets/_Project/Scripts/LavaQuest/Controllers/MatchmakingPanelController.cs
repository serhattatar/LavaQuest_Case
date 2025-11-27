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

    [Header("Timing Config")]
    [SerializeField] private float totalDuration = 3.0f;
    [SerializeField] private float finishDelay = 0.2f;

    [Header("Population Config")]
    [SerializeField] private int targetPlayerCount = 100;
    [SerializeField] private int maxVisualAvatars = 45;

    [Header("Layout Pattern")]
    [SerializeField] private float rowHeight = 70f;
    [SerializeField] private float colWidth = 90f;
    [SerializeField] private float randomness = 15f;
    [SerializeField] private float startYOffset = -200f;

    // Defines diamond pattern capacity per row
    private readonly int[] rowCapacityPattern = { 1, 2, 3, 4, 5, 5, 4, 3, 2 };

    private ObjectPooler objectPooler;
    private List<AvatarView> spawnedAvatars = new List<AvatarView>();

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

        // Calculate total capacity of our pattern
        int patternCapacity = 0;
        foreach (int count in rowCapacityPattern) patternCapacity += count;

        // Ensure we don't spawn more than visually needed
        int visualsLimit = Mathf.Min(data.Count, Mathf.Min(maxVisualAvatars, patternCapacity));

        while (currentCount < targetPlayerCount)
        {
            timer += Time.deltaTime;
            float progress = timer / totalDuration;

            currentCount = Mathf.FloorToInt(Mathf.Lerp(0, targetPlayerCount, progress));
            UpdateCounter(currentCount);

            int visualsTarget = Mathf.FloorToInt(progress * visualsLimit);

            while (visualsTarget > visualsSpawned && visualsSpawned < data.Count)
            {
                SpawnVisualAvatar(data[visualsSpawned], visualsSpawned);
                visualsSpawned++;
            }

            yield return null;
        }

        UpdateCounter(targetPlayerCount);
        yield return new WaitForSeconds(finishDelay);
        btnTapToContinue.gameObject.SetActive(true);
    }

    private void SpawnVisualAvatar(ParticipantData data, int visualIndex)
    {
        AvatarView avatar = objectPooler.GetAvatar(avatarPileContainer);
        avatar.Configure(data);
        avatar.PlayPopAnimation();

        Vector2 targetPos = CalculatePatternPosition(visualIndex);
        avatar.SetPosition(targetPos);

        float randomAngle = UnityEngine.Random.Range(-10f, 10f);
        avatar.transform.localRotation = Quaternion.Euler(0, 0, randomAngle);

        // Sorting Logic: Player (0) at Front, others Back
        if (visualIndex == 0)
        {
            avatar.transform.SetAsLastSibling();
            avatar.transform.localScale = Vector3.one * 1.2f; // Slight highlight
        }
        else
        {
            avatar.transform.SetAsFirstSibling();
        }

        spawnedAvatars.Add(avatar);
    }

    private Vector2 CalculatePatternPosition(int index)
    {
        int currentRow = 0;
        int itemsInCurrentRow = 0;
        int countConsumed = 0;

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

        int indexInRow = index - countConsumed;
        float yPos = startYOffset + (currentRow * rowHeight);
        float totalRowWidth = itemsInCurrentRow * colWidth;
        float xPos = (indexInRow * colWidth) - (totalRowWidth / 2f) + (colWidth / 2f);

        xPos += UnityEngine.Random.Range(-randomness, randomness);
        yPos += UnityEngine.Random.Range(-randomness, randomness);

        return new Vector2(xPos, yPos);
    }

    private void UpdateCounter(int count)
    {
        if (textCounter) textCounter.text = $"{count}/{targetPlayerCount}";
    }
}