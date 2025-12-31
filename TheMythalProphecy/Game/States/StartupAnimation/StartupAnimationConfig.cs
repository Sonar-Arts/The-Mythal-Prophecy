using Microsoft.Xna.Framework;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Configuration constants for the startup animation
/// </summary>
public static class StartupAnimationConfig
{
    // Phase durations (seconds)
    public const float SubmarineDuration = 2.5f;
    public const float FlashDuration = 2.0f;
    public const float AirshipDuration = 3.5f;
    public const float TextRevealDuration = 1.2f;
    public const float HoldDuration = 2.5f;
    public const float FadeOutDuration = 1.5f;

    // Sonar timing
    public const float SonarSpawnInterval = 0.6f;
    public const float SonarBaseSpeed = 200f;
    public const float SonarSpeedVariance = 50f;
    public const float SonarInitialRadius = 30f;

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
