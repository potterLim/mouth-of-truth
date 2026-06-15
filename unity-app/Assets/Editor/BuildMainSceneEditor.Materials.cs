using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private static Bounds calculateCombinedBounds(Transform rootTransform)
        {
            Renderer[] renderers = rootTransform.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length == 0)
            {
                throw new InvalidOperationException("환경 루트에서 Renderer를 찾을 수 없습니다.");
            }

            Bounds bounds = renderers[0].bounds;

            foreach (Renderer renderer in renderers.Skip(1))
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        private static void sanitizeRendererMaterials(Transform rootTransform)
        {
            Dictionary<Material, Material> sanitizedMaterialsBySource = new Dictionary<Material, Material>();
            Renderer[] renderers = rootTransform.GetComponentsInChildren<Renderer>(true);
            Material defaultSourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(DUNGEON_WALL_MATERIAL_PATH);
            Material fallbackSafeMaterial = defaultSourceMaterial != null ? getOrCreateSafeMaterial(defaultSourceMaterial) : null;

            foreach (Renderer renderer in renderers)
            {
                Material[] sourceSharedMaterials = renderer.sharedMaterials;

                if (sourceSharedMaterials == null || sourceSharedMaterials.Length == 0)
                {
                    if (fallbackSafeMaterial != null)
                    {
                        renderer.sharedMaterials = new[] { fallbackSafeMaterial };
                        EditorUtility.SetDirty(renderer);
                    }

                    continue;
                }

                bool wasUpdated = false;
                Material[] sanitizedSharedMaterials = new Material[sourceSharedMaterials.Length];

                for (int materialIndex = 0; materialIndex < sourceSharedMaterials.Length; materialIndex += 1)
                {
                    Material sourceMaterial = sourceSharedMaterials[materialIndex];

                    if (sourceMaterial == null)
                    {
                        sanitizedSharedMaterials[materialIndex] = fallbackSafeMaterial;
                        wasUpdated = fallbackSafeMaterial != null;
                        continue;
                    }

                    if (shouldSanitizeMaterial(sourceMaterial) == false)
                    {
                        sanitizedSharedMaterials[materialIndex] = sourceMaterial;
                        continue;
                    }

                    Material sanitizedMaterial;
                    if (sanitizedMaterialsBySource.TryGetValue(sourceMaterial, out sanitizedMaterial) == false)
                    {
                        sanitizedMaterial = getOrCreateSafeMaterial(sourceMaterial);
                        sanitizedMaterialsBySource[sourceMaterial] = sanitizedMaterial;
                    }

                    sanitizedSharedMaterials[materialIndex] = sanitizedMaterial;
                    wasUpdated = true;
                }

                if (wasUpdated)
                {
                    renderer.sharedMaterials = sanitizedSharedMaterials;
                    EditorUtility.SetDirty(renderer);
                }
            }
        }

        private static Shader findFirstAvailableShaderOrNull(IReadOnlyList<string> shaderNames)
        {
            foreach (string shaderName in shaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    return shader;
                }
            }

            return null;
        }

        private static bool shouldSanitizeMaterial(Material material)
        {
            Shader shader = material.shader;
            string shaderName = shader != null ? shader.name : string.Empty;

            return shaderName.Contains("DungeonKitShader", StringComparison.OrdinalIgnoreCase)
                || shaderName.StartsWith("LB Shader/", StringComparison.OrdinalIgnoreCase)
                || shaderName.Equals("Universal Render Pipeline/Lit", StringComparison.Ordinal)
                || shaderName.Equals("Universal Render Pipeline/Simple Lit", StringComparison.Ordinal);
        }

        private static Material getOrCreateSafeMaterial(Material sourceMaterial)
        {
            ensureFolderHierarchy(GENERATED_MATERIAL_DIRECTORY_PATH);

            string sanitizedMaterialAssetPath = $"{GENERATED_MATERIAL_DIRECTORY_PATH}/{sourceMaterial.name}_SceneSafe.mat";
            Shader safeShader = findFirstAvailableShaderOrNull(
                new[]
                {
                    "Universal Render Pipeline/Unlit",
                    "Universal Render Pipeline/Lit",
                    "Unlit/Texture",
                    "Unlit/Color",
                });

            if (safeShader == null)
            {
                throw new InvalidOperationException("빌드용 안전 셰이더를 찾을 수 없습니다.");
            }

            Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(sanitizedMaterialAssetPath);

            if (existingMaterial != null)
            {
                if (existingMaterial.shader != safeShader)
                {
                    existingMaterial.shader = safeShader;
                }

                synchronizeSafeMaterial(existingMaterial, sourceMaterial);
                return existingMaterial;
            }

            Material safeMaterial = new Material(safeShader);
            synchronizeSafeMaterial(safeMaterial, sourceMaterial);
            AssetDatabase.CreateAsset(safeMaterial, sanitizedMaterialAssetPath);
            return safeMaterial;
        }

        private static void synchronizeSafeMaterial(Material safeMaterial, Material sourceMaterial)
        {
            if (safeMaterial == null || sourceMaterial == null)
            {
                return;
            }

            Texture baseTexture = getFirstAvailableTextureOrNull(sourceMaterial, "_BaseMap", "_MainTex", "_BaseColorMap");

            if (baseTexture != null && safeMaterial.HasProperty("_BaseMap"))
            {
                safeMaterial.SetTexture("_BaseMap", baseTexture);
            }

            if (baseTexture != null && safeMaterial.HasProperty("_MainTex"))
            {
                safeMaterial.SetTexture("_MainTex", baseTexture);
            }

            Color baseColor = getFirstAvailableColor(sourceMaterial, Color.white, "_BaseColor", "_Color");

            if (safeMaterial.HasProperty("_BaseColor"))
            {
                safeMaterial.SetColor("_BaseColor", baseColor);
            }

            if (safeMaterial.HasProperty("_Color"))
            {
                safeMaterial.SetColor("_Color", baseColor);
            }

            EditorUtility.SetDirty(safeMaterial);
        }

        private static Texture getFirstAvailableTextureOrNull(Material material, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (material.HasProperty(propertyName))
                {
                    Texture texture = material.GetTexture(propertyName);

                    if (texture != null)
                    {
                        return texture;
                    }
                }
            }

            return null;
        }

        private static Color getFirstAvailableColor(Material material, Color fallbackColor, params string[] propertyNames)
        {
            foreach (string propertyName in propertyNames)
            {
                if (material.HasProperty(propertyName))
                {
                    return material.GetColor(propertyName);
                }
            }

            return fallbackColor;
        }

        private static void ensureFolderHierarchy(string folderPath)
        {
            string[] folderSegments = folderPath.Split('/');
            string currentPath = folderSegments[0];

            for (int folderIndex = 1; folderIndex < folderSegments.Length; folderIndex += 1)
            {
                string nextPath = $"{currentPath}/{folderSegments[folderIndex]}";

                if (AssetDatabase.IsValidFolder(nextPath) == false)
                {
                    AssetDatabase.CreateFolder(currentPath, folderSegments[folderIndex]);
                }

                currentPath = nextPath;
            }
        }
    }
}
