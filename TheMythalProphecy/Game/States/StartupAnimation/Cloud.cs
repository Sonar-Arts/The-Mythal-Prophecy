using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// A cloud that drifts across the screen
/// </summary>
public class Cloud
{
    private float _x;
    private readonly float _y;
    private readonly float _speed;
    private readonly float _scale;
    private readonly float _alpha;
    private readonly int _screenWidth;

    private static readonly Color CloudColor = new(255, 255, 255);
    private static readonly Color CloudShadow = new(220, 225, 235);

    public bool IsExpired => _x > _screenWidth + 150;

    public Cloud(int screenWidth, int screenHeight, bool randomX = false)
    {
        _screenWidth = screenWidth;

        // Start from left side (ship moving west, clouds appear to drift east/right)
        _x = randomX ? Random.Shared.NextSingle() * (screenWidth + 200) - 100 : -100;

        // Random Y in upper portion of screen
        _y = Random.Shared.NextSingle() * screenHeight * 0.6f + screenHeight * 0.1f;

        // Varying speeds - clouds in back move slower
        _speed = 30f + Random.Shared.NextSingle() * 40f;

        // Varying sizes
        _scale = 0.5f + Random.Shared.NextSingle() * 0.8f;

        // Depth-based alpha (slower = further = more transparent)
        _alpha = 0.3f + (_speed - 30f) / 40f * 0.5f;
    }

    public void Update(float deltaTime)
    {
        _x += _speed * deltaTime;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        // Cloud made of overlapping ellipses
        float baseSize = 40 * _scale;
        Color mainColor = CloudColor * _alpha;
        Color shadowColor = CloudShadow * (_alpha * 0.6f);

        // Shadow underneath
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(_x + 5, _y + baseSize * 0.3f),
            baseSize * 1.2f, baseSize * 0.4f, shadowColor);

        // Main cloud body - multiple overlapping ellipses
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(_x - baseSize * 0.5f, _y),
            baseSize * 0.7f, baseSize * 0.5f, mainColor);

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(_x, _y - baseSize * 0.1f),
            baseSize * 0.9f, baseSize * 0.6f, mainColor);

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(_x + baseSize * 0.6f, _y),
            baseSize * 0.75f, baseSize * 0.5f, mainColor);

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(_x + baseSize * 0.3f, _y - baseSize * 0.2f),
            baseSize * 0.6f, baseSize * 0.45f, mainColor);
    }
}
