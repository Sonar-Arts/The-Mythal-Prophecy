using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TheMythalProphecy.Game.States.StartupAnimation.StartupAnimationConfig;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// A bird that flies across the screen with flapping wings
/// </summary>
public class Bird
{
    private float _x;
    private readonly float _y;
    private readonly float _speed;
    private readonly float _scale;
    private readonly float _flapSpeed;
    private readonly float _flapOffset;
    private readonly int _screenWidth;
    private float _time;

    private static readonly Color BirdColor = new(40, 35, 30);

    public bool IsExpired => _x > _screenWidth + S(30);

    public Bird(int screenWidth, int screenHeight, bool randomX = false)
    {
        _screenWidth = screenWidth;

        // Start from left side (ship speeding past them westward, birds appear to drift right)
        _x = randomX ? Random.Shared.NextSingle() * screenWidth : -S(20);

        // Random Y - birds fly in upper/middle area
        _y = Random.Shared.NextSingle() * screenHeight * 0.5f + screenHeight * 0.15f;

        // Birds move slower than clouds (ship is faster than birds) - scaled
        _speed = S(15f) + Random.Shared.NextSingle() * S(20f);

        // Small birds
        _scale = 0.6f + Random.Shared.NextSingle() * 0.5f;

        // Wing flap timing
        _flapSpeed = 8f + Random.Shared.NextSingle() * 4f;
        _flapOffset = Random.Shared.NextSingle() * MathF.PI * 2;

        _time = 0f;
    }

    public void Update(float deltaTime)
    {
        _x += _speed * deltaTime;
        _time += deltaTime;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        // Wing flap animation (scaled)
        float flapAngle = MathF.Sin(_time * _flapSpeed + _flapOffset);
        float wingY = flapAngle * S(4) * _scale;

        float size = S(8) * _scale;

        // Body (small circle)
        renderer.DrawFilledCircle(spriteBatch, new Vector2(_x, _y), size * 0.4f, BirdColor);

        // Left wing - simple V shape
        Vector2 bodyLeft = new(_x - size * 0.3f, _y);
        Vector2 wingLeftTip = new(_x - size * 1.5f, _y - wingY - size * 0.3f);
        renderer.DrawLine(spriteBatch, bodyLeft, wingLeftTip, BirdColor, Math.Max(1, Si(2 * _scale)));

        // Right wing
        Vector2 bodyRight = new(_x + size * 0.3f, _y);
        Vector2 wingRightTip = new(_x + size * 1.5f, _y - wingY - size * 0.3f);
        renderer.DrawLine(spriteBatch, bodyRight, wingRightTip, BirdColor, Math.Max(1, Si(2 * _scale)));
    }
}
