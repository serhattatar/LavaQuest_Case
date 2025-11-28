using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for any UI element that needs a Pop-In/Pop-Out scale animation.
/// Inherit from this to get Show/Hide functionality automatically.
/// </summary>
public abstract class UIScaleAnimation : MonoBehaviour
{
    [Header("Base Animation Config")]
    [SerializeField] protected Transform animTransform;
    [SerializeField] protected float animDuration = 0.3f;
    [SerializeField] protected AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public virtual void Show()
    {
        gameObject.SetActive(true);
        animTransform.localScale = Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(AnimateScale(Vector3.one, showCurve, null));
    }

    public virtual void Hide(Action onComplete = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateScale(Vector3.zero, hideCurve, () =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }));
    }

    protected IEnumerator AnimateScale(Vector3 target, AnimationCurve curve, Action onComplete)
    {
        Vector3 start = animTransform.localScale;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float curveValue = curve.Evaluate(t);
            animTransform.localScale = Vector3.LerpUnclamped(start, target, curveValue);
            yield return null;
        }

        animTransform.localScale = target;
        onComplete?.Invoke();
    }
}