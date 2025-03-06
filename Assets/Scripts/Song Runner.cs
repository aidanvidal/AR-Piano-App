using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class NotePlaneSpawner : MonoBehaviour
{
    public static NotePlaneSpawner Instance { get; private set; }  // Singleton instance

    public GameObject sharpNote; // Assign your note plane prefab
    public GameObject naturalNote;
    public float spawnPoint = 5.0f;  // Where notes appear (e.g., z = 5)
    public float noteSpeed = 1.0f;   // Units per second
    public float noteGap = 0.05f;    // Gap between consecutive notes (in units)

    private float songStartTime;
    private int currentNoteIndex;
    private ARAnchor anchor;
    private float octaveSize;
    private bool songEnding = false;
    private float estimatedEndTime = 0f;

    private List<NoteData> noteList = new List<NoteData>
    {
        new NoteData("A#5", 0.0f, 0.416f),
        new NoteData("C6", 0.417f, 0.832f),
        new NoteData("A#5", 0.833f, 1.249f),
        new NoteData("A#5", 1.25f, 1.457f),
        new NoteData("G5", 1.458f, 1.666f),
    };

    private List<GameObject> activeNotes = new List<GameObject>();

    void Awake()
    {
        // Ensure that there's only one instance of the singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy the new instance if one already exists
        }
        else
        {
            Instance = this;  // Set the singleton instance to this object
            DontDestroyOnLoad(gameObject);  // Optionally, make the instance persist across scenes
        }
    }

    void Start()
    {
        this.enabled = false;
    }

    private void OnEnable()
    {
        // Set static variables to match inspector values
        NoteBehavior.noteSpeed = noteSpeed;
        NoteBehavior.noteGap = noteGap;
    }

    public void StartSong(List<NoteData> newNotes)
    {
        if (this.enabled) return; // Prevent resetting if already started
        this.enabled = true;
        noteList = newNotes;
        octaveSize = PianoNoteMapper.Instance.GetOctaveSize();
        anchor = PianoNoteMapper.Instance.GetAnchor();
        currentNoteIndex = 0;
        songStartTime = Time.time;
        songEnding = false;

        Debug.Log("Starting song with noteSpeed: " + noteSpeed + " and noteGap: " + noteGap);
    }

    private void HandleEndSong()
    {
        anchor = null;
        NoteMappingEventManager.Instance.TriggerNotesMappedEvent();
        this.enabled = false;
        Debug.Log("Song ended completely");
    }

    void Update()
    {
        float elapsedTime = Time.time - songStartTime;

        // Calculate time offset based on spawn point and speed to ensure notes arrive at z=0 at the correct time
        Debug.Log("Time: " + (elapsedTime));

        // Spawn new notes at the correct time, adjusted for travel time
        while (currentNoteIndex < noteList.Count &&
               noteList[currentNoteIndex].startTime <= elapsedTime)
        {
            try
            {
                Debug.Log("Spawning Note: " + currentNoteIndex + " at time: " + (elapsedTime));
                SpawnNote(noteList[currentNoteIndex]);
                Debug.Log($"Spawned Note {currentNoteIndex}: {noteList[currentNoteIndex].noteName} - Duration: {noteList[currentNoteIndex].Duration}");
                currentNoteIndex++;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error spawning note {currentNoteIndex}: {e.Message}");
                currentNoteIndex++; // Skip problem notes
            }
        }

        // Update movement and destroy notes when they've passed z = 0
        float latestEndTime = 0f;
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            GameObject noteObj = activeNotes[i];
            NoteBehavior noteBehavior = noteObj.GetComponent<NoteBehavior>();

            float noteEndTime = noteBehavior.GetEndTime();
            if (noteEndTime > latestEndTime)
            {
                latestEndTime = noteEndTime;
            }

            if (noteBehavior.ShouldBeDestroyed())
            {
                Debug.Log("Destroying Note!");
                Destroy(noteObj);
                activeNotes.RemoveAt(i);
            }
            else
            {
                noteBehavior.MoveNote();
            }
        }

        // Update estimated end time if we have active notes
        if (activeNotes.Count > 0)
        {
            estimatedEndTime = latestEndTime;
        }

        // Check if we should start ending the song
        if (currentNoteIndex >= noteList.Count && !songEnding)
        {
            songEnding = true;
            Debug.Log("All notes spawned, waiting for notes to finish... Estimated end time: " + (estimatedEndTime - Time.time) + " seconds");
        }

        // Only end the song when all notes are gone AND we've spawned all notes
        if (songEnding && activeNotes.Count == 0 && Time.time >= estimatedEndTime)
        {
            HandleEndSong();
        }
    }

    void SpawnNote(NoteData note)
    {
        GameObject noteObj;
        float octave = Convert.ToInt32(note.noteName[^1]) - '0' - 4;
        octave *= octaveSize;

        Vector3 spawnPosition = new Vector3(0, 0, spawnPoint);

        if (note.noteName.Length > 1 && note.noteName[1] == '#')
        {
            noteObj = Instantiate(sharpNote, anchor.transform);
            float noteDistance = PianoNoteMapper.Instance.GetNoteDistance(note.noteName.Substring(0, 2));
            noteDistance += octave;
            spawnPosition.x = noteDistance;
            spawnPosition.y = noteObj.transform.localPosition.y; // Keep original Y
        }
        else
        {
            noteObj = Instantiate(naturalNote, anchor.transform);
            float noteDistance = PianoNoteMapper.Instance.GetNoteDistance(note.noteName.Substring(0, 1));
            noteDistance += octave;
            spawnPosition.x = noteDistance;
            spawnPosition.y = noteObj.transform.localPosition.y; // Keep original Y
        }

        NoteBehavior behavior = noteObj.AddComponent<NoteBehavior>();
        // Initialize with the spawn position (the note will adjust itself internally)
        behavior.Initialize(note, spawnPosition);
        activeNotes.Add(noteObj);
    }
}

[System.Serializable]
public class NoteData
{
    public string noteName;
    public float startTime;
    public float endTime;

    public float Duration => endTime - startTime;

    public NoteData(string name, float start, float end)
    {
        noteName = name;
        startTime = start;
        endTime = end;
    }
}