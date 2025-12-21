using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;
using TheMythalProphecy.Game.UI.Components;

namespace TheMythalProphecy.Game.UI;

/// <summary>
/// Manages UI element hierarchy, input routing, and rendering
/// </summary>
public class UIManager
{
    private readonly List<UIElement> _rootElements = new();
    private UIElement _focusedElement;
    private UIElement _activeModal;
    private Texture2D _pixelTexture;

    public UITheme Theme { get; private set; }
    public GraphicsDevice GraphicsDevice { get; private set; }

    // Input state
    private MouseState _previousMouseState;
    private MouseState _currentMouseState;

    public UIManager(GraphicsDevice graphicsDevice)
    {
        GraphicsDevice = graphicsDevice;
        Theme = new UITheme();

        // Create a 1x1 white pixel texture for drawing solid colors
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    /// <summary>
    /// Initialize the UI manager with a theme
    /// </summary>
    public void Initialize(UITheme theme = null)
    {
        if (theme != null)
        {
            Theme = theme;
        }
    }

    /// <summary>
    /// Get the 1x1 pixel texture for drawing solid colors and rectangles
    /// </summary>
    public Texture2D PixelTexture => _pixelTexture;

    /// <summary>
    /// Add a root UI element
    /// </summary>
    public void AddElement(UIElement element)
    {
        if (!_rootElements.Contains(element))
        {
            _rootElements.Add(element);
        }
    }

    /// <summary>
    /// Remove a root UI element
    /// </summary>
    public void RemoveElement(UIElement element)
    {
        _rootElements.Remove(element);

        if (_focusedElement == element)
        {
            _focusedElement = null;
        }
    }

    /// <summary>
    /// Clear all root UI elements
    /// </summary>
    public void Clear()
    {
        _rootElements.Clear();
        _focusedElement = null;
        _activeModal = null;
    }

    /// <summary>
    /// Set focus on a specific UI element
    /// </summary>
    public void SetFocus(UIElement element)
    {
        if (_focusedElement == element) return;

        // Remove focus from previous element
        _focusedElement?.SetFocus(false);

        // Set focus on new element
        _focusedElement = element;
        _focusedElement?.SetFocus(true);
    }

    /// <summary>
    /// Get the currently focused element
    /// </summary>
    public UIElement GetFocusedElement() => _focusedElement;

    /// <summary>
    /// Update all UI elements and handle input
    /// </summary>
    public void Update(GameTime gameTime)
    {
        // Update input state
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();

        // Get mouse position
        Vector2 mousePosition = new Vector2(_currentMouseState.X, _currentMouseState.Y);
        bool mouseClicked = _currentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed &&
                           _previousMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released;

        // Update all root elements (sorted by Z-index, highest first)
        var sortedElements = _rootElements.OrderByDescending(e => e.ZIndex).ToList();

        foreach (var element in sortedElements)
        {
            element.Update(gameTime);
        }

        // Handle input - if modal is active, only route input to the modal
        if (_activeModal != null && _activeModal.Visible && _activeModal.Enabled)
        {
            // Modal blocks all input - only the modal receives it
            _activeModal.HandleInput(mousePosition, mouseClicked);
        }
        else
        {
            // Normal input routing (from front to back)
            bool inputHandled = false;
            for (int i = 0; i < sortedElements.Count && !inputHandled; i++)
            {
                inputHandled = sortedElements[i].HandleInput(mousePosition, mouseClicked);
            }
        }
    }

    /// <summary>
    /// Draw all UI elements
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        // Draw all root elements (sorted by Z-index, lowest first)
        var sortedElements = _rootElements.OrderBy(e => e.ZIndex).ToList();

        foreach (var element in sortedElements)
        {
            element.Draw(spriteBatch, Theme);
        }
    }

    /// <summary>
    /// Show a modal dialog (brings to front and blocks input to other elements)
    /// </summary>
    public void ShowModal(UIElement modal)
    {
        // Find highest Z-index
        int maxZ = _rootElements.Count > 0 ? _rootElements.Max(e => e.ZIndex) : 0;

        // Set modal to highest Z + 1
        modal.ZIndex = maxZ + 1;

        AddElement(modal);
        SetFocus(modal);

        // Set as active modal to block input to other elements
        _activeModal = modal;
    }

    /// <summary>
    /// Close a modal dialog
    /// </summary>
    public void CloseModal(UIElement modal)
    {
        RemoveElement(modal);

        // Clear active modal if this was it
        if (_activeModal == modal)
        {
            _activeModal = null;
        }
    }

    /// <summary>
    /// Find UI element at a specific position
    /// </summary>
    public UIElement FindElementAt(Vector2 position)
    {
        var sortedElements = _rootElements.OrderByDescending(e => e.ZIndex).ToList();

        foreach (var element in sortedElements)
        {
            if (element.Visible && element.Bounds.Contains(position))
            {
                return FindElementAtRecursive(element, position);
            }
        }

        return null;
    }

    private UIElement FindElementAtRecursive(UIElement element, Vector2 position)
    {
        // Check children first (front to back)
        for (int i = element.Children.Count - 1; i >= 0; i--)
        {
            var child = element.Children[i];
            if (child.Visible && child.Bounds.Contains(position))
            {
                var found = FindElementAtRecursive(child, position);
                if (found != null) return found;
            }
        }

        // Return this element if no children matched
        return element;
    }
}
