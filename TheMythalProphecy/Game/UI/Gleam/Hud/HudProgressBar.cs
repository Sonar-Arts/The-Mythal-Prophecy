using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

public enum HudBarType { HP, MP }

/// <summary>
/// Progress bar for HUD with HP/MP coloring from HudTheme.
/// Features gradient fills, inner glow, pulsing at low values, and flash effects.
/// </summary>
public class HudProgressBar : GleamElement
{
    private float _currentValue;
    private float _maxValue;
    private float _displayValue;
    private float _previousValue;
    private readonly HudTheme _hudTheme;
    private readonly HudBarType _barType;

    // Animation state
    private float _pulsePhase;
    private float _flashIntensity;
    private Color _flashColor;

    // Text label
    private readonly GleamLabel _valueLabel;
    private string _cachedText = "";

    public float CurrentValue
    {
        get => _currentValue;
        set
        {
            float newValue = MathHelper.Clamp(value, 0, _maxValue);
            if (Math.Abs(newValue - _currentValue) > 0.1f)
            {
                _previousValue = _currentValue;
                // Auto-trigger flash on significant value change
                if (newValue < _previousValue - 1f)
                {
                    TriggerDamageFlash();
                }
                else if (newValue > _previousValue + 1f)
                {
                    TriggerHealFlash();
                }
            }
            _currentValue = newValue;
            UpdateLabelText();
        }
    }

    public float MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = MathHelper.Max(value, 1);
            _currentValue = MathHelper.Clamp(_currentValue, 0, _maxValue);
            UpdateLabelText();
        }
    }

    public float Percentage => _maxValue > 0 ? _currentValue / _maxValue : 0;
    public float TransitionSpeed { get; set; } = 8f;

    public bool ShowText
    {
        get => _valueLabel.Visible;
        set => _valueLabel.Visible = value;
    }

    public string TextFormat { get; set; } = "{0}/{1}";

    // Effect toggles
    public bool EnableGradient { get; set; } = true;
    public bool EnableGlow { get; set; } = true;
    public bool EnablePulse { get; set; } = true;
    public bool EnableFlash { get; set; } = true;

    public HudProgressBar(Vector2 position, Vector2 size, HudTheme hudTheme, HudBarType barType)
    {
        Position = position;
        Size = size;
        _hudTheme = hudTheme;
        _barType = barType;
        _maxValue = 100;
        _currentValue = 100;
        _displayValue = 100;
        _previousValue = 100;

        // Create value label as child element with auto-fit
        _valueLabel = new GleamLabel("", Vector2.Zero, size)
        {
            Alignment = TextAlignment.Center,
            ShowShadow = true,
            TextColor = hudTheme.TextPrimary,
            AutoFit = true,
            AutoFitPadding = 2f
        };
        AddChild(_valueLabel);

        UpdateLabelText();
    }

    private void UpdateLabelText()
    {
        string newText = string.Format(TextFormat, (int)_currentValue, (int)_maxValue);
        if (newText != _cachedText)
        {
            _cachedText = newText;
            _valueLabel.Text = newText;
        }
    }

    public override void Update(GameTime gameTime, GleamRenderer renderer)
    {
        base.Update(gameTime, renderer);

        float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float displayPercentage = _maxValue > 0 ? _displayValue / _maxValue : 0;

        // Smooth value transition
        if (Math.Abs(_displayValue - _currentValue) > 0.1f)
        {
            float difference = _currentValue - _displayValue;
            _displayValue += difference * TransitionSpeed * delta;
        }
        else
        {
            _displayValue = _currentValue;
        }

        // Pulse animation when low (below 25%)
        if (displayPercentage <= 0.25f && EnablePulse)
        {
            _pulsePhase += delta * 4f;
        }

        // Decay flash effect
        if (_flashIntensity > 0)
        {
            _flashIntensity -= delta * 3f;
            if (_flashIntensity < 0) _flashIntensity = 0;
        }

        // Keep label sized to bar and use HUD font
        _valueLabel.Size = Size;
        _valueLabel.Font = _hudTheme.HudFont;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        Rectangle bounds = Bounds;
        float displayPercentage = _maxValue > 0 ? MathHelper.Clamp(_displayValue / _maxValue, 0, 1) : 0;
        bool isLow = displayPercentage <= 0.25f;

        // 1. Background with subtle gradient
        Color bgColor = _barType == HudBarType.HP ? _hudTheme.HpBackground : _hudTheme.MpBackground;
        if (EnableGradient)
        {
            Color bgDark = new Color(
                (int)(bgColor.R * 0.4f),
                (int)(bgColor.G * 0.4f),
                (int)(bgColor.B * 0.4f)
            );
            renderer.DrawVerticalGradient(spriteBatch, bounds, bgDark, bgColor, Alpha);
        }
        else
        {
            renderer.DrawRect(spriteBatch, bounds, bgColor, Alpha);
        }

        // 2. Fill bar
        int fillWidth = (int)(bounds.Width * displayPercentage);
        if (fillWidth > 0)
        {
            Rectangle fillBounds = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);

            // Get colors based on bar type and percentage
            Color brightColor, darkColor, glowColor;
            if (_barType == HudBarType.HP)
            {
                brightColor = _hudTheme.GetHpColor(displayPercentage);
                darkColor = _hudTheme.GetHpColorDark(displayPercentage);
                glowColor = _hudTheme.GetHpGlowColor(displayPercentage);
            }
            else
            {
                brightColor = _hudTheme.GetMpColor(displayPercentage);
                darkColor = _hudTheme.GetMpColorDark(displayPercentage);
                glowColor = _hudTheme.GetMpGlowColor(displayPercentage);
            }

            // Apply pulse effect if low
            if (isLow && EnablePulse)
            {
                float pulse = (MathF.Sin(_pulsePhase) + 1f) * 0.5f; // 0-1
                brightColor = Color.Lerp(brightColor, Color.White, pulse * 0.3f);
                glowColor = Color.Lerp(glowColor, Color.White, pulse * 0.4f);
            }

            // Draw fill with gradient or solid
            if (EnableGradient)
            {
                renderer.DrawVerticalGradient(spriteBatch, fillBounds, darkColor, brightColor, Alpha);
            }
            else
            {
                renderer.DrawRect(spriteBatch, fillBounds, brightColor, Alpha);
            }

            // Inner glow at top
            if (EnableGlow)
            {
                int glowHeight = Math.Max(2, bounds.Height / 4);
                renderer.DrawInnerGlow(spriteBatch, fillBounds, glowColor, glowHeight, Alpha);
            }

            // Bottom shadow for depth
            if (EnableGradient)
            {
                renderer.DrawBottomShadow(spriteBatch, fillBounds, 2, Alpha);
            }
        }

        // 3. Flash overlay (damage/heal effect)
        if (_flashIntensity > 0 && EnableFlash)
        {
            renderer.DrawRect(spriteBatch, bounds, _flashColor, _flashIntensity * Alpha * 0.6f);
        }

        // 4. Border
        renderer.DrawRectBorder(spriteBatch, bounds, _hudTheme.Gold, 1, Alpha * 0.7f);

        // Note: Text is drawn by the _valueLabel child element in Draw()
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        return false; // Progress bars don't handle input
    }

    /// <summary>
    /// Trigger a red damage flash effect.
    /// </summary>
    public void TriggerDamageFlash()
    {
        if (!EnableFlash) return;
        _flashColor = new Color(255, 50, 50);
        _flashIntensity = 0.8f;
    }

    /// <summary>
    /// Trigger a green/gold heal flash effect.
    /// </summary>
    public void TriggerHealFlash()
    {
        if (!EnableFlash) return;
        _flashColor = new Color(100, 255, 150);
        _flashIntensity = 0.6f;
    }
}
