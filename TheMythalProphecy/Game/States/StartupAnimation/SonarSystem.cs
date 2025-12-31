using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Unified sonar system that manages expanding rings and color transitions
/// Rings persist through the animation, transitioning from green to purple
/// </summary>
public class SonarSystem
{
    private readonly List<SonarRing> _rings = new();
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private readonly float _maxRadius;
    private float _spawnTimer;
    private float _colorTransition;

    public Vector2 Center { get; }

    public SonarSystem(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _maxRadius = MathF.Max(screenWidth, screenHeight) * 0.8f;
        Center = new Vector2(screenWidth * 0.5f, screenHeight * 0.5f);
        _colorTransition = 0f;
    }

    /// <summary>
    /// Updates ring animations. Set useInternalSpawning to false to control spawning externally.
    /// </summary>
    public void Update(float deltaTime, bool useInternalSpawning = true)
    {
        // Spawn new rings (can be disabled for external control)
        if (useInternalSpawning)
        {
            _spawnTimer += deltaTime;
            if (_spawnTimer >= StartupAnimationConfig.SonarSpawnInterval)
            {
                _spawnTimer = 0f;
                SpawnRing();
            }
        }

        // Update existing rings
        for (int i = _rings.Count - 1; i >= 0; i--)
        {
            _rings[i].Update(deltaTime);
            _rings[i].SetColorTransition(_colorTransition);

            if (_rings[i].IsExpired)
            {
                _rings.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Manually spawn a sonar ring. Use this for syncing with audio.
    /// </summary>
    public void SpawnRing()
    {
        var ring = new SonarRing(
            Center,
            StartupAnimationConfig.SonarGreen,
            StartupAnimationConfig.SonarPurple,
            _maxRadius
        );
        ring.SetColorTransition(_colorTransition);
        _rings.Add(ring);
    }

    /// <summary>
    /// Set the color transition (0 = green, 1 = purple)
    /// </summary>
    public void SetColorTransition(float t)
    {
        _colorTransition = MathHelper.Clamp(t, 0f, 1f);

        // Update all existing rings
        foreach (var ring in _rings)
        {
            ring.SetColorTransition(_colorTransition);
        }
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        foreach (var ring in _rings)
        {
            ring.Draw(spriteBatch, renderer);
        }
    }

    /// <summary>
    /// Get the largest ring radius (for text reveal effect)
    /// </summary>
    public float GetLargestRadius()
    {
        float maxRadius = 0f;
        foreach (var ring in _rings)
        {
            if (ring.Radius > maxRadius)
                maxRadius = ring.Radius;
        }
        return maxRadius;
    }
}
