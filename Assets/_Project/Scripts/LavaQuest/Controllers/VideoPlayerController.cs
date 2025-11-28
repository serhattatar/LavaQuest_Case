using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class SimpleVideoController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RawImage targetDisplay;

    [Header("Settings")]
    [Tooltip("Play automatically on start?")]
    [SerializeField] private bool playOnAwake = true;

    // VideoPlayer component reference
    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();

        // Disable RawImage initially to avoid black frame flickering before video loads
        if (targetDisplay != null)
        {
            targetDisplay.enabled = false;
        }

        // Subscribe to events
        _videoPlayer.prepareCompleted += OnVideoPrepared;
        _videoPlayer.errorReceived += OnVideoError;
    }

    private void Start()
    {
        if (playOnAwake)
        {
            PrepareAndPlay();
        }
    }

    public void PrepareAndPlay()
    {
        // Preparation is crucial for smooth mobile playback
        _videoPlayer.Prepare();
    }

    private void OnVideoPrepared(VideoPlayer source)
    {
        // Video is ready, enable the UI and play
        if (targetDisplay != null)
        {
            targetDisplay.enabled = true;
        }

        source.Play();
    }

    private void OnVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[VideoController] Error: {message}");
    }

    // Clean up to prevent memory leaks or background playing
    private void OnDestroy()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.errorReceived -= OnVideoError;
            _videoPlayer.Stop();
        }
    }
}