using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class MidiNoteExtractor : MonoBehaviour
{
    // Singleton instance
    public static MidiNoteExtractor Instance { get; private set; }

    [SerializeField]
    private string[] defaultMidiFileNames;

    [SerializeField]
    private bool processOnStart = true;

    [SerializeField]
    private bool logToConsole = true;

    [TextArea(5, 10)]
    [SerializeField]
    private string outputText;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    private void Start()
    {
        if (processOnStart && defaultMidiFileNames != null && defaultMidiFileNames.Length > 0)
        {
            TextAsset midiAsset = LoadMidiFromResources(defaultMidiFileNames[0]);
            if (midiAsset != null)
            {
                ProcessMidiFile(midiAsset);
            }
            else
            {
                Debug.LogError($"Could not load default MIDI file: {defaultMidiFileNames[0]}");
            }
        }
    }

    public List<NoteData> SelectedSong(string SongName)
    {
        TextAsset midiPart = LoadMidiFromResources(SongName);
        List<NoteData> notes = new List<NoteData>();
        if (midiPart != null)
        {
            notes = ProcessMidiFile(midiPart);
        }
        else
        {
            Debug.LogError($"Could not load default MIDI file: {defaultMidiFileNames[0]}");
        }
        return notes;
    }

    public TextAsset LoadMidiFromResources(string midiFileName)
    {
        midiFileName = Path.GetFileNameWithoutExtension(midiFileName);
        Debug.Log($"Attempting to load MIDI file: {midiFileName}");

        TextAsset midiAsset = Resources.Load<TextAsset>(midiFileName);
        if (midiAsset == null)
        {
            Debug.LogError($"Failed to load MIDI file: {midiFileName}");
        }

        return midiAsset;
    }

    public List<NoteData> ProcessMidiFile(TextAsset midiAsset)
    {
        try
        {
            List<NoteData> notes = new List<NoteData>();
            using (var memoryStream = new MemoryStream(midiAsset.bytes))
            {
                notes = ExtractNotesFromMidi(memoryStream);

                if (logToConsole)
                {
                    Debug.Log(outputText);
                }
            }
            return notes;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing MIDI file: {e.Message}");
            return null;
        }
    }

    public List<NoteData> ProcessMidiFile(string filePath)
    {
        try
        {
            List<NoteData> notes = new List<NoteData>();
            using (var fileStream = File.OpenRead(filePath))
            {
                notes = ExtractNotesFromMidi(fileStream);
            }

            if (logToConsole)
            {
                Debug.Log(outputText);
            }

            return notes;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing MIDI file: {e.Message}");
            return null;
        }
    }

    private List<NoteData> ExtractNotesFromMidi(Stream midiStream)
    {
        Debug.Log("Starting MIDI note extraction!");
        var midiFile = MidiFile.Read(midiStream);
        var noteGroups = new SortedDictionary<double, List<Note>>();
        var tempoMap = midiFile.GetTempoMap();
        var notes = midiFile.GetNotes();
        List<NoteData> noteList = new List<NoteData>();

        foreach (var note in notes)
        {
            double startTime = note.TimeAs<MetricTimeSpan>(tempoMap).TotalSeconds;
            double roundedStartTime = Math.Round(startTime, 6);
            double endTime = note.EndTimeAs<MetricTimeSpan>(tempoMap).TotalSeconds;

            if (!noteGroups.ContainsKey(roundedStartTime))
            {
                noteGroups[roundedStartTime] = new List<Note>();
            }
            Debug.Log($"MIDI note: {GetNoteName(note.NoteNumber)} Start Time: {roundedStartTime} End Time: {endTime}");
            noteGroups[roundedStartTime].Add(note);
            noteList.Add(new NoteData(GetNoteName(note.NoteNumber), (float)startTime, (float)endTime));
        }

        return noteList;
    }

    private string GetNoteName(int noteNumber)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        return $"{noteNames[noteNumber % 12]}{(noteNumber / 12) - 1}";
    }
}