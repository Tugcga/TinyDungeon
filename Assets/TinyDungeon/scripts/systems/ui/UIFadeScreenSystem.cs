using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Tiny.Rendering;

namespace TD
{
    public class UIFadeSceenSystem : SystemBase
    {
        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = EntityManager;
            RequireSingletonForUpdate<UIFadeScreenComponent>();
        }

        void SetVisibility(bool isShow)
        {
            Entities.
#if USE_FOREACH_SYSTEM
#else
                WithoutBurst().
#endif
                WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled).
                ForEach((Entity entity, ref UIFadeScreenComponent screen) =>
                {
                    EntityManager.SetEnabled(entity, isShow);
                }).WithStructuralChanges().Run();
        }

        protected override void OnUpdate()
        {
            UIFadeScreenComponent fade = GetSingleton<UIFadeScreenComponent>();

            if(fade.isActive)
            {
                Entity fadeEntity = GetSingletonEntity<UIFadeScreenComponent>();
                //if it disabled, enable it

                float deltaTime = Time.DeltaTime;
                fade.alphaValue += fade.speed * deltaTime * (fade.direction ? 1 : -1);
                if(fade.direction && fade.alphaValue >= 1.0f)
                {
                    fade.alphaValue = 1.0f;
                    fade.isActive = false;

                    //fading is finish, try to update loading scenes
                    SceneControllerComponent sceneController = GetSingleton<SceneControllerComponent>();
                    sceneController.tagUpdate = true;
                    SetSingleton<SceneControllerComponent>(sceneController);
                }
                else if(!fade.direction && fade.alphaValue <= 0.0f)
                {
                    fade.alphaValue = 0.0f;
                    fade.isActive = false;

                    //add disable component
                }

                MeshRenderer mesh = manager.GetComponentData<MeshRenderer>(fadeEntity);
                Entity material = mesh.material;
                SimpleMaterial lit = manager.GetComponentData<SimpleMaterial>(material);
                lit.constOpacity = fade.alphaValue;
                manager.SetComponentData(material, lit);

                SetSingleton<UIFadeScreenComponent>(fade);
            }
        }
    }

}