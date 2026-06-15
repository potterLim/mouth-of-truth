using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private static void validateRequiredThirdPartyAssets()
        {
            foreach (string requiredAssetPath in REQUIRED_THIRD_PARTY_ASSET_PATHS)
            {
                if (AssetDatabase.IsValidFolder(requiredAssetPath) || AssetDatabase.LoadMainAssetAtPath(requiredAssetPath) != null)
                {
                    continue;
                }

                throw new BuildFailedException(
                    $"Required Unity Asset Store asset is missing: {requiredAssetPath}\n"
                    + "Restore the third-party environment assets by following THIRD_PARTY_ASSETS.md.");
            }
        }

        private static void normalizeThirdPartyModelImports()
        {
            foreach (string modelDirectoryPath in THIRD_PARTY_MODEL_DIRECTORIES)
            {
                string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { modelDirectoryPath });

                foreach (string modelGuid in modelGuids)
                {
                    string modelAssetPath = AssetDatabase.GUIDToAssetPath(modelGuid);
                    ModelImporter modelImporter = AssetImporter.GetAtPath(modelAssetPath) as ModelImporter;

                    if (modelImporter == null)
                    {
                        continue;
                    }

                    if (modelImporter.materialImportMode == ModelImporterMaterialImportMode.None)
                    {
                        continue;
                    }

                    modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                    modelImporter.SaveAndReimport();
                }
            }
        }

        private static Camera findSourceSceneCameraOrNull(Scene sourceScene)
        {
            foreach (GameObject rootGameObject in sourceScene.GetRootGameObjects())
            {
                Camera rootCamera = rootGameObject.GetComponent<Camera>();

                if (rootCamera != null)
                {
                    return rootCamera;
                }

                Camera childCamera = rootGameObject.GetComponentInChildren<Camera>(true);

                if (childCamera != null)
                {
                    return childCamera;
                }
            }

            return null;
        }

        private static List<GameObject> cloneSourceSceneRoots(Scene sourceScene)
        {
            List<GameObject> rootClones = new List<GameObject>();

            foreach (GameObject rootGameObject in sourceScene.GetRootGameObjects())
            {
                if (rootGameObject.name == "Main Camera")
                {
                    continue;
                }

                GameObject rootClone = UnityEngine.Object.Instantiate(rootGameObject);
                rootClone.name = rootGameObject.name;
                rootClones.Add(rootClone);
            }

            return rootClones;
        }

        private static Vector3 getProjectedHorizontalForward(Camera sourceSceneCamera, Vector3 environmentCenter)
        {
            if (sourceSceneCamera == null)
            {
                return Vector3.zero;
            }

            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(sourceSceneCamera.transform.forward, Vector3.up);

            if (projectedCameraForward.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 normalizedForward = projectedCameraForward.normalized;
            Vector3 directionToEnvironmentCenter = Vector3.ProjectOnPlane(environmentCenter - sourceSceneCamera.transform.position, Vector3.up);

            if (directionToEnvironmentCenter.sqrMagnitude > 0.0001f && Vector3.Dot(normalizedForward, directionToEnvironmentCenter.normalized) < 0.0f)
            {
                normalizedForward = -normalizedForward;
            }

            return normalizedForward;
        }

        private static SourceSceneCameraLayout captureSourceSceneCameraLayout(Camera sourceSceneCamera)
        {
            if (sourceSceneCamera == null)
            {
                return SourceSceneCameraLayout.Invalid;
            }

            return new SourceSceneCameraLayout(
                sourceSceneCamera.transform.position,
                sourceSceneCamera.transform.rotation,
                sourceSceneCamera.fieldOfView,
                sourceSceneCamera.backgroundColor);
        }

        private static void unpackScenePrefabInstances(Scene scene)
        {
            HashSet<GameObject> outermostPrefabRoots = new HashSet<GameObject>();

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                foreach (Transform childTransform in rootGameObject.GetComponentsInChildren<Transform>(true))
                {
                    GameObject candidate = childTransform.gameObject;

                    if (PrefabUtility.IsPartOfPrefabInstance(candidate) == false)
                    {
                        continue;
                    }

                    GameObject outermostRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(candidate);

                    if (outermostRoot != null)
                    {
                        outermostPrefabRoots.Add(outermostRoot);
                    }
                }
            }

            foreach (GameObject outermostPrefabRoot in outermostPrefabRoots)
            {
                PrefabUtility.UnpackPrefabInstance(outermostPrefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }
    }
}
