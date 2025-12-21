using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheMythalProphecy.Game.UI.Components;

/// <summary>
/// UI Button visual states
/// </summary>
public enum UIButtonState
{
    Normal,
    Hover,
    Pressed,
    Disabled
}

/// <summary>
/// Interactive button UI element
/// </summary>
public class UIButton : UIElement
{
    private UILabel _label;
    private UIButtonState _state = UIButtonState.Normal;
    private bool _isPressed;

    public string Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public Color NormalColor { get; set; } = new Color(60, 60, 80);
    public Color HoverColor { get; set; } = new Color(80, 120, 160);
    public Color PressedColor { get; set; } = new Color(60, 100, 140);
    public Color DisabledColor { get; set; } = new Color(40, 40, 40);
    public Color BorderColor { get; set; } = new Color(200, 200, 220);
    public int BorderThickness { get; set; } = 2;

    public Texture2D NormalTexture { get; set; }
    public Texture2D HoverTexture { get; set; }
    public Texture2D PressedTexture { get; set; }

    public new event Action<UIButton> OnClick;

    public UIButton()
    {
        _label = new UILabel
        {
            Alignment = TextAlignment.Center
        };
        AddChild(_label);
        SetPadding(8, 16);
    }

    public UIButton(string text, Vector2 position, Vector2 size) : this()
    {
        Text = text;
        Position = position;
        Size = size;
    }

    public override void Update(GameTime gameTime)
    {
        // Update button state
        if (!Enabled)
        {
            _state = UIButtonState.Disabled;
        }
        else if (_isPressed)
        {
            _state = UIButtonState.Pressed;
        }
        else if (IsHovered)
        {
            _state = UIButtonState.Hover;
        }
        else
        {
            _state = UIButtonState.Normal;
        }

        // Update label size to match button
        _label.Size = new Vector2(
            Size.X - PaddingLeft - PaddingRight,
            Size.Y - PaddingTop - PaddingBottom
        );

        base.Update(gameTime);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, UITheme theme)
    {
        Rectangle bounds = Bounds;
        Texture2D pixel = Core.GameServices.UI?.PixelTexture;
        if (pixel == null) return;

        // Get texture based on state
        Texture2D texture = _state switch
        {
            UIButtonState.Hover => HoverTexture ?? NormalTexture,
            UIButtonState.Pressed => PressedTexture ?? HoverTexture ?? NormalTexture,
            _ => NormalTexture
        };

        // Get color based on state
        Color backgroundColor = _state switch
        {
            UIButtonState.Hover => HoverColor,
            UIButtonState.Pressed => PressedColor,
            UIButtonState.Disabled => DisabledColor,
            _ => NormalColor
        };

        // Draw button background
        if (texture != null)
        {
            spriteBatch.Draw(texture, bounds, backgroundColor * Alpha);
        }
        else
        {
            spriteBatch.Draw(pixel, bounds, backgroundColor * Alpha);
        }

        // Draw border
        if (BorderThickness > 0)
        {
            Color border = IsFocused ? theme.HighlightColor : BorderColor;
            // Top
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderThickness), border * Alpha);
            // Bottom
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderThickness, bounds.Width, BorderThickness), border * Alpha);
            // Left
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderThickness, bounds.Height), border * Alpha);
            // Right
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderThickness, bounds.Y, BorderThickness, bounds.Height), border * Alpha);
        }
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseClicked)
    {
        if (!Enabled || !Visible) return false;

        bool wasHovered = IsHovered;
        IsHovered = Bounds.Contains(mousePosition);

        if (IsHovered)
        {
            if (mouseClicked)
            {
                _isPressed = true;
                OnClick?.Invoke(this);
                return true;
            }
        }
        else
        {
            _isPressed = false;
        }

        return false;
    }

    /// <summary>
    /// Convenience method to create a button with default size
    /// </summary>
    public static UIButton CreateDefault(string text, Vector2 position)
    {
        return new UIButton(text, position, new Vector2(150, 40));
    }
}
