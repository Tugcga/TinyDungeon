using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Scenes;

namespace TD
{
    [UpdateAfter(typeof(KeyboardInputSystem))]
    public class LoadingSceneSystem : SystemBase
    {
        EntityQuery sceneGroup;
        //EntityQuery uiGameGroup;

        public bool firstLoad;
        public bool isInitialState;

        EntityManager manager;

        protected override void OnCreate()
        {
            base.OnCreate();
            manager = EntityManager;
            sceneGroup = manager.CreateEntityQuery(typeof(SceneTag));

            firstLoad = false;
            isInitialState = true;

            RequireSingletonForUpdate<SceneControllerComponent>();
            RequireSingletonForUpdate<UIFadeScreenComponent>();
            RequireSingletonForUpdate<UIGlobalStateComponent>();
        }
        
        private void SetStartLabelVisibility(bool isShow)
        {
            Entities.
                WithoutBurst().
                WithEntityQueryOptions(EntityQueryOptions.IncludeDisabled).
                ForEach((Entity entity, ref UIStartLabelComponent startLabel) =>
                {
                    manager.SetEnabled(entity, isShow);
                }).WithStructuralChanges().Run();
        }

        private void SetGameLabelVisibility(bool isShow)
        {
            UIGlobalStateComponent globalUI = GetSingleton<UIGlobalStateComponent>();

            Entities.
                WithoutBurst().
                WithNone<UIGearValueComponent>().
                WithAny<UIGameLabelComponent>().
                ForEach((Entity entity, ref Translation translation) =>
                {
                    translation.Value = new float3(translation.Value.x, translation.Value.y, isShow ? globalUI.activeUIHeight : globalUI.nonActiveUIHeight);
                }).Run();

            if(isShow == false)
            {//also hide gear label
                
                Entities.
                WithoutBurst().
                WithAny<UIGearValueComponent>().
                ForEach((Entity entity, ref Translation translation) =>
                {
                    translation.Value = new float3(translation.Value.x, translation.Value.y, isShow ? globalUI.activeUIHeight : globalUI.nonActiveUIHeight);
                    globalUI.isGearActive = false;
                }).Run();
                SetSingleton<UIGlobalStateComponent>(globalUI);
            }
        }

        protected override void OnUpdate()
        {
            SceneControllerComponent controller = GetSingleton<SceneControllerComponent>();

            if(isInitialState)
            {//call this only once at start of the system
                //hide all game ui elements
                SetGameLabelVisibility(false);
                isInitialState = false;
            }

            //loading scene by hand
            //for test only
            KeyboardInputComponent input = GetSingleton<KeyboardInputComponent>();
            if((firstLoad || input.isPressAny) && !controller.loadingProcess && controller.loadedSceneIndex == 0)
            {//load the first scene only if we current in the 0-th scene and press any button
                firstLoad = false;
                UIFadeScreenComponent fade = GetSingleton<UIFadeScreenComponent>();
                fade.isActive = true;
                fade.direction = true;
                SetSingleton<UIFadeScreenComponent>(fade);

                controller.targetSceneIndex = 1;
                controller.loadingProcess = true;
                SetSingleton<SceneControllerComponent>(controller);
            }

            SceneSystem sceneSystem = World.GetExistingSystem<SceneSystem>();
            if (controller.tagUpdate)
            {
                controller.tagUpdate = false;
                if (controller.loadedSceneIndex != 0)
                {//unload the scene
                    NativeArray<Entity> allScenes = sceneGroup.ToEntityArray(Allocator.Temp);
                    for (int i = 0; i < allScenes.Length; i++)
                    {
                        Entity sceneEntity = allScenes[i];
                        SceneTag sceneTag = manager.GetComponentData<SceneTag>(sceneEntity);
                        if (sceneTag.index == controller.loadedSceneIndex)
                        {
                            //UnityEngine.Debug.Log("unload scene " + sceneTag.index.ToString());
                            sceneSystem.UnloadScene(sceneEntity);
                        }
                    }
                    allScenes.Dispose();
                }
                //and then load target scene
                if(controller.targetSceneIndex > 0)
                {
                    controller.loadedSceneIndex = controller.targetSceneIndex;
                    //hide start label
                    SetStartLabelVisibility(false);
                    SetGameLabelVisibility(true);

                    NativeArray<Entity> scenes = sceneGroup.ToEntityArray(Allocator.Temp);
                    for (int i = 0; i < scenes.Length; i++)
                    {
                        Entity sceneEntity = scenes[i];
                        SceneTag sceneTag = manager.GetComponentData<SceneTag>(sceneEntity);
                        if (sceneTag.index == controller.targetSceneIndex)
                        {
                            //UnityEngine.Debug.Log("loading scene " + sceneTag.index.ToString());
                            SceneReference sceneRef = EntityManager.GetComponentData<SceneReference>(sceneEntity);
                            sceneSystem.LoadSceneAsync(sceneRef.SceneGUID, new SceneSystem.LoadParameters() { AutoLoad = true });

                            controller.startSceneLoading = true;
                            controller.loadingScene = sceneEntity;
                        }
                    }

                    scenes.Dispose();
                }
                else
                {
                    controller.loadedSceneIndex = 0;
                    //nothing to load, show the screen
                    SetGameLabelVisibility(false);
                    SetStartLabelVisibility(true);

                    UIFadeScreenComponent fade = GetSingleton<UIFadeScreenComponent>();
                    fade.isActive = true;
                    fade.direction = false;
                    SetSingleton<UIFadeScreenComponent>(fade);
                }

                SetSingleton<SceneControllerComponent>(controller);
            }

            //check is loaded scene is finishing
            if(controller.startSceneLoading)
            {
                if(sceneSystem.IsSceneLoaded(controller.loadingScene))
                {
                    controller.startSceneLoading = false;
                    controller.loadingProcess = false;
                    SceneTag sceneTag = manager.GetComponentData<SceneTag>(controller.loadingScene);
                    controller.loadedSceneIndex = sceneTag.index;
                    SetSingleton<SceneControllerComponent>(controller);

                    //show the screen
                    UIFadeScreenComponent fade = GetSingleton<UIFadeScreenComponent>();
                    fade.isActive = true;
                    fade.direction = false;
                    SetSingleton<UIFadeScreenComponent>(fade);
                }
            }
        }
    }

}