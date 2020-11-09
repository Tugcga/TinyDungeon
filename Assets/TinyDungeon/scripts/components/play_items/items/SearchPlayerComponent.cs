using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct SearchPlayerComponent : IComponentData
    {
        public Entity rotateObject;

        public bool searchDirection;
        public float angle;

        public float searchAngle;  // actual angle is equal to x2, because the search from -angle to +angle
        public float searchRotateSpeed;

        public TowerState state;
        public float2 checkDirection;  // for the state STATE_CHECK_DIRECTION
        public double startWaitTime;  // remember ehere start time of the mait for STATE_WAIT

        public float visibleAngle;  // how wide the visible area of the tower, +- from the forward direction
        public float activeRotateSpeed;
        public float visibleDistance;

        public float checkPlayerTimeDelta;  // delta time between player position check
        public double lastCheckTime;

        //data of the last check of the player position
        public float2 toPlayer;
        public float toPlayerdistance;
        public float2 toPlayerDirection;
        public float toPlayerAngle;
        public bool isVisiblePlayer;  // turn on, if the searcher can see the player (no collisions between searcher and player), false if there is any collider between searcher and player
    }
}
