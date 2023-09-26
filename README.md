# ExoskeletonLearningExperiment

This is the Unity project that contains the full experiment described in the paper titled: *"First Steps Towards Facilitating the Learning of Using Wearable Lower-limb Exoskeletons with Immersive Virtual Reality"*. Which can be found [here](TODO-ADD-LINK.com).

The protocol we used for starting and running the experiment can be found [here](Documentation/protocol.md).

## Launch the project
- Load the project into Unity 2020.3.21f1 (using Unity Hub).
- Import [FinalIK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290) - This package is not included because it is a paid package, so you will need to import it yourself.
- Open & Start `Assets/scenes/ExperimentScene`

## Packages used
- **NOT INCLUDED IN REPO**: [FinalIK](https://assetstore.unity.com/packages/tools/animation/final-ik-14290), by [RootMotion](http://root-motion.com)
- [Unity Experiment Framework (UXF)](https://immersivecognition.com/unity-experiment-framework/)
- [SteamVR, v2.7.3](https://github.com/ValveSoftware/steamvr_unity_plugin/releases/tag/2.7.3)
- [OpenVR XR Plugin 1.1.4](https://github.com/ValveSoftware/unity-xr-plugin) 
- [VR Questionnaire Toolkit](https://github.com/MartinFk/VRQuestionnaireToolkit)
- [Universal RP 10.6](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.6/manual/index.html)

Models and Materials
- Skybox from [Skybox Series Free](https://assetstore.unity.com/packages/2d/textures-materials/sky/skybox-series-free-103633)
- Grass material [Unity Technologies: Terrain Sample Asset Pack](https://assetstore.unity.com/packages/3d/environments/landscapes/terrain-sample-asset-pack-145808)
- Brick Material [Avionx: World Materials Free unity package](https://assetstore.unity.com/packages/2d/textures-materials/world-materials-free-150182)
- [Walker model, by printable_models, on Free3D.com](https://free3d.com/3d-model/walker-no-wheels-v1--32317.html)
- Avatar Model [asoliddev: Modular Fantasy Character](https://assetstore.unity.com/packages/3d/characters/modular-fantasy-character-165896)
- Shoes Model [CheinSojang: Shoes Pack (50+ shoes)](https://vrcmods.com/item/4592-Shoes-Pack-50-shoes-)

## Assets
- `Models/` - Avatar, Shoe, and Walker model
- `Plugins/` - Contains UXF, and this is where FinalIK was placed (but now removed).
- `Questionnaires/` - Contains VR Questionnaire TK assets.
- `Resources/` - All materials, sprites, and textures. This also contains the **stepping data** used for the stepping animation.
- `Scenes/` - The ExerimentScene scene is placed here.
- `Scripts/` - All the (custom) scripts that handle the functionality of the scene, experiment, visual cues, ... (see below).
- `SteamVR/`, `SteamVR_Input`, `SteamVR_Resources` - SteamVR plugin files you don't need to touch.
- `StreamingAssets/` - Contains config file for Vive trackers, and UXF setting files.
- `XR/` - OpenXR related files.

## Scripts
There are more scripts then outlined here, but these are the most important ones to help you get started with different parts of the experiment.

- `DelsysConnection/` - Custom package that sets up a little UDP client that picks up the Delsys data. Note: there is also an [API by Delsys](http://data.delsys.com/DelsysServicePortal/api/web-examples/unity-integration.html).
- `UnityDataReplay/` - Custom package written for this project to take position data from a .csv, and replay it in any position, with any time and position scaling that is desired. This was used to make the avatar take steps in the scene.
- `Inputs/` - Scripts that handle the inputs from the Vive Trackers and use them to determine the walker position, weight shifting, hip thrust input.
  - `HipThrustInput.cs` - The script defines a HipThrustInput class in Unity that detects hip thrust motions using sensor data, evaluates the thrust based on various thresholds, and triggers corresponding events, along with a HipThrust class to encapsulate information about a single hip thrust action.
  - `TrunkInclinationInput.cs` - The script defines a `TrunkInclinationInput` class in Unity that calculates the inclination angle of an avatar's trunk based on the positions of its head and hips using the VRIK system, and provides methods to retrieve the raw and scaled inclination angles.
  - `WalkerMotion.cs` - The script defines a `WalkerMotion` class in Unity that manages the movement and calibration of a virtual walker based on tracker positions, with functionalities to match walker positions, calibrate initial positions, and handle step events.
  - `WeightShiftingInput.cs` - The script defines a `WeightShiftingInput` class in Unity that calculates the midpoint between the avatar's feet using the VRIK system and determines the hip's position relative to this midpoint.
  - `SensorCalibration.cs` - The script defines a `SensorCalibration` class in Unity that calibrates an IMU (Inertial Measurement Unit) sensor by averaging acceleration measurements to determine the gravity direction, then adjusts the IMU transform to align with the world's down direction.
- `VisualCues/`
  - `GaitFeedback.cs` - This class handles the color, shape, and functionality of the fusiform object projected on the floor.
  - `GradientSetter.cs` - Calculates the new color gradient for the fusiform object, and bakes it onto the texture.
  - `TextureCurve.cs` - Calculates the new shape of the fisuform object, and bakes it onto the texture.
  - `PointsFeedback.cs` - Helper class to make the points show the right number and color, and let it appear and fade out.
  - `WeighShiftDetection.cs` - The script defines a `WeightShiftDetection` class in Unity that monitors the position of the avatar's leading leg, adjusts a mask's position based on the step width, and changes a sprite's color to indicate weight shifting, with additional functionality to blink the sprite's color when certain conditions are met.
- `UXF/` - Contains scripts that interact with the UXF (custom trackers, and task/UI management).
