using System;
using Godot;

namespace HanoiTowers.Gameplay;

public partial class DiskView : Control
{
    [Signal]
    public delegate void DragStartedEventHandler(DiskView disk);

    [Signal]
    public delegate void DragUpdatedEventHandler(DiskView disk, Vector2 globalPos);

    [Signal]
    public delegate void DragEndedEventHandler(DiskView disk, Vector2 globalPos);

    [Signal]
    public delegate void DiskClickedEventHandler(DiskView disk);

    public int SizeIndex { get; private set; } // 1 (smallest) to N (largest)
    public int CurrentRod { get; set; } = 0;
    public bool IsDraggable { get; set; } = false;

    private bool _isDragging = false;
    private Vector2 _dragOffset;
    private Vector2 _homePosition;
    private Color _baseColor;
    private readonly StyleBoxFlat _diskStyle = new();
    private Label _label = null!;
    private Tween? _activeTween;

    // Vibrant modern color palette for disks 1-10
    private static readonly Color[] Palette = new[]
    {
        new Color("f43f5e"), // 1: Rose / Red
        new Color("fb923c"), // 2: Coral / Orange
        new Color("f59e0b"), // 3: Amber / Yellow
        new Color("10b981"), // 4: Emerald / Green
        new Color("06b6d4"), // 5: Cyan / Teal
        new Color("3b82f6"), // 6: Blue
        new Color("6366f1"), // 7: Indigo
        new Color("8b5cf6"), // 8: Purple
        new Color("d946ef"), // 9: Fuchsia
        new Color("64748b")  // 10: Slate
    };

    public void Setup(int sizeIndex, int totalDisks, float pillarBaseWidth, float diskHeight = 28f)
    {
        SizeIndex = sizeIndex;
        _baseColor = Palette[(sizeIndex - 1) % Palette.Length];

        // Width scales from min 70px to max pillarBaseWidth - 20px
        float minWidth = 70f;
        float maxWidth = Math.Max(220f, pillarBaseWidth - 30f);
        float width = minWidth + ((maxWidth - minWidth) * (sizeIndex - 1) / Math.Max(1, totalDisks - 1));

        CustomMinimumSize = new Vector2(width, diskHeight);
        Size = CustomMinimumSize;
        PivotOffset = Size / 2f;

        _diskStyle.BgColor = _baseColor;
        _diskStyle.CornerRadiusTopLeft = 8;
        _diskStyle.CornerRadiusTopRight = 8;
        _diskStyle.CornerRadiusBottomLeft = 8;
        _diskStyle.CornerRadiusBottomRight = 8;
        _diskStyle.ShadowColor = new Color(0, 0, 0, 0.35f);
        _diskStyle.ShadowSize = 4;
        _diskStyle.ShadowOffset = new Vector2(0, 2);
        _diskStyle.BorderWidthBottom = 2;
        _diskStyle.BorderColor = _baseColor.Darkened(0.3f);

        _label = new Label
        {
            Text = sizeIndex.ToString(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        _label.AddThemeFontSizeOverride("font_size", Math.Max(13, (int)diskHeight - 12));
        _label.AddThemeColorOverride("font_color", Colors.White);
        AddChild(_label);

        MouseFilter = MouseFilterEnum.Stop;
    }

    public void SetHomePosition(Vector2 pos)
    {
        _homePosition = pos;
    }

    public Vector2 GetHomePosition() => _homePosition;

    public override void _Draw()
    {
        DrawStyleBox(_diskStyle, new Rect2(Vector2.Zero, Size));
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!IsDraggable) return;

        if (@event is InputEventMouseButton mouseBtn)
        {
            if (mouseBtn.ButtonIndex == MouseButton.Left)
            {
                if (mouseBtn.Pressed)
                {
                    StartDrag(mouseBtn.GlobalPosition);
                }
                else if (_isDragging)
                {
                    EndDrag(mouseBtn.GlobalPosition);
                }
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isDragging)
        {
            GlobalPosition = mouseMotion.GlobalPosition - _dragOffset;
            EmitSignal(SignalName.DragUpdated, this, mouseMotion.GlobalPosition);
        }
    }

    public void StartDrag(Vector2 globalMousePos)
    {
        _isDragging = true;
        _dragOffset = globalMousePos - GlobalPosition;
        ZIndex = 100;
        Scale = new Vector2(1.05f, 1.05f);
        EmitSignal(SignalName.DragStarted, this);
    }

    public void EndDrag(Vector2 globalMousePos)
    {
        if (!_isDragging) return;
        _isDragging = false;
        Scale = Vector2.One;
        EmitSignal(SignalName.DragEnded, this, globalMousePos);
    }

    public void AnimateTo(Vector2 targetPos, float duration = 0.25f, Action? onComplete = null)
    {
        _activeTween?.Kill();
        _activeTween = CreateTween();
        _activeTween.SetTrans(Tween.TransitionType.Cubic);
        _activeTween.SetEase(Tween.EaseType.Out);
        _activeTween.TweenProperty(this, "position", targetPos, duration);
        _activeTween.TweenCallback(Callable.From(() =>
        {
            ZIndex = 0;
            _homePosition = targetPos;
            onComplete?.Invoke();
        }));
    }

    public void AnimateArcTo(Vector2 peakPos, Vector2 targetPos, float duration, Action? onComplete = null)
    {
        _activeTween?.Kill();
        _activeTween = CreateTween();
        _activeTween.SetTrans(Tween.TransitionType.Quad);
        _activeTween.SetEase(Tween.EaseType.Out);

        float half = duration * 0.5f;
        // 1. Up to peak
        _activeTween.TweenProperty(this, "position", peakPos, half);
        // 2. Down to target
        _activeTween.SetEase(Tween.EaseType.In);
        _activeTween.TweenProperty(this, "position", targetPos, half);

        _activeTween.TweenCallback(Callable.From(() =>
        {
            ZIndex = 0;
            _homePosition = targetPos;
            onComplete?.Invoke();
        }));
    }

    public void ShakeAndReturn(Vector2 returnPos, float duration = 0.25f, Action? onComplete = null)
    {
        _activeTween?.Kill();
        _activeTween = CreateTween();
        
        // Shake sequence
        Vector2 startPos = Position;
        float shakeDist = 12f;
        _activeTween.TweenProperty(this, "position", startPos + new Vector2(-shakeDist, 0), 0.04f);
        _activeTween.TweenProperty(this, "position", startPos + new Vector2(shakeDist, 0), 0.04f);
        _activeTween.TweenProperty(this, "position", startPos + new Vector2(-shakeDist * 0.5f, 0), 0.04f);
        _activeTween.TweenProperty(this, "position", startPos, 0.04f);

        // Flash red style
        _diskStyle.BgColor = new Color("ef4444");
        QueueRedraw();

        _activeTween.TweenInterval(0.06f);
        _activeTween.TweenCallback(Callable.From(() =>
        {
            _diskStyle.BgColor = _baseColor;
            QueueRedraw();
        }));

        // Return to home
        _activeTween.SetTrans(Tween.TransitionType.Cubic);
        _activeTween.SetEase(Tween.EaseType.Out);
        _activeTween.TweenProperty(this, "position", returnPos, duration);
        _activeTween.TweenCallback(Callable.From(() =>
        {
            ZIndex = 0;
            _homePosition = returnPos;
            onComplete?.Invoke();
        }));
    }

    public void SetHovered(bool hovered)
    {
        if (_isDragging) return;
        Scale = hovered ? new Vector2(1.03f, 1.03f) : Vector2.One;
    }
}
