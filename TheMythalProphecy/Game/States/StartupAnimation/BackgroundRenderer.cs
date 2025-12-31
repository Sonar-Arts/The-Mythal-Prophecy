using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Renders ocean and sky backgrounds with animated effects
/// </summary>
public class BackgroundRenderer
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    // Sun colors
    private static readonly Color SunCore = new(255, 250, 220);
    private static readonly Color SunGlow = new(255, 220, 150);
    private static readonly Color SunOuter = new(255, 180, 100);
    private static readonly Color SunHaze = new(255, 200, 120);

    // Cloud colors - rich palette for beautiful volumetric clouds
    private static readonly Color CloudBright = new(255, 255, 255);
    private static readonly Color CloudLight = new(250, 252, 255);
    private static readonly Color CloudMid = new(235, 240, 248);
    private static readonly Color CloudShadow = new(200, 210, 225);
    private static readonly Color CloudDeep = new(170, 185, 210);
    private static readonly Color CloudDark = new(140, 160, 190);
    private static readonly Color CloudAmbient = new(220, 230, 245);

    public BackgroundRenderer(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void DrawOceanBackground(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float totalElapsed)
    {
        int bands = 60;
        int bandHeight = _screenHeight / bands + 1;

        for (int i = 0; i < bands; i++)
        {
            float t = (float)i / bands;
            Color color;

            if (t < 0.4f)
            {
                color = Color.Lerp(StartupAnimationConfig.OceanDeep, StartupAnimationConfig.OceanMid, t / 0.4f);
            }
            else
            {
                color = Color.Lerp(StartupAnimationConfig.OceanMid, StartupAnimationConfig.OceanSurface, (t - 0.4f) / 0.6f);
            }

            // Add subtle wave animation
            float wave = MathF.Sin(totalElapsed * 0.5f + i * 0.1f) * 3;
            int y = i * bandHeight + (int)wave;

            renderer.DrawRectangle(spriteBatch, new Rectangle(0, y, _screenWidth, bandHeight + 2), color);
        }

        // Add caustic light rays
        DrawCaustics(spriteBatch, renderer, totalElapsed);
    }

    private void DrawCaustics(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float totalElapsed)
    {
        for (int i = 0; i < 5; i++)
        {
            float x = (_screenWidth * 0.1f) + i * (_screenWidth * 0.2f);
            float wave = MathF.Sin(totalElapsed * 0.3f + i * 1.5f) * 30;
            x += wave;

            float opacity = 0.03f + MathF.Sin(totalElapsed * 0.5f + i) * 0.02f;

            // Draw subtle light rays
            for (int j = 0; j < _screenHeight; j += 4)
            {
                float rayWidth = 20 + MathF.Sin(j * 0.01f + totalElapsed) * 10;
                var rect = new Rectangle((int)(x - rayWidth / 2), j, (int)rayWidth, 4);
                renderer.DrawRectangle(spriteBatch, rect, Color.White * opacity);
            }
        }
    }

    public void DrawSkyBackground(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float transition, float totalElapsed)
    {
        int bands = 60;
        int bandHeight = _screenHeight / bands + 1;

        for (int i = 0; i < bands; i++)
        {
            float t = (float)i / bands;
            Color skyColor;

            if (t < 0.3f)
            {
                skyColor = Color.Lerp(StartupAnimationConfig.SkyTop, StartupAnimationConfig.SkyMid, t / 0.3f);
            }
            else
            {
                skyColor = Color.Lerp(StartupAnimationConfig.SkyMid, StartupAnimationConfig.SkyBottom, (t - 0.3f) / 0.7f);
            }

            // Blend from ocean to sky
            Color oceanColor;
            if (t < 0.4f)
            {
                oceanColor = Color.Lerp(StartupAnimationConfig.OceanDeep, StartupAnimationConfig.OceanMid, t / 0.4f);
            }
            else
            {
                oceanColor = Color.Lerp(StartupAnimationConfig.OceanMid, StartupAnimationConfig.OceanSurface, (t - 0.4f) / 0.6f);
            }

            Color finalColor = Color.Lerp(oceanColor, skyColor, transition);

            int y = i * bandHeight;
            renderer.DrawRectangle(spriteBatch, new Rectangle(0, y, _screenWidth, bandHeight + 1), finalColor);
        }

        // Draw elements if transitioned to sky
        if (transition > 0.3f)
        {
            float skyOpacity = (transition - 0.3f) / 0.7f;

            // Draw two suns high in the sky
            DrawTwinSuns(spriteBatch, renderer, totalElapsed, skyOpacity);

            // Draw beautiful layered background clouds
            DrawBackgroundClouds(spriteBatch, renderer, skyOpacity, totalElapsed);
        }
    }

    private void DrawTwinSuns(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float totalElapsed, float opacity)
    {
        // Two suns close together - binary star system
        // They orbit slowly around each other

        // Center point for the binary system
        float centerX = _screenWidth * 0.65f;
        float centerY = _screenHeight * 0.28f;

        // Slow orbital motion
        float orbitAngle = totalElapsed * 0.05f;
        float orbitRadius = 45f;

        // Primary sun - larger
        float sun1X = centerX + MathF.Cos(orbitAngle) * orbitRadius;
        float sun1Y = centerY + MathF.Sin(orbitAngle) * orbitRadius * 0.4f;
        float sun1Radius = 30f;

        DrawSun(spriteBatch, renderer, sun1X, sun1Y, sun1Radius, opacity);

        // Secondary sun - smaller, opposite side of orbit
        float sun2X = centerX + MathF.Cos(orbitAngle + MathF.PI) * orbitRadius;
        float sun2Y = centerY + MathF.Sin(orbitAngle + MathF.PI) * orbitRadius * 0.4f;
        float sun2Radius = 20f;

        DrawSun(spriteBatch, renderer, sun2X, sun2Y, sun2Radius, opacity * 0.85f);
    }

    private void DrawSun(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float x, float y, float radius, float opacity)
    {
        Vector2 pos = new Vector2(x, y);

        // Outer haze glow (very large, very faint)
        for (int i = 5; i >= 0; i--)
        {
            float hazeRadius = radius * (3.5f + i * 0.8f);
            float hazeOpacity = opacity * (0.03f - i * 0.004f);
            renderer.DrawFilledCircle(spriteBatch, pos, hazeRadius, SunHaze * hazeOpacity);
        }

        // Outer glow layers
        for (int i = 3; i >= 0; i--)
        {
            float glowRadius = radius * (1.8f + i * 0.4f);
            float glowOpacity = opacity * (0.12f - i * 0.025f);
            renderer.DrawFilledCircle(spriteBatch, pos, glowRadius, SunOuter * glowOpacity);
        }

        // Inner glow
        renderer.DrawFilledCircle(spriteBatch, pos, radius * 1.4f, SunGlow * (opacity * 0.35f));
        renderer.DrawFilledCircle(spriteBatch, pos, radius * 1.15f, SunGlow * (opacity * 0.5f));

        // Sun core
        renderer.DrawFilledCircle(spriteBatch, pos, radius, SunCore * opacity);

        // Bright center highlight
        renderer.DrawFilledCircle(spriteBatch, pos - new Vector2(radius * 0.15f, radius * 0.15f),
            radius * 0.4f, Color.White * (opacity * 0.4f));
    }

    private void DrawBackgroundClouds(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float opacity, float totalElapsed)
    {
        // Beautiful layered cloudscape with multiple depth layers
        // Creates a rich, volumetric sky scene

        float drift = totalElapsed * 2f;

        // Layer 1: Distant hazy clouds (furthest back, most transparent)
        DrawDistantCloudLayer(spriteBatch, renderer, drift * 0.2f, opacity * 0.25f);

        // Layer 2: Mid-distance large cumulus formations
        DrawMidLayerClouds(spriteBatch, renderer, drift * 0.4f, opacity * 0.5f, totalElapsed);

        // Layer 3: Closer volumetric clouds with detail
        DrawDetailedClouds(spriteBatch, renderer, drift * 0.6f, opacity * 0.7f, totalElapsed);

        // Layer 4: Foreground wisps and highlights
        DrawForegroundWisps(spriteBatch, renderer, drift * 0.8f, opacity * 0.4f, totalElapsed);
    }

    private void DrawDistantCloudLayer(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float drift, float opacity)
    {
        // Very large, soft, hazy clouds in the distance
        float[] cloudXPositions = { 0.1f, 0.35f, 0.6f, 0.85f };
        float[] cloudYPositions = { 0.55f, 0.62f, 0.58f, 0.65f };
        float[] cloudSizes = { 280, 320, 260, 300 };

        for (int i = 0; i < cloudXPositions.Length; i++)
        {
            float x = ((_screenWidth * cloudXPositions[i] + drift * (0.3f + i * 0.1f)) % (_screenWidth * 1.4f)) - _screenWidth * 0.2f;
            float y = _screenHeight * cloudYPositions[i];
            float size = cloudSizes[i];

            // Soft ambient glow
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(x, y),
                size * 1.2f, size * 0.35f, CloudAmbient * (opacity * 0.3f));

            // Main body - very soft
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(x, y),
                size, size * 0.28f, CloudMid * (opacity * 0.5f));
        }
    }

    private void DrawMidLayerClouds(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float drift, float opacity, float time)
    {
        // Medium-distance cumulus clouds with more definition
        DrawVolumCloud(spriteBatch, renderer,
            ((_screenWidth * 0.15f + drift * 0.5f) % (_screenWidth * 1.5f)) - _screenWidth * 0.25f,
            _screenHeight * 0.42f, 180, opacity, time, 0);

        DrawVolumCloud(spriteBatch, renderer,
            ((_screenWidth * 0.5f + drift * 0.4f) % (_screenWidth * 1.5f)) - _screenWidth * 0.25f,
            _screenHeight * 0.48f, 220, opacity * 0.9f, time, 1);

        DrawVolumCloud(spriteBatch, renderer,
            ((_screenWidth * 0.8f + drift * 0.45f) % (_screenWidth * 1.5f)) - _screenWidth * 0.25f,
            _screenHeight * 0.38f, 160, opacity * 0.85f, time, 2);
    }

    private void DrawVolumCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float x, float y, float size, float opacity, float time, int seed)
    {
        // Volumetric cloud with multiple layers for depth
        float breathe = MathF.Sin(time * 0.3f + seed) * 0.03f;
        float sizeMultiplier = 1f + breathe;

        // Deep shadow layer (bottom)
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x + size * 0.08f, y + size * 0.18f),
            size * 1.1f * sizeMultiplier, size * 0.28f, CloudDark * (opacity * 0.4f));

        // Shadow layer
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x + size * 0.04f, y + size * 0.12f),
            size * 1.05f * sizeMultiplier, size * 0.32f, CloudDeep * (opacity * 0.5f));

        // Mid shadow
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x, y + size * 0.06f),
            size * 0.95f * sizeMultiplier, size * 0.35f, CloudShadow * (opacity * 0.6f));

        // Main body - multiple overlapping puffs
        float[] puffOffsetsX = { -0.35f, -0.1f, 0.2f, 0.45f, 0.0f, 0.25f };
        float[] puffOffsetsY = { 0.02f, -0.06f, -0.04f, 0.0f, -0.12f, -0.08f };
        float[] puffSizesW = { 0.4f, 0.55f, 0.5f, 0.35f, 0.45f, 0.38f };
        float[] puffSizesH = { 0.28f, 0.38f, 0.35f, 0.25f, 0.32f, 0.28f };

        for (int i = 0; i < puffOffsetsX.Length; i++)
        {
            float px = x + size * puffOffsetsX[i];
            float py = y + size * puffOffsetsY[i];
            float pw = size * puffSizesW[i] * sizeMultiplier;
            float ph = size * puffSizesH[i] * sizeMultiplier;

            renderer.DrawFilledEllipse(spriteBatch, new Vector2(px, py), pw, ph, CloudMid * opacity);
        }

        // Bright highlights on top
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x - size * 0.15f, y - size * 0.1f),
            size * 0.4f * sizeMultiplier, size * 0.25f, CloudLight * opacity);

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x + size * 0.1f, y - size * 0.12f),
            size * 0.35f * sizeMultiplier, size * 0.22f, CloudLight * opacity);

        // Sun-kissed bright spots
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x - size * 0.08f, y - size * 0.14f),
            size * 0.25f, size * 0.15f, CloudBright * (opacity * 0.8f));

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x + size * 0.15f, y - size * 0.1f),
            size * 0.2f, size * 0.12f, CloudBright * (opacity * 0.7f));
    }

    private void DrawDetailedClouds(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float drift, float opacity, float time)
    {
        // Closer clouds with more detail and definition
        DrawHighDefCloud(spriteBatch, renderer,
            ((_screenWidth * 0.25f + drift * 0.6f) % (_screenWidth * 1.6f)) - _screenWidth * 0.3f,
            _screenHeight * 0.28f, 140, opacity, time, 0);

        DrawHighDefCloud(spriteBatch, renderer,
            ((_screenWidth * 0.65f + drift * 0.55f) % (_screenWidth * 1.6f)) - _screenWidth * 0.3f,
            _screenHeight * 0.22f, 170, opacity * 0.95f, time, 1);

        DrawHighDefCloud(spriteBatch, renderer,
            ((_screenWidth * 0.05f + drift * 0.65f) % (_screenWidth * 1.6f)) - _screenWidth * 0.3f,
            _screenHeight * 0.35f, 120, opacity * 0.9f, time, 2);

        DrawHighDefCloud(spriteBatch, renderer,
            ((_screenWidth * 0.9f + drift * 0.5f) % (_screenWidth * 1.6f)) - _screenWidth * 0.3f,
            _screenHeight * 0.3f, 150, opacity * 0.85f, time, 3);
    }

    private void DrawHighDefCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float x, float y, float size, float opacity, float time, int seed)
    {
        // High definition cloud with many layers
        float pulse = MathF.Sin(time * 0.4f + seed * 1.5f) * 0.02f;
        float sz = size * (1f + pulse);

        // Ambient glow
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x, y + sz * 0.1f),
            sz * 1.3f, sz * 0.4f, CloudAmbient * (opacity * 0.25f));

        // Deep core shadow
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x + sz * 0.1f, y + sz * 0.2f),
            sz * 0.9f, sz * 0.3f, CloudDark * (opacity * 0.5f));

        // Shadow gradient layers
        for (int i = 3; i >= 0; i--)
        {
            float layerY = y + sz * (0.15f - i * 0.03f);
            float layerOffset = sz * (0.08f - i * 0.02f);
            Color layerColor = Color.Lerp(CloudDeep, CloudShadow, i / 3f);

            renderer.DrawFilledEllipse(spriteBatch,
                new Vector2(x + layerOffset, layerY),
                sz * (0.95f - i * 0.05f), sz * (0.32f + i * 0.02f),
                layerColor * (opacity * (0.4f + i * 0.1f)));
        }

        // Fluffy puff details - many small overlapping shapes
        int puffCount = 10;
        for (int i = 0; i < puffCount; i++)
        {
            float angle = (i / (float)puffCount) * MathF.PI * 1.5f - MathF.PI * 0.25f;
            float dist = sz * (0.2f + (i % 3) * 0.12f);
            float puffX = x + MathF.Cos(angle) * dist;
            float puffY = y + MathF.Sin(angle) * dist * 0.4f - sz * 0.05f;
            float puffW = sz * (0.25f + (i % 4) * 0.08f);
            float puffH = sz * (0.18f + (i % 3) * 0.05f);

            Color puffColor = (i < puffCount / 2) ? CloudMid : CloudLight;
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(puffX, puffY), puffW, puffH, puffColor * opacity);
        }

        // Bright crown
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x, y - sz * 0.12f),
            sz * 0.5f, sz * 0.28f, CloudLight * opacity);

        // Sunlit peaks
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x - sz * 0.12f, y - sz * 0.18f),
            sz * 0.28f, sz * 0.16f, CloudBright * opacity);

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x + sz * 0.08f, y - sz * 0.15f),
            sz * 0.22f, sz * 0.14f, CloudBright * (opacity * 0.9f));

        // Tiny bright accents
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(x - sz * 0.05f, y - sz * 0.2f),
            sz * 0.12f, sz * 0.08f, Color.White * (opacity * 0.6f));
    }

    private void DrawForegroundWisps(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float drift, float opacity, float time)
    {
        // Thin wispy clouds in the foreground for depth
        float[] wispXs = { 0.1f, 0.3f, 0.55f, 0.75f, 0.95f };

        for (int i = 0; i < wispXs.Length; i++)
        {
            float x = ((_screenWidth * wispXs[i] + drift * (0.7f + i * 0.1f)) % (_screenWidth * 1.3f)) - _screenWidth * 0.15f;
            float y = _screenHeight * (0.15f + (i % 3) * 0.08f);

            float wispWidth = 100 + (i % 3) * 40;
            float wispHeight = 12 + (i % 2) * 6;

            // Soft wisp
            renderer.DrawFilledEllipse(spriteBatch,
                new Vector2(x, y),
                wispWidth, wispHeight,
                CloudLight * (opacity * 0.5f));

            // Brighter center
            renderer.DrawFilledEllipse(spriteBatch,
                new Vector2(x, y - wispHeight * 0.1f),
                wispWidth * 0.6f, wispHeight * 0.6f,
                CloudBright * (opacity * 0.3f));
        }

        // Add some tiny floating puffs
        for (int i = 0; i < 6; i++)
        {
            float x = ((_screenWidth * (0.08f + i * 0.17f) + drift * (0.9f + i * 0.05f)) % (_screenWidth * 1.2f)) - _screenWidth * 0.1f;
            float y = _screenHeight * (0.08f + (i % 4) * 0.06f);
            float puffSize = 15 + (i % 3) * 10;

            float wobble = MathF.Sin(time * 0.5f + i * 1.2f) * 3f;

            renderer.DrawFilledEllipse(spriteBatch,
                new Vector2(x, y + wobble),
                puffSize, puffSize * 0.6f,
                CloudBright * (opacity * 0.4f));
        }
    }
}
