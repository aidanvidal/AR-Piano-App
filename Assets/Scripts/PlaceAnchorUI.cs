using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class ARAnchorPlacementWithUI : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARAnchorManager anchorManager;
    [SerializeField] private ARPointCloudManager pointCloudManager;

    [Header("Prefabs")]
    [SerializeField] private GameObject prefabToPlace;

    [Header("UI Components")]
    [SerializeField] private Canvas anchorPlacementCanvas;
    [SerializeField] private Button confirmAnchorButton;
    [SerializeField] private Button replaceAnchorButton;
    [SerializeField] private Slider rotationSlider;

    [Header("Mapping Settings")]
    [SerializeField] private float minRotation = -1f;
    [SerializeField] private float maxRotation = 1f;

    // State tracking
    private bool canPlaceAnchor = true;
    private ARAnchor mainAnchor = null;
    private GameObject currentPlacedObject = null;
     

    // Raycast hit list
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private Quaternion lastAnchorRotation;

    void Start()
    {
        // Disable Canvas
        anchorPlacementCanvas.enabled = false;

        Application.targetFrameRate = 30; // Maintain 30 FPS

        // Setup slider
        rotationSlider.minValue = minRotation;
        rotationSlider.maxValue = maxRotation;
        rotationSlider.value = (minRotation + maxRotation) / 2;
        rotationSlider.onValueChanged.AddListener(UpdateAnchorRotation);

        // Setup button listeners
        confirmAnchorButton.onClick.AddListener(ConfirmAnchorPlacement);
        replaceAnchorButton.onClick.AddListener(ReplaceAnchor);

        // Start Inital
        ResetAnchorPlacement();

        // Attach Listener to Start
        NoteMappingEventManager.Instance.onResetAnchor.AddListener(ResetAnchorPlacement);
        anchorManager.trackablesChanged.AddListener(OnTrackablesChanged);
    }

    void Update()
    {
        // Only allow anchor placement if not confirmed
        if (canPlaceAnchor && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Raycast against feature points
            if (raycastManager.Raycast(Input.GetTouch(0).position, hits, TrackableType.FeaturePoint))
            {
                // Get the pose from the raycast hit
                Pose hitPose = hits[0].pose;
                PlaceAnchor(hitPose);
            }
        }
    }

    public void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
    {
        if (mainAnchor == null) return;
        if(changes.updated.Count != 0)
        {
            mainAnchor.transform.rotation = lastAnchorRotation;
        }
    }

    private void UpdateAnchorRotation(float value)
    {
        if (mainAnchor)
        {
            mainAnchor.transform.rotation = Quaternion.Euler(0, value, 0);
        }
    }

    async void PlaceAnchor(Pose hitPose)
    {
        // Remove existing anchor if present
        if (currentPlacedObject != null)
        {
            Destroy(currentPlacedObject);
        }

        // Disable canPlaceAnchor
        canPlaceAnchor = false;

        // Enable slider
        if (!rotationSlider.interactable)
        {
            rotationSlider.interactable = true;
        }

        // Force a fixed rotation for the anchor before placing it
        Quaternion fixedRotation = Quaternion.Euler(0, 0, 0);  // Adjust if needed
        Pose adjustedPose = new Pose(hitPose.position, fixedRotation);

        var result = await anchorManager.TryAddAnchorAsync(adjustedPose);
        if (result.status.IsSuccess())
        {
            Debug.Log("Created Anchor");
            mainAnchor = result.value;
            mainAnchor.transform.rotation = Quaternion.Euler(0, 0, 0);

            // Instantiate the object and set its local position/rotation explicitly
            currentPlacedObject = Instantiate(prefabToPlace, mainAnchor.transform);
            currentPlacedObject.transform.localPosition = Vector3.zero;
            currentPlacedObject.transform.localRotation = fixedRotation;

            // Enable confirm button
            confirmAnchorButton.interactable = true;

            Debug.Log("Rotation is: " + currentPlacedObject.transform.localRotation);
        }
        else
        {
            Debug.Log("Failed to create Anchor");
        }
    }

    void ReplaceAnchor()
    {
        // Destroy existing anchor object
        if (currentPlacedObject != null)
        {
            Destroy(currentPlacedObject);
            currentPlacedObject = null;
        }

        // Disable rotation slider
        rotationSlider.interactable = false;
        rotationSlider.value = (minRotation + maxRotation) / 2;

        // Disable confirm button
        confirmAnchorButton.interactable = false;

        // Ensure raycast manager is enabled for new placement
        raycastManager.enabled = true;
        canPlaceAnchor = true;
    }

    void ConfirmAnchorPlacement()
    {
        // Ensure an anchor is placed before confirming
        if (currentPlacedObject == null)
        {
            Debug.LogWarning("No anchor placed to confirm!");
            return;
        }

        // Set last rotation
        lastAnchorRotation = mainAnchor.transform.rotation;

        // Disable further anchor placement
        canPlaceAnchor = false;
        confirmAnchorButton.interactable = false;

        // Disable point cloud and raycast managers
        raycastManager.enabled = false;
        foreach(var pointCloud in pointCloudManager.trackables)
        {
            pointCloud.gameObject.SetActive(false); // Hide each point cloud object
        }
        
        pointCloudManager.enabled = false;
        
        // Hide anchor placement canvas
        anchorPlacementCanvas.enabled = false;
        
        // Add it to piano mapper
        PianoNoteMapper.Instance.SetAnchor(mainAnchor);
        PianoNoteMapper.Instance.ResetNoteDictionary();

        // Trigger note mapping event
        NoteMappingEventManager.Instance.TriggerAnchorPlacedEvent();

        Debug.Log("Confirming Anchor!");

        // Disable script
        this.enabled = false;
    }

    // Optional: Method to manually reset if needed
    public void ResetAnchorPlacement()
    {
        if (currentPlacedObject != null)
        {
            Destroy(currentPlacedObject);
        }

        canPlaceAnchor = true;
        anchorPlacementCanvas.enabled = true;
        raycastManager.enabled = true;
        pointCloudManager.enabled = true;
        foreach (var pointCloud in pointCloudManager.trackables)
        {
            pointCloud.gameObject.SetActive(true); // Show each point cloud object
        }
        confirmAnchorButton.interactable = false;
        rotationSlider.value = (minRotation + maxRotation) / 2;
        rotationSlider.interactable = false;
        this.enabled = true;
    }

    private void OnDestroy()
    {
        // Unsubscribe from event to prevent memory leaks
        NoteMappingEventManager.Instance.onResetAnchor.RemoveListener(ResetAnchorPlacement);
    }

}