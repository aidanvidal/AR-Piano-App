using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MidiFileListUI : MonoBehaviour
{
    [SerializeField] private RectTransform contentPanel; // The panel inside the ScrollView
    [SerializeField] private Button buttonPrefab; // Prefab for buttons
    [SerializeField] private ScrollRect scrollRect; // Scroll container
    [SerializeField] private string[] midiFileNames; // MIDI file names
    [SerializeField] private Button playButton; // Play button from the Main Menu

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(ShowSongList);
        }
        this.enabled = false;
    }

    public void ShowSongList()
    {
        this.enabled = true;
        Debug.Log("Showing song list now");
        // Clear existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Loop through MIDI files and create buttons
        foreach (string midiFile in midiFileNames)
        {
            Debug.Log($"Song List: {midiFile}");
            Button newButton = Instantiate(buttonPrefab, contentPanel);
            newButton.GetComponentInChildren<TMP_Text>().text = midiFile;

            // Assign a click event with a placeholder function
            newButton.onClick.AddListener(() => OnMidiFileSelected(midiFile));
        }

        // Refresh the ScrollRect
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentPanel);
    }

    private void HideFeatures()
    {
        // Clear existing buttons
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }
    private void OnMidiFileSelected(string fileName)
    {
        Debug.Log($"Selected MIDI file: {fileName}");
        HideFeatures();
        List<NoteData> notes = MidiNoteExtractor.Instance.SelectedSong(fileName);
        NotePlaneSpawner.Instance.StartSong(notes);
        this.enabled = false;
    }
}