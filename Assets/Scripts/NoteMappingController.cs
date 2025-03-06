using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class NoteMappingUIController : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Slider distanceSlider;
    [SerializeField] private TextMeshProUGUI currentNoteText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private GameObject planePrefab;

    [Header("Mapping Settings")]
    [SerializeField] private float minDistance = 0f;
    [SerializeField] private float maxDistance = 5f;

    [Header("Note Mapping")]
    private string[] notesToMap = new string[]
    {
        "C", "C#", "D", "D#", "E", "F",
        "F#", "G", "G#", "A", "A#", "B", "Octave"
    };
    private int currentNoteIndex = 0;
    private GameObject currentPlanePrefab;
    private ARAnchor anchor;

    private void Start()
    {
        // Subscribe to anchor placed event
        NoteMappingEventManager.Instance.OnAnchorPlaced.AddListener(StartNoteMapping);

        // Setup slider
        distanceSlider.minValue = minDistance;
        distanceSlider.maxValue = maxDistance;
        distanceSlider.value = (minDistance + maxDistance) / 2;
        distanceSlider.onValueChanged.AddListener(UpdatePlanePosition);

        // Setup confirm button
        confirmButton.onClick.AddListener(ConfirmNotePosition);

        // Initially hide UI
        uiPanel.SetActive(false);
    }

    private void StartNoteMapping()
    {
        // Set the anchor
        anchor = PianoNoteMapper.Instance.GetAnchor();

        // Show UI
        uiPanel.SetActive(true);

        // Reset mapping process
        currentNoteIndex = 0;
        UpdateCurrentNoteDisplay();
        SpawnNotePlane();
    }

    private void UpdateCurrentNoteDisplay()
    {
        // Update UI to show current note being mapped
        currentNoteText.text = $"Mapping Note: {notesToMap[currentNoteIndex]}";
    }

    private void SpawnNotePlane()
    {
        // Destroy previous plane if exists
        if (currentPlanePrefab != null)
        {
            Destroy(currentPlanePrefab);
        }

        // Spawn new plane at anchor point
        currentPlanePrefab = Instantiate(planePrefab, anchor.transform);
    }

    private void UpdatePlanePosition(float value)
    {
        currentPlanePrefab.transform.localPosition = new Vector3(value, planePrefab.transform.localPosition.y, planePrefab.transform.localPosition.z);
    }

    private void ConfirmNotePosition()
    {
        // Get current slider value (distance)
        float noteDistance = distanceSlider.value;

        if(notesToMap[currentNoteIndex] == "Octave")
        {
            PianoNoteMapper.Instance.SetOctaveSize(noteDistance);
        }
        else
        {
            // Set note distance in mapper
            PianoNoteMapper.Instance.SetNoteDistance(notesToMap[currentNoteIndex], noteDistance);
        }

        // Move to next note
        currentNoteIndex++;

        // Check if all notes are mapped
        if (currentNoteIndex >= notesToMap.Length)
        {
            CompleteNoteMapping();
            return;
        }

        // Update for next note
        UpdateCurrentNoteDisplay();
        SpawnNotePlane();
    }

    private void CompleteNoteMapping()
    {
        // Hide UI
        uiPanel.SetActive(false);

        // Destroy final plane
        if (currentPlanePrefab != null)
        {
            Destroy(currentPlanePrefab);
        }

        // Optional: Trigger event that mapping is complete
        Debug.Log("Note Mapping Completed!");
        NoteMappingEventManager.Instance.TriggerNotesMappedEvent();
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        NoteMappingEventManager.Instance.OnAnchorPlaced.RemoveListener(StartNoteMapping);
    }
}