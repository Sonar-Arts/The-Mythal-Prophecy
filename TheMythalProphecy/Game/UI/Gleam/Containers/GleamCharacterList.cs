using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheMythalProphecy.Game.Entities;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Specialized list container for character items with selection and scrolling.
/// Displays GleamCharacterListItem children with keyboard navigation support.
/// </summary>
public class GleamCharacterList : GleamElement
{
    private readonly List<GleamCharacterListItem> _items = new();
    private int _selectedIndex = -1;
    private int _scrollOffset;
    private int _visibleItemCount;
    private int _hoveredIndex = -1;

    public int ItemHeight { get; set; } = 55;
    public int Padding { get; set; } = 4;
    public int MaxSlots { get; set; } = -1; // -1 = unlimited
    public bool ShowEmptySlots { get; set; }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            int maxIndex = GetMaxSelectableIndex();
            if (_selectedIndex != value && value >= -1 && value <= maxIndex)
            {
                _selectedIndex = value;
                UpdateItemSelectionState();
                OnSelectionChanged?.Invoke(this, _selectedIndex);
                EnsureVisible(_selectedIndex);
            }
        }
    }

    public Entity SelectedCharacter
    {
        get
        {
            if (_selectedIndex >= 0 && _selectedIndex < _items.Count)
            {
                var item = _items[_selectedIndex];
                return item.IsEmpty ? null : item.Character;
            }
            return null;
        }
    }

    public int ItemCount => _items.Count;
    public int CharacterCount => _items.FindAll(i => !i.IsEmpty).Count;

    public event Action<GleamCharacterList, int> OnSelectionChanged;
    public event Action<GleamCharacterList, int> OnItemActivated;

    public GleamCharacterList(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    private int GetMaxSelectableIndex()
    {
        // Can select empty slots if ShowEmptySlots is true
        if (ShowEmptySlots && MaxSlots > 0)
        {
            return MaxSlots - 1;
        }
        return _items.Count - 1;
    }

    private void UpdateItemSelectionState()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i].IsSelected = (i == _selectedIndex);
        }
    }

    public override void Update(GameTime gameTime, GleamRenderer renderer)
    {
        base.Update(gameTime, renderer);

        // Calculate visible items
        Rectangle contentArea = GetContentArea();
        _visibleItemCount = Math.Max(1, contentArea.Height / ItemHeight);

        // Update item positions based on scroll
        UpdateItemPositions();
    }

    private void UpdateItemPositions()
    {
        Rectangle contentArea = GetContentArea();
        int y = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            var item = _items[i];
            if (i >= _scrollOffset && i < _scrollOffset + _visibleItemCount)
            {
                item.Position = new Vector2(Padding, Padding + y);
                item.Visible = true;
                y += ItemHeight;
            }
            else
            {
                item.Visible = false;
            }
        }
    }

    private Rectangle GetContentArea()
    {
        Rectangle bounds = Bounds;
        return new Rectangle(
            bounds.X + Padding,
            bounds.Y + Padding,
            bounds.Width - Padding * 2,
            bounds.Height - Padding * 2
        );
    }

    private void EnsureVisible(int index)
    {
        if (index < 0) return;

        if (index < _scrollOffset)
        {
            _scrollOffset = index;
        }
        else if (index >= _scrollOffset + _visibleItemCount)
        {
            _scrollOffset = index - _visibleItemCount + 1;
        }
    }

    /// <summary>
    /// Set the characters to display in this list.
    /// </summary>
    public void SetCharacters(IReadOnlyList<Entity> characters)
    {
        // Clear existing items
        foreach (var item in _items)
        {
            RemoveChild(item);
        }
        _items.Clear();

        Rectangle contentArea = GetContentArea();
        float itemWidth = contentArea.Width;

        // Determine how many items to create
        int itemCount = characters.Count;
        if (ShowEmptySlots && MaxSlots > 0)
        {
            itemCount = MaxSlots;
        }

        // Create items
        for (int i = 0; i < itemCount; i++)
        {
            var item = new GleamCharacterListItem(Vector2.Zero, new Vector2(itemWidth, ItemHeight));

            if (i < characters.Count)
            {
                item.SetCharacter(characters[i], i);
            }
            else
            {
                item.SetEmpty(i);
            }

            item.OnSelected += OnItemSelected;
            item.OnActivated += OnItemActivatedHandler;

            _items.Add(item);
            AddChild(item);
        }

        // Reset selection
        _selectedIndex = -1;
        _scrollOffset = 0;
        UpdateItemPositions();
    }

    /// <summary>
    /// Clear all items from the list.
    /// </summary>
    public void ClearItems()
    {
        foreach (var item in _items)
        {
            RemoveChild(item);
        }
        _items.Clear();
        _selectedIndex = -1;
        _scrollOffset = 0;
    }

    /// <summary>
    /// Refresh stats for all character items.
    /// </summary>
    public void RefreshStats()
    {
        foreach (var item in _items)
        {
            item.UpdateStats();
        }
    }

    private void OnItemSelected(GleamCharacterListItem item)
    {
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            SelectedIndex = index;
        }
    }

    private void OnItemActivatedHandler(GleamCharacterListItem item)
    {
        int index = _items.IndexOf(item);
        if (index >= 0)
        {
            OnItemActivated?.Invoke(this, index);
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        Rectangle bounds = Bounds;

        // Background
        renderer.DrawRect(spriteBatch, bounds, theme.DeepPurple, Alpha);

        // Border (brighter when focused)
        Color borderColor = IsFocused ? theme.GoldBright : theme.Gold;
        renderer.DrawRectBorder(spriteBatch, bounds, borderColor, 2, Alpha);

        // Scrollbar
        if (_items.Count > _visibleItemCount)
        {
            DrawScrollbar(spriteBatch, renderer, bounds);
        }
    }

    private void DrawScrollbar(SpriteBatch spriteBatch, GleamRenderer renderer, Rectangle bounds)
    {
        var theme = renderer.Theme;
        int scrollbarWidth = 6;
        int scrollbarX = bounds.Right - scrollbarWidth - Padding;
        int scrollbarY = bounds.Y + Padding;
        int scrollbarHeight = bounds.Height - Padding * 2;

        // Track
        Rectangle track = new Rectangle(scrollbarX, scrollbarY, scrollbarWidth, scrollbarHeight);
        renderer.DrawRect(spriteBatch, track, theme.DarkPurple, Alpha);

        // Thumb
        float thumbHeight = Math.Max(20f, scrollbarHeight * ((float)_visibleItemCount / _items.Count));
        int maxScroll = Math.Max(1, _items.Count - _visibleItemCount);
        float thumbY = scrollbarY + (scrollbarHeight - thumbHeight) * (_scrollOffset / (float)maxScroll);

        Rectangle thumb = new Rectangle(scrollbarX, (int)thumbY, scrollbarWidth, (int)thumbHeight);
        renderer.DrawRect(spriteBatch, thumb, theme.Gold, Alpha);
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        if (!Enabled || !Visible) return false;

        Rectangle bounds = Bounds;
        IsHovered = bounds.Contains(mousePosition);

        // Reset hover state on items
        _hoveredIndex = -1;

        // Let items handle their own input
        bool consumed = base.HandleInput(mousePosition, mouseDown, mouseClicked);

        // Update hovered index
        if (IsHovered && !consumed)
        {
            Rectangle contentArea = GetContentArea();
            if (contentArea.Contains(mousePosition))
            {
                int relativeY = (int)mousePosition.Y - contentArea.Y;
                int hoverIndex = _scrollOffset + (relativeY / ItemHeight);
                if (hoverIndex >= 0 && hoverIndex < _items.Count)
                {
                    _hoveredIndex = hoverIndex;
                }
            }
        }

        return consumed;
    }

    // Navigation methods
    public void SelectNext()
    {
        int maxIndex = GetMaxSelectableIndex();
        if (maxIndex >= 0)
        {
            SelectedIndex = (_selectedIndex + 1) % (maxIndex + 1);
        }
    }

    public void SelectPrevious()
    {
        int maxIndex = GetMaxSelectableIndex();
        if (maxIndex >= 0)
        {
            SelectedIndex = _selectedIndex <= 0 ? maxIndex : _selectedIndex - 1;
        }
    }

    public void ScrollUp() => _scrollOffset = Math.Max(0, _scrollOffset - 1);
    public void ScrollDown() => _scrollOffset = Math.Min(Math.Max(0, _items.Count - _visibleItemCount), _scrollOffset + 1);
}
