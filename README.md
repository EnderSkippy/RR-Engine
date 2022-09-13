![Reel to Reel Icon Stroke - Copy](https://user-images.githubusercontent.com/69170079/180670348-4175cc09-545a-4611-802f-b2d50ef1c0e3.png)

# Description
A Repository for my Reel to Real bug fixes. I want testers to ensure these work on other hardware before I submit pull requests. Please submit results under the issues tab, positive or negative.

 Orignal Description:
The RR Engine is a Unity codebase for simulating and animating old animatronics, with various features to customize the viewing experience.

The sofware is split into two major sections. The Editor Mode allows users to maticulously look and edit animation files,
while giving a variety of analysis options. The Game Mode provides a first person view of the animatronic stages, 
allowing the user more customizablity in the presentation, while also giving simplified controls. The Game Mode also
has features to unlock special items as rewards for animating the simulated animatronics. This is usually stage props
or movable animatronics, allowing the player to create a fully custom stage or layout to host their animations.

# Credits
- The 64th Gamer (Codebase)
- Himitsu (Improved simulation effeciency)

The RR Engine and its creator should be credited in any derivative works.

# Plugins
A set of plugins are required for building properly, and are not included within the public codebase.
- [Open WAV Parser](https://assetstore.unity.com/packages/tools/audio/open-wav-parser-90832#publisher)
- [Dynamic Bone ($20)](https://assetstore.unity.com/packages/tools/animation/dynamic-bone-16743#description)
- [Off Screen Target Indicator](https://assetstore.unity.com/packages/tools/gui/off-screen-target-indicator-71799#publisher)
- [Standalone File Browser](https://github.com/gkngkc/UnityStandaloneFileBrowser)

# Switching Branches
Depending on whichever branch is pushed with new updates, the default branch you might start in when opening the project could be different. Navigate to the "Scenes/Manger/Manager.unity" scene and open it. Click on the only object in the scene and look in the inspector. There a custom script will have buttons for switching between the 4 different builds. Click the branch button you want, then toggle the VR switch on or off to activate the branch switch. You may need to flick the VR switch a few times if it doesn't work. Be sure to open the game to be sure your branch has been switched to before leaving the Manager scene.

