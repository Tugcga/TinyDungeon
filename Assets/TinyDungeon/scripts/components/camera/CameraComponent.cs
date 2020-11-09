using Unity.Entities;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct CameraComponent : IComponentData
    {
        public float distance;
        public float positionU;
        public float positionV;
        public float speedU;
        public float speedV;
        public float limitVMin;
        public float limitVMax;

        public float verticalHeight;
    }
}

