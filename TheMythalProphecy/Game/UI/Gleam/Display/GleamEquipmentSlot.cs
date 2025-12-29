using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheMythalProphecy.Game.UI.Gleam;

/// <summary>
/// Clean single-line equipment slot display.
/// Format: "SlotType: Item Name" with formatted item names.
/// </summary>
public class GleamEquipmentSlot : GleamElement
{
    private const int Padding = 8;

    /// <summary>
    /// Equipment slot type (e.g., "Weapon", "Armor", "Accessory")
    /// </summary>
    public string SlotType { get; set; } = "Slot";

    /// <summary>
    /// Name of the equipped item (null or empty for unequipped)
    /// </summary>
    public string ItemName { get; set; }

    /// <summary>
    /// Whether the slot is empty (no item equipped)
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(ItemName);

    public GleamEquipmentSlot(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }

    /// <summary>
    /// Set the equipment slot data.
    /// </summary>
    public void SetItem(string slotType, string itemName, Texture2D icon = null)
    {
        SlotType = slotType;
        ItemName = itemName;
    }

    /// <summary>
    /// Clear the slot (set to empty state).
    /// </summary>
    public void Clear()
    {
        ItemName = null;
    }

    /// <summary>
    /// Format item name from snake_case to Title Case.
    /// Example: "mystic_staff" -> "Mystic Staff"
    /// </summary>
    private string FormatItemName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "---";

        var sb = new StringBuilder();
        bool capitalizeNext = true;

        foreach (char c in rawName)
        {
            if (c == '_' || c == '-')
            {
                sb.Append(' ');
                capitalizeNext = true;
            }
            else if (capitalizeNext)
            {
                sb.Append(char.ToUpper(c));
                capitalizeNext = false;
            }
            else
            {
                sb.Append(char.ToLower(c));
            }
        }

        return sb.ToString();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch, GleamRenderer renderer)
    {
        var theme = renderer.Theme;
        var font = theme.DefaultFont;
        Rectangle bounds = Bounds;

        if (font == null) return;

        // Build display text
        string slotLabel = $"{SlotType}: ";
        string itemText = IsEmpty ? "---" : FormatItemName(ItemName);

        // Measure text for positioning
        Vector2 slotSize = font.MeasureString(slotLabel);
        Vector2 itemSize = font.MeasureString(itemText);
        float totalWidth = slotSize.X + itemSize.X;

        // Center vertically
        float textY = bounds.Y + (bounds.Height - slotSize.Y) / 2f;
        float textX = bounds.X + Padding;

        // Draw slot type in gold
        renderer.DrawText(spriteBatch, font, slotLabel,
            new Vector2(textX, textY), theme.Gold, true, Alpha);

        // Draw item name in white (or disabled if empty)
        Color itemColor = IsEmpty ? theme.TextDisabled : theme.TextPrimary;
        renderer.DrawText(spriteBatch, font, itemText,
            new Vector2(textX + slotSize.X, textY), itemColor, true, Alpha);

        // Subtle bottom separator line
        renderer.DrawRect(spriteBatch,
            new Rectangle(bounds.X + Padding, bounds.Bottom - 1, bounds.Width - Padding * 2, 1),
            theme.GoldDim, Alpha * 0.2f);
    }

    public override bool HandleInput(Vector2 mousePosition, bool mouseDown, bool mouseClicked)
    {
        // Equipment slots are display-only
        return false;
    }
}
