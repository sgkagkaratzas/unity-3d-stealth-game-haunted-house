# 🏚️ 3D Stealth Game: Haunted House (Gamification Edition)

Welcome to the **Haunted House Gamification Project**. This is a custom-engineered puzzle-stealth game built in Unity. The player must navigate a haunted environment, evade patrolling enemies, and solve quizzes to progress. Every interaction, success, and failure is meticulously tracked in real-time for behavioral analysis.

🎮 **[PLAY THE DEMO HERE](https://play.unity.com/en/games/e07aca2f-639e-4a0c-a695-f2464d706da4/aegean-gamification)** (contact with the author for the password)

---

## 📖 About the Game

This project is an advanced gamification layer built on top of a classic stealth framework. The player's objective is to escape the haunted house by unlocking doors. However, doors are blocked by "Keys" that require the player to answer specific questions.

### ⚙️ Core Mechanisms

1. **Question & Answer Flow:** Interacting with a Key opens a quiz UI. Answering correctly unlocks the path.
2. **Dynamic AI Hunting (A* Algorithm):** If the player fails a question too many times, a silent alarm is triggered. The "Guardian" (a ghost enemy) will break its standard patrol route, calculate the shortest path to the player's exact location, and actively hunt the player down. 
3. **Behavioral Telemetry:** Every critical action the player takes is streamed in real-time out of the game engine to an external Python server using the **Lab Streaming Layer (LSL)** protocol.

---

## 🛠️ Technical Stack

* **Game Engine:** Unity 6.3 LTS
* **Analytics Backend:** Python 3.12
* **Data Streaming:** Lab Streaming Layer (LSL) / pylsl
* **UI Framework:** Unity UI Toolkit (UXML/USS)

---

## 📂 Project Structure

**Notice on Licensing:** To respect the Unity Asset Store License, the base 3D models, audio, and environment assets from the original 3D Stealth Game Haunted House Tutorial are **not** included in this repository. 

This repository exclusively hosts the custom-developed C# scripts, AI logic, UI documents, and Python servers created for this specific gamification implementation.

    📦 Repository Root
    ├── Assets/
    │   └── _MyProject/
    │       ├── Plugins/
    │       │   ├── lsl.dll
    │       │   └── liblsl-1.17.5-Win_amd64.zip
    │       ├── Resources/
    │       │   └── game_content.json
    │       ├── UI/
    │       │   ├── MainUI.uxml
    │       │   ├── MainMenu.uxml
    │       │   ├── Info.uxml
    │       │   └── Credits.uxml
    │       └── Scripts/
    │           ├── Logging/
    │           │   ├── LabStramingLayer.cs
    │           │   ├── IEventLogger.cs
    │           │   ├── LoggerFactory.cs
    │           │   ├── LslLogger.cs
    │           │   └── NullLogger.cs
    │           ├── Global/
    │           │   ├── GameEnding.cs
    │           │   ├── PauseMenu.cs
    │           │   ├── Observer.cs
    │           │   └── GlobalGameData.cs
    │           ├── Obstacles/
    │           │   ├── Door.cs
    │           │   ├── QuestionDatabase.cs
    │           │   ├── KeyQuizUI.cs
    │           │   └── Key.cs
    │           ├── Player/
    │           │   └── PlayerMovement.cs
    │           ├── Enemy/
    │           │   ├── VisualHuntManager.cs
    │           │   ├── SearchNode.cs
    │           │   ├── GuardianPatrol.cs
    │           │   ├── AStarPathfinder.cs
    │           │   └── README.md
    │           ├── Menu/
    │           │   ├── MainMenu.cs
    │           │   ├── Info.cs
    │           │   └── Credits.cs
    │           └── UI/
    │               ├── FooterPopup.cs
    │               └── KeyDisplay.cs
    └── lsl/
        └── LSL_Python/
            ├── listener.py
            ├── README_LSL.md
            └── .gitignore

---

## 📊 Lab Streaming Layer (LSL) Telemetry

This project features a robust, custom-built LSL integration. The C# client in Unity packages game state data and pushes it over the local network to a Python listener script (`listener.py`). The Python script captures these events and writes them safely to a timestamped CSV file for future data analysis.

*(Note: The custom implementation handling the memory marshaling between Unity C# strings and the native C++ LSL .dll has been heavily modified and fixed to ensure stable, crash-free data streaming. See the `LabStreamingLayer.cs` and `listener.py` files for the final working implementation).*

### Logged Data Dictionary

The game logs the following events in a structured format: `scene_name | user_id | event_type | context | value`.

| Player Action | Event Type String | Context Logged | Value |
| :--- | :--- | :--- | :--- |
| **Help opened** | `Help opened` | `GlobalGameData.GameTimer` | empty |
| **Help closed** | `Help closed` | Seconds spent in help (float, realtime) | empty |
| **Question opened** | `Question opened;Key:{keyName};QID:{qid}` | `GlobalGameData.GameTimer` | empty |
| **Question answered** | `Question answered;Key:{key};QID:{qid};SelectedIndex:{n};Correct:{True/False};CorrectIndex:{m}` | Time spent on question (sec) or `GlobalGameData.GameTimer` | empty |
| **Question canceled** | `Question canceled;Key:{key};QID:{qid}` | Seconds spent on question (float, realtime) | empty |
| **Entered vision cone** | `Player entered vision cone trigger` | `GlobalGameData.GameTimer` | empty |
| **Exited vision cone** | `Player exited vision cone trigger` | `GlobalGameData.GameTimer` | empty |
| **Caught by observer** | `Caught by observer` | `GlobalGameData.GameTimer` | empty |
| **Caught (Game Over)** | `Caught` | `GlobalGameData.GameTimer` | empty |
| **Success (Escaped)** | `Success` | `GlobalGameData.GameTimer` | empty |
| **Rating popup opened** | `Rating popup opened` | `GlobalGameData.GameTimer` | empty |
| **Rating answered** | `Rating: Yes` or `Rating: No` | Seconds spent on rating popup (float) | empty |
| **Pause menu opened** | `Pause menu opened` | `GlobalGameData.GameTimer` | empty |
| **Pause menu closed** | `Pause menu closed` | `GlobalGameData.GameTimer` | empty |
| **Exit menu opened** | `Exit menu opened` | `GlobalGameData.GameTimer` | empty |
| **Exit menu closed** | `Exit menu closed` | Seconds spent in exit (float, realtime) | empty |
| **Quit to main menu** | `Quit to main menu` | `GlobalGameData.GameTimer` | empty |

---

## 👻 Ghost AI: Smart Pathfinding & Investigation System

This module handles the advanced AI logic for the "Guardian" enemy. Instead of wandering randomly when an alarm is triggered, the ghost uses a custom implementation of the **A* (A-Star) Pathfinding Algorithm**.

### What is the A* Algorithm?
A* calculates the most efficient route through the haunted house to investigate the exact location of a disturbance. It looks at a network of interconnected points (Nodes) and calculates a score for each node using the formula `F = G + H`:
* **G (Ground Cost):** The distance from the starting point to the current node.
* **H (Heuristic):** An educated guess of the distance from the current node to the final destination.
* **F (Total Cost):** The sum of G and H. 

The algorithm steps toward the node with the lowest F score, ensuring it explores the most promising paths first while avoiding obstacles.

### Why A* in This Project?
1. **Deterministic Control:** Manually placed "Search Nodes" dictate exactly where the ghost is allowed to walk, preventing it from clipping through furniture.
2. **Pacing:** A node-based system forces the ghost through specific "choke points" (like the center of a creepy hallway) to maximize tension.
3. **Modular Investigation:** The A* implementation allows us to feed the ghost a specific sequence of locations to search once it reaches a room.

### How it is Implemented
1. **The Map (`SearchNode.cs`):** Invisible waypoints placed throughout the scene, linked by "Neighbors" to form a traversable web.
2. **The Brain (`AStarPathfinder.cs`):** The mathematical utility that calculates the shortest path from the Ghost's current node to the target node.
3. **The Body (`GuardianPatrol.cs`):** The manager script. When a player fails a puzzle, it maps the specific `keyID` to a room's nodes, asks the pathfinder for a route, and forces the ghost to walk the calculated itinerary.

---

## 📜 Credits & Acknowledgments
* **Original Framework:** [Unity Technologies - 3D Stealth Game Haunted House](https://assetstore.unity.com/packages/essentials/tutorial-projects/3d-stealth-game-haunted-house-143848)
* **Sounds:** [Pixabay](https://pixabay.com)
* **Icons:** [Google AI Nano Banana](https://gemini.google/gr/overview/image-generation)
