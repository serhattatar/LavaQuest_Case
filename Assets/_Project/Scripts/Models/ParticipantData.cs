using UnityEngine;

/// <summary>
/// Represents the state and identity of a single participant (Player or Bot).
/// </summary>
[System.Serializable]
public class ParticipantData
{
    public string Id;
    public Sprite ProfileSprite;
    public bool IsMainPlayer;
    public bool IsEliminated;
}