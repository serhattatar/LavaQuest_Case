using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestPopup : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnClose;

    [Header("Animation")]
    [SerializeField] private float animDuration = 0.3f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public event Action OnStartClicked;
    public event Action OnCloseClicked;

    private void Start()
    {
        btnStart.onClick.AddListener(() => OnStartClicked?.Invoke());
        btnClose.onClick.AddListener(() => OnCloseClicked?.Invoke());
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.localScale = Vector3.zero;
        StopAllCoroutines();
        StartCoroutine(AnimateScale(Vector3.one, showCurve, null));
    }

    // Accepts a callback to notify when animation finishes
    public void Hide(Action onComplete = null)
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

    private IEnumerator AnimateScale(Vector3 target, AnimationCurve curve, Action onComplete)
    {
        Vector3 start = transform.localScale;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / animDuration;
            float curveValue = curve.Evaluate(t);
            transform.localScale = Vector3.LerpUnclamped(start, target, curveValue);
            yield return null;
        }

        transform.localScale = target;
        onComplete?.Invoke();
    }
}