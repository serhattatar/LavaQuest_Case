using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// [CORE SYSTEM]
/// Mocks the backend/server connection of the main game.
/// NOTE: Not part of the evaluation scope.
/// </summary>
public class GameSessionBridge : MonoBehaviour
{
    public static GameSessionBridge Instance { get; private set; }

    public event Action<bool> OnMiniGameFinished;

    [Space(10)]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Header("   [ CORE SYSTEM - MOCK BACKEND ]")]
    [Header("   Simulates server/player data.")]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Space(20)]
    [Header("Mock Settings")]
    [SerializeField] private List<Sprite> botAvatarSpritePool;
    [SerializeField] private List<Sprite> botAvatarFrameSpritePool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<ParticipantData> GetMockSessionData(int totalCount)
    {
        var list = new List<ParticipantData>();

        list.Add(new ParticipantData
        {
            UserId = "local_player",
            DisplayName = "YOU",
            IsMainPlayer = true
        });

        for (int i = 0; i < totalCount - 1; i++)
        {
            list.Add(new ParticipantData
            {
                UserId = $"bot_{Guid.NewGuid().ToString().Substring(0, 5)}",
                DisplayName = $"Guest_{Random.Range(1000, 9999)}",
                IsMainPlayer = false,
                ProfileSprite = GetRandomSprite(botAvatarSpritePool),
                AvatarFrameSprite = GetRandomSprite(botAvatarFrameSpritePool)
            });
        }

        return list;
    }

    public void NotifyGameResult(bool isWin)
    {
        Debug.Log($"[CoreGame] Match-3 Finished. Result: {(isWin ? "WIN" : "FAIL")}");
        OnMiniGameFinished?.Invoke(isWin);
    }

    private Sprite GetRandomSprite(List<Sprite> spriteList)
    {
        if (spriteList == null || spriteList.Count == 0) return null;
        return spriteList[Random.Range(0, spriteList.Count)];
    }
}