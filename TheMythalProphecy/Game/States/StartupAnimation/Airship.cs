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

    // Da Vinci wing colors - fabric and wood frame
    private static readonly Color FabricLight = new(235, 225, 200);      // Canvas/linen highlight
    private static readonly Color FabricMid = new(210, 195, 165);        // Main fabric
    private static readonly Color FabricShadow = new(175, 160, 135);     // Fabric in shadow
    private static readonly Color FabricDark = new(145, 130, 110);       // Deep creases
    private static readonly Color WoodFrame = new(85, 60, 35);           // Wooden ribs
    private static readonly Color WoodLight = new(110, 80, 50);          // Wood highlights
    private static readonly Color LeatherStrap = new(70, 45, 25);        // Binding straps

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

        // Gentle bobbing motion - slow primary wave with subtle secondary
        float bobPrimary = MathF.Sin(_animationTime * 0.8f) * 3f;
        float bobSecondary = MathF.Sin(_animationTime * 1.3f + 0.5f) * 1f;
        float bobOffset = bobPrimary + bobSecondary;

        cy += bobOffset;

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
        // Da Vinci style flying wing - fabric stretched over wooden frame
        // Attached to main hull, extends downward (we see it from above/side)
        float wingX = cx - 15;
        float wingY = cy + 18;

        // Wing mount - heavy wooden bracket attached to hull
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(wingX + 3, wingY + 4), 14, 10, HullDark);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(wingX + 2, wingY + 2), 12, 8, WoodFrame);
        renderer.DrawFilledEllipse(spriteBatch, new Vector2(wingX + 1, wingY), 10, 6, WoodLight);

        // Main spar (primary structural beam running down the wing)
        Vector2 sparRoot = new(wingX, wingY + 6);
        Vector2 sparMid = new(wingX + 4, wingY + 32);
        Vector2 sparTip = new(wingX + 8, wingY + 55);

        // Wing frame points - ribs radiate from the spar
        // Leading edge (front of wing)
        Vector2 leadRoot = new(wingX - 14, wingY + 2);
        Vector2 leadMid = new(wingX - 8, wingY + 30);
        Vector2 leadTip = new(wingX + 2, wingY + 52);

        // Trailing edge (back of wing)
        Vector2 trailRoot = new(wingX + 18, wingY + 10);
        Vector2 trailMid = new(wingX + 16, wingY + 34);
        Vector2 trailTip = new(wingX + 12, wingY + 54);

        // === FABRIC PANELS (drawn first, behind the frame) ===

        // Upper panel (root to mid) - catches more light
        renderer.DrawTriangle(spriteBatch, leadRoot, sparRoot, leadMid, FabricLight);
        renderer.DrawTriangle(spriteBatch, sparRoot, leadMid, sparMid, FabricMid);
        renderer.DrawTriangle(spriteBatch, sparRoot, trailRoot, sparMid, FabricMid);
        renderer.DrawTriangle(spriteBatch, trailRoot, sparMid, trailMid, FabricShadow);

        // Lower panel (mid to tip) - more shadow
        renderer.DrawTriangle(spriteBatch, leadMid, sparMid, leadTip, FabricMid);
        renderer.DrawTriangle(spriteBatch, sparMid, leadTip, sparTip, FabricShadow);
        renderer.DrawTriangle(spriteBatch, sparMid, trailMid, sparTip, FabricShadow);
        renderer.DrawTriangle(spriteBatch, trailMid, sparTip, trailTip, FabricDark);

        // Fabric tension lines (subtle creases in the canvas)
        renderer.DrawLine(spriteBatch, new Vector2(wingX - 6, wingY + 14), new Vector2(wingX + 8, wingY + 18), FabricDark * 0.3f, 1);
        renderer.DrawLine(spriteBatch, new Vector2(wingX - 3, wingY + 24), new Vector2(wingX + 10, wingY + 26), FabricDark * 0.25f, 1);
        renderer.DrawLine(spriteBatch, new Vector2(wingX, wingY + 38), new Vector2(wingX + 12, wingY + 40), FabricDark * 0.2f, 1);

        // === WOODEN FRAME (ribs and spars) ===

        // Main spar (thick central beam)
        renderer.DrawLine(spriteBatch, sparRoot, sparMid, WoodFrame, 4);
        renderer.DrawLine(spriteBatch, sparMid, sparTip, WoodFrame, 3);
        // Highlight on spar
        renderer.DrawLine(spriteBatch, sparRoot + new Vector2(-1, 0), sparMid + new Vector2(-1, 0), WoodLight, 1);

        // Leading edge rib
        renderer.DrawLine(spriteBatch, leadRoot, leadMid, WoodFrame, 3);
        renderer.DrawLine(spriteBatch, leadMid, leadTip, WoodFrame, 2);

        // Trailing edge rib
        renderer.DrawLine(spriteBatch, trailRoot, trailMid, WoodFrame, 3);
        renderer.DrawLine(spriteBatch, trailMid, trailTip, WoodFrame, 2);

        // Cross ribs (horizontal struts connecting the frame)
        // Upper cross rib
        renderer.DrawLine(spriteBatch, leadRoot, sparRoot, WoodFrame, 2);
        renderer.DrawLine(spriteBatch, sparRoot, trailRoot, WoodFrame, 2);

        // Middle cross ribs
        Vector2 crossMidLead = new(wingX - 10, wingY + 16);
        Vector2 crossMidSpar = new(wingX + 2, wingY + 18);
        Vector2 crossMidTrail = new(wingX + 16, wingY + 20);
        renderer.DrawLine(spriteBatch, crossMidLead, crossMidSpar, WoodFrame, 2);
        renderer.DrawLine(spriteBatch, crossMidSpar, crossMidTrail, WoodFrame, 2);

        // Lower cross ribs
        renderer.DrawLine(spriteBatch, leadMid, sparMid, WoodFrame, 2);
        renderer.DrawLine(spriteBatch, sparMid, trailMid, WoodFrame, 2);

        // Tip cross rib
        renderer.DrawLine(spriteBatch, leadTip, sparTip, WoodFrame, 1);
        renderer.DrawLine(spriteBatch, sparTip, trailTip, WoodFrame, 1);

        // === LEATHER BINDING STRAPS (at joints) ===
        // Where ribs meet the spar
        renderer.DrawFilledCircle(spriteBatch, sparRoot, 4, LeatherStrap);
        renderer.DrawFilledCircle(spriteBatch, sparMid, 3, LeatherStrap);
        renderer.DrawFilledCircle(spriteBatch, sparTip, 2, LeatherStrap);

        // Cross rib joints
        renderer.DrawFilledCircle(spriteBatch, crossMidSpar, 2, LeatherStrap);
        renderer.DrawFilledCircle(spriteBatch, new Vector2(wingX + 2, wingY + 32), 2, LeatherStrap);

        // === MAGICAL ENHANCEMENT (subtle glow on leading edge) ===
        renderer.DrawLine(spriteBatch, leadRoot, leadMid, MagicMid * 0.25f, 2);
        renderer.DrawLine(spriteBatch, leadMid, leadTip, MagicMid * 0.2f, 1);
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
