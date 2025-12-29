using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Scrollable panel container that can hold any GleamElements.
/// Content is clipped to the panel bounds and can be scrolled vertically.
/// </summary>
public class GleamScrollPanel : GleamElement
{
    // Layout
    private float _scrollOffset;
    private float _contentHeight;
    private float _targetScrollOffset;
    private bool _isDraggingScrollbar;
    private float _dragStartY;
    private float _dragStartOffset;

    // Configuration
    public int Padding { get; set; } = 8;
    public int Spacing { get; set; } = 8;
    public int ScrollbarWidth { get; set; } = 8;
    public float ScrollSpeed { get; set; } = 40f;
    public float ScrollSmoothing { get; set; } = 10f;
    public bool ShowScrollbarAlways { get; set; } = false;
    public bool DrawBackground { get; set; } = true;
    public bool DrawBorder { get; set; } = true;
    public float BackgroundAlpha { get; set; } = 0.5f;

    /// <summary>
    /// Current scroll offset (0 = top)
    /// </summary>
    public float ScrollOffset
    {
        get => _scrollOffset;
        set => _scrollOffset = MathHelper.Clamp(value, 0, MaxScrollOffset);
    }

    /// <summary>
    /// Maximum scroll offset based on content height
    /// </summary>
    public float MaxScrollOffset => Math.Max(0, _contentHeight - ViewportHeight);

    /// <summary>
    /// Height of the visible area
    /// </summary>
    public float ViewportHeight => Size.Y - Padding * 2;

    /// <summary>
    /// Whether the content exceeds the viewport and scrolling is available
    /// </summary>
    public bool CanScroll => _contentHeight > ViewportHeight;

    /// <summary>
    /// Percentage scrolled (0 to 1)
    /// </summary>
    public float ScrollPercentage => MaxScrollOffset > 0 ? _scrollOffset / MaxScrollOffset : 0;

    public GleamScrollPanel(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    /// <summary>
    /// Recalculate content height based on children using vertical layout.
    /// Call this after adding/removing children.
    /// </summary>
    public void RefreshLayout()
    {
        float y = 0;
        int contentWidth = (int)(Size.X - Padding * 2 - (CanScroll || ShowScrollbarAlways ? ScrollbarWidth + 4 : 0));

        foreach (var child in Children)
        {
            child.Position = new Vector2(0, y);
            // Optionally resize width to fit
            if (child.Size.X <= 0 || child.Size.X > contentWidth)
            {
                child.Size = new Vector2(contentWidth, child.Size.Y);
            }
            y += child.Size.Y + Spacing;
        }

        _contentHeight = y > 0 ? y - Spacing : 0;

        // Clamp scroll if content shrank
        _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, MaxScrollOffset);
        _targetScrollOffset = _scrollOffset;
    }

    public override void Update(GameTime gameTime, GleamRenderer renderer)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Smooth scrolling
        if (Math.Abs(_targetScrollOffset - _scrollOffset) > 0.1f)
        {
            _scrollOffset = MathHelper.Lerp(_scrollOffset, _targetScrollOffset, deltaTime * ScrollSmoothing);
        }
        else
        {
            _scrollOffset = _targetScrollOffset;
        }

        // Clamp scroll
        _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, MaxScrollOffset);
        _targetScrollOffset = MathHelper.Clamp(_targetScrollOffset, 0, MaxScrollOffset);

        // Update children with offset positions
        base.Update(gameTime, renderer);
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        Rectangle bounds = Bounds;

        // Background
        if (DrawBackground)
        {
            renderer.DrawRect(spriteBatch, bounds, theme.DeepPurple, Alpha * BackgroundAlpha);
        }

        // Border
        if (DrawBorder)
        {
            Color borderColor = IsFocused ? theme.GoldBright : theme.Gold;
            renderer.DrawRectBorder(spriteBatch, bounds, borderColor, 2, Alpha);
        }

        // Scrollbar
        if (CanScroll || ShowScrollbarAlways)
        {
            DrawScrollbar(spriteBatch, renderer, bounds);
        }
    }

    /// <summary>
    /// Override Draw to implement content clipping via scissor rectangle.
    /// </summary>
    public override void Draw(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        if (!Visible) return;

        // Draw panel background/border first
        DrawSelf(spriteBatch, renderer);

        // Set up clipping for children
        Rectangle bounds = Bounds;
        Rectangle contentArea = new Rectangle(
            bounds.X + Padding,
            bounds.Y + Padding,
            (int)(bounds.Width - Padding * 2 - (CanScroll || ShowScrollbarAlways ? ScrollbarWidth + 4 : 0)),
            (int)(bounds.Height - Padding * 2)
        );

        // End current batch to apply scissor
        spriteBatch.End();

        // Store original scissor state
        var graphicsDevice = spriteBatch.GraphicsDevice;
        var originalScissor = graphicsDevice.ScissorRectangle;
        var originalRasterizerState = graphicsDevice.RasterizerState;

        // Set scissor rectangle for clipping
        graphicsDevice.ScissorRectangle = contentArea;

        // Begin with scissor test enabled
        var scissorRasterizer = new RasterizerState { ScissorTestEnable = true };
        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            rasterizerState: scissorRasterizer
        );

        // Draw children with scroll offset applied
        // Only add Padding offset (not full contentArea position) since
        // child.AbsolutePosition will add this panel's absolute position
        foreach (var child in Children)
        {
            if (!child.Visible) continue;

            // Check if child is in visible area
            float childTop = child.Position.Y - _scrollOffset;
            float childBottom = childTop + child.Size.Y;

            if (childBottom < 0 || childTop > ViewportHeight)
                continue; // Skip if completely out of view

            // Temporarily offset child for drawing (add padding and apply scroll)
            Vector2 originalPos = child.Position;
            child.Position = new Vector2(
                originalPos.X + Padding,
                originalPos.Y + Padding - _scrollOffset
            );

            child.Draw(spriteBatch, renderer);

            // Restore position
            child.Position = originalPos;
        }

        // End clipped batch
        spriteBatch.End();
        scissorRasterizer.Dispose();

        // Restore original state and resume normal batch
        graphicsDevice.ScissorRectangle = originalScissor;
        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            rasterizerState: originalRasterizerState
        );
    }

    private void DrawScrollbar(SpriteBatch spriteBatch, GleamRenderer renderer, Rectangle bounds)
    {
        var theme = renderer.Theme;

        int scrollbarX = bounds.Right - ScrollbarWidth - Padding;
        int scrollbarY = bounds.Y + Padding;
        int scrollbarHeight = bounds.Height - Padding * 2;

        // Track
        Rectangle track = new Rectangle(scrollbarX, scrollbarY, ScrollbarWidth, scrollbarHeight);
        renderer.DrawRect(spriteBatch, track, theme.DarkPurple, Alpha);

        if (!CanScroll) return;

        // Thumb
        float thumbRatio = ViewportHeight / _contentHeight;
        int thumbHeight = Math.Max(20, (int)(scrollbarHeight * thumbRatio));
        int maxThumbY = scrollbarHeight - thumbHeight;
        int thumbY = scrollbarY + (int)(maxThumbY * ScrollPercentage);

        Rectangle thumb = new Rectangle(scrollbarX, thumbY, ScrollbarWidth, thumbHeight);
        Color thumbColor = _isDraggingScrollbar ? theme.GoldBright : theme.Gold;
        renderer.DrawRect(spriteBatch, thumb, thumbColor, Alpha);
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        if (!Enabled || !Visible) return false;

        Rectangle bounds = Bounds;
        bool isOverPanel = bounds.Contains(mousePosition);

        // Handle scrollbar dragging
        if (_isDraggingScrollbar)
        {
            if (mouseDown)
            {
                float deltaY = mousePosition.Y - _dragStartY;
                float scrollbarHeight = bounds.Height - Padding * 2;
                float thumbRatio = ViewportHeight / _contentHeight;
                int thumbHeight = Math.Max(20, (int)(scrollbarHeight * thumbRatio));
                float scrollableTrack = scrollbarHeight - thumbHeight;

                if (scrollableTrack > 0)
                {
                    float scrollDelta = (deltaY / scrollableTrack) * MaxScrollOffset;
                    _targetScrollOffset = MathHelper.Clamp(_dragStartOffset + scrollDelta, 0, MaxScrollOffset);
                    _scrollOffset = _targetScrollOffset;
                }
                return true;
            }
            else
            {
                _isDraggingScrollbar = false;
            }
        }

        // Check scrollbar click
        if (isOverPanel && CanScroll && mouseClicked)
        {
            int scrollbarX = bounds.Right - ScrollbarWidth - Padding;
            if (mousePosition.X >= scrollbarX)
            {
                _isDraggingScrollbar = true;
                _dragStartY = mousePosition.Y;
                _dragStartOffset = _scrollOffset;
                return true;
            }
        }

        // Handle mouse wheel (check current mouse state)
        if (isOverPanel && CanScroll)
        {
            var currentMouse = Mouse.GetState();
            int wheelDelta = currentMouse.ScrollWheelValue;

            // We need to track previous wheel value - for now just check if scroll changed
            // This is a simplified approach; ideally track previous state
        }

        // Check children input (with scroll offset)
        Rectangle contentArea = new Rectangle(
            bounds.X + Padding,
            bounds.Y + Padding,
            (int)(bounds.Width - Padding * 2 - (CanScroll || ShowScrollbarAlways ? ScrollbarWidth + 4 : 0)),
            (int)(bounds.Height - Padding * 2)
        );

        if (contentArea.Contains(mousePosition))
        {
            // Translate mouse position for children
            Vector2 childMousePos = new Vector2(
                mousePosition.X - contentArea.X,
                mousePosition.Y - contentArea.Y + _scrollOffset
            );

            // Check children in reverse order (front to back)
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                var child = Children[i];
                if (!child.Visible || !child.Enabled) continue;

                // Check if child is in visible area
                float childTop = child.Position.Y - _scrollOffset;
                float childBottom = childTop + child.Size.Y;

                if (childBottom < 0 || childTop > ViewportHeight)
                    continue;

                // Check child bounds
                Rectangle childBounds = new Rectangle(
                    (int)child.Position.X,
                    (int)child.Position.Y,
                    (int)child.Size.X,
                    (int)child.Size.Y
                );

                if (childBounds.Contains(childMousePos))
                {
                    if (child.HandleInput(childMousePos, mouseDown, mouseClicked))
                        return true;
                }
            }
        }

        // Update hover state
        IsHovered = isOverPanel;

        return false;
    }

    /// <summary>
    /// Scroll by a delta amount (positive = down, negative = up)
    /// </summary>
    public void ScrollBy(float delta)
    {
        _targetScrollOffset = MathHelper.Clamp(_targetScrollOffset + delta, 0, MaxScrollOffset);
    }

    /// <summary>
    /// Scroll to top
    /// </summary>
    public void ScrollToTop()
    {
        _targetScrollOffset = 0;
    }

    /// <summary>
    /// Scroll to bottom
    /// </summary>
    public void ScrollToBottom()
    {
        _targetScrollOffset = MaxScrollOffset;
    }

    /// <summary>
    /// Ensure a child element is visible by scrolling if needed
    /// </summary>
    public void EnsureVisible(GleamElement child)
    {
        if (!Children.Contains(child)) return;

        float childTop = child.Position.Y;
        float childBottom = childTop + child.Size.Y;

        if (childTop < _scrollOffset)
        {
            _targetScrollOffset = childTop;
        }
        else if (childBottom > _scrollOffset + ViewportHeight)
        {
            _targetScrollOffset = childBottom - ViewportHeight;
        }
    }
}
