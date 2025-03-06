using UnityEngine;
using UnityEngine.Events;

public class NoteMappingEventManager : MonoBehaviour
{
    // Singleton instance
    public static NoteMappingEventManager Instance { get; private set; }

    // Event triggered when anchor is placed and ready for note mapping
    public UnityEvent OnAnchorPlaced = new UnityEvent();
    public UnityEvent OnNotesMapped = new UnityEvent();
    public UnityEvent onResetAnchor = new UnityEvent();
    public UnityEvent onStartSong = new UnityEvent();

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to invoke anchor placed event
    public void TriggerAnchorPlacedEvent()
    {
        OnAnchorPlaced?.Invoke();
    }

    // Invoke mapped notes
    public void TriggerNotesMappedEvent()
    {
        OnNotesMapped?.Invoke();
    }

    // Invoke reset Anchor
    public void TriggerResetAnchorEvent()
    {
        onResetAnchor?.Invoke();
    }

    // Invoke Start Song
    public void TriggerStartSongEvent()
    {
        onStartSong?.Invoke();
    }
}