using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Progress bar for displaying values like HP/MP with cosmic styling.
/// Features gradient fills, inner glow, pulsing at low values, and flash effects.
/// </summary>
public class GleamProgressBar : GleamElement
{
    private float _currentValue;
    private float _maxValue;
    private float _displayValue;
    private float _previousValue;

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
            _maxValue = MathHelper.Max(value, 0);
            _currentValue = MathHelper.Clamp(_currentValue, 0, _maxValue);
            UpdateLabelText();
        }
    }

    public float Percentage => _maxValue > 0 ? _currentValue / _maxValue : 0;

    // Gradient colors (bright = top, dark = bottom)
    public Color FillColor { get; set; } = new Color(100, 220, 100);
    public Color FillColorDark { get; set; } = new Color(30, 80, 30);
    public Color LowFillColor { get; set; } = new Color(220, 60, 60);
    public Color LowFillColorDark { get; set; } = new Color(80, 20, 20);
    public Color GlowColor { get; set; } = new Color(200, 255, 200);
    public Color LowGlowColor { get; set; } = new Color(255, 150, 150);

    public float LowThreshold { get; set; } = 0.25f;
    public float TransitionSpeed { get; set; } = 5f;

    public bool ShowText
    {
        get => _valueLabel.Visible;
        set => _valueLabel.Visible = value;
    }

    public string TextFormat { get; set; } = "{0}/{1}";

    public Color? TextColor
    {
        get => _valueLabel.TextColor;
        set => _valueLabel.TextColor = value;
    }

    // Effect toggles
    public bool EnableGradient { get; set; } = true;
    public bool EnableGlow { get; set; } = true;
    public bool EnablePulse { get; set; } = true;
    public bool EnableFlash { get; set; } = true;
    public bool AutoFlash { get; set; } = true;

    public GleamProgressBar(Vector2 position, Vector2 size, float maxValue = 100f)
    {
        Position = position;
        Size = size;
        _maxValue = maxValue;
        _currentValue = maxValue;
        _displayValue = maxValue;
        _previousValue = maxValue;

        // Create value label as child element with auto-fit
        _valueLabel = new GleamLabel("", Vector2.Zero, size)
        {
            Alignment = TextAlignment.Center,
            ShowShadow = true,
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

        // Pulse animation when low
        if (displayPercentage <= LowThreshold && EnablePulse)
        {
            _pulsePhase += delta * 4f;
        }

        // Decay flash effect
        if (_flashIntensity > 0)
        {
            _flashIntensity -= delta * 3f;
            if (_flashIntensity < 0) _flashIntensity = 0;
        }

        // Keep label sized to bar
        _valueLabel.Size = Size;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        Rectangle bounds = Bounds;
        float displayPercentage = _maxValue > 0 ? MathHelper.Clamp(_displayValue / _maxValue, 0, 1) : 0;
        bool isLow = displayPercentage <= LowThreshold;

        // 1. Background with subtle gradient
        if (EnableGradient)
        {
            renderer.DrawVerticalGradient(spriteBatch, bounds,
                new Color(5, 2, 10), theme.DarkPurple, Alpha);
        }
        else
        {
            renderer.DrawRect(spriteBatch, bounds, theme.DarkPurple, Alpha);
        }

        // 2. Fill bar
        int fillWidth = (int)(bounds.Width * displayPercentage);
        if (fillWidth > 0)
        {
            Rectangle fillBounds = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);

            // Get colors based on health state
            Color brightColor = isLow ? LowFillColor : FillColor;
            Color darkColor = isLow ? LowFillColorDark : FillColorDark;
            Color glowColor = isLow ? LowGlowColor : GlowColor;

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
                int glowHeight = Math.Max(3, bounds.Height / 4);
                renderer.DrawInnerGlow(spriteBatch, fillBounds, glowColor, glowHeight, Alpha);
            }

            // Bottom shadow for depth
            if (EnableGradient)
            {
                renderer.DrawBottomShadow(spriteBatch, fillBounds, 3, Alpha);
            }
        }

        // 3. Flash overlay (damage/heal effect)
        if (_flashIntensity > 0 && EnableFlash)
        {
            renderer.DrawRect(spriteBatch, bounds, _flashColor, _flashIntensity * Alpha * 0.6f);
        }

        // 4. Border
        renderer.DrawRectBorder(spriteBatch, bounds, theme.Gold, 2, Alpha);

        // Note: Text is drawn by the _valueLabel child element in Draw()
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        // Progress bars don't handle input
        return false;
    }

    public void SetPercentage(float percentage)
    {
        CurrentValue = _maxValue * MathHelper.Clamp(percentage, 0, 1);
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
