using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SimpleButtonTween : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    [SerializeField] private float pressScale = 0.9f;
    [SerializeField] private float duration = 0.1f;

    private Vector3 originalScale;

    private void Awake() => originalScale = transform.localScale;

    private void OnDisable()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale * pressScale));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        float t = 0;
        Vector3 start = transform.localScale;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            transform.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }
        transform.localScale = target;
    }
}