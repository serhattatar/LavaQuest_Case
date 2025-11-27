using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the input from the simulated Match-3 game screen.
/// Reports results back to the Core system.
/// </summary>
public class FakeGamePanel : MonoBehaviour
{
    [SerializeField] private Button btnWin;
    [SerializeField] private Button btnFail;

    // Events for the Core System to listen to
    public event Action OnWinSelected;
    public event Action OnFailSelected;

    private void Start()
    {
        btnWin.onClick.AddListener(() => OnWinSelected?.Invoke());
        btnFail.onClick.AddListener(() => OnFailSelected?.Invoke());
    }
}