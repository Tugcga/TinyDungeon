using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct AmmoAnimatorComponent : IComponentData
    {
        public float minScale;
        public float maxScale;
        public float scaleFrequency;

        public float rotationSpeed;

        public float minHeight;
        public float maxHeight;
        public float heightFrequency;
    }
}