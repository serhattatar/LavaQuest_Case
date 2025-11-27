using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestPanelController : MonoBehaviour
{
    public System.Action OnPlayRequested;
    public System.Action OnRestartRequested;
    public System.Action OnQuestCompleted;

    [Header("Refs")]
    [SerializeField] private LavaQuestHUD hudController;
    [SerializeField] private Button btnPlayAction;
    [SerializeField] private Transform stepsContainer;
    [SerializeField] private Transform avatarsContainer;

    [Header("Config")]
    [SerializeField] private float stepRadius = 80f;

    private ObjectPooler objectPooler;

    private List<AvatarView> activeAvatars = new List<AvatarView>();
    private List<Vector2> stepAnchoredPositions = new List<Vector2>();
    private int currentStepIndex = 0;
    private int totalParticipants = 0;
    private bool isRoundInProgress = false;

    private void Start()
    {
        btnPlayAction.onClick.AddListener(HandlePlayClick);
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

            avatar.OnLanded += HandleAvatarLanded;

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

    private IEnumerator ProcessRoundRoutine(bool playerWon)
    {
        isRoundInProgress = true;
        SetPlayButtonState(false);

        List<AvatarView> winners = new List<AvatarView>();
        List<AvatarView> losers = new List<AvatarView>();

        foreach (var avatar in activeAvatars)
        {
            if (avatar.Data.IsEliminated) continue;

            bool passed = avatar.Data.IsMainPlayer ? playerWon : Random.value > 0.3f;

            if (passed && currentStepIndex < stepAnchoredPositions.Count - 1)
                winners.Add(avatar);
            else
                losers.Add(avatar);
        }

        if (playerWon)
        {           
            UpdateHUD("Congratulations! You Advanced to the next step!");           
        }
        else
        {
            UpdateHUD("You Failed The Challange.");
        }

        // --- WINNERS ANIMATION ---
        foreach (var w in winners)
        {
            Vector2 target = GetRandomPos(currentStepIndex + 1);
            w.PlayJump(target, Random.Range(0f, 0.4f));
        }

        if (winners.Count > 0) yield return new WaitForSeconds(0.6f);

        // --- LOSERS ANIMATION (Forward & Down) ---
        foreach (var l in losers)
        {
            l.Data.IsEliminated = true;

            // CRITICAL: Calculate a target that is FORWARD (X axis of next step) but DEEP DOWN (Y axis)
            // This makes the arc look like they tried to jump to the next step but failed.

            Vector2 nextStepCenter;
            if (currentStepIndex + 1 < stepAnchoredPositions.Count)
                nextStepCenter = stepAnchoredPositions[currentStepIndex + 1];
            else
                nextStepCenter = stepAnchoredPositions[currentStepIndex]; // Fallback for final step

            // Add randomness to the failed landing X pos
            float randomXOffset = Random.Range(-stepRadius, stepRadius);

            // Drop target: Forward X, Deep Down Y
            Vector2 dropTarget = new Vector2(nextStepCenter.x + randomXOffset, nextStepCenter.y - 3000f);

            l.PlayFall(dropTarget, Random.Range(0f, 0.4f));
        }

        if (playerWon)
        {
            currentStepIndex++;

            if (currentStepIndex >= stepAnchoredPositions.Count - 1)
            {
                yield return new WaitForSeconds(1.5f);
                OnQuestCompleted?.Invoke();
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                isRoundInProgress = false;
                SetPlayButtonState(true);
            }
        }
        else
        {
            yield return new WaitForSeconds(2.0f);
            OnRestartRequested?.Invoke();
        }

        int survivors = 0;
        foreach (var a in activeAvatars) if (!a.Data.IsEliminated) survivors++;
        if (hudController) hudController.UpdatePlayersText(survivors, totalParticipants);
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