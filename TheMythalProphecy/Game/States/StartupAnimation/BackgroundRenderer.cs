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

        // Draw clouds if transitioned
        if (transition > 0.5f)
        {
            DrawClouds(spriteBatch, renderer, (transition - 0.5f) * 2f, totalElapsed);
        }
    }

    private void DrawClouds(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float opacity, float totalElapsed)
    {
        float[] cloudXs = { 0.15f, 0.35f, 0.6f, 0.8f, 0.95f };
        float[] cloudYs = { 0.1f, 0.15f, 0.08f, 0.18f, 0.12f };

        for (int i = 0; i < cloudXs.Length; i++)
        {
            float x = _screenWidth * cloudXs[i] + MathF.Sin(totalElapsed * 0.1f + i) * 20;
            float y = _screenHeight * cloudYs[i];

            DrawCloud(spriteBatch, renderer, x, y, 80 + i * 20, opacity * 0.3f);
        }
    }

    private void DrawCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float x, float y, float size, float opacity)
    {
        Color cloudColor = Color.White * opacity;

        // Draw overlapping circles for cloud shape
        float[] offsets = { 0, -0.3f, 0.3f, -0.15f, 0.15f };
        float[] sizes = { 1f, 0.7f, 0.7f, 0.8f, 0.8f };
        float[] yOffsets = { 0, 0.1f, 0.1f, -0.1f, -0.1f };

        for (int i = 0; i < offsets.Length; i++)
        {
            float cx = x + offsets[i] * size;
            float cy = y + yOffsets[i] * size * 0.5f;
            float r = size * sizes[i] * 0.4f;

            renderer.DrawFilledCircle(spriteBatch, new Vector2(cx, cy), r, cloudColor);
        }
    }
}
