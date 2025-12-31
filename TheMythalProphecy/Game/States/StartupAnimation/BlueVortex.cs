using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Renders a gentle swirling blue vortex effect during the flash transition
/// with an initial starlight flash burst
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

    // Starlight flash colors
    private static readonly Color StarFlashCore = new(220, 240, 255);
    private static readonly Color StarFlashMid = new(150, 200, 255);
    private static readonly Color StarFlashOuter = new(80, 140, 220);

    // Starlight flash timing
    private const float StarFlashDuration = 0.35f;
    private const float StarFlashPeakTime = 0.08f; // Very quick peak

    public BlueVortex(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _center = new Vector2(screenWidth / 2f, screenHeight / 2f);
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float phaseElapsed, float opacity,
        float phaseDuration = 2.0f)
    {
        if (opacity <= 0) return;

        // Calculate if we're in the end flash window
        float endFlashStart = phaseDuration - StarFlashDuration;
        bool inEndFlash = phaseElapsed >= endFlashStart;
        float endFlashElapsed = inEndFlash ? phaseElapsed - endFlashStart : 0f;

        // During end flash, fade out the vortex as the flash takes over
        float vortexOpacity = opacity;
        if (inEndFlash)
        {
            float endFlashProgress = endFlashElapsed / StarFlashDuration;
            // Fade vortex out as flash progresses
            vortexOpacity = opacity * (1f - endFlashProgress * endFlashProgress);
        }

        float time = phaseElapsed * 0.5f; // Slow, gentle animation
        float maxRadius = MathF.Sqrt(_screenWidth * _screenWidth + _screenHeight * _screenHeight) / 2f + 50;

        // Draw vortex elements (fade out during end flash)
        if (vortexOpacity > 0.01f)
        {
            // Base layer - deep void black
            renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
                VoidBlack * vortexOpacity);

            // Secondary layer - deep space
            renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
                DeepSpace * (vortexOpacity * 0.7f));

            // Gentle layered glow rings - creates depth
            DrawSoftGlowLayers(spriteBatch, renderer, time, maxRadius, vortexOpacity);

            // Subtle flowing waves
            DrawFlowingWaves(spriteBatch, renderer, time, maxRadius, vortexOpacity);

            // Mystical center glow
            float coreSize = 180f + MathF.Sin(time * 1.5f) * 15f;
            for (int i = 4; i >= 0; i--)
            {
                float glowRadius = coreSize + i * 70f;
                float glowOpacity = vortexOpacity * (0.08f - i * 0.012f);
                renderer.DrawFilledCircle(spriteBatch, _center, glowRadius, StarGlow * glowOpacity);
            }
        }

        // Draw starlight flash on top at the beginning of the phase
        if (phaseElapsed < StarFlashDuration)
        {
            DrawStarlightFlash(spriteBatch, renderer, phaseElapsed, opacity, maxRadius);
        }

        // Draw starlight flash at the end of the phase
        if (inEndFlash)
        {
            DrawStarlightFlash(spriteBatch, renderer, endFlashElapsed, opacity, maxRadius);
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

    private void DrawStarlightFlash(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float phaseElapsed, float opacity, float maxRadius)
    {
        // Calculate flash intensity - quick peak then fade
        float flashProgress = phaseElapsed / StarFlashDuration;
        float flashIntensity;

        if (phaseElapsed < StarFlashPeakTime)
        {
            // Rapid rise to peak
            flashIntensity = phaseElapsed / StarFlashPeakTime;
            flashIntensity = flashIntensity * flashIntensity; // Accelerate in
        }
        else
        {
            // Smooth decay
            float decayProgress = (phaseElapsed - StarFlashPeakTime) / (StarFlashDuration - StarFlashPeakTime);
            flashIntensity = 1f - decayProgress;
            flashIntensity = flashIntensity * flashIntensity; // Ease out
        }

        flashIntensity *= opacity;

        if (flashIntensity <= 0.01f) return;

        // Bright core flash - covers whole screen initially
        float coreOpacity = flashIntensity * 0.9f;
        renderer.DrawRectangle(spriteBatch, new Rectangle(0, 0, _screenWidth, _screenHeight),
            StarFlashCore * coreOpacity);

        // Central bright glow
        float coreRadius = 200f + flashIntensity * 150f;
        for (int i = 0; i < 4; i++)
        {
            float glowRadius = coreRadius + i * 80f;
            float glowOpacity = flashIntensity * (0.6f - i * 0.12f);
            renderer.DrawFilledCircle(spriteBatch, _center, glowRadius, StarFlashCore * glowOpacity);
        }

        // Star rays - radiating from center
        int rayCount = 12;
        float rayExpansion = flashProgress * maxRadius * 1.5f; // Rays shoot outward

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i / (float)rayCount) * MathF.PI * 2f;

            // Alternating long and short rays for star effect
            bool isLongRay = i % 2 == 0;
            float rayLength = isLongRay ? rayExpansion : rayExpansion * 0.6f;
            float rayWidth = isLongRay ? 8f : 5f;

            // Ray fades as it extends
            float rayOpacity = flashIntensity * (isLongRay ? 0.7f : 0.5f);

            Vector2 rayDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 rayStart = _center + rayDir * 20f;
            Vector2 rayEnd = _center + rayDir * rayLength;

            // Draw ray with gradient (thick at center, thin at end)
            DrawTaperedRay(spriteBatch, renderer, rayStart, rayEnd, rayWidth, rayOpacity);
        }

        // Secondary smaller rays between main rays
        for (int i = 0; i < rayCount; i++)
        {
            float angle = ((i + 0.5f) / rayCount) * MathF.PI * 2f;
            float rayLength = rayExpansion * 0.35f;

            float rayOpacity = flashIntensity * 0.3f;

            Vector2 rayDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 rayStart = _center + rayDir * 30f;
            Vector2 rayEnd = _center + rayDir * rayLength;

            DrawTaperedRay(spriteBatch, renderer, rayStart, rayEnd, 3f, rayOpacity);
        }

        // Sparkle points at ray tips (for long rays)
        for (int i = 0; i < rayCount; i += 2)
        {
            float angle = (i / (float)rayCount) * MathF.PI * 2f;
            float rayLength = rayExpansion;

            Vector2 rayDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
            Vector2 sparklePos = _center + rayDir * rayLength;

            // Only draw sparkles after rays have extended a bit
            if (rayLength > 100f)
            {
                float sparkleOpacity = flashIntensity * 0.8f;
                float sparkleSize = 6f + MathF.Sin(phaseElapsed * 20f + i) * 2f;
                renderer.DrawFilledCircle(spriteBatch, sparklePos, sparkleSize, StarFlashCore * sparkleOpacity);
            }
        }
    }

    private void DrawTaperedRay(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        Vector2 start, Vector2 end, float maxWidth, float opacity)
    {
        // Draw multiple segments with decreasing width
        int segments = 8;
        Vector2 direction = end - start;
        float length = direction.Length();

        if (length < 1f) return;

        direction /= length;

        for (int i = 0; i < segments; i++)
        {
            float t1 = i / (float)segments;
            float t2 = (i + 1) / (float)segments;

            Vector2 p1 = start + direction * (length * t1);
            Vector2 p2 = start + direction * (length * t2);

            // Width tapers from maxWidth to 1
            float width = MathHelper.Lerp(maxWidth, 1f, (t1 + t2) / 2f);

            // Opacity fades toward the end
            float segmentOpacity = opacity * (1f - t1 * 0.7f);

            // Color transitions from core to outer
            Color segmentColor = Color.Lerp(StarFlashCore, StarFlashMid, t1);

            renderer.DrawLine(spriteBatch, p1, p2, segmentColor * segmentOpacity, (int)MathF.Ceiling(width));
        }
    }
}
