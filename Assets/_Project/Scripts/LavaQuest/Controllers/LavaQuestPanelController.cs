using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestPanelController : MonoBehaviour
{
    public System.Action OnPlayRequested;
    public System.Action OnRestartRequested;
    public System.Action<List<ParticipantData>> OnQuestCompleted;

    [Header("References")]
    [SerializeField] private LavaQuestHUD hudController;
    [SerializeField] private Button btnPlayAction;
    [SerializeField] private Transform stepsContainer;
    [SerializeField] private Transform avatarsContainer;

    [Header("Configuration")]
    [SerializeField] private float stepRadius = 80f;
    [SerializeField] private float dropDistance = 3000f; 
    [SerializeField] private float scrollSpeed = 2f;

    [Header("Animation Timings")]
    [SerializeField] private float animationDelay = 0.6f; 
    [SerializeField] private float roundEndDelay = 0.5f;
    [SerializeField] private float winDelay = 1.5f;
    [SerializeField] private float failDelay = 2.0f;

    private ObjectPooler objectPooler;
    private List<AvatarView> activeAvatars = new List<AvatarView>();
    private List<Vector2> stepAnchoredPositions = new List<Vector2>();
    private int currentStepIndex = 0;
    private int totalParticipants = 0;
    private bool isRoundInProgress = false;

    private Coroutine timerCoroutine;
    private System.DateTime cachedEndTime;
    private bool isInitialized = false;

    private void Start()
    {
        btnPlayAction.onClick.AddListener(HandlePlayClick);
    }
    private void OnEnable()
    {
        if (isInitialized)
        {
            StartTimer();
        }
    }

    private void OnDisable()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private void HandlePlayClick()
    {
        if (isRoundInProgress) return;

        SetPlayButtonState(false);
        ForceStopAllAvatars();
        OnPlayRequested?.Invoke();
    }

    public void Initialize(List<ParticipantData> participants, ObjectPooler pooler)
    {
        this.objectPooler = pooler;
        this.totalParticipants = participants.Count;
        this.currentStepIndex = 0;
        this.isRoundInProgress = false;

        SetPlayButtonState(true);
        CacheStepPositions();
        SpawnAvatars(participants);
        UpdateHUD("Beat 8 Levels to complete the challange!");


        cachedEndTime = GameSessionBridge.Instance.GetEventEndTime();
        isInitialized = true;
        StartTimer();
    }

    private void ForceStopAllAvatars()
    {
        foreach (var avatar in activeAvatars)
        {
            if (avatar.gameObject.activeSelf) avatar.ForceStop();
        }
    }

    private void SetPlayButtonState(bool isInteractable)
    {
        btnPlayAction.interactable = isInteractable;
    }

    private void CacheStepPositions()
    {
        stepAnchoredPositions.Clear();
        foreach (Transform t in stepsContainer)
        {
            RectTransform rt = t.GetComponent<RectTransform>();
            if (rt != null) stepAnchoredPositions.Add(rt.anchoredPosition);
        }
    }

    private void SpawnAvatars(List<ParticipantData> participants)
    {
        // Cleanup existing
        foreach (var avatar in activeAvatars)
        {
            avatar.OnLanded -= HandleAvatarLanded;
            objectPooler.ReturnAvatar(avatar);
        }
        activeAvatars.Clear();

        if (stepAnchoredPositions.Count == 0) return;

        foreach (var data in participants)
        {
            AvatarView avatar = objectPooler.GetAvatar(avatarsContainer);
            avatar.Configure(data);
            avatar.SetPosition(GetRandomPos(0));
            avatar.OnLanded += HandleAvatarLanded; // Subscribe for VFX
            activeAvatars.Add(avatar);
        }
    }

    private void HandleAvatarLanded(Vector3 worldPos)
    {
        objectPooler.PlayVFX(worldPos);
    }
    
    public void ExecuteRoundLogic(bool playerWon)
    {
        if (isRoundInProgress) return;
        StartCoroutine(ProcessRoundRoutine(playerWon));
    }

    /// <summary>
    /// Orchestrates the entire flow of a game round: Logic -> Animation -> Result.
    /// </summary>
    private IEnumerator ProcessRoundRoutine(bool playerWon)
    {
        isRoundInProgress = true;
        SetPlayButtonState(false);

        // 1. LOGIC: Determine who passes and who fails
        DetermineRoundOutcomes(playerWon, out List<AvatarView> winners, out List<AvatarView> losers);

        // 2. UI: Update HUD based on the main player's result
        UpdateRoundHUD(playerWon, winners.Count);
        // 3. ANIMATION: Winners Jump
        PlayJumpAnimations(winners);

        // Wait before losers fall (only if there were winners jumping)
        if (winners.Count > 0)
            yield return new WaitForSeconds(animationDelay);

        // 4. ANIMATION: Losers Fall & Data Update
        PlayEliminationAnimations(losers);

        // 5. FLOW: Handle Level Progression or Failure
        if (playerWon)
        {
            // Move logic index to next step
            currentStepIndex++;
            yield return StartCoroutine(HandleWinProgression(winners));
        }
        else
        {
            yield return StartCoroutine(HandleFailure());
        }
    }

    private void DetermineRoundOutcomes(bool playerWon, out List<AvatarView> winners, out List<AvatarView> losers)
    {
        winners = new List<AvatarView>();
        losers = new List<AvatarView>();

        foreach (var avatar in activeAvatars)
        {
            if (avatar.Data.IsEliminated) continue;

            // Logic: Main player outcome is fixed, bots have random chance
            bool passed = avatar.Data.IsMainPlayer ? playerWon : Random.value > 0.3f;

            // Safety Check: Can't advance beyond the final step
            if (passed && currentStepIndex < stepAnchoredPositions.Count - 1)
            {
                winners.Add(avatar);
            }
            else
            {
                losers.Add(avatar);
            }
        }
    }

    private void PlayJumpAnimations(List<AvatarView> winners)
    {
        int nextStepIndex = currentStepIndex + 1;

        foreach (var w in winners)
        {
            Vector2 target = GetRandomPos(nextStepIndex);
            w.PlayJump(target, Random.Range(0f, 0.4f));
        }
    }

    private void PlayEliminationAnimations(List<AvatarView> losers)
    {
        // Determine where losers should fall from (current or next step visual center)
        Vector2 dropOriginCenter;
        if (currentStepIndex + 1 < stepAnchoredPositions.Count)
            dropOriginCenter = stepAnchoredPositions[currentStepIndex + 1];
        else
            dropOriginCenter = stepAnchoredPositions[currentStepIndex];

        foreach (var l in losers)
        {
            // CRITICAL: Mark as eliminated in data
            l.Data.IsEliminated = true;

            // Calculate Fall Target
            float randomXOffset = Random.Range(-stepRadius, stepRadius);
            Vector2 dropTarget = new Vector2(dropOriginCenter.x + randomXOffset, dropOriginCenter.y - dropDistance);

            l.PlayFall(dropTarget, Random.Range(0f, 0.4f));
        }
    }

    private void UpdateRoundHUD(bool playerWon, int predictedSurvivorCount)
    {
        if (playerWon)
            UpdateHUD("Congratulations! You Advanced to the next step!");
        else
            UpdateHUD("You Failed The Challenge.");

        // Update survivor count immediately for feedback
        if (hudController)
            hudController.UpdatePlayersText(predictedSurvivorCount, totalParticipants);
    }

    // -------------------------------------------------------------------------
    // FLOW HANDLERS
    // -------------------------------------------------------------------------

    private IEnumerator HandleWinProgression(List<AvatarView> currentRoundWinners)
    {
        // Check if this was the final step
        if (currentStepIndex >= stepAnchoredPositions.Count - 1)
        {
            // FINAL VICTORY
            yield return new WaitForSeconds(winDelay);

            // Create final data list from the VISUAL winners to ensure consistency
            List<ParticipantData> finalWinnersData = new List<ParticipantData>();
            foreach (var w in currentRoundWinners)
            {
                finalWinnersData.Add(w.Data);
            }

            OnQuestCompleted?.Invoke(finalWinnersData);
        }
        else
        {
            // NEXT ROUND PREPARATION
            yield return new WaitForSeconds(roundEndDelay);
            isRoundInProgress = false;
            SetPlayButtonState(true);
        }
    }

    private IEnumerator HandleFailure()
    {
        yield return new WaitForSeconds(failDelay);
        OnRestartRequested?.Invoke();
    }

    private void StartTimer()
    {
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(RoutineEventTimer(cachedEndTime));
    }

    private IEnumerator RoutineEventTimer(System.DateTime targetEndTime)
    {
        var waitForOneSecond = new WaitForSeconds(1f);

        while (true)
        {
            System.TimeSpan remaining = targetEndTime - System.DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                hudController.SetClockMessage("00:00:00");
                yield break; 
            }

            string timeStr = string.Format("{0:D2}:{1:D2}:{2:D2}",
                remaining.Hours,
                remaining.Minutes,
                remaining.Seconds);

            hudController.SetClockMessage(timeStr);

            yield return waitForOneSecond;
        }
    }

    private Vector2 GetRandomPos(int idx)
    {
        if (idx >= stepAnchoredPositions.Count) return Vector2.zero;
        Vector2 rnd = Random.insideUnitCircle * stepRadius;
        return stepAnchoredPositions[idx] + new Vector2(rnd.x, rnd.y * 0.2f);
    }

    private void UpdateHUD(string status)
    {
        if (hudController)
        {
            hudController.SetInfoMessage(status);

            int displayLevel = currentStepIndex + 1;
            int totalLevels = Mathf.Max(1, stepAnchoredPositions.Count - 1);
            if (displayLevel > totalLevels) displayLevel = totalLevels;

            hudController.UpdateLevelText(displayLevel, totalLevels);
        }
    }
}