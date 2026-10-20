using System;
using Godot;
using HanoiTowers.Core;

namespace HanoiTowers.UI;

public partial class LevelSelectController : Control
{
    [Signal]
    public delegate void LevelSelectedEventHandler(int levelNumber, int diskCount);

    [Signal]
    public delegate void BackToMenuEventHandler();

    private GridContainer _grid = null!;

    public override void _Ready()
    {
        SetupUI();
    }

    public void RefreshLevels()
    {
        GameProgress.Load();
        foreach (Node child in _grid.GetChildren())
        {
            child.QueueFree();
        }

        for (int lvl = 1; lvl <= 8; lvl++)
        {
            int diskCount = lvl + 2;
            bool isUnlocked = GameProgress.Current.Levels.TryGetValue(lvl, out var record) && record.IsUnlocked;
            int stars = record?.Stars ?? 0;
            int bestMoves = record?.BestMoves ?? int.MaxValue;

            var card = CreateLevelCard(lvl, diskCount, isUnlocked, stars, bestMoves);
            _grid.AddChild(card);
        }
    }

    private void SetupUI()
    {
        AnchorRight = 1.0f;
        AnchorBottom = 1.0f;

        // Background
        var bg = new ColorRect
        {
            Color = new Color("090d16"),
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f
        };
        AddChild(bg);

        // Header Panel
        var header = new PanelContainer
        {
            AnchorRight = 1.0f,
            CustomMinimumSize = new Vector2(0, 68)
        };
        var headerStyle = new StyleBoxFlat
        {
            BgColor = new Color("0f172a"),
            BorderWidthBottom = 2,
            BorderColor = new Color("1e293b"),
            ContentMarginLeft = 24,
            ContentMarginRight = 24
        };
        header.AddThemeStyleboxOverride("panel", headerStyle);
        AddChild(header);

        var headerHBox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Begin };
        headerHBox.AddThemeConstantOverride("separation", 20);
        header.AddChild(headerHBox);

        var backBtn = new Button { Text = "← В меню", CustomMinimumSize = new Vector2(100, 38) };
        backBtn.Pressed += () => EmitSignal(SignalName.BackToMenu);
        headerHBox.AddChild(backBtn);

        var title = new Label
        {
            Text = "Кампания: Выбор уровня (Только ручной режим)",
            VerticalAlignment = VerticalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color("38bdf8"));
        headerHBox.AddChild(title);

        // Scroll Container for Grid
        var scroll = new ScrollContainer
        {
            AnchorRight = 1.0f,
            AnchorBottom = 1.0f,
            OffsetTop = 90,
            OffsetLeft = 40,
            OffsetRight = -40,
            OffsetBottom = -30
        };
        AddChild(scroll);

        var centerBox = new CenterContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill
        };
        scroll.AddChild(centerBox);

        _grid = new GridContainer
        {
            Columns = 4
        };
        _grid.AddThemeConstantOverride("h_separation", 20);
        _grid.AddThemeConstantOverride("v_separation", 20);
        centerBox.AddChild(_grid);

        RefreshLevels();
    }

    private Control CreateLevelCard(int levelNumber, int diskCount, bool isUnlocked, int stars, int bestMoves)
    {
        var card = new PanelContainer
        {
            CustomMinimumSize = new Vector2(240, 200)
        };

        var style = new StyleBoxFlat
        {
            BgColor = isUnlocked ? new Color("1e293b") : new Color("0f172a"),
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            CornerRadiusBottomLeft = 12,
            CornerRadiusBottomRight = 12,
            BorderWidthLeft = 2,
            BorderWidthRight = 2,
            BorderWidthTop = 2,
            BorderWidthBottom = 2,
            BorderColor = isUnlocked ? (stars == 3 ? new Color("fbbf24") : new Color("38bdf8")) : new Color("334155"),
            ContentMarginLeft = 16,
            ContentMarginRight = 16,
            ContentMarginTop = 16,
            ContentMarginBottom = 16
        };
        card.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        vbox.AddThemeConstantOverride("separation", 10);
        card.AddChild(vbox);

        var numLabel = new Label
        {
            Text = isUnlocked ? $"Уровень {levelNumber}" : $"🔒 Уровень {levelNumber}",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        numLabel.AddThemeFontSizeOverride("font_size", 20);
        numLabel.AddThemeColorOverride("font_color", isUnlocked ? Colors.White : new Color("64748b"));
        vbox.AddChild(numLabel);

        var diskLabel = new Label
        {
            Text = $"{diskCount} дисков (Идеал: {StarCalculator.GetOptimalMoves(diskCount)})",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        diskLabel.AddThemeFontSizeOverride("font_size", 14);
        diskLabel.AddThemeColorOverride("font_color", new Color("94a3b8"));
        vbox.AddChild(diskLabel);

        // Stars Row
        var starsHBox = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        starsHBox.AddThemeConstantOverride("separation", 6);
        for (int s = 1; s <= 3; s++)
        {
            bool earned = isUnlocked && s <= stars;
            var starLabel = new Label
            {
                Text = earned ? "★" : "☆",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            starLabel.AddThemeFontSizeOverride("font_size", 22);
            starLabel.AddThemeColorOverride("font_color", earned ? new Color("fbbf24") : new Color("475569"));
            starsHBox.AddChild(starLabel);
        }
        vbox.AddChild(starsHBox);

        // Best score label
        var bestLabel = new Label
        {
            Text = (isUnlocked && bestMoves < int.MaxValue) ? $"Рекорд: {bestMoves} ходов" : (isUnlocked ? "Не пройден" : "Закрыто"),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        bestLabel.AddThemeFontSizeOverride("font_size", 13);
        bestLabel.AddThemeColorOverride("font_color", new Color("64748b"));
        vbox.AddChild(bestLabel);

        // Play Button
        var playBtn = new Button
        {
            Text = isUnlocked ? "Играть" : "Заблокировано",
            Disabled = !isUnlocked,
            CustomMinimumSize = new Vector2(0, 36)
        };
        if (isUnlocked)
        {
            int lvl = levelNumber;
            int disks = diskCount;
            playBtn.Pressed += () => EmitSignal(SignalName.LevelSelected, lvl, disks);
        }
        vbox.AddChild(playBtn);

        return card;
    }
}
