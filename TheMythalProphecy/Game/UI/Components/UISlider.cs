using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TheMythalProphecy.Game.UI.Components;

/// <summary>
/// Slider for selecting numeric values
/// </summary>
public class UISlider : UIElement
{
    private float _value;
    private float _minValue;
    private float _maxValue;
    private bool _isDragging;

    public float Value
    {
        get => _value;
        set
        {
            float newValue = MathHelper.Clamp(value, _minValue, _maxValue);
            if (_value != newValue)
            {
                _value = newValue;
                OnValueChanged?.Invoke(this, _value);
            }
        }
    }

    public float MinValue
    {
        get => _minValue;
        set
        {
            _minValue = value;
            Value = _value; // Re-clamp
        }
    }

    public float MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            Value = _value; // Re-clamp
        }
    }

    public float Percentage => _maxValue > _minValue ? (_value - _minValue) / (_maxValue - _minValue) : 0;

    public Color TrackColor { get; set; } = new Color(60, 60, 60);
    public Color FillColor { get; set; } = new Color(100, 200, 100);
    public Color ThumbColor { get; set; } = new Color(200, 200, 200);
    public Color BorderColor { get; set; } = new Color(150, 150, 150);
    public int BorderThickness { get; set; } = 1;
    public float ThumbWidth { get; set; } = 12;

    public event Action<UISlider, float> OnValueChanged;

    private MouseState _previousMouseState;

    public UISlider()
    {
        _minValue = 0;
        _maxValue = 1;
        _value = 0.5f;
    }

    public UISlider(Vector2 position, Vector2 size, float minValue = 0, float maxValue = 1, float initialValue = 0.5f) : this()
    {
        Position = position;
        Size = size;
        MinValue = minValue;
        MaxValue = maxValue;
        Value = initialValue;
    }

    public override void Update(GameTime gameTime)
    {
        MouseState mouseState = Mouse.GetState();

        Rectangle bounds = Bounds;
        Vector2 mousePos = new Vector2(mouseState.X, mouseState.Y);

        // Check if mouse button just pressed
        bool wasPressed = _previousMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        bool isPressed = mouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed;
        bool justPressed = isPressed && !wasPressed;

        // Start dragging if clicked on slider
        if (justPressed && bounds.Contains(mousePos) && Enabled)
        {
            _isDragging = true;
            UpdateValueFromMouse(mousePos);
        }

        // Continue dragging
        if (_isDragging && isPressed)
        {
            UpdateValueFromMouse(mousePos);
        }

        // Stop dragging
        if (!isPressed)
        {
            _isDragging = false;
        }

        _previousMouseState = mouseState;

        base.Update(gameTime);
    }

    private void UpdateValueFromMouse(Vector2 mousePos)
    {
        Rectangle bounds = Bounds;
        float relativeX = mousePos.X - bounds.X;
        float percentage = MathHelper.Clamp(relativeX / bounds.Width, 0, 1);
        Value = _minValue + percentage * (_maxValue - _minValue);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, UITheme theme)
    {
        Rectangle bounds = Bounds;
        Texture2D pixel = Core.GameServices.UI?.PixelTexture;
        if (pixel == null) return;

        // Draw track
        spriteBatch.Draw(pixel, bounds, TrackColor * Alpha);

        // Draw fill
        int fillWidth = (int)(bounds.Width * Percentage);
        if (fillWidth > 0)
        {
            Rectangle fillRect = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
            spriteBatch.Draw(pixel, fillRect, FillColor * Alpha);
        }

        // Draw thumb
        int thumbX = bounds.X + (int)(bounds.Width * Percentage) - (int)(ThumbWidth / 2);
        Rectangle thumbRect = new Rectangle(thumbX, bounds.Y - 2, (int)ThumbWidth, bounds.Height + 4);
        spriteBatch.Draw(pixel, thumbRect, ThumbColor * Alpha);

        // Draw border
        if (BorderThickness > 0)
        {
            Color borderColor = IsFocused ? theme.HighlightColor : BorderColor;
            // Top
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderThickness), borderColor * Alpha);
            // Bottom
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderThickness, bounds.Width, BorderThickness), borderColor * Alpha);
            // Left
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderThickness, bounds.Height), borderColor * Alpha);
            // Right
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderThickness, bounds.Y, BorderThickness, bounds.Height), borderColor * Alpha);
        }
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseClicked)
    {
        if (!Enabled || !Visible) return false;

        IsHovered = Bounds.Contains(mousePosition);

        if (IsHovered && mouseClicked)
        {
            _isDragging = true;
            UpdateValueFromMouse(mousePosition);
            return true;
        }

        return false;
    }
}
