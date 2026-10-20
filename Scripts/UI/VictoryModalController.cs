using System;
using Godot;
using HanoiTowers.Core;

namespace HanoiTowers.UI;

public partial class VictoryModalController : Control
{
    [Signal]
    public delegate void NextLevelClickedEventHandler();

    [Signal]
    public delegate void RetryClickedEventHandler();

    [Signal]
    public delegate void MenuClickedEventHandler();

    private PanelContainer _cardPanel = null!;
    private Label _titleLabel = null!;
    private HBoxContainer _starsContainer = null!;
    private Label _movesLabel = null!;
    private Label _optimalLabel = null!;
    private Label _timeLabel = null!;
    private Label _ratingCommentLabel = null!;
    private Button _nextLevelBtn = null!;
    private Button _retryBtn = null!;
    private Button _menuBtn = null!;

    public override void _Ready()
    {
        Visible = false;
        SetupUI();
    }

    private void SetupUI()
    {
        // Full screen dimmed background
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;

        var dimBg = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.65f),
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        AddChild(dimBg);

        _cardPanel = new PanelContainer
        {
            CustomMinimumSize = new Vector2(460, 420),
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -230,
            OffsetTop = -210,
            OffsetRight = 230,
            OffsetBottom = 210
        };

        var panelStyle = new StyleBoxFlat
        {
            BgColor = new Color("0f172a"), // Slate 900
            CornerRadiusTopLeft = 16,
            CornerRadiusTopRight = 16,
            CornerRadiusBottomLeft = 16,
            CornerRadiusBottomRight = 16,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderColor = new Color("38bdf8"), // Sky 400
            ShadowColor = new Color(0, 0, 0, 0.5f),
            ShadowSize = 16,
            ContentMarginLeft = 28,
            ContentMarginRight = 28,
            ContentMarginTop = 24,
            ContentMarginBottom = 24
        };
        _cardPanel.AddThemeStyleboxOverride("panel", panelStyle);
        AddChild(_cardPanel);

        var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        vbox.AddThemeConstantOverride("separation", 14);
        _cardPanel.AddChild(vbox);

        _titleLabel = new Label
        {
            Text = "Уровень завершён!",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        _titleLabel.AddThemeColorOverride("font_color", new Color("38bdf8"));
        vbox.AddChild(_titleLabel);

        // Stars Container
        _starsContainer = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        _starsContainer.AddThemeConstantOverride("separation", 12);
        vbox.AddChild(_starsContainer);

        _ratingCommentLabel = new Label
        {
            Text = "Идеальное решение!",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _ratingCommentLabel.AddThemeFontSizeOverride("font_size", 16);
        _ratingCommentLabel.AddThemeColorOverride("font_color", new Color("fbbf24"));
        vbox.AddChild(_ratingCommentLabel);

        // Separator
        vbox.AddChild(new HSeparator());

        // Stats grid
        var statsVBox = new VBoxContainer();
        statsVBox.AddThemeConstantOverride("separation", 6);

        _movesLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _movesLabel.AddThemeFontSizeOverride("font_size", 16);
        statsVBox.AddChild(_movesLabel);

        _optimalLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _optimalLabel.AddThemeFontSizeOverride("font_size", 14);
        _optimalLabel.AddThemeColorOverride("font_color", new Color("94a3b8"));
        statsVBox.AddChild(_optimalLabel);

        _timeLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _timeLabel.AddThemeFontSizeOverride("font_size", 14);
        _timeLabel.AddThemeColorOverride("font_color", new Color("94a3b8"));
        statsVBox.AddChild(_timeLabel);

        vbox.AddChild(statsVBox);

        // Buttons
        var btnVBox = new VBoxContainer();
        btnVBox.AddThemeConstantOverride("separation", 8);

        _nextLevelBtn = CreateButton("Следующий уровень", new Color("0284c7"));
        _nextLevelBtn.Pressed += () => EmitSignal(SignalName.NextLevelClicked);
        btnVBox.AddChild(_nextLevelBtn);

        var rowBtns = new HBoxContainer();
        rowBtns.AddThemeConstantOverride("separation", 10);

        _retryBtn = CreateButton("Играть снова", new Color("334155"));
        _retryBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _retryBtn.Pressed += () => EmitSignal(SignalName.RetryClicked);
        rowBtns.AddChild(_retryBtn);

        _menuBtn = CreateButton("В меню", new Color("334155"));
        _menuBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _menuBtn.Pressed += () => EmitSignal(SignalName.MenuClicked);
        rowBtns.AddChild(_menuBtn);

        btnVBox.AddChild(rowBtns);
        vbox.AddChild(btnVBox);
    }

    private Button CreateButton(string text, Color bgColor)
    {
        var btn = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(0, 42)
        };
        btn.AddThemeFontSizeOverride("font_size", 15);

        var normalStyle = new StyleBoxFlat
        {
            BgColor = bgColor,
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 12,
            ContentMarginRight = 12
        };
        var hoverStyle = new StyleBoxFlat
        {
            BgColor = bgColor.Lightened(0.15f),
            CornerRadiusTopLeft = 8,
            CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ContentMarginLeft = 12,
            ContentMarginRight = 12
        };

        btn.AddThemeStyleboxOverride("normal", normalStyle);
        btn.AddThemeStyleboxOverride("hover", hoverStyle);
        btn.AddThemeStyleboxOverride("pressed", hoverStyle);
        return btn;
    }

    public void ShowVictory(int moves, int optimal, int stars, float timeSeconds, bool hasNextLevel, bool isAutoMode)
    {
        Visible = true;
        _nextLevelBtn.Visible = hasNextLevel && !isAutoMode;

        _movesLabel.Text = $"Сделано ходов: {moves}";
        _optimalLabel.Text = $"Идеально возможно: {optimal} ходов";

        int mins = (int)(timeSeconds / 60);
        int secs = (int)(timeSeconds % 60);
        _timeLabel.Text = isAutoMode ? "Решено автоматически" : $"Затрачено времени: {mins:D2}:{secs:D2}";

        _ratingCommentLabel.Text = stars switch
        {
            3 => "⭐⭐⭐ Идеально! Минимальное число ходов!",
            2 => "⭐⭐ Отлично! Почти рекордный результат!",
            1 => "⭐ Хорошо! Головоломка решена!",
            _ => "Завершено, но можно решить быстрее!"
        };

        // Render animated stars
        foreach (Node child in _starsContainer.GetChildren())
        {
            child.QueueFree();
        }

        for (int i = 1; i <= 3; i++)
        {
            bool earned = i <= stars;
            var starLabel = new Label
            {
                Text = earned ? "★" : "☆",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            starLabel.AddThemeFontSizeOverride("font_size", 42);
            starLabel.AddThemeColorOverride("font_color", earned ? new Color("fbbf24") : new Color("475569"));
            _starsContainer.AddChild(starLabel);

            if (earned)
            {
                int starIndex = i;
                GetTree().CreateTimer(0.15f * starIndex).Timeout += () =>
                {
                    SoundManager.Instance?.PlayStar();
                };
            }
        }
    }
}
