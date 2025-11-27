using UnityEngine;

/// <summary>
/// Data Transfer Object (DTO) representing a user in the event.
/// </summary>
[System.Serializable]
public class ParticipantData
{
    public string UserId;
    public string DisplayName;
    public Sprite ProfileSprite;
    public Sprite AvatarFrameSprite;
    public bool IsMainPlayer;
    public bool IsEliminated;
}