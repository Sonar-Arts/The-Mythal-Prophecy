using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Centralized rendering utilities for GleamUI components.
/// Provides cosmic-themed drawing primitives with optional shader effects.
/// </summary>
public class GleamRenderer
{
    private Texture2D _pixelTexture;
    private Effect _shimmerEffect;
    private Effect _nebulaEffect;
    private IGleamTheme _theme;
    private GraphicsDevice _graphicsDevice;

    public IGleamTheme Theme => _theme;
    public Texture2D PixelTexture => _pixelTexture;
    public bool ShadersLoaded => _shimmerEffect != null;

    public void Initialize(GraphicsDevice graphicsDevice, ContentManager content, IGleamTheme theme)
    {
        _graphicsDevice = graphicsDevice;
        _theme = theme;

        // Create 1x1 white pixel for solid color drawing
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });

        // Load shaders (optional - graceful fallback if missing)
        try
        {
            _shimmerEffect = content.Load<Effect>("Effects/LogoShimmer");
            _nebulaEffect = content.Load<Effect>("Effects/Nebula");
        }
        catch
        {
            // Shaders are optional - continue without them
        }
    }

    /// <summary>
    /// Draws a parallelogram (slanted rectangle) using horizontal scan lines.
    /// The parallelogram is centered within the bounds.
    /// </summary>
    public void DrawParallelogram(SpriteBatch spriteBatch, Rectangle bounds, Color color, float skewFactor, float alpha = 1f)
    {
        if (_pixelTexture == null) return;

        float skewOffset = bounds.Height * skewFactor;
        float centerOffset = skewOffset / 2f;

        // Calculate parallelogram corners (centered within bounds)
        var topLeft = new Vector2(bounds.X + skewOffset - centerOffset, bounds.Y);
        var topRight = new Vector2(bounds.Right + skewOffset - centerOffset, bounds.Y);
        var bottomLeft = new Vector2(bounds.X - centerOffset, bounds.Bottom);
        var bottomRight = new Vector2(bounds.Right - centerOffset, bounds.Bottom);

        // Draw as horizontal scan lines for filled shape
        for (int y = 0; y < bounds.Height; y++)
        {
            float t = y / (float)bounds.Height;
            float leftX = MathHelper.Lerp(topLeft.X, bottomLeft.X, t);
            float rightX = MathHelper.Lerp(topRight.X, bottomRight.X, t);
            int posY = bounds.Y + y;

            spriteBatch.Draw(
                _pixelTexture,
                new Rectangle((int)leftX, posY, (int)(rightX - leftX), 1),
                color * alpha
            );
        }
    }

    /// <summary>
    /// Draws a parallelogram border (outline only).
    /// The parallelogram is centered within the bounds.
    /// </summary>
    public void DrawParallelogramBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, float skewFactor, int thickness, float alpha = 1f)
    {
        if (_pixelTexture == null) return;

        float skewOffset = bounds.Height * skewFactor;
        float centerOffset = skewOffset / 2f;

        var topLeft = new Vector2(bounds.X + skewOffset - centerOffset, bounds.Y);
        var topRight = new Vector2(bounds.Right + skewOffset - centerOffset, bounds.Y);
        var bottomRight = new Vector2(bounds.Right - centerOffset, bounds.Bottom);
        var bottomLeft = new Vector2(bounds.X - centerOffset, bounds.Bottom);

        DrawLine(spriteBatch, topLeft, topRight, thickness, color * alpha);
        DrawLine(spriteBatch, topRight, bottomRight, thickness, color * alpha);
        DrawLine(spriteBatch, bottomRight, bottomLeft, thickness, color * alpha);
        DrawLine(spriteBatch, bottomLeft, topLeft, thickness, color * alpha);
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    public void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, int thickness, Color color)
    {
        if (_pixelTexture == null) return;

        var edge = end - start;
        float length = edge.Length();
        float angle = MathF.Atan2(edge.Y, edge.X);

        spriteBatch.Draw(
            _pixelTexture,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0f
        );
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    public void DrawRect(SpriteBatch spriteBatch, Rectangle bounds, Color color, float alpha = 1f)
    {
        if (_pixelTexture == null) return;
        spriteBatch.Draw(_pixelTexture, bounds, color * alpha);
    }

    /// <summary>
    /// Draws a rectangle border (outline only).
    /// </summary>
    public void DrawRectBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness, float alpha = 1f)
    {
        if (_pixelTexture == null) return;
        var c = color * alpha;

        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), c);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), c);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), c);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), c);
    }

    /// <summary>
    /// Draws a horizontal slider track and fill.
    /// </summary>
    public void DrawSliderTrack(SpriteBatch spriteBatch, Rectangle bounds, float percentage, Color trackColor, Color fillColor, float alpha = 1f)
    {
        if (_pixelTexture == null) return;

        // Track background
        spriteBatch.Draw(_pixelTexture, bounds, trackColor * alpha);

        // Fill based on percentage
        int fillWidth = (int)(bounds.Width * MathHelper.Clamp(percentage, 0f, 1f));
        if (fillWidth > 0)
        {
            spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height), fillColor * alpha);
        }
    }

    /// <summary>
    /// Draws centered text with optional shadow.
    /// </summary>
    public void DrawTextCentered(SpriteBatch spriteBatch, SpriteFont font, string text, Rectangle bounds, Color color, bool withShadow = false, float alpha = 1f)
    {
        if (font == null || string.IsNullOrEmpty(text)) return;

        var textSize = font.MeasureString(text);
        var position = new Vector2(
            bounds.X + (bounds.Width - textSize.X) / 2f,
            bounds.Y + (bounds.Height - textSize.Y) / 2f
        );

        if (withShadow)
        {
            spriteBatch.DrawString(font, text, position + new Vector2(1, 1), Color.Black * alpha * 0.5f);
        }

        spriteBatch.DrawString(font, text, position, color * alpha);
    }

    /// <summary>
    /// Draws text at a specific position.
    /// </summary>
    public void DrawText(SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, bool withShadow = false, float alpha = 1f)
    {
        if (font == null || string.IsNullOrEmpty(text)) return;

        if (withShadow)
        {
            spriteBatch.DrawString(font, text, position + new Vector2(1, 1), Color.Black * alpha * 0.5f);
        }

        spriteBatch.DrawString(font, text, position, color * alpha);
    }

    /// <summary>
    /// Begins a sprite batch with shimmer shader effect.
    /// </summary>
    public bool BeginShimmerPass(SpriteBatch spriteBatch, float shimmerPhase)
    {
        if (_shimmerEffect == null) return false;

        _shimmerEffect.Parameters["ShimmerPhase"]?.SetValue(shimmerPhase);

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            effect: _shimmerEffect
        );
        return true;
    }

    /// <summary>
    /// Begins a sprite batch with nebula shader effect.
    /// </summary>
    public bool BeginNebulaPass(SpriteBatch spriteBatch, float time, float intensity = 0.5f)
    {
        if (_nebulaEffect == null) return false;

        _nebulaEffect.Parameters["Time"]?.SetValue(time);
        _nebulaEffect.Parameters["Intensity"]?.SetValue(intensity);

        spriteBatch.Begin(
            blendState: BlendState.AlphaBlend,
            effect: _nebulaEffect
        );
        return true;
    }

    /// <summary>
    /// Draws nebula background effect on a rectangle.
    /// Falls back to solid color if shader not available.
    /// </summary>
    public void DrawNebulaBackground(SpriteBatch spriteBatch, Rectangle bounds, float time, float intensity = 0.5f)
    {
        spriteBatch.End();

        if (BeginNebulaPass(spriteBatch, time, intensity))
        {
            spriteBatch.Draw(_pixelTexture, bounds, Color.White);
            spriteBatch.End();
        }
        else
        {
            // Fallback: draw solid dark purple
            spriteBatch.Begin();
            DrawRect(spriteBatch, bounds, _theme.DeepPurple, 0.8f);
            spriteBatch.End();
        }

        spriteBatch.Begin();
    }

    /// <summary>
    /// Checks if a point is inside a parallelogram (centered within bounds).
    /// </summary>
    public bool IsPointInParallelogram(Vector2 point, Rectangle bounds, float skewFactor)
    {
        float skewOffset = bounds.Height * skewFactor;
        float centerOffset = skewOffset / 2f;
        float relY = point.Y - bounds.Y;
        float t = relY / bounds.Height;

        if (t < 0 || t > 1) return false;

        // At t=0 (top): left edge at bounds.X + skewOffset/2, right edge at bounds.Right + skewOffset/2
        // At t=1 (bottom): left edge at bounds.X - skewOffset/2, right edge at bounds.Right - skewOffset/2
        float leftX = bounds.X + skewOffset * (1 - t) - centerOffset;
        float rightX = bounds.Right + skewOffset * (1 - t) - centerOffset;

        return point.X >= leftX && point.X <= rightX &&
               point.Y >= bounds.Y && point.Y <= bounds.Bottom;
    }
}
