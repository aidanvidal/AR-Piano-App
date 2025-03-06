using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class PianoNoteMapper : MonoBehaviour
{
    // Octave size (distance between two consecutive C notes)
    [SerializeField] private float octaveSize = 1f;

    // Dictionary to store note distances
    private Dictionary<string, float> noteDictionary = new Dictionary<string, float>();

    private ARAnchor mainAnchor;

    // Singleton instance
    private static PianoNoteMapper instance;
    public static PianoNoteMapper Instance { get { return instance; } }

    private void Awake()
    {
        // Singleton implementation
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Optional based on your needs
        }
        InitializeNoteDictionary();
    }

    // Initialize the dictionary with default note distances
    private void InitializeNoteDictionary()
    {
        // Natural notes
        SetNoteDistance("C", 0f);
        SetNoteDistance("D", 0.25f);
        SetNoteDistance("E", 0.5f);
        SetNoteDistance("F", 0.6f);
        SetNoteDistance("G", 0.75f);
        SetNoteDistance("A", 0.875f);
        SetNoteDistance("B", 1f);

        // Sharp notes
        SetNoteDistance("C#", 0.125f);
        SetNoteDistance("D#", 0.375f);
        SetNoteDistance("F#", 0.675f);
        SetNoteDistance("G#", 0.8f);
        SetNoteDistance("A#", 0.95f);
    }

    // Add method to reset dictionary
    public void ResetNoteDictionary()
    {
        noteDictionary.Clear();
        InitializeNoteDictionary();
    }

    // Setter method for note distances
    public void SetNoteDistance(string note, float distance)
    {
        // If the note already exists, update its distance
        if (noteDictionary.ContainsKey(note))
        {
            noteDictionary[note] = distance;
        }
        // Otherwise, add the new note
        else
        {
            noteDictionary.Add(note, distance);
        }
    }

    // Getter method for note distances
    public float GetNoteDistance(string note)
    {
        // Check if the note exists in the dictionary
        if (noteDictionary.TryGetValue(note, out float distance))
        {
            return distance;
        }

        // Return a default value or throw an exception if note not found
        Debug.LogWarning($"Note {note} not found in dictionary.");
        return -1f;
    }

    // Setter for octave size
    public void SetOctaveSize(float size)
    {
        octaveSize = size;
    }

    // Getter for octave size
    public float GetOctaveSize()
    {
        return octaveSize;
    }

    public void SetAnchor(ARAnchor anchor)
    {
        mainAnchor = anchor;
    }

    public ARAnchor GetAnchor()
    {
        return mainAnchor;
    }

    // Example method to demonstrate usage
    public void ExampleNoteDistanceUsage()
    {
        float cNoteDistance = GetNoteDistance("C");
        float gSharpDistance = GetNoteDistance("G#");

        Debug.Log($"C Note Distance: {cNoteDistance}");
        Debug.Log($"G# Note Distance: {gSharpDistance}");
        Debug.Log($"Current Octave Size: {GetOctaveSize()}");
    }
}