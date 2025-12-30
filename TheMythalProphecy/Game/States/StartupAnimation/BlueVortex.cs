using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Renders a gentle swirling blue vortex effect during the flash transition
/// </summary>
public class BlueVortex
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private readonly Vector2 _center;

    // Mystical space palette - dark blues and blacks
    private static readonly Color VoidBlack = new(8, 12, 24);
    private static readonly Color DeepSpace = new(15, 25, 50);
    private static readonly Color CosmicBlue = new(30, 50, 90);
    private static readonly Color MysticBlue = new(60, 90, 140);
    private static readonly Color StarGlow = new(100, 140, 200);

    public BlueVortex(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _center = new Vector2(screenWidth / 2f, screenHeight / 2f);
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float phaseElapsed, float opacity)
    {
        if (opacity <= 0) return;

        float time = phaseElapsed * 0.5f; // Slow, gentle animation
        float maxRadius = MathF.Sqrt(_screenWidth * _screenWidth + _screenHeight * _screenHeight) / 2f + 50;

        // Base layer - deep void black
        renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
            VoidBlack * opacity);

        // Secondary layer - deep space
        renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
            DeepSpace * (opacity * 0.7f));

        // Gentle layered glow rings - creates depth
        DrawSoftGlowLayers(spriteBatch, renderer, time, maxRadius, opacity);

        // Subtle flowing waves
        DrawFlowingWaves(spriteBatch, renderer, time, maxRadius, opacity);

        // Mystical center glow
        float coreSize = 180f + MathF.Sin(time * 1.5f) * 15f;
        for (int i = 4; i >= 0; i--)
        {
            float glowRadius = coreSize + i * 70f;
            float glowOpacity = opacity * (0.08f - i * 0.012f);
            renderer.DrawFilledCircle(spriteBatch, _center, glowRadius, StarGlow * glowOpacity);
        }
    }

    private void DrawSoftGlowLayers(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float time, float maxRadius, float opacity)
    {
        // Large soft rotating layers
        int layerCount = 5;
        for (int i = 0; i < layerCount; i++)
        {
            float layerProgress = i / (float)layerCount;
            float baseRadius = maxRadius * (0.3f + layerProgress * 0.6f);

            // Very slow rotation, alternating directions
            float rotation = time * (0.3f + i * 0.1f) * (i % 2 == 0 ? 1 : -1);

            // Gentle breathing
            float breath = MathF.Sin(time * 1.2f + i * 0.8f) * 15f;
            float radius = baseRadius + breath;

            // Blend from cosmic blue to mystic blue
            Color layerColor = Color.Lerp(CosmicBlue, MysticBlue, layerProgress);
            float layerOpacity = opacity * 0.12f;

            DrawSoftRing(spriteBatch, renderer, radius, rotation, layerColor * layerOpacity);
        }
    }

    private void DrawSoftRing(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float radius, float rotation, Color color)
    {
        int segments = 36;

        for (int i = 0; i < segments; i++)
        {
            float segmentAngle = (i / (float)segments) * MathF.PI * 2f;

            // Soft fade pattern instead of hard gaps
            float fadePattern = (MathF.Sin(segmentAngle * 2f + rotation) * 0.5f + 0.5f);
            fadePattern = fadePattern * fadePattern; // Smooth curve

            float angle = segmentAngle + rotation;
            float nextAngle = ((i + 1) / (float)segments) * MathF.PI * 2f + rotation;

            Vector2 p1 = _center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            Vector2 p2 = _center + new Vector2(MathF.Cos(nextAngle), MathF.Sin(nextAngle)) * radius;

            renderer.DrawLine(spriteBatch, p1, p2, color * fadePattern, 4);
        }
    }

    private void DrawFlowingWaves(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float time, float maxRadius, float opacity)
    {
        // Gentle spiral waves
        int waveCount = 3;
        for (int wave = 0; wave < waveCount; wave++)
        {
            float waveAngle = (wave / (float)waveCount) * MathF.PI * 2f;
            DrawSingleWave(spriteBatch, renderer, time, waveAngle, maxRadius, opacity);
        }
    }

    private void DrawSingleWave(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float time, float baseAngle, float maxRadius, float opacity)
    {
        int points = 25;
        float spiralSpeed = time * 0.4f;

        for (int i = 0; i < points; i++)
        {
            float t = i / (float)points;
            float radius = maxRadius * (0.2f + t * 0.7f);
            float spiralAngle = baseAngle + spiralSpeed + t * MathF.PI * 1.5f;

            // Soft size variation
            float size = 8f + MathF.Sin(t * MathF.PI) * 12f;

            // Fade at edges
            float edgeFade = MathF.Sin(t * MathF.PI);
            float waveOpacity = opacity * 0.08f * edgeFade;

            Vector2 pos = _center + new Vector2(MathF.Cos(spiralAngle), MathF.Sin(spiralAngle)) * radius;

            renderer.DrawFilledCircle(spriteBatch, pos, size, MysticBlue * waveOpacity);
        }
    }
}
