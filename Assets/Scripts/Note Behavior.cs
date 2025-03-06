using UnityEngine;

public class NoteBehavior : MonoBehaviour
{
    private float startTime;
    private NoteData noteData;
    public static float noteSpeed = 1.0f; // Units per second - adjust as needed
    public static float noteGap = 0.05f;  // Gap between consecutive notes
    private float frontEdgeZ; // Track the front edge position

    public void Initialize(NoteData note, Vector3 startPosition)
    {
        noteData = note;
        startTime = Time.time;

        // Calculate the length based on duration (minus gap)
        float noteLength = (note.Duration * noteSpeed) - noteGap;
        if (noteLength < 0.1f) noteLength = 0.1f; // Minimum size for visibility

        // Scale the note in z-axis
        Vector3 newScale = transform.localScale;
        newScale.z = noteLength / 10;
        transform.localScale = newScale;

        // Calculate the correct position offset to ensure the front edge stays at the original z position
        // This compensates for Unity's center-based scaling
        Vector3 adjustedPosition = startPosition;
        adjustedPosition.z += noteLength / 2; // * 5 Move back by half the length

        // Set position with the offset applied
        transform.localPosition = adjustedPosition;

        // Store the front edge z position for destruction logic
        frontEdgeZ = startPosition.z;
    }

    public void MoveNote()
    {
        // Move the note at constant speed
        Vector3 position = transform.localPosition;
        position.z -= noteSpeed * Time.deltaTime;
        transform.localPosition = position;

        // Update front edge position
        frontEdgeZ -= noteSpeed * Time.deltaTime;
    }

    public bool ShouldBeDestroyed()
    {
        // The note's back edge is at: frontEdgeZ - noteLength
        float noteLength = transform.localScale.z;
        float backEdgeZ = frontEdgeZ + noteLength * 10; // * 10

        // Note should be destroyed when its back edge passes z=0
        return backEdgeZ <= 0.0f;
    }

    public float GetEndTime()
    {
        // Return the estimated time when this note will fully pass the z=0 line
        float noteLength = transform.localScale.z;
        float backEdgeZ = frontEdgeZ + noteLength * 10; // * 10
        float remainingDistance = backEdgeZ; // How far the back edge is from z=0

        return Time.time + (remainingDistance / noteSpeed);
    }
}