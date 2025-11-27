using System;
using UnityEngine;
using UnityEngine.UI;

public class FakeGamePanel : MonoBehaviour
{
    [SerializeField] private Button btnWin;
    [SerializeField] private Button btnFail;

    public event Action OnWinSelected;
    public event Action OnFailSelected;

    private void Start()
    {
        btnWin.onClick.AddListener(() => OnWinSelected?.Invoke());
        btnFail.onClick.AddListener(() => OnFailSelected?.Invoke());
    }
}