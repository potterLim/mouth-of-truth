using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private static Sprite createPointerCursorSprite()
        {
            Texture2D texture = new Texture2D(POINTER_CURSOR_TEXTURE_SIZE, POINTER_CURSOR_TEXTURE_SIZE, TextureFormat.RGBA32, mipChain: false);
            texture.hideFlags = HideFlags.DontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            float center = (POINTER_CURSOR_TEXTURE_SIZE - 1.0f) * 0.5f;
            float fillRadius = POINTER_CURSOR_TEXTURE_SIZE * 0.24f;
            float ringInnerRadius = POINTER_CURSOR_TEXTURE_SIZE * 0.31f;
            float ringOuterRadius = POINTER_CURSOR_TEXTURE_SIZE * 0.39f;
            Color[] pixels = new Color[POINTER_CURSOR_TEXTURE_SIZE * POINTER_CURSOR_TEXTURE_SIZE];

            for (int y = 0; y < POINTER_CURSOR_TEXTURE_SIZE; y += 1)
            {
                for (int x = 0; x < POINTER_CURSOR_TEXTURE_SIZE; x += 1)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    Color pixelColor = Color.clear;

                    if (distance <= fillRadius)
                    {
                        pixelColor = POINTER_CURSOR_FILL_COLOR;
                    }
                    else if (distance >= ringInnerRadius && distance <= ringOuterRadius)
                    {
                        pixelColor = POINTER_CURSOR_RING_COLOR;
                    }

                    pixels[(y * POINTER_CURSOR_TEXTURE_SIZE) + x] = pixelColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, POINTER_CURSOR_TEXTURE_SIZE, POINTER_CURSOR_TEXTURE_SIZE), new Vector2(0.5f, 0.5f), POINTER_CURSOR_TEXTURE_SIZE);
        }

        private static Sprite createRadialGlowSprite()
        {
            const int textureSize = 256;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, mipChain: false);
            texture.hideFlags = HideFlags.DontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[textureSize * textureSize];
            float center = (textureSize - 1.0f) * 0.5f;
            float radius = textureSize * 0.48f;

            for (int y = 0; y < textureSize; y += 1)
            {
                for (int x = 0; x < textureSize; x += 1)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float normalizedDistance = Mathf.Clamp01(distance / radius);
                    float alpha = Mathf.Pow(1.0f - normalizedDistance, 2.2f);
                    pixels[(y * textureSize) + x] = new Color(1.0f, 1.0f, 1.0f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        }

        private static Sprite createRingGlowSprite()
        {
            const int textureSize = 256;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, mipChain: false);
            texture.hideFlags = HideFlags.DontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[textureSize * textureSize];
            float center = (textureSize - 1.0f) * 0.5f;
            float ringRadius = textureSize * 0.36f;
            float ringThickness = textureSize * 0.12f;

            for (int y = 0; y < textureSize; y += 1)
            {
                for (int x = 0; x < textureSize; x += 1)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float ringDistance = Mathf.Abs(distance - ringRadius) / ringThickness;
                    float alpha = Mathf.Pow(Mathf.Clamp01(1.0f - ringDistance), 1.6f);
                    float coreGlow = Mathf.Pow(Mathf.Clamp01(1.0f - (distance / (textureSize * 0.52f))), 3.0f) * 0.45f;
                    pixels[(y * textureSize) + x] = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(alpha + coreGlow));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        }

        private static Sprite createEyeBeamSprite(EEyeBeamSourceSide eyeBeamSourceSide)
        {
            const int textureWidth = 192;
            const int textureHeight = 256;
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, mipChain: false);
            texture.hideFlags = HideFlags.DontSave;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Color[] pixels = new Color[textureWidth * textureHeight];
            float sourceCenterX = eyeBeamSourceSide == EEyeBeamSourceSide.Right ? 0.52f : 0.48f;

            for (int y = 0; y < textureHeight; y += 1)
            {
                for (int x = 0; x < textureWidth; x += 1)
                {
                    float normalizedX = x / (textureWidth - 1.0f);
                    float downProgress = 1.0f - (y / (textureHeight - 1.0f));
                    float halfWidth = Mathf.Lerp(0.018f, 0.50f, Mathf.Pow(downProgress, 0.88f));
                    float horizontalDistance = Mathf.Abs(normalizedX - sourceCenterX) / halfWidth;
                    float edgeFade = Mathf.Pow(Mathf.Clamp01(1.0f - horizontalDistance), 0.72f);
                    float verticalFade = Mathf.Lerp(1.0f, 0.76f, downProgress);
                    float sourceFlare = Mathf.Pow(Mathf.Clamp01(1.0f - (downProgress * 7.5f)), 2.0f);
                    float alpha = horizontalDistance <= 1.0f ? (edgeFade * verticalFade) + (sourceFlare * 0.62f) : 0.0f;
                    pixels[(y * textureWidth) + x] = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(alpha));
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(updateMipmaps: false, makeNoLongerReadable: true);
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, textureWidth, textureHeight), new Vector2(0.5f, 0.5f), textureWidth);
        }
    }
}
