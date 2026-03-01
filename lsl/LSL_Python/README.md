# 📡 Lab Streaming Layer (LSL) — Setup & Testing Guide

This guide explains what the Lab Streaming Layer (LSL) is, how this project utilizes it, and provides step-by-step instructions to install and test LSL locally on Windows for both the Unity client and the Python listener. It is written to be accessible for developers with little or no prior LSL knowledge.

---

## 🧠 What is LSL?

The Lab Streaming Layer (LSL) is an open-source system for the real-time streaming of timestamped data between applications on the same machine or across a local network. It is widely used in neuroscience, Human-Computer Interaction (HCI), and experimental setups to transmit events, markers, and sampled data. LSL automatically handles the discovery of streams, precise clock synchronization, and data transport.



**Official Links:**
* **Project & Releases:** [liblsl GitHub Repository](https://github.com/labstreaminglayer/liblsl)
* **LSL Python API (pylsl):** [pylsl GitHub Repository](https://github.com/labstreaminglayer/pylsl)

---

## ⚙️ How This Project Uses LSL



* **The Unity Client:** The game creates an LSL *outlet* named `StealthGame_Events` and pushes event strings to it. The string payloads follow a pipe-separated format:
  `sceneName | userId | eventType | context | value`
  *Example:* `Scene_01|User_ABC123|Caught by observer|14.87|`
* **The Python Listener:** The `listener.py` script resolves the stream by name (`StealthGame_Events`) and writes incoming events into a CSV file with the following columns: `timestamp_unix, scene_name, user_id, event_type, context, value`.

*(Note: Web builds/WebGL do not support native LSL. LSL is supported exclusively for Editor and Standalone Windows/macOS/Linux builds).*

---

## 📂 Relevant Files in This Repo

* `Assets/_MyProject/Plugins/lsl.dll` — The expected Windows native LSL DLL. If placed here, Unity's native P/Invoke can utilize it.
* `Assets/_MyProject/Scripts/Logging/LabStramingLayer.cs` — Contains the C# P/Invoke definitions and native checks for the LSL library.
* `Assets/_MyProject/Scripts/Logging/LslLogger.cs` — The Unity component that creates the LSL outlet and pushes events.
* `lsl/LSL_Python/listener.py` — The Python script that listens for the `StealthGame_Events` stream and generates the CSV logs.

**Important:** If you move or replace the DLL, it is highly recommended to keep it under the `Assets/_MyProject/Plugins/` directory so Unity can load it correctly.

---

## ⬇️ Downloading the Native LSL Library (Windows)

1. Go to the liblsl GitHub releases: [Releases Page](https://github.com/labstreaminglayer/liblsl/releases)
2. Download the appropriate Windows ZIP for your platform (usually the x64 build for modern Windows). Extract the file to locate `lsl.dll` (or `liblsl.dll`).
3. Copy the DLL into your Unity project's plugin folder:
   **Preferred path:** `Assets/_MyProject/Plugins/lsl.dll`
4. In the Unity Editor, select the DLL in the Project view and check the Plugin Import Settings in the Inspector. 



**Unity Inspector Settings:**
Ensure the platform is set to **Any Platform** or **Windows**, and target the correct CPU (x86_64) that matches your Editor and build target. 
* *Note: The Unity Editor on modern Windows is 64-bit; you must use the x64 DLL.*
* *If Unity cannot find the native DLL, check the Console for warning messages from `LabStreamingLayer` or `LslLogger`.*

---

## 🐍 Python Listener Setup (Windows)

These steps will help you create a Python virtual environment, install the `pylsl` library, and run the `listener.py` script.

1. **Install Python** (3.8+ recommended) from python.org and ensure `python` is added to your system PATH.
2. **Create a Virtual Environment:** Open a terminal (PowerShell recommended), navigate to the project's Python directory, create the environment, and activate it:

    cd "C:\Path\To\Your\Project\lsl\LSL_Python"
    python -m venv .venv
    .\.venv\Scripts\Activate.ps1

    *(If using Command Prompt, activate using: `.\.venv\Scripts\activate`)*

3. **Install Dependencies:** Upgrade pip and install `pylsl`:

    python -m pip install --upgrade pip
    python -m pip install pylsl

**Notes on `pylsl`:**
`pylsl` provides Python bindings to the native liblsl. On Windows, the pip package usually includes the native `lsl.dll` inside the package folder (e.g., `.venv\Lib\site-packages\pylsl\lib\lsl.dll`). Because of this, the Python listener can usually run without extra configuration.

---

## 🏃‍♂️ Running and Testing Locally

It is recommended to start the Unity stream before launching the listener.

1. **Start Unity:** Open the scene you want to test and press Play in the Editor. Unity should automatically create an `LslLogger` GameObject (via the `LoggerFactory`).
2. **Check the Console:** Watch the Unity Console for these diagnostic messages to confirm successful initialization:
   * `LabStreamingLayer: loaded native library from <path>`
   * `[LSL] Outlet created successfully.`
3. **Start the Listener:** In a separate terminal, activate your Python virtual environment and run the listener:

    cd "C:\Path\To\Your\Project\lsl\LSL_Python"
    .\.venv\Scripts\Activate.ps1
    python listener.py

4. **Verify Connection:** The listener will print `Waiting for StealthGame_Events stream...`. Once found, it will print `Connected to LSL stream` and begin generating a CSV file (`game_logs_<timestamp>.csv`).
5. **Test Interactions:** Interact with the Unity game (e.g., open help menus, answer questions, get caught). You should see these events populate as new lines in the Python listener's terminal and CSV file.

---

## ⚠️ Common Problems & Troubleshooting

**No stream found / `ERROR: LSL stream not found`**
* Ensure Unity is actively playing and the `LslLogger` created the outlet. Check the Unity Console for `[LSL] Outlet created successfully.`
* Verify the stream name is exactly `StealthGame_Events` (case-sensitive) in both C# and Python.
* Ensure your Windows firewall or network policy is not blocking local multicast/UDP traffic, which LSL relies on for discovery.

**DLL load errors in Unity**
* Verify `lsl.dll` is physically present at `Assets/_MyProject/Plugins/lsl.dll`.
* Confirm you are not suffering from a 32-bit vs 64-bit mismatch. The Unity Editor requires the x64 DLL. Check your Plugin Import Settings in the Inspector.

**`pylsl` import errors in Python**
* Ensure your virtual environment is activated *before* you run `python listener.py`.
* If `pylsl` complains about a missing native lib, verify the file exists at `.venv\Lib\site-packages\pylsl\lib\lsl.dll`.

**Duplicate events in the CSV**
* This is usually caused by game logic calling the log function multiple frames in a row (e.g., inside an `Update()` loop without a boolean flag). Guard clauses have been added to the UI code to prevent this, but if it persists, check your event triggers.

---

## 📝 Notes and Limitations

* **WebGL Limitations:** WebGL builds do not support native LSL. The project falls back to a `NullLogger` to prevent crashes in non-standalone builds.
* **Architecture:** Always ensure Unity and your native DLLs match your target architecture.
* **Metadata Format:** The `event_type` field in the CSV may contain semicolon-separated metadata depending on the interaction (e.g., `Question answered;Key:key0001;QID:k1_a;SelectedIndex:2;Correct:True;CorrectIndex:2`).

---

## ✅ Quick Test Checklist

1. Place `lsl.dll` (x64) in `Assets/_MyProject/Plugins/`.
2. Start the Unity Editor, press Play, and watch the Console for LSL initialization messages.
3. Activate your Python venv and run `python listener.py`.
4. Interact with the game and confirm CSV lines are actively writing to the `lsl/LSL_Python/` folder.
