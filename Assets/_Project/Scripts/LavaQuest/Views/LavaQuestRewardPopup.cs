using System;
using UnityEngine;
using UnityEngine.UI;

public class LavaQuestRewardPopup : UIScaleAnimation
{
    [Header("Interaction")]
    [SerializeField] private Button btnCollect;

    public event Action OnCollectClicked;

    private void Start()
    {
        btnCollect.onClick.AddListener(() => OnCollectClicked?.Invoke());
    }
}