using System;
using System.Linq;
using MouthOfTruth.Game.App;
using MouthOfTruth.Game.Presentation.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private static void ensureApplicationRoot(Scene scene)
        {
            GameObject appObject = getOrCreateRootObject(scene, "MouthOfTruthApp");
            ensureComponent<MouthOfTruthGameView>(appObject);
            ensureComponent<MouthOfTruthAppController>(appObject);
        }

        private static void ensureEventSystem(Scene scene)
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindAnyObjectByType<EventSystem>();

            if (existingEventSystem != null)
            {
                ensureComponent<StandaloneInputModule>(existingEventSystem.gameObject);
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystemObject, scene);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private static Transform findRequiredRoot(Scene scene, string rootObjectName)
        {
            GameObject rootObject = scene.GetRootGameObjects()
                .FirstOrDefault(candidate => candidate.name == rootObjectName);

            if (rootObject == null)
            {
                throw new InvalidOperationException(
                    $"씬 루트 '{rootObjectName}' 을(를) 찾을 수 없습니다.");
            }

            return rootObject.transform;
        }

        private static GameObject getOrCreateRootObject(Scene scene, string objectName)
        {
            GameObject existingObject = scene.GetRootGameObjects()
                .FirstOrDefault(candidate => candidate.name == objectName);

            if (existingObject != null)
            {
                return existingObject;
            }

            GameObject createdObject = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(createdObject, scene);
            return createdObject;
        }

        private static Transform createChild(Transform parentTransform, string objectName)
        {
            Transform existingChild = parentTransform.Find(objectName);

            if (existingChild != null)
            {
                return existingChild;
            }

            GameObject childObject = new GameObject(objectName);
            childObject.transform.SetParent(parentTransform, false);
            return childObject.transform;
        }

        private static GameObject instantiatePrefab(GameObject prefab, Transform parentTransform, string objectName)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parentTransform) as GameObject;

            if (instance == null)
            {
                throw new InvalidOperationException($"프리팹 생성에 실패했습니다: {prefab.name}");
            }

            instance.name = objectName;
            return instance;
        }

        private static T ensureComponent<T>(GameObject gameObject)
            where T : Component
        {
            T existingComponent = gameObject.GetComponent<T>();
            return existingComponent != null ? existingComponent : gameObject.AddComponent<T>();
        }

        private static void destroyChildren(Transform parentTransform)
        {
            for (int childIndex = parentTransform.childCount - 1; childIndex >= 0; childIndex--)
            {
                UnityEngine.Object.DestroyImmediate(parentTransform.GetChild(childIndex).gameObject);
            }
        }
    }
}
