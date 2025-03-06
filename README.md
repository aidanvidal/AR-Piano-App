# 🎵 AR Piano Trainer

## 📌 Project Overview

This Unity-based Augmented Reality (AR) application helps users learn and play songs by visualizing notes in real-time.

## 🚀 Features

- **MIDI File Integration** – Reads MIDI files to extract song notes and timing.  
- **AR Note Visualization** – Generates planes in AR space that represent notes.  
- **Dynamic Note Timing** – Notes move at the correct speed to reach the target at the right time.  
- **Piano Note Mapping** – Maps MIDI notes to corresponding positions.  

## 🛠️ Technologies Used

- **Unity** (AR Foundation, XR Plugin)  
- **C#** for logic and interactions  
- **MIDI Processing** (e.g., Melanchall.DryWetMIDI)  
- **ARKit / ARCore** for augmented reality functionality  

## 📖 How It Works

1. **Load a MIDI File** – The app reads the MIDI file and extracts note data.  
2. **Map Notes to AR Space** – The app calculates each note's position and movement based on a mapped piano keyboard.  
3. **Spawn AR Note Planes** – Notes appear as moving planes in AR, guiding the user on when and where to press.  
4. **Sync with Song Timing** – Notes arrive at the designated location exactly when they should be played.  

## 🔧 Setup & Installation

1. Clone the repository.  
2. Open the project in Unity.  
3. Install AR Foundation and the necessary AR platform packages (**ARKit for iOS, ARCore for Android**).  
4. Upload your own MIDI files in the app’s Resources folder and add them to the list in the Midi Reader game object.  
5. Build and deploy to a compatible AR-supported device.  
