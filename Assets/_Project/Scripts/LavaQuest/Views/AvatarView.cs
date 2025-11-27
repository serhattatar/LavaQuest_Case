using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AvatarView : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image profileImage;
    [SerializeField] private Image frameImage;

    [Header("Animation Timings")]
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float fallDuration = 0.8f;
    [SerializeField] private float popDuration = 0.3f;

    [Header("Physics & Juice")]
    [SerializeField] private float jumpHeight = 200f;
    [SerializeField] private float fallHopHeight = 250f;

    [Header("Curves")]
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private AnimationCurve fallCurve;
    [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public event Action<Vector3> OnLanded;
    public ParticipantData Data { get; private set; }

    private Coroutine currentAnimationRoutine;
    private Vector2 pendingTargetPos;

    public void Configure(ParticipantData data)
    {
        Data = data;

        if (profileImage != null && data.ProfileSprite != null)
            profileImage.sprite = data.ProfileSprite;

        if (frameImage != null && data.AvatarFrameSprite != null)
            frameImage.sprite = data.AvatarFrameSprite;

        if (data.IsMainPlayer)
            transform.SetAsLastSibling();

        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }

    public void SetPosition(Vector2 anchoredPos)
    {
        rectTransform.anchoredPosition = anchoredPos;
        pendingTargetPos = anchoredPos;
    }

    public void ForceStop()
    {
        if (currentAnimationRoutine != null) StopCoroutine(currentAnimationRoutine);

        rectTransform.anchoredPosition = pendingTargetPos;
        rectTransform.rotation = Quaternion.identity;
        currentAnimationRoutine = null;
    }

    public void PlayJump(Vector2 targetAnchoredPos, float delay)
    {
        ForceStop();
        pendingTargetPos = targetAnchoredPos;
        currentAnimationRoutine = StartCoroutine(JumpRoutine(targetAnchoredPos, delay));
    }

    public void PlayFall(Vector2 targetAnchoredPos, float delay)
    {
        ForceStop();
        pendingTargetPos = targetAnchoredPos;
        currentAnimationRoutine = StartCoroutine(FallRoutine(targetAnchoredPos, delay));
    }

    public void PlayPopAnimation()
    {
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        transform.localScale = Vector3.zero;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / popDuration;
            float scale = popCurve.Evaluate(t);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
        transform.localScale = Vector3.one;
    }

    private IEnumerator JumpRoutine(Vector2 target, float delay)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        Vector2 start = rectTransform.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            float clampedT = Mathf.Clamp01(t);

            Vector2 currentPos = Vector2.Lerp(start, target, clampedT);
            currentPos.y += jumpCurve.Evaluate(clampedT) * jumpHeight;

            rectTransform.anchoredPosition = currentPos;
            yield return null;
        }

        rectTransform.anchoredPosition = target;
        OnLanded?.Invoke(rectTransform.position);
        currentAnimationRoutine = null;
    }

    private IEnumerator FallRoutine(Vector2 target, float delay)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        Vector2 start = rectTransform.anchoredPosition;
        float startY = start.y;
        float targetY = target.y;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fallDuration;
            float clampedT = Mathf.Clamp01(t);

            float currentX = Mathf.Lerp(start.x, target.x, clampedT);

            // Vertical: Gravity + Hop blend
            float gravityY = Mathf.LerpUnclamped(startY, targetY, fallCurve.Evaluate(clampedT));
            float hopY = jumpCurve.Evaluate(clampedT) * fallHopHeight;

            Vector2 currentPos = new Vector2(currentX, gravityY + hopY);
            rectTransform.anchoredPosition = currentPos;

            rectTransform.rotation = Quaternion.Euler(0, 0, clampedT * 90f);

            yield return null;
        }

        gameObject.SetActive(false);
        currentAnimationRoutine = null;
    }
}