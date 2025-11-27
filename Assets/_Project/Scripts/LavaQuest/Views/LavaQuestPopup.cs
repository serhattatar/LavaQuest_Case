using System;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestPopup : UIScaleAnimation
{
    [Header("Interaction")]
    [SerializeField] private Button btnStart;
    [SerializeField] private Button btnClose;

    public event Action OnStartClicked;
    public event Action OnCloseClicked;

    private void Start()
    {
        btnStart.onClick.AddListener(() => OnStartClicked?.Invoke());
        btnClose.onClick.AddListener(() => OnCloseClicked?.Invoke());
    }
}