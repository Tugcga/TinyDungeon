using Unity.Entities;
using Unity.Mathematics;

namespace TD
{
    [GenerateAuthoringComponent]
    public struct SceneControllerComponent : IComponentData
    {
        public bool tagUpdate;  // if true, then check loading systems
        public bool startSceneLoading;
        public Entity loadingScene;

        public int targetSceneIndex;
        public bool loadingProcess;

        public int loadedSceneIndex;  // 0 - no scene load
    }
}