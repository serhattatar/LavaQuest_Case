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
   
    [Header("Mock Data Assets")]
    [SerializeField] private List<Sprite> botAvatarSpritePool;
    [SerializeField] private List<Sprite> botAvatarFrameSpritePool;

    private System.DateTime? _fixedEventEndTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public List<ParticipantData> GetMockSessionData(int totalCount)
    {
        var list = new List<ParticipantData>();

        // Main Player
        list.Add(new ParticipantData
        {
            UserId = "local_player",
            DisplayName = "YOU",
            IsMainPlayer = true
        });

        // Bots
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
        OnMiniGameFinished?.Invoke(isWin);
    }

    private Sprite GetRandomSprite(List<Sprite> spriteList)
    {
        if (spriteList == null || spriteList.Count == 0) return null;
        return spriteList[Random.Range(0, spriteList.Count)];
    }

    public System.DateTime GetEventEndTime()
    {
        if (_fixedEventEndTime == null)
        {
            _fixedEventEndTime = System.DateTime.Now.AddHours(23).AddMinutes(59).AddSeconds(59);
        }

        return _fixedEventEndTime.Value;
    }
}