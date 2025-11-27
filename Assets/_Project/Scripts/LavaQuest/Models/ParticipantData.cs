using UnityEngine;

/// <summary>
/// Data Transfer Object (DTO) representing a user in the event.
/// This data is expected to come from the Main Game's backend/save system.
/// </summary>
[System.Serializable]
public class ParticipantData
{
    public string UserId;
    public string DisplayName;
    public Sprite ProfileSprite; // Nullable if image loading fails
    public Sprite AvatarFrameSprite;  // Nullable if image loading fails
    public bool IsMainPlayer;

    // Runtime state tracking (Not persistent in backend, logic specific)
    public bool IsEliminated;
}