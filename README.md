# vitrivr-VR Evaluation
This repository is a fork of vitrivr-VR for the purposes of evaluating individual parts and methods in isolation.

## Multimedia Drawer Evaluation
![vitrivr-VR-drawer](https://user-images.githubusercontent.com/9721543/154434477-5397c3f5-5d2a-4874-84d8-1f1ea62b2d43.gif)

The current version of the Unity project within this repository was used for the evaluation of the multimedia drawer as a video browsing method.

## Setup
Setup is very easy and should not involve much more than having a working OpenXR runtime and a compatible version of the Unity engine installed.

## Usage
To use this software for the evaluation of the multimedia drawer, a compatible instance of [Cineast](https://github.com/vitrivr/cineast) and an [evaluation configuration](Assets/Scripts/VitrivrVR/Evaluation/EvaluationConfig.cs) file are required.
If started from the editor, the [Multimedia Drawer Evaluation Scene](Assets/Scenes/Multimedia%20Drawer%20Evaluation.unity) must be used.

Due to limitations of Unity, during VR usage the on-screen UI cannot be interacted with using a mouse.
It can, however, be fully controlled using only the keyboard:
- Escape: Closes the program.
- Any key at start: Selects the evaluation file text-field.
- Enter when evalutation file text-field is selected: Load configuration.
- S during a task: Skip to the next task.
