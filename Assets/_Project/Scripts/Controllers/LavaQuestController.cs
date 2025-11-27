using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject gameSimPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button winDebugButton;
    [SerializeField] private Button failDebugButton;

    [Header("Scene References")]
    [SerializeField] private RectTransform avatarRect;
    [SerializeField] private Transform stepsContainer;

    [Header("Animation Configuration")]
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private float jumpHeight = 200f;
    [SerializeField] private AnimationCurve jumpArcCurve;
    [SerializeField] private AnimationCurve squashStretchCurve;
    [SerializeField] private AnimationCurve fallCurve;

    private List<Transform> steps = new List<Transform>();
    private int currentStepIndex = 0;
    private bool isAnimating = false;

    private void Start()
    {
        InitializeSteps();
        RegisterEvents();
    }

    private void InitializeSteps()
    {
        foreach (Transform step in stepsContainer)
        {
            steps.Add(step);
        }

        if (steps.Count > 0)
        {
            avatarRect.position = steps[0].position;
        }
    }

    private void RegisterEvents()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        winDebugButton.onClick.AddListener(OnWinClicked);
        failDebugButton.onClick.AddListener(OnFailClicked);
    }

    #region Event Handlers

    private void OnPlayClicked()
    {
        if (isAnimating) return;

        gameSimPanel.SetActive(true);
    }

    private void OnWinClicked()
    {
        gameSimPanel.SetActive(false);
        mapPanel.SetActive(true);

        if (currentStepIndex < steps.Count - 1)
        {
            currentStepIndex++;
            StartCoroutine(RoutineJumpToStep(steps[currentStepIndex].position));
        }
    }

    private void OnFailClicked()
    {
        gameSimPanel.SetActive(false);
        mapPanel.SetActive(true);

        Vector3 dropTarget = avatarRect.position + (Vector3.down * 2500f);
        StartCoroutine(RoutineFallDown(dropTarget));
    }

    #endregion

    #region Coroutines

    private IEnumerator RoutineJumpToStep(Vector3 targetPos)
    {
        isAnimating = true;

        Vector3 startPos = avatarRect.position;
        Vector3 initialScale = Vector3.one;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, normalizedTime);
            float heightOffset = jumpArcCurve.Evaluate(normalizedTime) * jumpHeight;
            currentPos.y += heightOffset;

            avatarRect.position = currentPos;

            float deformation = squashStretchCurve.Evaluate(normalizedTime);
            avatarRect.localScale = new Vector3(
                initialScale.x * (1f - deformation),
                initialScale.y * (1f + deformation),
                1f
            );

            yield return null;
        }

        avatarRect.position = targetPos;
        avatarRect.localScale = initialScale;
        isAnimating = false;
    }

    private IEnumerator RoutineFallDown(Vector3 targetPos)
    {
        isAnimating = true;

        Vector3 startPos = avatarRect.position;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / animationDuration);

            float t = fallCurve.Evaluate(normalizedTime);

            avatarRect.position = Vector3.LerpUnclamped(startPos, targetPos, t);

            avatarRect.rotation = Quaternion.Euler(0, 0, t * 45f);

            yield return null;
        }

        isAnimating = false;
    }

    #endregion
}