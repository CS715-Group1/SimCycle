# SimCycle
Group 1 Members
- Andy Fong
- Patrick Oliver
- Ou-An Chuang

## Hardware Requirements
There are no specific hardware requirements for running the simulations. However, your system should support Unity Editor 2022.3.6f1.

# Installation
1. Please ensure you have Unity Editor 2022.3.6f1 installed. You can do this through Unity Hub.
2. Clone this repo and open it as a project in Unity Hub.

# Running the Simulation
There are two parts that demonstrates the system:
- Scenario simulation
- Vision system demo

## Scenario Simulation
1. Navigate to Assets > Scenes > Scenarios.
2. Choose one of the four scenarios, for example: `Scenario_B`.
3. Press the Play button in the Unity Editor.
4. From the editor's Game View, initialise the scenario by pressing [SPACE], then [A] to start the simulation.
5. You can choose to view the scenario from the driver's perspective in the Game View, or choose to view from any angle in the Scene View.
6. Stop the simulation by pressing the Play button again.

<img width="842" alt="Screen Shot 2023-10-24 at 8 33 18 PM" src="https://github.com/CS715-Group1/SimCycle/assets/75078332/ecd971be-8f89-4465-bcc7-5f9e3bc26fd4">


## Vision System Demo
1. Navigate to Assets > Scenes and open the `Revised_Vision` scene.
2. Set the Unity Editor layout (the dropdown menu in the top right corner) to '2 by 3'.
3. Locate the game object called `View Plane` in the Hierarchy. This will show the simulated human vision of the car agent that references this plane.
4. Press the Play button in the Unity Editor.
5. Navigate to the editor's Scene View.
6. Try moving `Car A` (the target car whose simulated vision is being shown) from an obscured to a clear view of the other cars. You can also try moving the gray walls.
7. Observe how the image on the `View Plane` changes.

<img width="692" alt="Screen Shot 2023-10-24 at 8 37 55 PM" src="https://github.com/CS715-Group1/SimCycle/assets/75078332/73b45375-9cd1-4d74-a1c4-1ba163f71646">

