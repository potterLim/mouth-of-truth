using MouthOfTruth.Game.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private static void buildPresentationStage(Scene scene, Bounds environmentBounds, CorridorAxes corridorAxes)
        {
            GameObject stageRoot = getOrCreateRootObject(scene, "MouthOfTruthStage");
            destroyChildren(stageRoot.transform);

            Vector3 stageBasePosition = calculateStageBasePosition(environmentBounds, corridorAxes);
            float floorY = environmentBounds.min.y;

            Transform scenicRoot = createChild(stageRoot.transform, "ScenicStage");
            createPodium(scenicRoot, stageBasePosition, floorY);
            createStageAccents(scenicRoot, stageBasePosition, corridorAxes, floorY);
            createStageRunner(scenicRoot, environmentBounds, corridorAxes, floorY, stageBasePosition);

            Transform cardAnchorRoot = createChild(stageRoot.transform, "CardPresentationAnchors");
            Transform leftCardAnchor = createChild(cardAnchorRoot, "LeftCard");
            Transform centerCardAnchor = createChild(cardAnchorRoot, "CenterCard");
            Transform rightCardAnchor = createChild(cardAnchorRoot, "RightCard");
            Vector3 cardAnchorBasePosition = stageBasePosition - (corridorAxes.Forward * CARD_DEPTH_OFFSET) + (Vector3.up * 1.75f);
            centerCardAnchor.position = cardAnchorBasePosition;
            leftCardAnchor.position = cardAnchorBasePosition - (corridorAxes.Lateral * CARD_ANCHOR_SPACING);
            rightCardAnchor.position = cardAnchorBasePosition + (corridorAxes.Lateral * CARD_ANCHOR_SPACING);
            CardPresentationAnchorSet cardPresentationAnchorSet = ensureComponent<CardPresentationAnchorSet>(cardAnchorRoot.gameObject);
            cardPresentationAnchorSet.Configure(leftCardAnchor, centerCardAnchor, rightCardAnchor);

            Transform mouthAnchorRoot = createChild(stageRoot.transform, "MouthAnchors");
            Transform truthMouthAnchor = createChild(mouthAnchorRoot, "TruthMouth");
            Transform mouthFrontAnchor = createChild(mouthAnchorRoot, "MouthFrontAnchor");
            Transform mouthInnerAnchor = createChild(mouthAnchorRoot, "MouthInnerAnchor");
            truthMouthAnchor.position = stageBasePosition + new Vector3(0.0f, 1.9f, 0.0f);
            mouthFrontAnchor.position = truthMouthAnchor.position
                - (corridorAxes.Forward * 0.42f)
                - (Vector3.up * 1.55f);
            mouthInnerAnchor.position = truthMouthAnchor.position - (Vector3.up * 0.45f);
            MouthAnchorSet mouthAnchorSet = ensureComponent<MouthAnchorSet>(mouthAnchorRoot.gameObject);
            mouthAnchorSet.Configure(truthMouthAnchor, mouthFrontAnchor, mouthInnerAnchor);
        }

        private static void createPodium(Transform parentTransform, Vector3 stageBasePosition, float floorY)
        {
            GameObject podiumObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            podiumObject.name = "TruthMouthPodium";
            podiumObject.transform.SetParent(parentTransform, false);
            podiumObject.transform.position = new Vector3(stageBasePosition.x, floorY + 0.55f, stageBasePosition.z);
            podiumObject.transform.localScale = new Vector3(2.4f, 0.55f, 2.4f);

            Material wallMaterial = AssetDatabase.LoadAssetAtPath<Material>(DUNGEON_WALL_MATERIAL_PATH);

            if (wallMaterial != null)
            {
                Renderer podiumRenderer = podiumObject.GetComponent<Renderer>();
                podiumRenderer.sharedMaterial = wallMaterial;
            }
        }

        private static void createStageAccents(Transform parentTransform, Vector3 stageBasePosition, CorridorAxes corridorAxes, float floorY)
        {
            GameObject archPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ARCH_PREFAB_PATH);
            GameObject torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TORCH_PREFAB_PATH);

            if (archPrefab != null)
            {
                GameObject archInstance = instantiatePrefab(archPrefab, parentTransform, "StageArch");
                archInstance.transform.position = new Vector3(stageBasePosition.x, floorY + 0.1f, stageBasePosition.z + 0.25f);
                alignLongAxisToForward(archInstance.transform, corridorAxes.Forward, EObjectLongAxis.XAxis);
                archInstance.transform.localScale = new Vector3(1.18f, 1.18f, 1.18f);
            }

            if (torchPrefab == null)
            {
                return;
            }

            createTorchPair(parentTransform, stageBasePosition, corridorAxes, floorY, torchPrefab);
        }

        private static void createTorchPair(Transform parentTransform, Vector3 stageBasePosition, CorridorAxes corridorAxes, float floorY, GameObject torchPrefab)
        {
            Vector3 leftTorchPosition = stageBasePosition - (corridorAxes.Lateral * 4.25f) + (Vector3.up * 2.2f);
            Vector3 rightTorchPosition = stageBasePosition + (corridorAxes.Lateral * 4.25f) + (Vector3.up * 2.2f);

            GameObject leftTorch = instantiatePrefab(torchPrefab, parentTransform, "StageTorchLeft");
            leftTorch.transform.position = leftTorchPosition;
            alignLongAxisToForward(leftTorch.transform, corridorAxes.Forward, EObjectLongAxis.ZAxis);

            GameObject rightTorch = instantiatePrefab(torchPrefab, parentTransform, "StageTorchRight");
            rightTorch.transform.position = rightTorchPosition;
            alignLongAxisToForward(rightTorch.transform, corridorAxes.Forward, EObjectLongAxis.ZAxis);
        }

        private static void createStageRunner(Transform parentTransform, Bounds environmentBounds, CorridorAxes corridorAxes, float floorY, Vector3 stageBasePosition)
        {
            Material runnerMaterial = getOrCreateRunnerMaterialOrNull();

            if (runnerMaterial == null)
            {
                return;
            }

            float corridorExtent = corridorAxes.GetExtent(environmentBounds);
            float startOffset = -corridorExtent + 2.0f;
            float endOffset = Vector3.Dot(stageBasePosition - environmentBounds.center, corridorAxes.Forward.normalized)
                + 0.75f;
            float runnerLength = Mathf.Max(8.0f, endOffset - startOffset);
            float runnerMidpointOffset = (startOffset + endOffset) * 0.5f;

            GameObject runnerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            runnerObject.name = "StageRunner";
            runnerObject.transform.SetParent(parentTransform, false);
            runnerObject.transform.position = environmentBounds.center
                + (corridorAxes.Forward * runnerMidpointOffset)
                + (Vector3.up * (floorY + 0.02f));
            runnerObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, corridorAxes.Forward.normalized);
            runnerObject.transform.localScale = new Vector3(3.25f, 0.035f, runnerLength);
            runnerObject.GetComponent<Renderer>().sharedMaterial = runnerMaterial;
        }

        private static Material getOrCreateRunnerMaterialOrNull()
        {
            ensureFolderHierarchy(GENERATED_MATERIAL_DIRECTORY_PATH);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(RED_RUNNER_MATERIAL_PATH);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(RED_RUNNER_TEXTURE_PATH);
            Shader shader = findFirstAvailableShaderOrNull(
                new[]
                {
                    "Universal Render Pipeline/Lit",
                    "Universal Render Pipeline/Simple Lit",
                    "Standard",
                });

            if (texture == null || shader == null)
            {
                return null;
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, RED_RUNNER_MATERIAL_PATH);
            }

            material.shader = shader;

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", new Color(0.58f, 0.50f, 0.46f, 0.86f));
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", Color.white);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", 0.1f);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0.0f);
            }

            material.mainTextureScale = new Vector2(1.0f, 6.0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void configureEnvironmentLighting(Scene scene)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.17f, 0.17f, 0.20f, 1.0f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.10f, 0.10f, 0.12f, 1.0f);
            RenderSettings.fogDensity = 0.015f;

            foreach (Light light in Resources.FindObjectsOfTypeAll<Light>())
            {
                if (light == null || light.gameObject.scene != scene)
                {
                    continue;
                }

                switch (light.type)
                {
                    case LightType.Directional:
                        light.color = new Color(0.78f, 0.81f, 0.88f, 1.0f);
                        light.intensity = 0.16f;
                        light.shadows = LightShadows.Soft;
                        light.shadowStrength = 0.48f;
                        break;

                    case LightType.Point:
                    case LightType.Spot:
                        light.color = new Color(0.92f, 0.68f, 0.40f, 1.0f);
                        light.intensity = Mathf.Max(3.0f, light.intensity);
                        light.range = Mathf.Max(6.0f, light.range);
                        light.shadows = LightShadows.Soft;
                        light.shadowStrength = 0.50f;
                        break;

                    default:
                        break;
                }

                EditorUtility.SetDirty(light);
            }
        }
    }
}
