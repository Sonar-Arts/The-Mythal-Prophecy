using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TheMythalProphecy.Game.States.StartupAnimation.StartupAnimationConfig;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Represents an expanding sonar ring that can transition between colors
/// </summary>
public class SonarRing
{
    public Vector2 Center { get; set; }
    public float Radius { get; set; }
    public float Speed { get; }
    public float Opacity { get; private set; }
    public float MaxRadius { get; }

    private readonly Color _startColor;
    private readonly Color _endColor;
    private float _colorTransition;

    public bool IsExpired => Opacity <= 0;

    public Color CurrentColor => Color.Lerp(_startColor, _endColor, _colorTransition);

    public SonarRing(Vector2 center, Color startColor, Color endColor, float maxRadius)
    {
        Center = center;
        _startColor = startColor;
        _endColor = endColor;
        _colorTransition = 0f;
        MaxRadius = maxRadius;
        Radius = SonarInitialRadius;
        Speed = SonarBaseSpeed + Random.Shared.NextSingle() * SonarSpeedVariance;
        Opacity = 1f;
    }

    public void Update(float deltaTime)
    {
        Radius += Speed * deltaTime;
        // Super aggressive fade - fully gone by 30% of max radius
        float fadeProgress = Radius / (MaxRadius * 0.3f);
        Opacity = MathHelper.Clamp(1f - fadeProgress, 0f, 1f);
    }

    /// <summary>
    /// Set the color transition amount (0 = start color, 1 = end color)
    /// </summary>
    public void SetColorTransition(float t)
    {
        _colorTransition = MathHelper.Clamp(t, 0f, 1f);
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        Color ringColor = CurrentColor * Opacity;
        renderer.DrawCircleOutline(spriteBatch, Center, Radius, ringColor, SiMin1(3));

        // Inner glow for more visual impact (scaled offset)
        if (Opacity > 0.5f)
        {
            Color glowColor = CurrentColor * ((Opacity - 0.5f) * 0.5f);
            renderer.DrawCircleOutline(spriteBatch, Center, Radius - S(5), glowColor, SiMin1(2));
        }
    }
}
