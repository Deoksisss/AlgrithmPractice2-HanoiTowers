using System;
using Godot;

namespace HanoiTowers.Gameplay;

public partial class RodView : Control
{
    [Signal]
    public delegate void RodClickedEventHandler(int rodIndex);

    public int RodIndex { get; set; }
    public string RodName { get; set; } = "A";

    public float PillarWidth { get; set; } = 14f;
    public float PillarHeight { get; set; } = 340f;
    public float BaseWidth { get; set; } = 280f;
    public float BaseHeight { get; set; } = 24f;

    private bool _isHovered;
    private bool _isValidDropTarget;
    private bool _isInvalidDropTarget;

    private readonly StyleBoxFlat _baseStyle = new()
    {
        BgColor = new Color("334155"), // Slate 700
        CornerRadiusTopLeft = 8,
        CornerRadiusTopRight = 8,
        CornerRadiusBottomLeft = 8,
        CornerRadiusBottomRight = 8,
        BorderWidthBottom = 4,
        BorderColor = new Color("1e293b")
    };

    private readonly StyleBoxFlat _pillarStyle = new()
    {
        BgColor = new Color("64748b"), // Slate 500
        CornerRadiusTopLeft = 6,
        CornerRadiusTopRight = 6,
        CornerRadiusBottomLeft = 0,
        CornerRadiusBottomRight = 0
    };

    private Label _nameLabel = null!;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(BaseWidth, PillarHeight + BaseHeight + 50);
        MouseFilter = MouseFilterEnum.Pass;

        _nameLabel = new Label
        {
            Text = RodName,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorRight = 1.0f,
            Position = new Vector2(0, PillarHeight + BaseHeight + 10)
        };
        _nameLabel.AddThemeFontSizeOverride("font_size", 20);
        _nameLabel.AddThemeColorOverride("font_color", new Color("cbd5e1"));
        AddChild(_nameLabel);
    }

    public void UpdateName(string name)
    {
        RodName = name;
        if (_nameLabel != null)
        {
            _nameLabel.Text = name;
        }
    }

    public void SetDropHighlight(bool isTarget, bool isValid)
    {
        _isValidDropTarget = isTarget && isValid;
        _isInvalidDropTarget = isTarget && !isValid;
        QueueRedraw();
    }

    public void ClearHighlight()
    {
        _isValidDropTarget = false;
        _isInvalidDropTarget = false;
        _isHovered = false;
        QueueRedraw();
    }

    public Vector2 GetDiskSpawnPosition(int slotIndex, float diskHeight)
    {
        // Calculate center bottom of the rod
        float centerX = Size.X / 2f;
        float baseY = PillarHeight; // top of base
        float y = baseY - (slotIndex * (diskHeight + 3f)) - diskHeight;
        return new Vector2(centerX, y);
    }

    public Vector2 GetTopPillarPosition()
    {
        float centerX = Size.X / 2f;
        return new Vector2(centerX, 20f);
    }

    public override void _Draw()
    {
        float centerX = Size.X / 2f;

        // Optional drop-zone aura highlight
        if (_isValidDropTarget)
        {
            var auraRect = new Rect2(centerX - (BaseWidth / 2f) - 10, 0, BaseWidth + 20, PillarHeight + BaseHeight + 10);
            DrawRect(auraRect, new Color(0.1f, 0.8f, 0.3f, 0.15f), true);
            DrawRect(auraRect, new Color(0.1f, 0.9f, 0.3f, 0.6f), false, 3f);
        }
        else if (_isInvalidDropTarget)
        {
            var auraRect = new Rect2(centerX - (BaseWidth / 2f) - 10, 0, BaseWidth + 20, PillarHeight + BaseHeight + 10);
            DrawRect(auraRect, new Color(0.9f, 0.2f, 0.2f, 0.15f), true);
            DrawRect(auraRect, new Color(0.9f, 0.2f, 0.2f, 0.6f), false, 3f);
        }

        // Draw pillar (vertical rod)
        var pillarRect = new Rect2(centerX - (PillarWidth / 2f), 10, PillarWidth, PillarHeight);
        DrawStyleBox(_pillarStyle, pillarRect);

        // Draw base (horizontal base platform)
        var baseRect = new Rect2(centerX - (BaseWidth / 2f), PillarHeight, BaseWidth, BaseHeight);
        DrawStyleBox(_baseStyle, baseRect);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.RodClicked, RodIndex);
        }
    }
}
