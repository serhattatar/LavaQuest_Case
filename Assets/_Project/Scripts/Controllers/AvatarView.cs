using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvatarView : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image profileImage;

    [Header("Animation Settings")]
    [SerializeField] private float jumpDuration = 0.6f;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private float jumpHeight = 250f;

    [Header("Juice Curves")]
    [SerializeField] private AnimationCurve jumpArcCurve;
    [SerializeField] private AnimationCurve squashStretchCurve;
    [SerializeField] private AnimationCurve fallCurve;

    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;
    public ParticipantData Data { get; private set; }

    public void Setup(ParticipantData data)
    {
        this.Data = data;

        if (profileImage != null && data.ProfileSprite != null)
            profileImage.sprite = data.ProfileSprite;

        // Render the main player on top of bots
        if (data.IsMainPlayer)
        {
            transform.SetAsLastSibling();
        }
    }

    public void SetPosition(Vector3 position)
    {
        rectTransform.position = position;
    }

    public void PlayJump(Vector3 targetPos, System.Action onComplete = null)
    {
        if (isAnimating) return;
        StartCoroutine(RoutineJump(targetPos, onComplete));
    }

    public void PlayFall(Vector3 targetPos, System.Action onComplete = null)
    {
        if (isAnimating) return;
        StartCoroutine(RoutineFall(targetPos, onComplete));
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
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            // Parabolic Arc Logic
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);
            currentPos.y += jumpArcCurve.Evaluate(t) * jumpHeight;
            rectTransform.position = currentPos;

            // Squash & Stretch Logic
            float deformation = squashStretchCurve.Evaluate(t);
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
            float t = Mathf.Clamp01(elapsed / fallDuration);

            // Accelerated Fall Logic
            float fallT = fallCurve.Evaluate(t);

            rectTransform.position = Vector3.LerpUnclamped(startPos, targetPos, fallT);
            rectTransform.rotation = Quaternion.Euler(0, 0, t * 60f);

            yield return null;
        }

        isAnimating = false;
        gameObject.SetActive(false);
        onComplete?.Invoke();
    }
}