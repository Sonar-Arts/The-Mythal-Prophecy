using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.Entities.Components;

/// <summary>
/// Component that handles entity visual representation
/// </summary>
public class SpriteComponent : IComponent
{
    public Entity Owner { get; set; }
    public bool Enabled { get; set; } = true;

    public Texture2D Texture { get; set; }
    public Rectangle SourceRectangle { get; set; }
    public Vector2 Origin { get; set; }
    public Color Tint { get; set; } = Color.White;
    public float Scale { get; set; } = 1f;
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    public float LayerDepth { get; set; } = 0.5f;

    private TransformComponent _transform;

    /// <summary>
    /// Parameterless constructor for deferred texture initialization
    /// </summary>
    public SpriteComponent()
    {
        Texture = null;
        SourceRectangle = Rectangle.Empty;
        Origin = Vector2.Zero;
    }

    public SpriteComponent(Texture2D texture)
    {
        Texture = texture;
        SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        Origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
    }

    public SpriteComponent(Texture2D texture, Rectangle sourceRect)
    {
        Texture = texture;
        SourceRectangle = sourceRect;
        Origin = new Vector2(sourceRect.Width / 2f, sourceRect.Height / 2f);
    }

    public void Initialize()
    {
        _transform = Owner.GetComponent<TransformComponent>();
    }

    public void Update(GameTime gameTime)
    {
        // Sprite doesn't need per-frame updates unless animated
    }

    /// <summary>
    /// Draw the sprite at the entity's position
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (!Enabled || Texture == null || _transform == null) return;

        spriteBatch.Draw(
            Texture,
            _transform.Position,
            SourceRectangle,
            Tint,
            _transform.Rotation,
            Origin,
            Scale,
            Effects,
            LayerDepth
        );
    }

    /// <summary>
    /// Set the source rectangle for sprite sheet animation
    /// </summary>
    public void SetSourceRectangle(int x, int y, int width, int height)
    {
        SourceRectangle = new Rectangle(x, y, width, height);
        Origin = new Vector2(width / 2f, height / 2f);
    }
}
