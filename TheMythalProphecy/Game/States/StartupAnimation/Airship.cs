using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.States.StartupAnimation;

/// <summary>
/// Fantasy Flying Galleon with magical propulsion
/// </summary>
public class Airship
{
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private float _animationTime;

    // Hull colors
    private static readonly Color HullColor = new(60, 40, 25);
    private static readonly Color HullHighlight = new(90, 65, 42);
    private static readonly Color HullDark = new(40, 25, 15);

    // Sail colors
    private static readonly Color SailColor = new(220, 210, 190);
    private static readonly Color SailShadow = new(185, 175, 158);
    private static readonly Color MastColor = new(65, 45, 28);

    // Magic colors
    private static readonly Color MagicCore = new(255, 220, 140);
    private static readonly Color MagicMid = new(255, 200, 100);
    private static readonly Color MagicOuter = new(255, 180, 80);

    // Wing colors
    private static readonly Color WingColor = new(245, 245, 250);
    private static readonly Color WingShadow = new(200, 205, 215);

    public float CenterX => _screenWidth * 0.5f;
    public float CenterY => _screenHeight * 0.5f;

    public Airship(int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
    }

    public void Update(float totalElapsed)
    {
        _animationTime = totalElapsed;
    }

    public void Draw(SpriteBatch spriteBatch, PrimitiveRenderer renderer)
    {
        float cx = CenterX;
        float cy = CenterY;

        DrawMagicGlow(spriteBatch, renderer, cx, cy);
        DrawHull(spriteBatch, renderer, cx, cy);
        DrawMasts(spriteBatch, renderer, cx, cy);
        DrawWings(spriteBatch, renderer, cx, cy); // Wing in front (nearest to viewer)
    }

    private void DrawMagicGlow(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float cx, float cy)
    {
        float glowX = cx;
        float glowY = cy + 38;

        float pulse1 = 0.85f + 0.15f * MathF.Sin(_animationTime * 2.5f);
        float pulse2 = 0.9f + 0.1f * MathF.Sin(_animationTime * 3.2f + 0.5f);
        float pulse3 = 0.8f + 0.2f * MathF.Sin(_animationTime * 1.8f + 1.0f);

        float shimmerX = MathF.Sin(_animationTime * 1.5f) * 2f;
        float shimmerY = MathF.Cos(_animationTime * 2.0f) * 1.5f;

        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(glowX + shimmerX * 0.5f, glowY + 6 + shimmerY),
            60 * pulse3, 20 * pulse3, MagicOuter * 0.12f);
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(glowX, glowY),
            40 * pulse1, 14 * pulse1, MagicMid * 0.22f);
        renderer.DrawFilledEllipse(spriteBatch,
            new Vector2(glowX, glowY - 2),
            18 * pulse2, 6 * pulse2, MagicCore * 0.35f);
    }

    private void DrawWings(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float cx, float cy)
    {
        // Single wing on the near side (ship facing west, we see the south-facing wing)
        // Attached to main hull body, away from captain's quarters
        float wingX = cx - 15;
        float wingY = cy + 20;

        // Wing mount plate (clean attachment to hull)
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(wingX + 2, wingY + 2), 12, 8, HullDark);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(wingX + 2, wingY), 10, 6, HullHighlight);

        // Wing root attachment point
        Vector2 root = new(wingX, wingY + 5);

        // Wing extends down and slightly back (toward viewer)
        Vector2 leadingRoot = new(wingX - 12, wingY);
        Vector2 leadingMid = new(wingX - 6, wingY + 28);
        Vector2 leadingTip = new(wingX + 6, wingY + 50);

        Vector2 trailingRoot = new(wingX + 16, wingY + 6);
        Vector2 trailingMid = new(wingX + 12, wingY + 30);
        Vector2 trailingTip = new(wingX + 10, wingY + 48);

        // Main wing surface
        renderer.DrawTriangle(spriteBatch, leadingRoot, trailingRoot, leadingMid, WingColor);
        renderer.DrawTriangle(spriteBatch, trailingRoot, leadingMid, trailingMid, WingColor);
        renderer.DrawTriangle(spriteBatch, leadingMid, trailingMid, leadingTip, WingShadow);
        renderer.DrawTriangle(spriteBatch, trailingMid, leadingTip, trailingTip, WingShadow);

        // Wing spar (center structural beam)
        Vector2 sparMid = new(wingX + 2, wingY + 28);
        Vector2 sparTip = new(wingX + 7, wingY + 48);
        renderer.DrawLine(spriteBatch, root, sparMid, HullDark, 3);
        renderer.DrawLine(spriteBatch, sparMid, sparTip, HullDark, 2);

        // Struts
        renderer.DrawLine(spriteBatch, new Vector2(wingX - 4, wingY + 12),
            new Vector2(wingX + 10, wingY + 14), HullDark * 0.6f, 1);
        renderer.DrawLine(spriteBatch, new Vector2(wingX - 1, wingY + 26),
            new Vector2(wingX + 10, wingY + 28), HullDark * 0.6f, 1);

        // Magical trim on leading edge
        renderer.DrawLine(spriteBatch, leadingRoot, leadingMid, MagicMid * 0.4f, 2);
        renderer.DrawLine(spriteBatch, leadingMid, leadingTip, MagicMid * 0.3f, 1);
    }

    private void DrawHull(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float cx, float cy)
    {
        // === MAIN HULL AS CONNECTED POLYGON ===
        // Define hull outline vertices, then fill with triangles from center

        // Hull center point for triangle fan
        Vector2 hullCenter = new(cx, cy + 10);

        // Bow (left side, pointed)
        Vector2 bowTip = new(cx - 110, cy + 5);
        Vector2 bowUpper = new(cx - 85, cy - 15);
        Vector2 bowLower = new(cx - 85, cy + 30);

        // Main hull body
        Vector2 midUpperLeft = new(cx - 50, cy - 20);
        Vector2 midUpperRight = new(cx + 30, cy - 20);
        Vector2 midLowerLeft = new(cx - 50, cy + 35);
        Vector2 midLowerRight = new(cx + 30, cy + 35);

        // Stern (right side, where quarters attach)
        Vector2 sternUpper = new(cx + 60, cy - 20);
        Vector2 sternMid = new(cx + 75, cy + 5);
        Vector2 sternLower = new(cx + 60, cy + 35);

        // Fill hull with triangles (fan from center)
        // Bow section
        renderer.DrawTriangle(spriteBatch, hullCenter, bowTip, bowUpper, HullColor);
        renderer.DrawTriangle(spriteBatch, hullCenter, bowTip, bowLower, HullColor);

        // Upper hull
        renderer.DrawTriangle(spriteBatch, hullCenter, bowUpper, midUpperLeft, HullColor);
        renderer.DrawTriangle(spriteBatch, hullCenter, midUpperLeft, midUpperRight, HullColor);
        renderer.DrawTriangle(spriteBatch, hullCenter, midUpperRight, sternUpper, HullColor);

        // Lower hull
        renderer.DrawTriangle(spriteBatch, hullCenter, bowLower, midLowerLeft, HullColor);
        renderer.DrawTriangle(spriteBatch, hullCenter, midLowerLeft, midLowerRight, HullColor);
        renderer.DrawTriangle(spriteBatch, hullCenter, midLowerRight, sternLower, HullColor);

        // Stern connection
        renderer.DrawTriangle(spriteBatch, hullCenter, sternUpper, sternMid, HullColor);
        renderer.DrawTriangle(spriteBatch, hullCenter, sternMid, sternLower, HullColor);

        // === CAPTAIN'S QUARTERS - CONNECTED TO STERN ===
        Vector2 quartersCenter = new(cx + 72, cy - 25);

        // Quarters base (connects to sternUpper/sternMid)
        Vector2 qBottomLeft = sternUpper;
        Vector2 qBottomRight = new(cx + 85, cy - 15);
        Vector2 qMidLeft = new(cx + 58, cy - 40);
        Vector2 qMidRight = new(cx + 88, cy - 35);
        Vector2 qTopLeft = new(cx + 62, cy - 55);
        Vector2 qTopRight = new(cx + 85, cy - 50);
        Vector2 qRoof = new(cx + 73, cy - 62);

        // Fill quarters with triangles
        renderer.DrawTriangle(spriteBatch, quartersCenter, qBottomLeft, qBottomRight, HullColor);
        renderer.DrawTriangle(spriteBatch, quartersCenter, qBottomLeft, qMidLeft, HullColor);
        renderer.DrawTriangle(spriteBatch, quartersCenter, qBottomRight, qMidRight, HullColor);
        renderer.DrawTriangle(spriteBatch, quartersCenter, qMidLeft, qTopLeft, HullColor);
        renderer.DrawTriangle(spriteBatch, quartersCenter, qMidRight, qTopRight, HullColor);
        renderer.DrawTriangle(spriteBatch, quartersCenter, qMidLeft, qMidRight, HullColor);
        renderer.DrawTriangle(spriteBatch, quartersCenter, qTopLeft, qTopRight, HullColor);

        // Roof
        renderer.DrawTriangle(spriteBatch, qTopLeft, qTopRight, qRoof, HullDark);

        // Stern back wall (fills gap between hull and quarters)
        renderer.DrawTriangle(spriteBatch, sternUpper, sternMid, qBottomRight, HullColor);
        renderer.DrawTriangle(spriteBatch, sternMid, new Vector2(cx + 80, cy + 5), qBottomRight, HullColor);

        // === DECK SURFACE (flat top) ===
        Vector2 deckFrontLeft = new(cx - 70, cy - 18);
        Vector2 deckFrontRight = new(cx + 50, cy - 18);
        Vector2 deckBackLeft = new(cx - 60, cy - 12);
        Vector2 deckBackRight = new(cx + 50, cy - 12);
        Vector2 deckBowPoint = new(cx - 85, cy - 10);

        // Flat deck rectangle
        renderer.DrawTriangle(spriteBatch, deckFrontLeft, deckFrontRight, deckBackLeft, HullHighlight);
        renderer.DrawTriangle(spriteBatch, deckFrontRight, deckBackLeft, deckBackRight, HullHighlight);
        // Bow deck extension
        renderer.DrawTriangle(spriteBatch, deckBowPoint, deckFrontLeft, deckBackLeft, HullHighlight);

        // Deck planking lines (horizontal for flat look)
        for (int i = 0; i < 5; i++)
        {
            float lineY = cy - 17 + i * 1.5f;
            renderer.DrawLine(spriteBatch,
                new Vector2(cx - 55, lineY),
                new Vector2(cx + 45, lineY),
                HullDark * 0.25f, 1);
        }

        // === DETAILS ===

        // Windows on quarters
        renderer.DrawFilledCircle(spriteBatch, new Vector2(cx + 68, cy - 38), 3, new Color(255, 210, 150, 120));
        renderer.DrawFilledCircle(spriteBatch, new Vector2(cx + 80, cy - 38), 3, new Color(255, 210, 150, 120));

        // Hull runes
        float[] runeX = { -50, -15, 20 };
        foreach (float rx in runeX)
        {
            Vector2 pos = new(cx + rx, cy + 20);
            renderer.DrawFilledCircle(spriteBatch, pos, 4, MagicMid * 0.2f);
            renderer.DrawFilledCircle(spriteBatch, pos, 2, MagicCore * 0.4f);
        }
    }

    private void DrawMasts(SpriteBatch spriteBatch, PrimitiveRenderer renderer, float cx, float cy)
    {
        float deckY = cy - 12;

        // Foremast (front)
        DrawMast(spriteBatch, renderer, cx - 30, deckY, 50, 28);

        // Mainmast (center, tallest)
        float mainmastX = cx + 10;
        float mainmastHeight = 65;
        DrawMast(spriteBatch, renderer, mainmastX, deckY, mainmastHeight, 36);

        // Mizzenmast (on quarterdeck - smaller, positioned on the raised stern)
        DrawMast(spriteBatch, renderer, cx + 70, cy - 55, 30, 18);

        // Flag at very top of mainmast (above the mast pole)
        float mastTop = deckY - mainmastHeight;
        renderer.DrawTriangle(spriteBatch,
            new Vector2(mainmastX, mastTop - 2),
            new Vector2(mainmastX, mastTop + 8),
            new Vector2(mainmastX + 14, mastTop + 3),
            new Color(180, 45, 45));
        renderer.DrawTriangle(spriteBatch,
            new Vector2(mainmastX + 1, mastTop),
            new Vector2(mainmastX + 1, mastTop + 6),
            new Vector2(mainmastX + 11, mastTop + 3),
            new Color(200, 65, 65));
    }

    private void DrawMast(SpriteBatch spriteBatch, PrimitiveRenderer renderer,
        float x, float deckY, float height, float sailWidth)
    {
        float mastTop = deckY - height;

        // Mast pole
        renderer.DrawRectangle(spriteBatch,
            new Rectangle((int)(x - 1), (int)mastTop, 3, (int)height),
            MastColor);

        // Yard arm (horizontal beam for sail)
        float yardY = mastTop + 8;
        renderer.DrawRectangle(spriteBatch,
            new Rectangle((int)(x - sailWidth / 2), (int)yardY, (int)sailWidth, 2),
            MastColor);

        // Sail as triangles (billowing shape)
        float sailTop = yardY + 2;
        float sailBottom = sailTop + height * 0.55f;
        float sailBulge = sailWidth * 0.15f;

        // Main sail body
        Vector2 sailTL = new(x - sailWidth / 2 + 2, sailTop);
        Vector2 sailTR = new(x + sailWidth / 2 - 2, sailTop);
        Vector2 sailBL = new(x - sailWidth / 2 + 4, sailBottom);
        Vector2 sailBR = new(x + sailWidth / 2, sailBottom);
        Vector2 sailMidRight = new(x + sailWidth / 2 + sailBulge, (sailTop + sailBottom) / 2);

        // Fill sail with triangles
        renderer.DrawTriangle(spriteBatch, sailTL, sailTR, sailBL, SailColor);
        renderer.DrawTriangle(spriteBatch, sailTR, sailBL, sailBR, SailColor);
        renderer.DrawTriangle(spriteBatch, sailTR, sailBR, sailMidRight, SailShadow);

        // Bottom yard
        renderer.DrawRectangle(spriteBatch,
            new Rectangle((int)(x - sailWidth / 2 + 3), (int)sailBottom, (int)(sailWidth - 2), 2),
            MastColor);
    }
}
