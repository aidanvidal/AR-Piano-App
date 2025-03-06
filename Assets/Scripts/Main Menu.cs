using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MainMenu : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private Button resetAnchorButton;
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject naturalNotes;
    [SerializeField] private GameObject sharpNotes;
    private ARAnchor anchor;
    private readonly string[] notesToMap = new string[]
    {
        "C", "C#", "D", "D#", "E", "F",
        "F#", "G", "G#", "A", "A#", "B"
    };
    private List<GameObject> planes = new List<GameObject>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Subscribe to anchor placed event
        NoteMappingEventManager.Instance.OnNotesMapped.AddListener(StartMenu);

        // Attach buttons
        resetAnchorButton.onClick.AddListener(ResetAnchor);
        playButton.onClick.AddListener(HideMenu);
    }

    private void HideMenu()
    {
        ClearPlanes();
        menuCanvas.enabled = false;
        anchor = null;
    }

    private void StartMenu()
    {
        menuCanvas.enabled = true;
        DisplayNotes();
    }

    private void DisplayNotes()
    {
        // Set the anchor
        anchor = PianoNoteMapper.Instance.GetAnchor();

        // Loop through all notes and display a plane representing them
        foreach (string note in notesToMap)
        {
            float distance = PianoNoteMapper.Instance.GetNoteDistance(note);
            GameObject currentPlanePrefab;
            if (note.EndsWith('#'))
            {
                currentPlanePrefab = Instantiate(sharpNotes, anchor.transform);
            }
            else
            {
                currentPlanePrefab = Instantiate(naturalNotes, anchor.transform);
            }
            currentPlanePrefab.transform.localPosition = new Vector3(distance, currentPlanePrefab.transform.localPosition.y, currentPlanePrefab.transform.localPosition.z);
            planes.Add(currentPlanePrefab);
        }

    }

    private void ResetAnchor()
    {
        ClearPlanes();
        menuCanvas.enabled = false;
        anchor = null;
        NoteMappingEventManager.Instance.TriggerResetAnchorEvent();
    }

    private void ClearPlanes()
    {
        for(int i = planes.Count - 1; i >= 0; i--)
        {
            Destroy(planes[i]);
            planes.RemoveAt(i);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        NoteMappingEventManager.Instance.OnNotesMapped.RemoveListener(StartMenu);
    }

}