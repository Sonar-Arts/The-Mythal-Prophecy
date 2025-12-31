using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// HUD-specific theme with cosmic color variant.
/// Provides colors for HP/MP bars and message types.
/// </summary>
public class HudTheme : IGleamTheme
{
    // Core cosmic palette (same as GleamTheme for consistency)
    public Color DeepPurple { get; } = new Color(15, 4, 31);
    public Color MidPurple { get; } = new Color(31, 8, 56);
    public Color DarkPurple { get; } = new Color(8, 3, 15);
    public Color MutedPurple { get; } = new Color(20, 10, 30);

    // Gold accents
    public Color Gold { get; } = new Color(179, 128, 51);
    public Color GoldBright { get; } = new Color(220, 180, 80);
    public Color GoldDim { get; } = new Color(120, 85, 35);

    // Text colors
    public Color TextPrimary { get; } = Color.White;
    public Color TextSecondary { get; } = new Color(200, 200, 220);
    public Color TextDisabled { get; } = new Color(100, 100, 120);

    // HP Bar colors (3-tier cosmic variant)
    public Color HpFull { get; } = new Color(80, 220, 180);       // Ethereal teal (bright)
    public Color HpFullDark { get; } = new Color(30, 80, 60);     // Ethereal teal (dark)
    public Color HpMedium { get; } = new Color(220, 180, 80);     // Gold-amber warning (bright)
    public Color HpMediumDark { get; } = new Color(80, 60, 20);   // Gold-amber warning (dark)
    public Color HpLow { get; } = new Color(220, 80, 100);        // Cosmic rose-red (bright)
    public Color HpLowDark { get; } = new Color(80, 25, 35);      // Cosmic rose-red (dark)
    public Color HpBackground { get; } = new Color(20, 40, 35);   // Dark teal
    public Color HpGlow { get; } = new Color(150, 255, 220);      // Bright teal glow
    public Color HpGlowLow { get; } = new Color(255, 150, 160);   // Bright red glow

    // MP Bar colors
    public Color MpFull { get; } = new Color(120, 140, 255);      // Arcane blue (bright)
    public Color MpFullDark { get; } = new Color(35, 45, 100);    // Arcane blue (dark)
    public Color MpLow { get; } = new Color(80, 60, 140);         // Dim purple (bright)
    public Color MpLowDark { get; } = new Color(30, 25, 60);      // Dim purple (dark)
    public Color MpBackground { get; } = new Color(20, 20, 50);   // Deep blue-purple
    public Color MpGlow { get; } = new Color(180, 200, 255);      // Bright blue glow
    public Color MpGlowLow { get; } = new Color(140, 120, 200);   // Dim purple glow

    // Message log colors
    public Color MessageSystem { get; } = new Color(180, 220, 255);   // Soft cyan
    public Color MessageCombat { get; } = new Color(255, 220, 120);   // Golden yellow
    public Color MessageDamage { get; } = new Color(255, 120, 100);   // Coral red
    public Color MessageHeal { get; } = new Color(120, 255, 180);     // Mint green
    public Color MessageDefault { get; } = new Color(200, 200, 220);  // Silver-white

    // HUD panel styling
    public Color PanelBackground { get; } = new Color(10, 5, 20);     // Near-black purple
    public float PanelAlpha { get; } = 0.75f;

    // Animation timing
    public float HoverTransitionDuration { get; } = 0.15f;

    // Sizing
    public int BorderThickness { get; } = 2;
    public int PaddingSmall { get; } = 4;
    public int PaddingMedium { get; } = 8;
    public int PaddingLarge { get; } = 16;

    // Fonts
    public SpriteFont DefaultFont { get; private set; }
    public SpriteFont MenuFont { get; private set; }
    public SpriteFont HudFont { get; private set; }

    public bool IsInitialized => DefaultFont != null;

    public void Initialize(SpriteFont defaultFont, SpriteFont hudFont = null)
    {
        DefaultFont = defaultFont;
        MenuFont = defaultFont;
        HudFont = hudFont ?? defaultFont;
    }

    /// <summary>
    /// Gets the HP bar fill color (bright) based on current percentage.
    /// </summary>
    public Color GetHpColor(float percentage)
    {
        if (percentage <= 0.25f) return HpLow;
        if (percentage <= 0.5f) return HpMedium;
        return HpFull;
    }

    /// <summary>
    /// Gets the HP bar fill color (dark) for gradient bottom.
    /// </summary>
    public Color GetHpColorDark(float percentage)
    {
        if (percentage <= 0.25f) return HpLowDark;
        if (percentage <= 0.5f) return HpMediumDark;
        return HpFullDark;
    }

    /// <summary>
    /// Gets the HP glow color based on current percentage.
    /// </summary>
    public Color GetHpGlowColor(float percentage)
    {
        if (percentage <= 0.25f) return HpGlowLow;
        return HpGlow;
    }

    /// <summary>
    /// Gets the MP bar fill color (bright) based on current percentage.
    /// </summary>
    public Color GetMpColor(float percentage)
    {
        if (percentage <= 0.25f) return MpLow;
        return MpFull;
    }

    /// <summary>
    /// Gets the MP bar fill color (dark) for gradient bottom.
    /// </summary>
    public Color GetMpColorDark(float percentage)
    {
        if (percentage <= 0.25f) return MpLowDark;
        return MpFullDark;
    }

    /// <summary>
    /// Gets the MP glow color based on current percentage.
    /// </summary>
    public Color GetMpGlowColor(float percentage)
    {
        if (percentage <= 0.25f) return MpGlowLow;
        return MpGlow;
    }
}
