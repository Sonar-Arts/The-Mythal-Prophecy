using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheMythalProphecy.Game.Entities;
using TheMythalProphecy.Game.Entities.Components;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Rich character display widget for party lists.
/// Shows character name, level, and visual HP/MP progress bars.
/// </summary>
public class GleamCharacterListItem : GleamElement
{
    private const int Padding = 8;
    private const int BarHeight = 14;
    private const int BarSpacing = 4;
    private const int TopRowHeight = 22;

    private readonly GleamLabel _nameLabel;
    private readonly GleamLabel _levelLabel;
    private readonly GleamProgressBar _hpBar;
    private readonly GleamProgressBar _mpBar;

    public bool IsSelected { get; set; }
    public bool IsEmpty { get; private set; }
    public int SlotIndex { get; private set; }
    public Entity Character { get; private set; }

    public event Action<GleamCharacterListItem> OnSelected;
    public event Action<GleamCharacterListItem> OnActivated;

    public GleamCharacterListItem(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;

        float contentWidth = size.X - Padding * 2;
        float barWidth = (contentWidth - BarSpacing) / 2;

        // Name label (left side)
        _nameLabel = new GleamLabel("", new Vector2(Padding, Padding), new Vector2(contentWidth * 0.7f, TopRowHeight))
        {
            Alignment = TextAlignment.Left,
            ShowShadow = true
        };
        AddChild(_nameLabel);

        // Level label (right side)
        _levelLabel = new GleamLabel("", new Vector2(Padding + contentWidth * 0.7f, Padding), new Vector2(contentWidth * 0.3f, TopRowHeight))
        {
            Alignment = TextAlignment.Right,
            ShowShadow = true
        };
        AddChild(_levelLabel);

        // HP bar (bottom left)
        float barY = Padding + TopRowHeight + BarSpacing;
        _hpBar = new GleamProgressBar(new Vector2(Padding, barY), new Vector2(barWidth, BarHeight), 100f)
        {
            FillColor = new Color(100, 220, 100),
            LowFillColor = new Color(220, 60, 60),
            LowThreshold = 0.25f,
            ShowText = true,
            TextFormat = "HP:{0}/{1}"
        };
        AddChild(_hpBar);

        // MP bar (bottom right)
        _mpBar = new GleamProgressBar(new Vector2(Padding + barWidth + BarSpacing, barY), new Vector2(barWidth, BarHeight), 100f)
        {
            FillColor = new Color(80, 140, 220),
            LowFillColor = new Color(100, 80, 160),
            LowThreshold = 0.2f,
            ShowText = true,
            TextFormat = "MP:{0}/{1}"
        };
        AddChild(_mpBar);
    }

    /// <summary>
    /// Bind a character to this item.
    /// </summary>
    public void SetCharacter(Entity character, int slotIndex)
    {
        Character = character;
        SlotIndex = slotIndex;
        IsEmpty = false;

        UpdateStats();
    }

    /// <summary>
    /// Configure as an empty slot.
    /// </summary>
    public void SetEmpty(int slotIndex)
    {
        Character = null;
        SlotIndex = slotIndex;
        IsEmpty = true;

        _nameLabel.Text = $"{slotIndex + 1}. [Empty Slot]";
        _nameLabel.TextColor = null; // Use theme default (dimmed)
        _levelLabel.Text = "";
        _hpBar.Visible = false;
        _mpBar.Visible = false;
    }

    /// <summary>
    /// Refresh HP/MP values from the character's StatsComponent.
    /// </summary>
    public void UpdateStats()
    {
        if (Character == null || IsEmpty) return;

        var stats = Character.GetComponent<StatsComponent>();
        if (stats == null) return;

        _nameLabel.Text = $"{SlotIndex + 1}. {Character.Name}";
        _nameLabel.TextColor = null; // Use theme default
        _levelLabel.Text = $"Lv.{stats.Level}";
        _levelLabel.TextColor = null;

        _hpBar.MaxValue = stats.MaxHP;
        _hpBar.CurrentValue = stats.CurrentHP;
        _hpBar.Visible = true;

        _mpBar.MaxValue = stats.MaxMP;
        _mpBar.CurrentValue = stats.CurrentMP;
        _mpBar.Visible = true;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        Rectangle bounds = Bounds;

        // Selection/hover background
        if (IsSelected)
        {
            renderer.DrawRect(spriteBatch, bounds, theme.MidPurple, Alpha);
        }
        else if (IsHovered && !IsEmpty)
        {
            renderer.DrawRect(spriteBatch, bounds, theme.MutedPurple, Alpha * 0.7f);
        }

        // Dimmed appearance for empty slots
        if (IsEmpty)
        {
            _nameLabel.Alpha = 0.5f;
        }
        else
        {
            _nameLabel.Alpha = 1f;
        }
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        if (!Enabled || !Visible) return false;

        Rectangle bounds = Bounds;
        bool wasHovered = IsHovered;
        IsHovered = bounds.Contains(mousePosition);

        if (IsHovered && mouseClicked)
        {
            if (IsSelected)
            {
                OnActivated?.Invoke(this);
            }
            else
            {
                OnSelected?.Invoke(this);
            }
            return true;
        }

        return false;
    }
}
