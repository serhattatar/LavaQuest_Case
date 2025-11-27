using UnityEngine;

/// <summary>
/// [CORE SYSTEM]
/// Utility for handling device notches and safe areas.
/// </summary>
public class SafeArea : MonoBehaviour
{
    [Space(10)]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Header(" [ CORE SYSTEM - UTILITY ]")]
    [Header(" Adaptive UI / Notch Handler")]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Space(20)]
    [SerializeField][TextArea] private string _inspectorDescription = "Handles Safe Area for Notch devices automatically.";

    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void Update()
    {
        if (lastSafeArea != Screen.safeArea)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        if (panel == null) return;

        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;

        lastSafeArea = safeArea;
    }
}