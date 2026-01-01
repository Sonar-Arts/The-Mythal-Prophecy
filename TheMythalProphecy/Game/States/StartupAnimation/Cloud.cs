using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using static TheMythalProphecy.Game.States.StartupAnimation.StartupAnimationConfig;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Cloud types for variety
/// </summary>
public enum CloudType
{
    Cumulus,        // Classic fluffy cloud with volumetric shading
    Cirrus,         // High altitude wispy streaks
    Stratus,        // Wide layered cloud bank
    Cumulonimbus,   // Tall towering cloud with dramatic shading
    Altocumulus     // Mid-level scattered puffs
}

/// <summary>
/// A beautiful high-definition cloud that drifts across the screen
/// with volumetric shading and soft lighting
/// </summary>
public class Cloud
{
    private float _x;
    private readonly float _y;
    private readonly float _speed;
    private readonly float _scale;
    private readonly float _alpha;
    private readonly int _screenWidth;
    private readonly CloudType _type;
    private readonly int _variation;
    private readonly float _seed;
    private float _time;

    // Rich cloud color palette for volumetric effect
    private static readonly Color CloudBright = new(255, 255, 255);
    private static readonly Color CloudLight = new(250, 252, 255);
    private static readonly Color CloudMid = new(240, 244, 252);
    private static readonly Color CloudBase = new(225, 232, 245);
    private static readonly Color CloudShadow = new(195, 208, 230);
    private static readonly Color CloudDeep = new(165, 182, 212);
    private static readonly Color CloudDark = new(140, 160, 195);
    private static readonly Color CloudAmbient = new(210, 220, 240);

    public bool IsExpired => _x > _screenWidth + S(300) * _scale;

    public Cloud(int screenWidth, int screenHeight, bool randomX = false)
    {
        _screenWidth = screenWidth;
        _seed = Random.Shared.NextSingle() * 100f;
        _time = 0f;

        // Randomly select cloud type with weighted distribution
        float typeRoll = Random.Shared.NextSingle();
        _type = typeRoll switch
        {
            < 0.35f => CloudType.Cumulus,
            < 0.50f => CloudType.Cirrus,
            < 0.70f => CloudType.Stratus,
            < 0.85f => CloudType.Altocumulus,
            _ => CloudType.Cumulonimbus
        };
        _variation = Random.Shared.Next(3);

        // Start position (scaled)
        float extraWidth = _type == CloudType.Stratus ? S(400) : S(200);
        _x = randomX ? Random.Shared.NextSingle() * (screenWidth + extraWidth * 2) - extraWidth : -extraWidth;

        // Y position based on cloud type
        float yRange = _type switch
        {
            CloudType.Cirrus => 0.25f,
            CloudType.Stratus => 0.45f,
            CloudType.Cumulonimbus => 0.35f,
            CloudType.Altocumulus => 0.5f,
            _ => 0.45f
        };
        float yOffset = _type switch
        {
            CloudType.Cirrus => 0.05f,
            CloudType.Stratus => 0.2f,
            CloudType.Cumulonimbus => 0.25f,
            CloudType.Altocumulus => 0.15f,
            _ => 0.18f
        };
        _y = Random.Shared.NextSingle() * screenHeight * yRange + screenHeight * yOffset;

        // Speed based on type (scaled)
        float baseSpeed = _type switch
        {
            CloudType.Cirrus => S(15f),
            CloudType.Stratus => S(20f),
            CloudType.Cumulonimbus => S(28f),
            CloudType.Altocumulus => S(35f),
            _ => S(25f)
        };
        _speed = baseSpeed + Random.Shared.NextSingle() * S(20f);

        // Scale based on type (this multiplies the base size)
        float baseScale = _type switch
        {
            CloudType.Cirrus => 1.0f,
            CloudType.Stratus => 1.4f,
            CloudType.Cumulonimbus => 1.2f,
            CloudType.Altocumulus => 0.5f,
            _ => 0.9f
        };
        _scale = baseScale + Random.Shared.NextSingle() * 0.5f;

        // Alpha based on depth
        float minAlpha = _type == CloudType.Cirrus ? 0.25f : 0.45f;
        _alpha = minAlpha + (_speed - S(15f)) / S(40f) * 0.35f;
    }

    public void Update(float deltaTime)
    {
        _x += _speed * deltaTime;
        _time += deltaTime;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        switch (_type)
        {
            case CloudType.Cumulus:
                DrawCumulusCloud(spriteBatch, renderer);
                break;
            case CloudType.Cirrus:
                DrawCirrusCloud(spriteBatch, renderer);
                break;
            case CloudType.Stratus:
                DrawStratusCloud(spriteBatch, renderer);
                break;
            case CloudType.Cumulonimbus:
                DrawCumulonimbusCloud(spriteBatch, renderer);
                break;
            case CloudType.Altocumulus:
                DrawAltocumulusCloud(spriteBatch, renderer);
                break;
        }
    }

    private void DrawCumulusCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float sz = S(70) * _scale;
        float breathe = MathF.Sin(_time * 0.4f + _seed) * 0.015f;
        float s = sz * (1f + breathe);

        // Ambient glow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y + s * 0.08f),
            s * 1.4f, s * 0.45f, CloudAmbient * (_alpha * 0.2f));

        // Deep shadow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.1f, _y + s * 0.22f),
            s * 1.15f, s * 0.32f, CloudDark * (_alpha * 0.4f));

        // Shadow layer
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.06f, _y + s * 0.15f),
            s * 1.1f, s * 0.38f, CloudDeep * (_alpha * 0.5f));

        // Mid shadow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.02f, _y + s * 0.08f),
            s * 1.0f, s * 0.42f, CloudShadow * (_alpha * 0.6f));

        // Main body puffs
        float[] puffX = { -0.4f, -0.15f, 0.15f, 0.4f, -0.05f, 0.25f, -0.25f };
        float[] puffY = { 0.0f, -0.08f, -0.06f, 0.02f, -0.15f, -0.1f, -0.05f };
        float[] puffW = { 0.42f, 0.55f, 0.52f, 0.38f, 0.48f, 0.4f, 0.35f };
        float[] puffH = { 0.32f, 0.42f, 0.4f, 0.28f, 0.35f, 0.32f, 0.28f };

        for (int i = 0; i < puffX.Length; i++)
        {
            renderer.DrawFilledEllipse(spriteBatch,
                new Vector2(_x + s * puffX[i], _y + s * puffY[i]),
                s * puffW[i], s * puffH[i], CloudBase * _alpha);
        }

        // Light layer
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - s * 0.1f, _y - s * 0.1f),
            s * 0.6f, s * 0.35f, CloudMid * _alpha);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.15f, _y - s * 0.08f),
            s * 0.5f, s * 0.32f, CloudMid * _alpha);

        // Highlights
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - s * 0.05f, _y - s * 0.15f),
            s * 0.42f, s * 0.26f, CloudLight * _alpha);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.12f, _y - s * 0.12f),
            s * 0.35f, s * 0.22f, CloudLight * _alpha);

        // Bright peaks
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - s * 0.18f),
            s * 0.28f, s * 0.16f, CloudBright * (_alpha * 0.9f));
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.08f, _y - s * 0.14f),
            s * 0.2f, s * 0.12f, CloudBright * (_alpha * 0.8f));
    }

    private void DrawCirrusCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float sz = S(120) * _scale;
        float wave = MathF.Sin(_time * 0.2f + _seed) * 0.02f;

        // Wispy streaks with soft edges
        if (_variation == 0)
        {
            // Long flowing wisp
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y),
                sz * 2.0f, sz * 0.08f, CloudMid * (_alpha * 0.5f));
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + sz * 0.3f, _y - sz * 0.03f),
                sz * 1.6f, sz * 0.06f, CloudLight * (_alpha * 0.4f));
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - sz * 0.2f, _y + sz * 0.02f),
                sz * 1.3f, sz * 0.05f, CloudBright * (_alpha * 0.3f));
        }
        else if (_variation == 1)
        {
            // Curved feathery wisp
            for (int i = 0; i < 5; i++)
            {
                float offset = (i - 2) * sz * 0.15f;
                float yOff = MathF.Sin(i * 0.8f + _seed) * sz * 0.04f;
                float width = sz * (1.4f - MathF.Abs(i - 2) * 0.2f);
                float height = sz * (0.06f - MathF.Abs(i - 2) * 0.008f);
                Color c = (i == 2) ? CloudBright : CloudLight;
                renderer.DrawFilledEllipse(spriteBatch,
                    new Vector2(_x + offset, _y + yOff),
                    width, height, c * (_alpha * (0.5f - MathF.Abs(i - 2) * 0.1f)));
            }
        }
        else
        {
            // Multiple parallel wisps
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - sz * 0.06f),
                sz * 1.8f, sz * 0.05f, CloudLight * (_alpha * 0.45f));
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + sz * 0.2f, _y),
                sz * 1.5f, sz * 0.06f, CloudMid * (_alpha * 0.4f));
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - sz * 0.15f, _y + sz * 0.06f),
                sz * 1.2f, sz * 0.04f, CloudBase * (_alpha * 0.35f));
        }
    }

    private void DrawStratusCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float sz = S(100) * _scale;
        float pulse = MathF.Sin(_time * 0.25f + _seed) * 0.01f;
        float s = sz * (1f + pulse);

        // Wide ambient glow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y + s * 0.12f),
            s * 2.6f, s * 0.4f, CloudAmbient * (_alpha * 0.2f));

        // Bottom shadow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.08f, _y + s * 0.18f),
            s * 2.4f, s * 0.28f, CloudDark * (_alpha * 0.35f));

        // Shadow gradient
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.04f, _y + s * 0.1f),
            s * 2.3f, s * 0.32f, CloudDeep * (_alpha * 0.45f));

        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y + s * 0.04f),
            s * 2.2f, s * 0.35f, CloudShadow * (_alpha * 0.55f));

        // Main body
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y),
            s * 2.1f, s * 0.38f, CloudBase * _alpha);

        // Top surface variations
        float[] bumpX = { -0.7f, -0.35f, 0.0f, 0.35f, 0.7f };
        for (int i = 0; i < bumpX.Length; i++)
        {
            float bx = _x + s * bumpX[i];
            float bw = s * (0.5f + (i % 2) * 0.15f);
            float bh = s * (0.22f + (i % 3) * 0.04f);
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(bx, _y - s * 0.08f),
                bw, bh, CloudMid * _alpha);
        }

        // Highlights on top
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - s * 0.4f, _y - s * 0.12f),
            s * 0.6f, s * 0.18f, CloudLight * _alpha);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.3f, _y - s * 0.1f),
            s * 0.5f, s * 0.15f, CloudLight * _alpha);

        // Bright spots
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - s * 0.2f, _y - s * 0.14f),
            s * 0.35f, s * 0.12f, CloudBright * (_alpha * 0.85f));
    }

    private void DrawCumulonimbusCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float sz = S(80) * _scale;
        float sway = MathF.Sin(_time * 0.3f + _seed) * 0.02f;
        float s = sz * (1f + sway);

        // Dramatic towering cloud with dark base and bright top

        // Dark storm base
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.12f, _y + s * 0.55f),
            s * 1.1f, s * 0.3f, CloudDark * (_alpha * 0.6f));

        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.08f, _y + s * 0.45f),
            s * 1.0f, s * 0.35f, CloudDeep * (_alpha * 0.55f));

        // Middle body
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.04f, _y + s * 0.3f),
            s * 0.9f, s * 0.4f, CloudShadow * (_alpha * 0.6f));

        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y + s * 0.15f),
            s * 0.85f, s * 0.45f, CloudBase * _alpha);

        // Rising tower
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - s * 0.05f, _y - s * 0.05f),
            s * 0.75f, s * 0.5f, CloudBase * _alpha);

        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.05f, _y - s * 0.2f),
            s * 0.65f, s * 0.45f, CloudMid * _alpha);

        // Anvil top
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - s * 0.35f),
            s * 0.8f, s * 0.35f, CloudMid * _alpha);

        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + s * 0.15f, _y - s * 0.45f),
            s * 0.55f, s * 0.28f, CloudLight * _alpha);

        // Bright crown
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - s * 0.5f),
            s * 0.45f, s * 0.25f, CloudLight * _alpha);

        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - s * 0.05f, _y - s * 0.55f),
            s * 0.3f, s * 0.18f, CloudBright * _alpha);

        // Sunlit peak
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - s * 0.58f),
            s * 0.2f, s * 0.12f, CloudBright * (_alpha * 0.95f));
    }

    private void DrawAltocumulusCloud(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float sz = S(35) * _scale;
        float bob = MathF.Sin(_time * 0.5f + _seed) * S(2f);

        // Small fluffy puff with nice shading

        // Shadow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + sz * 0.08f, _y + sz * 0.15f + bob),
            sz * 0.95f, sz * 0.3f, CloudDeep * (_alpha * 0.4f));

        // Mid shadow
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + sz * 0.04f, _y + sz * 0.08f + bob),
            sz * 0.9f, sz * 0.35f, CloudShadow * (_alpha * 0.5f));

        // Main body
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y + bob),
            sz * 0.85f, sz * 0.55f, CloudBase * _alpha);

        // Top puffs based on variation
        if (_variation == 0)
        {
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - sz * 0.15f, _y - sz * 0.12f + bob),
                sz * 0.45f, sz * 0.35f, CloudMid * _alpha);
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + sz * 0.1f, _y - sz * 0.08f + bob),
                sz * 0.4f, sz * 0.3f, CloudLight * _alpha);
        }
        else if (_variation == 1)
        {
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - sz * 0.15f + bob),
                sz * 0.5f, sz * 0.32f, CloudMid * _alpha);
        }
        else
        {
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x - sz * 0.2f, _y - sz * 0.05f + bob),
                sz * 0.35f, sz * 0.28f, CloudMid * _alpha);
            renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x + sz * 0.15f, _y - sz * 0.1f + bob),
                sz * 0.38f, sz * 0.3f, CloudLight * _alpha);
        }

        // Highlight
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(_x, _y - sz * 0.18f + bob),
            sz * 0.28f, sz * 0.18f, CloudBright * (_alpha * 0.85f));
    }
}
