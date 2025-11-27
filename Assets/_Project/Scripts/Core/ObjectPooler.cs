using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    // Singleton used only for Core access, Modules receive reference via DI
    public static ObjectPooler Instance { get; private set; }

    [Space(10)]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Header(" [ CORE SYSTEM - UTILITY ]")]
    [Header(" Memory Management Module")]
    [Header("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀")]
    [Space(20)]


    [Header("Pool Configuration")]
    [SerializeField] private AvatarView avatarPrefab;
    [SerializeField] private int initialPoolSize = 30;

    [Header("VFX Configuration")]
    [SerializeField] private ParticleSystem jumpVfxPrefab;
    [SerializeField] private int vfxPoolSize = 10;
    [SerializeField] private float vfxZOffset = 0.5f; // Configurable Z-depth

    private Queue<AvatarView> avatarQueue = new Queue<AvatarView>();
    private Queue<ParticleSystem> vfxQueue = new Queue<ParticleSystem>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePools();
    }

    private void InitializePools()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            AvatarView obj = Instantiate(avatarPrefab, transform);
            obj.gameObject.SetActive(false);
            avatarQueue.Enqueue(obj);
        }

        for (int i = 0; i < vfxPoolSize; i++)
        {
            if (jumpVfxPrefab != null)
            {
                ParticleSystem vfx = Instantiate(jumpVfxPrefab, transform);
                vfx.gameObject.SetActive(false);
                vfxQueue.Enqueue(vfx);
            }
        }
    }

    public AvatarView GetAvatar(Transform parent)
    {
        AvatarView obj = (avatarQueue.Count > 0) ? avatarQueue.Dequeue() : Instantiate(avatarPrefab, transform);
        obj.transform.SetParent(parent, false);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public void ReturnAvatar(AvatarView obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform, false);
        avatarQueue.Enqueue(obj);
    }

    public void PlayVFX(Vector3 position)
    {
        if (vfxQueue.Count == 0) return;

        ParticleSystem vfx = vfxQueue.Dequeue();

        // Adjust Z to ensure visibility in front of UI/World objects
        position.z = vfxZOffset;

        vfx.transform.position = position;
        vfx.gameObject.SetActive(true);
        vfx.Play();

        vfxQueue.Enqueue(vfx);
    }
}