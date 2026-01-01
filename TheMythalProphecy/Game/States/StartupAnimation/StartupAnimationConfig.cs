using System;
using Microsoft.Xna.Framework;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Configuration constants for the startup animation
/// </summary>
public static class StartupAnimationConfig
{
    // Reference resolution - all pixel values are designed for this resolution
    public const float ReferenceWidth = 1920f;
    public const float ReferenceHeight = 1080f;

    // Scale factor - set at runtime based on actual screen resolution
    private static float _scale = 1f;

    /// <summary>
    /// Current scale factor relative to reference resolution (1920x1080)
    /// </summary>
    public static float Scale => _scale;

    /// <summary>
    /// Initialize the scale factor based on current screen dimensions.
    /// Call this once at the start of the animation.
    /// </summary>
    public static void InitializeScale(int screenWidth, int screenHeight)
    {
        // Use the smaller scale to maintain aspect ratio and fit everything on screen
        float scaleX = screenWidth / ReferenceWidth;
        float scaleY = screenHeight / ReferenceHeight;
        _scale = MathF.Min(scaleX, scaleY);
    }

    /// <summary>
    /// Scale a pixel value from reference resolution to current resolution
    /// </summary>
    public static float S(float value) => value * _scale;

    /// <summary>
    /// Scale an integer pixel value from reference resolution to current resolution.
    /// Uses rounding instead of truncation to prevent gaps between adjacent primitives.
    /// </summary>
    public static int Si(float value) => (int)MathF.Round(value * _scale);

    /// <summary>
    /// Scale an integer pixel value, ensuring a minimum of 1 pixel.
    /// Use this for line widths and stroke thicknesses to prevent them from disappearing at low resolutions.
    /// </summary>
    public static int SiMin1(float value) => Math.Max(1, (int)MathF.Round(value * _scale));

    // Phase durations (seconds)
    public const float SubmarineDuration = 2.5f;
    public const float FlashDuration = 2.0f;
    public const float AirshipDuration = 3.5f;
    public const float TextRevealDuration = 1.2f;
    public const float HoldDuration = 2.5f;
    public const float FadeOutDuration = 1.5f;

    // Sonar timing
    public const float SonarSpawnInterval = 0.6f;
    // Sonar speed/radius (scaled at runtime)
    public const float SonarBaseSpeedBase = 200f;
    public const float SonarSpeedVarianceBase = 50f;
    public const float SonarInitialRadiusBase = 30f;

    public static float SonarBaseSpeed => S(SonarBaseSpeedBase);
    public static float SonarSpeedVariance => S(SonarSpeedVarianceBase);
    public static float SonarInitialRadius => S(SonarInitialRadiusBase);

    // Ocean colors
    public static readonly Color OceanDeep = new(8, 24, 48);
    public static readonly Color OceanMid = new(16, 48, 80);
    public static readonly Color OceanSurface = new(32, 80, 120);

    // Sky colors
    public static readonly Color SkyTop = new(70, 130, 180);
    public static readonly Color SkyMid = new(135, 180, 220);
    public static readonly Color SkyBottom = new(200, 220, 240);

    // Sonar colors
    public static readonly Color SonarGreen = new(34, 197, 94);      // Submarine sonar
    public static readonly Color SonarPurple = new(139, 92, 246);    // Airship sonar

    // Entity colors
    public static readonly Color SubmarineColor = new(20, 30, 50);
    public static readonly Color SubmarineHighlight = new(60, 80, 100);
    public static readonly Color AirshipColor = new(40, 50, 70);
    public static readonly Color AirshipHighlight = new(80, 100, 130);

    // Text colors
    public static readonly Color TextColor = new(255, 255, 255);
    public static readonly Color TextGlow = new(180, 140, 255);

    // Bubble colors
    public static readonly Color BubbleColor = new(150, 200, 255);

    // Flash color
    public static readonly Color FlashColor = new(100, 150, 255);
}
