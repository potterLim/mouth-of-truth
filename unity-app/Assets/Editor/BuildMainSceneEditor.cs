using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private const string MAIN_SCENE_PATH = "Assets/Scenes/Main.unity";
        private const string DUNGEON_DEMO_SCENE_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Scenes/DemoScene.unity";
        private const string DUNGEON_WALL_MATERIAL_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Materials/M_Wall.mat";
        private const string GENERATED_MATERIAL_DIRECTORY_PATH = "Assets/Materials/GeneratedEnvironment";
        private const string TORCH_PREFAB_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Prefabs/Torch_B.prefab";
        private const string ARCH_PREFAB_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Prefabs/Arch_A.prefab";
        private const string RED_RUNNER_TEXTURE_PATH = "Assets/StreamingAssets/art/environment/floor_red_carpet_runner.png";
        private const string RED_RUNNER_MATERIAL_PATH = "Assets/Materials/GeneratedEnvironment/M_FloorRedRunner.mat";
        private static readonly string[] THIRD_PARTY_MODEL_DIRECTORIES =
        {
            "Assets/ThirdParty/Environment/DungeonModularPack/Meshes",
            "Assets/ThirdParty/Environment/PersianCarpetUrp/Models",
        };
        private static readonly string[] REQUIRED_THIRD_PARTY_ASSET_PATHS =
        {
            DUNGEON_DEMO_SCENE_PATH,
            DUNGEON_WALL_MATERIAL_PATH,
            TORCH_PREFAB_PATH,
            ARCH_PREFAB_PATH,
            "Assets/ThirdParty/Environment/DungeonModularPack/Meshes",
            "Assets/ThirdParty/Environment/PersianCarpetUrp/Models",
        };

        private const float CARD_ANCHOR_SPACING = 3.3f;
        private const float CARD_DEPTH_OFFSET = 7.2f;
        private const float CAMERA_DEPTH_OFFSET = 17.5f;
        private const float CAMERA_HEIGHT_OFFSET = 4.4f;
        private const float STAGE_FORWARD_MARGIN = 4.2f;

        [MenuItem("Mouth Of Truth/Build Main Scene")]
        public static void Run()
        {
            ConfigureUniversalRenderPipelineEditor.Run();
            validateRequiredThirdPartyAssets();
            normalizeThirdPartyModelImports();
            Scene sourceScene = EditorSceneManager.OpenScene(DUNGEON_DEMO_SCENE_PATH, OpenSceneMode.Single);
            Transform sourceEnvironmentRoot = findRequiredRoot(sourceScene, "Models");
            Bounds sourceEnvironmentBounds = calculateCombinedBounds(sourceEnvironmentRoot);
            Camera sourceSceneCamera = findSourceSceneCameraOrNull(sourceScene);
            SourceSceneCameraLayout sourceSceneCameraLayout = captureSourceSceneCameraLayout(sourceSceneCamera);
            Vector3 sourceSceneForward = getProjectedHorizontalForward(sourceSceneCamera, sourceEnvironmentBounds.center);
            List<GameObject> sourceSceneRootClones = cloneSourceSceneRoots(sourceScene);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);

            foreach (GameObject sourceSceneRootClone in sourceSceneRootClones)
            {
                SceneManager.MoveGameObjectToScene(sourceSceneRootClone, scene);
            }

            EditorSceneManager.CloseScene(sourceScene, true);

            unpackScenePrefabInstances(scene);

            Transform environmentRoot = findRequiredRoot(scene, "Models");
            Bounds environmentBounds = calculateCombinedBounds(environmentRoot);
            CorridorAxes corridorAxes = determineCorridorAxes(environmentBounds, sourceSceneForward);

            configureMainCamera(scene, environmentBounds, corridorAxes, sourceSceneCameraLayout);
            ensureEventSystem(scene);
            ensureApplicationRoot(scene);
            buildPresentationStage(scene, environmentBounds, corridorAxes);
            configureEnvironmentLighting(scene);
            unpackScenePrefabInstances(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MAIN_SCENE_PATH);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MAIN_SCENE_PATH, true),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
