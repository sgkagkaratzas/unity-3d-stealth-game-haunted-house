# Ghost AI: Smart Pathfinding & Investigation System

## 📖 Overview
This module handles the advanced AI logic for the "Guardian" (Ghost) enemy in our stealth puzzle game. When a player fails a puzzle or triggers an alarm, the ghost does not just teleport or wander aimlessly. Instead, it dynamically calculates the most efficient route through the haunted house to investigate the exact location of the disturbance. 

To achieve this, the project utilizes a custom implementation of the **A* (A-Star) Pathfinding Algorithm**.

---

## 🧠 What is the A* Algorithm?
A* (pronounced "A-Star") is one of the most popular and reliable pathfinding algorithms in computer science and game development. Its primary goal is to find the shortest possible path from a starting point to a destination while avoiding obstacles.

Instead of checking every single possible route blindly, A* uses a "smart" scoring system to guess which direction is the best to explore first. It looks at a network of interconnected points (called **Nodes**) and calculates a score for each node using this formula:

$$F = G + H$$

* **G (Ground Cost):** The exact distance from the starting point to the current node.
* **H (Heuristic):** An educated guess of the distance from the current node to the final destination (usually a straight-line measurement).
* **F (Total Cost):** The sum of G and H. 

The algorithm constantly looks for the node with the **lowest F score** and steps toward it, ensuring it explores the most promising paths first. Once it reaches the target, it traces its steps back to formulate the perfect route.

---

## 🎯 Why Use A* in This Project?
While Unity has built-in navigation tools (like the NavMesh), a custom A* Node Graph implementation was chosen for several specific design reasons:

1. **Deterministic Control (Level Design):** By manually placing "Search Nodes" in hallways and rooms, we dictate exactly *where* the ghost is allowed to walk. This prevents the ghost from getting stuck on strange geometry or clipping through furniture, ensuring a polished, cinematic pursuit.
2. **Tension and Pacing:** A node-based system allows us to create specific "choke points." We can force the ghost to walk down the center of a creepy hallway rather than hugging the walls, maximizing player tension.
3. **Modular Investigation:** When an alarm rings, we do not just want the ghost to go to one spot. We want it to search a *sequence* of locations in a specific room. A custom A* implementation allows us to feed the ghost a list of specific nodes to visit seamlessly.

---

## ⚙️ How It Works in Our Codebase
The system is divided into three main components: the map, the brain, and the body.

### 1. The Map (`SearchNode.cs`)
These are invisible waypoints placed manually throughout the Unity Scene. 
* Every node has a list of **Neighbors**. 
* If Node A is connected to Node B, it means the ghost is physically allowed to walk between them. 
* By linking nodes together (Room -> Hallway -> Next Room), we create a "spider web" or graph of traversable paths covering the entire house.

### 2. The Brain (`AStarPathfinder.cs`)
This is a pure mathematical utility script. It does not attach to any game object. 
* When the ghost needs to move, it asks this script: *"Find me the shortest path from my current Node to the Target Node."*
* The script runs the F = G + H math across the neighbor connections.
* It returns a `List<SearchNode>`, which is the step-by-step itinerary the ghost needs to walk.

### 3. The Body (`GuardianPatrol.cs`)
This is the script attached to the Ghost. It acts as the manager that ties the AI together.
* It contains an **Investigation Zones** configuration. This allows designers to map specific puzzle keys (e.g., `key0001`) to specific rooms (a list of Search Nodes).
* When a player fails a puzzle, the UI sends the `keyID` to this script.
* The Ghost stops its normal patrol, finds the closest node to its current physical feet, and asks the Pathfinder to route a path to the "Entrance Node" of the compromised room.

---

## 🏃‍♂️ Step-by-Step Execution Flow
Here is exactly what happens under the hood when a player triggers an alarm:

1. **The Trigger:** The player fails a puzzle. The `Key.cs` script calls `GuardianPatrol.AlertToPosition(position, "key0002")`.
2. **The Lookup:** The Ghost checks its "Investigation Zones" to find which nodes belong to `"key0002"`.
3. **Locating the Start:** The Ghost scans the entire map and finds the `SearchNode` closest to its current physical position.
4. **Path Calculation:** The Ghost asks `AStarPathfinder` for a path from its current node to the first node of the key0002 zone (the room entrance).
5. **Movement:** The Ghost walks linearly from node to node along the calculated path.
6. **The Search:** Once it reaches the room, it visits the remaining specific nodes for that zone, pausing at each to "search" for the player.
7. **Return to Normal:** If the player is not found, the ghost outputs "Area Clear" and resumes its standard patrol route.

---

## 🛠️ Setup Guide for Level Designers
If you are adding a new room or a new puzzle to the game, follow these steps to integrate the ghost:

1. **Place Nodes:** Drag empty GameObjects into the scene and attach the `SearchNode.cs` script. Place them in the center of walkable paths.
2. **Link Neighbors:** Select a node, and in the Inspector, add the adjacent nodes to its `Neighbors` list. **Important:** Connections must be mutual! If Node 1 points to Node 2, Node 2 *must* point back to Node 1.
3. **Visual Check:** Ensure the yellow Gizmo lines in the Unity Scene view form an unbroken path from the main hallways into your new room.
4. **Configure Ghost:** * Select the Ghost object in the hierarchy.
   * Open the **Investigation Zones** list in the Inspector.
   * Add a new zone. Type the exact JSON ID of your new puzzle (e.g., `key0004`).
   * Drag your newly created nodes into the **Specific Search Nodes** list. Put the room's entrance node at `Element 0`.
