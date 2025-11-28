using UnityEngine;
using UnityEngine.Events;

public class EditorTestTool : MonoBehaviour
{
    // The code inside this block compiles only in the Unity Editor.
    // It will be stripped out in the final build.
  

#if UNITY_EDITOR

    [Space(10)]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Header("   [ CORE SYSTEM - MOCK BACKEND ]")]
    [Header("   Simulates server/player data.")]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Space(20)]


    [Header("Target Objects (Toggle)")]
    [Tooltip("Objects added here will toggle their active state (Active <-> Inactive).")]
    public GameObject[] toggleTargets;

    [Header("Custom Actions")]
    [Tooltip("Bind functions from other scripts or UI actions here.")]
    public UnityEvent customActions;

    // Link this function to your Button's OnClick event
    public void RunTest()
    {
        // 1. Toggle Objects
        if (toggleTargets != null)
        {
            foreach (var obj in toggleTargets)
            {
                if (obj != null)
                {
                    // Registers the operation for Undo, allowing you to use CTRL+Z
                    UnityEditor.Undo.RecordObject(obj, "Toggle Active State");
                    obj.SetActive(!obj.activeSelf);
                }
            }
        }

        // 2. Execute Custom Events
        customActions?.Invoke();

        Debug.Log($"<color=cyan>[EditorTest]</color> Test executed on: {name}");
    }

    // Allows you to run the test via Right Click on the component in Inspector
    [ContextMenu("Trigger Manually")]    
    private void ManualTrigger()
    {
        RunTest();
    }


#else
    private void Awake()
    {
        Destroy(gameObject);
    }
#endif
}