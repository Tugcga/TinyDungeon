### Description

This is a project on an early WIP stage. Repository contains implementation of some basic gameplay mechanics. Based on Unity Tiny framework. This framework allows to create games in pure DOTS, and use Monobehaviour only for converting purposes. The version of Tiny is 0.31.

### Implementated gamplay features

* Move player by using WASD
* Rotate camera by holding right mouse button. Camera follows to the player
* Rotate player to the mouse cursor position
* Player can shoot a bullets.
* The number of bullets is limited, but there are ammunition stores, which increase the amount of bullets on the player.
* Convert Unity navmesh to the set of collidable edges, which stores on RTree structure. Use this structure to find intersections between these edges and small intervals (started on player position and ended on next player position during movement between two frames).
* The system of gates. Each gate is an area, which can be in two states: walkable and non-walkable. Switcher - is a special area, which allows to switch the state of an each gate. Each gate and switcher has a color property, and each switcher change the state of gates only with the same color.
* Barrels. Barrel is an item, which explode when any bullet collide with them.
* Towers. Each tower stay at some position and scan the area on the front of them. If the tower detects the player in the scanned area, then it starts to shot.
* Player beside barrel or gate is invisible for tower.
* Tower explodes when it come damage from the player.
* If tower get damage from any direction, then it rotates to scan the area from this direction. If this are does not contains player, then the tower returns to the scanning of the default area.
* If the player is dead, then it blocked on several seconds, after that it starts the level from the start point.
* One tutorial level and 4 simple levels as examples of gameplay.
* Very WIP build [here](https://tugcga.github.io/games/TinyDungeon/game_td.html)


### References

* [Advanced Algorithms](https://github.com/justcoding121/Advanced-Algorithms) RTree accelerating structure is used for implementation of collision detections.
* [Unity Tiny](https://forum.unity.com/forums/project-tiny.151/) Unity fully DOTS game engine.
* [Angry Bots 2](https://github.com/UnityTechnologies/AngryBots2) Some assets from this repository is used for levels constructing.
* [Dungeon Architect](https://assetstore.unity.com/packages/tools/utilities/dungeon-architect-53895) Use this addon for levels generations.

### Known issues

* The shadows are rendered with artifacts.
* Crashes in builds, builded with Burst enabled.
* In WebGL build the looped sound is played after destroying parent entity.
* Some crashes in a scene with many destroyed entities.
* In WebGL build mouse position translated to canvas with respect to top-left corner of the window, but not to the canvas html-element.