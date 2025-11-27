using System.Collections;
using UnityEngine;

public class AvatarController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private RectTransform rectTransform;

    [Header("Animation Settings")]
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float fallDuration = 0.6f;
    [SerializeField] private float jumpHeight = 250f;

    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve jumpArcCurve;
    [SerializeField] private AnimationCurve squashStretchCurve;
    [SerializeField] private AnimationCurve fallCurve;

    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;

    /// <summary>
    /// Initiates the parabolic jump sequence towards the target position.
    /// </summary>
    /// <param name="targetPos">The world space destination.</param>
    /// <param name="onComplete">Optional callback invoked when the animation finishes.</param>
    public void PlayJumpAnimation(Vector3 targetPos, System.Action onComplete = null)
    {
        if (isAnimating) return;
        StartCoroutine(RoutineJump(targetPos, onComplete));
    }

    /// <summary>
    /// Initiates the falling sequence (e.g. into lava).
    /// </summary>
    public void PlayFallAnimation(Vector3 targetPos, System.Action onComplete = null)
    {
        if (isAnimating) return;
        StartCoroutine(RoutineFall(targetPos, onComplete));
    }

    public void SetPosition(Vector3 pos)
    {
        rectTransform.position = pos;
    }

    private IEnumerator RoutineJump(Vector3 targetPos, System.Action onComplete)
    {
        isAnimating = true;
        Vector3 startPos = rectTransform.position;
        Vector3 initialScale = Vector3.one;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / jumpDuration);

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, normalizedTime);
            currentPos.y += jumpArcCurve.Evaluate(normalizedTime) * jumpHeight;
            rectTransform.position = currentPos;

            float deformation = squashStretchCurve.Evaluate(normalizedTime);
            rectTransform.localScale = new Vector3(
                initialScale.x * (1 - deformation),
                initialScale.y * (1 + deformation),
                1f
            );

            yield return null;
        }

        rectTransform.position = targetPos;
        rectTransform.localScale = initialScale;
        isAnimating = false;
        onComplete?.Invoke();
    }

    private IEnumerator RoutineFall(Vector3 targetPos, System.Action onComplete)
    {
        isAnimating = true;
        Vector3 startPos = rectTransform.position;
        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / fallDuration);
            float curveValue = fallCurve.Evaluate(normalizedTime);

            rectTransform.position = Vector3.LerpUnclamped(startPos, targetPos, curveValue);
            rectTransform.rotation = Quaternion.Euler(0, 0, normalizedTime * 45f);

            yield return null;
        }

        isAnimating = false;
        onComplete?.Invoke();
    }
}